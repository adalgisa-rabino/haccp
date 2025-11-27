using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTrackingOld : MonoBehaviour
{
    public UDPReceive udpReceive;
    public GameObject[] handPoints;
    [SerializeField] Transform handRoot;

    // posizione di partenza del root (offset "fisso" nel mondo)
    private Vector3 rootBaseLocalPos;

    // centro del palmo "di riferimento" (primo frame) per lavorare in offset
    private bool hasInitialPalmCenter = false;  // true dopo il primo frame valido
    private float cx0, cy0, cz0;                // centro del palmo al primo frame
    private bool isColliding = false;   // true se l'OverlapBox tocca il frigo


    [Header("cose")]

    public float movementScaleXY = 0.3f;   // riduce la sensibilità X/Y (0.3 = 30%)
    public bool enablePalmNormalization = true;   // attiva/disattiva
    public float referencePalmArea = 0.015f;      // area target (nel tuo spazio normalizzato)
    public float scaleGain = 5.0f;                // amplifica/attenua la riscalatura
    public float minScale = 0.1f;                 // clamp inferiore
    public float maxScale = 5.0f;                 // clamp superiore

    public LayerMask fridgeMask;              // seleziona SOLO "FridgeBlock" nell'Inspector
    public Vector3 handBoxSize = new Vector3(0.20f, 0.20f, 0.20f);
    public float freezeTime = 0.4f;       // tempo in secondi di blocco del tracking
    public float pushBackStrength = 0.8f; // usato mentre è congelata
    private float freezeTimer = 0f;
    private bool freezeActive = false;

    [Header("Camera mapping")]
    public float palmCenterRefX = 0f;   // valore X quando la mano è al centro della webcam
    public float palmCenterRefY = 0f;   // valore Y quando la mano è al centro della webcam

    public float baseZ = 0.6f;     // z di partenza del root
    public float depthScale = 0.2f;// quanto influisce lo z medio dei landmark

    void Start()
    {
        // memorizza la posizione iniziale del root impostata nell’Inspector
        if (handRoot != null)
            rootBaseLocalPos = handRoot.localPosition;
    }


    void Update()
    {

        // Se la mano è congelata, non aggiornare la posizione dal tracking
        if (freezeActive)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0f)
                freezeActive = false;   // sblocco

            // ma applico comunque il pushback (sarà fatto dopo)
            // quindi esco da qui PRIMA del tracking
            goto AfterTracking;
        }



        string data = udpReceive.data;
        if (string.IsNullOrEmpty(data)) return;

        data = data.Remove(0, 1);
        data = data.Remove(data.Length - 1, 1);
        string[] points = data.Split(',');

        // calcolo z medio (stessa scala dei punti: ÷100)
        float zMean = 0f;

        float xMin, xMax, yMin, yMax;
        xMin = xMax = 7.0f - float.Parse(points[0]) / 100f;
        yMin = yMax = float.Parse(points[1]) / 100f;
        for (int i = 0; i < 21; i++)
        {
            float x = 7f - float.Parse(points[i * 3]) / 100f;
            float y = float.Parse(points[i * 3 + 1]) / 100f;
            float z = -float.Parse(points[i * 3 + 2]) / 100f;
            xMin = x < xMin ? x : xMin;
            xMax = x > xMax ? x : xMax;
            yMin = y < yMin ? y : yMin;
            yMax = y > yMax ? y : yMax;
        }

        // --- Calcolo area del triangolo del palmo (0,5,17) in XY ---
        int i0 = 0, i5 = 5, i17 = 17;
        float x0 = 7f - float.Parse(points[i0 * 3]) / 100f;
        float y0 = float.Parse(points[i0 * 3 + 1]) / 100f;
        float x5 = 7f - float.Parse(points[i5 * 3]) / 100f;
        float y5 = float.Parse(points[i5 * 3 + 1]) / 100f;
        float x17 = 7f - float.Parse(points[i17 * 3]) / 100f;
        float y17 = float.Parse(points[i17 * 3 + 1]) / 100f;

        float areaTri = 0.5f * Mathf.Abs((x5 - x0) * (y17 - y0) - (y5 - y0) * (x17 - x0));
        float area = areaTri * 0.3f; // il fattore 5.5 è trovato sperimentalmente per avere un buon rapporto scala e movimento in Z


        // --- Centro del palmo in XY e centro di profondità cz ---
        float cx = (x0 + x5 + x17) / 3f;
        float cy = (y0 + y5 + y17) / 3f;

        Debug.Log($"Palm center cx={cx}, cy={cy}");

        // z dei tre landmark del palmo (NOTA: nello stato attuale z è grezza: -lmZ/100f)
        float z0 = -float.Parse(points[i0 * 3 + 2]) / 100f;
        float z5 = -float.Parse(points[i5 * 3 + 2]) / 100f;
        float z17 = -float.Parse(points[i17 * 3 + 2]) / 100f;
        float cz = (z0 + z5 + z17) / 3f;

        // inizializzazione del centro del palmo di riferimento (offset) al primo frame valido
        if (!hasInitialPalmCenter)
        {
            cx0 = cx;
            cy0 = cy;
            cz0 = cz;
            hasInitialPalmCenter = true;
        }

        for (int i = 0; i < 21; i++)
        {
            float z = -float.Parse(points[i * 3 + 2]) / 100f;   // z grezza del landmark
            zMean += z;
        }

        zMean /= 21f;   // media finale


        /// -- spostamento della mano nel mondo --
        // invece di leggere la posizione corrente e cambiare solo z,
        // parti dalla base memorizzata e aggiungi un offset solo in z
        float zOffset = area;   // qui puoi tenerti area o quello che userai per la profondità

        // Vector3 pos = handRoot.localPosition;               // versione precedente: dipendeva dal valore attuale
        // pos.z = area;                                      // spostamento assoluto in z
        // handRoot.localPosition = pos;

        Vector3 pos = rootBaseLocalPos;
        pos.z += zOffset;       // spostamento in profondità come prima

        // Offset rispetto al centro ideale della webcam
        float dx = cx - palmCenterRefX;
        float dy = cy - palmCenterRefY;

        // Applichiamo offset scalato
        // (segno di dx invertito se destra/sinistra sono ancora ribaltate in scena)
        pos.x = rootBaseLocalPos.x - dx * movementScaleXY;
        pos.y = rootBaseLocalPos.y + dy * movementScaleXY;

        handRoot.localPosition = pos;



        // --- Fattore di scala isotropo (XY e Z) in base all'area del palmo ---
        const float eps = 1e-6f;

        //referencePalmArea = 0.015f; // valore di riferimento per l'area del palmo
        // areaTri è l’area del triangolo del palmo (landmark 0, 5, 17).
        // In condizioni normali è > 0, ma in caso di rumore / perdita di tracking
        // può diventare molto piccola o addirittura zero.
        //
        // Poiché il fattore di scala usa il rapporto (referencePalmArea / areaTri),
        // se areaTri fosse 0 otterremmo una divisione per zero → s divergerebbe → la mano esploderebbe in scena.
        //
        // Per evitare questo imponiamo un valore minimo "eps" molto piccolo.
        // Mathf.Max(areaTri, eps) garantisce che il denominatore non sia mai zero o irrealisticamente piccolo.



        float s = Mathf.Sqrt(referencePalmArea / Mathf.Max(areaTri, eps)) * scaleGain;  //si usa la radice quadrata perché Perché l’area cresce con il quadrato della lunghezza:
                                                                                        //se la mano diventa 2 volte più "lunga", l’area apparente diventa 4 volte più grande.

        Debug.Log("ScaleIn: " + s.ToString("F3"));

        //s = Mathf.Clamp(s * scaleGain, minScale, maxScale); //controlla che s*scaleGain sia tra minScale e maxScale

        Debug.Log("Palm area: " + areaTri.ToString("F6") + " -> Scale: " + s.ToString("F3"));

        // --- Applicazione della scala ai landmark: SOLO scaling relativo, niente offset del root ---
        for (int i = 0; i < 21; i++)
        {
            // posizioni
            float xRaw = 7f - float.Parse(points[i * 3]) / 100f;
            float yRaw = float.Parse(points[i * 3 + 1]) / 100f;
            float zRaw = -float.Parse(points[i * 3 + 2]) / 100f;

            // scala attorno ai centri (cx, cy, cz)
            // (xRaw - cx), (yRaw - cy), (zRaw - cz) indicano i vettori che rappresentano la posizione del punto rispetto al centro.
            // Moltiplicandolo per s li rendiamo più lunghi (mano più lontana → scala > 1) o più corti (mano più vicina → scala < 1).
            // Infine ri - aggiungiamo il centro per riportare il punto nella posizione corretta nel sistema di coordinate: nuovo punto = centro + vettore_scalato

            // PRIMA (versione centrata sul palmo nel sistema "mondo"):
            // float x = cx + (xRaw - cx) * s;
            // float y = cy + (yRaw - cy) * s;
            // float z = cz + (zRaw - cz) * s;

            // ADESSO: i punti stanno in locale rispetto all'handRoot (centro approssimato a (0,0,0))
            float x = (xRaw - cx) * s;
            float y = (yRaw - cy) * s;
            float z = (zRaw - cz) * s;

            handPoints[i].transform.localPosition = new Vector3(x, y, z);
        }

    AfterTracking:

        //----------------------------------------------
        // COLLISIONE CON IL FRIGO (OverlapBox + Freeze)
        //----------------------------------------------

        Vector3 center = handRoot.position;

        Collider[] hits = Physics.OverlapBox(
            center,
            handBoxSize * 0.5f,
            handRoot.rotation,
            fridgeMask
        );

                // aggiorniamo lo stato di collisione per il gizmo
                isColliding = (hits != null && hits.Length > 0);

                if (hits.Length > 0)
                {
                    // 1) entra in freeze solo la prima volta
                    if (!freezeActive)
                    {
                        freezeActive = true;
                        freezeTimer = freezeTime;
                    }

                    // 2) pushback continuo finché la mano è congelata
                    Vector3 pushDir = Vector3.zero;

                    foreach (Collider hit in hits)
                    {
                        Vector3 closest = hit.ClosestPoint(center);
                        Vector3 away = (center - closest).normalized;
                        pushDir += away;
                    }

                    pushDir.Normalize();

                    handRoot.position += pushDir * pushBackStrength * Time.deltaTime;
                }



    }

    void OnDrawGizmos()
    {
        if (handRoot == null) return;

        // Colore del box (giallo quando non collide, rosso quando collide)
        // Gizmos.color = Color.yellow;
        Gizmos.color = isColliding ? Color.red : Color.yellow;

        // Disegna il box come un cubo wireframe
        Gizmos.matrix = Matrix4x4.TRS(
            handRoot.position,
            handRoot.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, handBoxSize);
    }


}
