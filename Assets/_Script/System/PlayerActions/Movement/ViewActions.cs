using UnityEngine;


/// Gestisce il pitch (asse X) della vista agendo su un "pivot"
/// In questo caso è il CameraPivot, figlio del Player
public class ViewActions : MonoBehaviour
{
    [Header("Target della vista")]
    [Tooltip("Il Transform da ruotare in X (di solito il CameraPivot). Deve essere figlio del Player.")]
    public Transform pivot;   // ← ASSEGNA il CameraPivot qui!

    [Header("Pitch")]
    [Tooltip("Velocità di rotazione verticale (gradi/s).")]
    public float rotationSpeed = 90f;

    [Tooltip("Limite massimo su/giù (gradi).")]
    public float maxPitch = 30f;

    [Header("Comportamento")]
    [Tooltip("Se true forza Y=0 e Z=0 sul pivot (consigliato se pivot = CameraPivot).")]
    public bool forceZeroYawRoll = true;

    // Stato input (arriva dai pulsanti UI)
    private bool lookUpHeld, lookDownHeld;

    // Stato interno del pitch (clampato a [-maxPitch, +maxPitch])
    private float _pitchX;

    // --- API chiamate dai pulsanti ---
    public void SetLookUp(bool active) => lookUpHeld = active;
    public void SetLookDown(bool active) => lookDownHeld = active;

    /// <summary>
    /// Imposta SUBITO il pitch interno e lo applica al pivot.
    /// Utile per teleport: evita che il LateUpdate ripristini il vecchio angolo.
    /// </summary>
    public void SetPitchImmediate(float pitchDegrees)
    {
        EnsurePivot();
        _pitchX = Mathf.Clamp(pitchDegrees, -maxPitch, +maxPitch);
        ApplyRotation();
        // opzionale: azzera eventuali pressioni ancora attive
        lookUpHeld = lookDownHeld = false;
    }

    private void Awake()
    {
        EnsurePivot();

        // Inizializza il pitch leggendo l'X attuale del pivot
        float x = pivot.localEulerAngles.x;
        if (x > 180f) x -= 360f;
        _pitchX = Mathf.Clamp(x, -maxPitch, +maxPitch);
    }

    private void LateUpdate()
    {
        EnsurePivot();

        float dir = 0f;
        if (lookUpHeld) dir += 1f;
        if (lookDownHeld) dir -= 1f;

        if (Mathf.Approximately(dir, 0f))
        {
            // Nessun input: rispetta eventuali modifiche esterne (teleport)
            float x = pivot.localEulerAngles.x;
            if (x > 180f) x -= 360f;
            _pitchX = Mathf.Clamp(x, -maxPitch, +maxPitch);
        }
        else
        {
            _pitchX += dir * rotationSpeed * Time.deltaTime;
            _pitchX = Mathf.Clamp(_pitchX, -maxPitch, +maxPitch);
        }

        ApplyRotation();
    }


    private void ApplyRotation()
    {
        if (!pivot) return;

        if (forceZeroYawRoll)
        {
            // Pitch puro sul pivot: Y=0, Z=0 (non influisce sullo yaw del Player)
            pivot.localRotation = Quaternion.Euler(_pitchX, 0f, 0f);
        }
        else
        {
            // (opzione rara) preserva Y/Z locali del pivot
            Vector3 e = pivot.localEulerAngles;
            pivot.localRotation = Quaternion.Euler(_pitchX, e.y, e.z);
        }
    }

    private void EnsurePivot()
    {
        if (pivot) return;

        // Prova a trovare un figlio chiamato "CameraPivot"
        var t = transform.Find("CameraPivot");
        if (t) { pivot = t; return; }

        // Fallback estremo: usa se stesso (sconsigliato, annulleresti lo yaw)
        pivot = transform;
#if UNITY_EDITOR
        Debug.LogWarning("[ViewActions] 'pivot' non assegnato: impostato di fallback a 'transform'. " +
                         "Assegna il CameraPivot per evitare di azzerare lo yaw del Player.", this);
#endif
    }
}
