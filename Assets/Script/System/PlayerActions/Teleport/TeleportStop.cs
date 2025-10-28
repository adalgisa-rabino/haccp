using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

[ExecuteAlways]
public class TeleportStop : MonoBehaviour
{
    [Header("Spline di riferimento (assegnare in Inspector)")]
    public SplineContainer spline;

    [Header("Campionamento")]
    [Range(10, 500)]
    public int samples = 150;

    [Header("Risultato (sola lettura)")]
    [SerializeField] private float TimeOnSpline;
    [SerializeField] private Vector3 closestPos;


    //Campi pubblici con annessi metodi che espongono pubblicamente il dato(T, ClosestPos) senza permettere la scrittura dall’esterno;
    public float timeOnSpline    {
        get { return TimeOnSpline; }
    }
  
    public Vector3 ClosestPos
    {
        get { return closestPos; }
    }


    private void OnValidate() //eseguita quando serve un ricalcolo perché si ricmpila uno script o si modifica qualcosa in inspector
    {
        if (spline != null) ClosestToTeleportStopOnSpline(); 
    }

    private void Awake() //eseguito runtime quando si apre la scena o si è in playmode
    {
        if (spline != null && (TimeOnSpline <= 0f || TimeOnSpline >= 1f))
            ClosestToTeleportStopOnSpline();
    }

    //ClosestToTeleportStopOnSpline() calcola il punto (nel percorso/tempo e nello spazio) della spline più vicino alla posizione world del TeleportStop
    //Assegna a tOnSpline = t(valore normalizzato 0..1 da usare come target sulla spline)
    //e a closestPos = pos(posizione world del punto più vicino sulla spline)
    public void ClosestToTeleportStopOnSpline()
    {
        var (t, pos, _) = SplineNearest.ClosestOnSpline(spline, samples, transform.position); //transform.position si riferisce al Transform del GameObject a cui è attaccato il componente TeleportStop
        TimeOnSpline = t;
        closestPos = pos;
    }

    //evidenzia la posizione più vicina sulla spline al TeleoportStop considerato,
    //crea una sfera di raggio 0.12 attorno alla posizione e una linea sulla splineche collega posizione e TeleportStop
    private void OnDrawGizmosSelected()
    {
        if (!spline) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(closestPos, 0.12f);
        Gizmos.DrawLine(transform.position, closestPos);
    }
}

