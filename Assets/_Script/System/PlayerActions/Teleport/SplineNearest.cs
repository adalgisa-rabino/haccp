using System;
using UnityEngine;
using UnityEngine.Splines;

public static class SplineNearest
{
    /// <summary>
    /// Trova il punto più vicino sulla prima spline del container (approssimato) campionando N punti.
    /// Ritorna t (0..1) della spline, la posizione world del punto e la distanza.
    /// </summary>
    public static (float t, Vector3 pos, float dist) ClosestOnSpline(
        SplineContainer container, int samples, Vector3 worldPoint)
    {
        if (container == null)
            throw new ArgumentException("SplineContainer nullo");


        //inizializziamo le variabili che ci servono per il calcolo del punto più vicino da confrontare al primo passo del loop

        float bestT = 0; //bestT è inizializzato a 0, ma è solo il valore iniziale della soglia: il loop campiona tutta la spline (t = 1/samples, 2/samples, …, 1)
                         //e se trova un campione più vicino aggiorna bestT.
                         //Se dopo il loop bestT è ancora 0 significa semplicemente che tra i campioni considerati
                         //il punto più vicino risultava essere l’inizio (t = 0).

        Vector3 bestPos = container.EvaluatePosition(0, 0); //il metodo Vector3 EvaluatePosition(int splineIndex, float t) restituisce la posizione
                                                            //sulla spline (splineIndex indica quale Spline, nel nostro caso ce n'è solo una quindi indice 0)
                                                            //al parametro t (con t = 0 inizio, t = 1 fine)

        float bestDist = Vector3.Distance(bestPos, worldPoint); //il metodo Vector3 Distance(Vector3 a, Vector3 b) restituisce la distanza tra due punti, in questo caso misura
                                                                // la distanza euclidea fra il punto iniziale e il punto di riferimento worldPoint (sarà la posizione della main camera) e la usa come soglia iniziale

        for (int i = 1; i <= samples; i++) //ciclo for che itera da 1 a "samples" (numero di campioni definito in input)
        {
            float time = i / (float)samples; //calcolo del parametro t normalizzato sul numero di campioni "samples" definito in input

            Vector3 position = container.EvaluatePosition(0, time); //il metodo Vector3 EvaluatePosition(int splineIndex, float t) restituisce la posizione
                                                                    //sulla spline (splineIndex indica quale Spline, nel nostro caso ce n'è solo una quindi indice 0)
                                                                    //al parametro t (con t = 0 inizio, t = 1 fine)

            // calcoliamo la distanza reale usando Mathf.Sqrt
            float dist = Mathf.Sqrt(Vector3.SqrMagnitude(position - worldPoint));

            // confronto diretto tra distanze reali (non usiamo più i quadrati)
            if (dist < bestDist)
            {
                bestDist = dist;
                bestT = time;
                bestPos = position;
            }
        }

        return (bestT, bestPos, bestDist);
    }
}