using UnityEngine;

public class HandPinchGrab : MonoBehaviour
{
    public Transform[] handPoints; // 0..20 come in MediaPipe (assegnali dal tuo HandTracking)
    [Header("Detection")]
    [Tooltip("Moltiplicatore della larghezza palmo per definire la soglia di pinch.")]
    public float pinchThresholdFactor = 0.35f;
    [Tooltip("Raggio di ricerca oggetti vicino al palmo (in unità di scena).")]
    public float grabSearchRadius = 0.15f;
    public LayerMask grabLayerMask = ~0; // di default tutto

    [Header("Attach")]
    public AttachMode attachMode = AttachMode.Parenting;
    [Tooltip("Se FixedJoint, rigidbody host creato/riuso per il joint (kinematic).")]
    public Rigidbody handAnchorRigidbody; // opzionale: se assente lo crea runtime

    [Header("Tuning")]
    [Tooltip("Distanza massima per considerare un nuovo target quando fai pinch.")]
    public float maxAttachDistance = 0.25f;
    [Tooltip("Smorzamento movimento dell'oggetto quando è parentato (0=rigido, 1=lento).")]
    [Range(0f, 1f)] public float followSmoothing = 0.15f;

    public enum AttachMode { Parenting, FixedJoint }

    // stato
    private Rigidbody grabbedRb;
    private Transform originalParent;
    private bool wasPinching;
    private FixedJoint currentJoint;

    // Indici MediaPipe
    const int WRIST = 0;
    const int THUMB_TIP = 4;
    const int INDEX_MCP = 5;
    const int INDEX_TIP = 8;
    const int PINKY_MCP = 17;

    void Awake()
    {
        if (attachMode == AttachMode.FixedJoint && handAnchorRigidbody == null)
        {
            var anchor = new GameObject("HandGrabAnchor");
            anchor.transform.SetParent(transform, false);
            handAnchorRigidbody = anchor.AddComponent<Rigidbody>();
            handAnchorRigidbody.isKinematic = true;
            handAnchorRigidbody.useGravity = false;
        }
    }

    void Update()
    {
        if (handPoints == null || handPoints.Length < 21) return;

        // Punti chiave
        var thumbTip = handPoints[THUMB_TIP].position;
        var indexTip = handPoints[INDEX_TIP].position;
        var indexMCP = handPoints[INDEX_MCP].position;
        var pinkyMCP = handPoints[PINKY_MCP].position;

        // Centro palmo: media wrist + MCP indice + MCP mignolo (semplice e stabile)
        Vector3 palmCenter = (handPoints[WRIST].position + indexMCP + pinkyMCP) / 3f;

        // Larghezza palmo e soglia pinch
        float palmWidth = Vector3.Distance(indexMCP, pinkyMCP);
        float pinchThreshold = Mathf.Max(0.001f, palmWidth * pinchThresholdFactor);

        // Distanza pinch (indice↔pollice)
        float pinchDist = Vector3.Distance(indexTip, thumbTip);
        bool isPinching = pinchDist < pinchThreshold;

        // tieni l’anchor (se usato) sul palmo
        if (handAnchorRigidbody) handAnchorRigidbody.transform.position = palmCenter;

        // Transizioni di stato
        if (isPinching && !wasPinching)
        {
            TryBeginGrab(palmCenter);
        }
        else if (!isPinching && wasPinching)
        {
            Release();
        }

        // Se parenting, render segui-palma più morbido (opzionale)
        if (attachMode == AttachMode.Parenting && grabbedRb)
        {
            // quando parentato, l’oggetto è figlio del palmo: usa smoothing su localPosition
            // (se vuoi fisica più vera, usa direttamente FixedJoint)
            // qui non serve altro: il transform segue automaticamente.
        }

        wasPinching = isPinching;
    }

    void TryBeginGrab(Vector3 palmCenter)
    {
        // se già stai tenendo qualcosa, ignora
        if (grabbedRb) return;

        // cerca rigidbody vicino al palmo
        Collider[] hits = Physics.OverlapSphere(palmCenter, grabSearchRadius, grabLayerMask, QueryTriggerInteraction.Ignore);
        Rigidbody best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            var rb = h.attachedRigidbody;
            if (rb == null || rb.isKinematic) continue;            // ignora statici/kinematic
            if (!rb.gameObject.activeInHierarchy) continue;

            // distanza dal palmo al punto più vicino del collider
            Vector3 closest = h.ClosestPoint(palmCenter);
            float d = Vector3.Distance(closest, palmCenter);
            if (d < bestDist && d <= maxAttachDistance)
            {
                bestDist = d;
                best = rb;
            }
        }

        if (best != null)
        {
            BeginGrab(best);
        }
    }

    void BeginGrab(Rigidbody target)
    {
        grabbedRb = target;

        switch (attachMode)
        {
            case AttachMode.Parenting:
                originalParent = target.transform.parent;
                // “stacca” dalla fisica durante la presa per evitare jitter
                target.isKinematic = true;
                target.useGravity = false;
                // aggancia al palmo
                Transform palm = handAnchorRigidbody ? handAnchorRigidbody.transform : transform;
                target.transform.SetParent(palm, true);
                // posiziona senza scatti (mantieni world space)
                break;

            case AttachMode.FixedJoint:
                if (!handAnchorRigidbody)
                {
                    Debug.LogWarning("FixedJoint richiesto ma manca handAnchorRigidbody.");
                    return;
                }
                // crea joint
                currentJoint = handAnchorRigidbody.gameObject.AddComponent<FixedJoint>();
                currentJoint.connectedBody = target;
                currentJoint.breakForce = Mathf.Infinity;
                currentJoint.breakTorque = Mathf.Infinity;
                // assicura fisica
                target.isKinematic = false;
                target.useGravity = true;
                break;
        }
    }

    void Release()
    {
        if (!grabbedRb) return;

        switch (attachMode)
        {
            case AttachMode.Parenting:
                // ripristina parent e fisica
                grabbedRb.transform.SetParent(originalParent, true);
                grabbedRb.isKinematic = false;
                grabbedRb.useGravity = true;
                break;

            case AttachMode.FixedJoint:
                if (currentJoint)
                {
                    Destroy(currentJoint);
                    currentJoint = null;
                }
                break;
        }

        grabbedRb = null;
        originalParent = null;
    }

    void OnDrawGizmosSelected()
    {
        if (handPoints != null && handPoints.Length > WRIST)
        {
            var indexMCP = handPoints.Length > INDEX_MCP ? handPoints[INDEX_MCP].position : transform.position;
            var pinkyMCP = handPoints.Length > PINKY_MCP ? handPoints[PINKY_MCP].position : transform.position;
            var palmCenter = (handPoints[WRIST].position + indexMCP + pinkyMCP) / 3f;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(palmCenter, grabSearchRadius);
        }
    }
}
    