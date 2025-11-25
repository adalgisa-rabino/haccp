using UnityEngine;

[ExecuteAlways]
public class FridgeSnapZone : MonoBehaviour
{
    public Color normalColor = new Color(0f, 1f, 0f, 0.2f);
    public Color highlightColor = new Color(1f, 1f, 0f, 0.4f);

    [Header("GO da usare come riferimento per la Z di appoggio (es. il ripiano).")]
    public Transform snapRoot;   // se null, userà il parent

    private bool highlighted = false;
    private BoxCollider box;

    void Awake()
    {
        box = GetComponent<BoxCollider>();
        if (box == null)
        {
            Debug.LogError("FridgeSnapZone richiede un BoxCollider (trigger).", this);
            return;
        }

        box.isTrigger = true;
    }

    public void SetHighlighted(bool active)
    {
        highlighted = active;
    }

    void OnDrawGizmos()
    {
        if (box == null)
            box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = highlighted ? highlightColor : normalColor;
        Gizmos.DrawCube(box.center, box.size);

        Gizmos.color = Color.white * 0.6f;
        Gizmos.DrawWireCube(box.center, box.size);
    }

    public Bounds GetBounds()
    {
        if (box == null)
            box = GetComponent<BoxCollider>();
        return box.bounds;
    }

    /// <summary>
    /// Ritorna la Z del GO che definisce il piano del ripiano.
    /// </summary>
    public float GetTargetZ()
    {
        Transform root = snapRoot;

        if (root == null)
            root = transform.parent != null ? transform.parent : transform;

        return root.position.z;
    }
}
