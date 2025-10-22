using System;
using UnityEngine;
using UnityEngine.Splines;

public static class SplineNearest
{
    /// <summary>
    /// Trova il punto più vicino sulla spline specificata (approssimato) campionando N punti.
    /// Ritorna t (0..1) della spline, la posizione world del punto e la distanza.
    /// </summary>
    public static (float t, Vector3 pos, float dist) ClosestOnSpline(
        SplineContainer container, int splineIndex, int samples, Vector3 worldPoint)
    {
        if (container == null || samples < 2)
            throw new ArgumentException("SplineNearest: container nullo o samples < 2");

        // sicurezza: clamp dello splineIndex in range
        splineIndex = Mathf.Clamp(splineIndex, 0, container.Splines.Count - 1);

        float bestT = 0f;
        Vector3 bestPos = container.EvaluatePosition(splineIndex, 0f);
        float bestDist = Vector3.Distance(bestPos, worldPoint);

        for (int i = 1; i <= samples; i++)
        {
            float t = i / (float)samples;
            Vector3 p = container.EvaluatePosition(splineIndex, t);
            float sq = Vector3.SqrMagnitude(p - worldPoint);
            if (sq < bestDist * bestDist)
            {
                bestDist = Mathf.Sqrt(sq);
                bestT = t;
                bestPos = p;
            }
        }

        return (bestT, bestPos, bestDist);
    }

    /// <summary>
    /// Overload per il caso con UNA sola spline nel container (indice implicito 0).
    /// Questo è il metodo che userai dalla scena, passando direttamente la spline selezionata da Inspector.
    /// </summary>
    public static (float t, Vector3 pos, float dist) ClosestOnSpline(
        SplineContainer container, int samples, Vector3 worldPoint)
    {
        return ClosestOnSpline(container, 0, samples, worldPoint);
    }
}
