using UnityEngine;
using UnityEngine.Splines;

public class DollySnapProbe : MonoBehaviour
{
    public SplineContainer spline;
    [Tooltip("Indice della spline da usare (0 = default)")]
    public int splineIndex = 0;

    public Transform probe;
    public int samples = 100;

    void Update()
    {
        if (!spline || !probe) return;

        var (t, pos, dist) = SplineNearest.ClosestOnSpline(spline, splineIndex, samples, probe.position);
        //Debug.Log($"[Probe] Spline={splineIndex}  Closest t={t:0.000}  pos={pos}  dist={dist:0.###}");
    }
}
