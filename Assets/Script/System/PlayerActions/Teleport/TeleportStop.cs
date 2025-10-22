using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
public class TeleportStop : MonoBehaviour
{
    [Header("Spline di riferimento (assegnare in Inspector)")]
    public SplineContainer spline;

    [Header("Campionamento")]
    [Range(10, 500)]
    public int samples = 150;

    [Header("Risultato (sola lettura)")]
    [SerializeField] private float tOnSpline;
    [SerializeField] private Vector3 closestPos;

    public float T => tOnSpline;
    public Vector3 ClosestPos => closestPos;

    private void OnValidate()
    {
        if (spline != null) Recompute();
    }

    private void Awake()
    {
        if (spline != null && (tOnSpline <= 0f || tOnSpline >= 1f))
            Recompute();
    }

    public void Recompute()
    {
        var (t, pos, _) = SplineNearest.ClosestOnSpline(spline, samples, transform.position);
        tOnSpline = t;
        closestPos = pos;
    }

    private void OnDrawGizmosSelected()
    {
        if (!spline) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(closestPos, 0.12f);
        Gizmos.DrawLine(transform.position, closestPos);
    }
}

