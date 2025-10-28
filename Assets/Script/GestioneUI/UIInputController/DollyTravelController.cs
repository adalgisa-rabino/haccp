using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Travel su spline con due DollyVCam (forward/backward), verso più corto, overshoot-safe,
/// sincronizzazione continua di Player (pos+yaw) e Pivot (pitch) alla Dolly attiva,
/// handoff finale alla MoveVCamera senza scarti.
/// </summary>
public class DollyTravelController : MonoBehaviour
{
    [Header("Spline & VCams")]
    public SplineContainer spline;               // Unity Splines (stessa su FWD/BWD)
    public CinemachineCamera moveVCam;           // POV libero (segue il pivot)
    public CinemachineCamera dollyVCamFwd;       // Dolly con track di look "forward"
    public CinemachineCamera dollyVCamBwd;       // Dolly con track di look "backward"

    [Header("Player rig (seguìto da MoveVCamera)")]
    public Transform playerTransform;           // Root del player (ruota Y e prende la posizione)
    public ViewActions viewActions;              // Per impostare il pitch del pivot in modo immediato
    public Transform cameraPivotTransform;        // Solo se non usi ViewActions: pivot della camera (figlio del player)

    [Header("Travel settings (t/sec, Units=Normalized)")]
    [Range(10, 500)] public int samples = 150;   // campioni per trovare tStart
    public bool closedSpline = true;             // spline chiusa → wrap su [0..1)
    public float travelSpeedT = 0.35f;           // velocità costante in t/s
    //public float stopEpsilonT = 0.005f;          // tolleranza di arrivo in t

    [Header("Facoltativo: disattiva input durante il travel")]
    public MonoBehaviour[] disableDuringTravel;  // es. UIWalkController, UIRotateController, ViewActions, ecc.

    // ---- stato interno ----
    private CinemachineSplineDolly CameraFwd;
    private CinemachineSplineDolly CameraBwd;
    private bool _isTravelling;
    private int _dir = +1;                     // +1 avanti, -1 indietro
    private float _tCurrent;
    private float _tTarget;
    private float _tripRemainingStartT = 1f;

    void Awake()
    {
        if (dollyVCamFwd) CameraFwd = dollyVCamFwd.GetComponent<CinemachineSplineDolly>();
        if (dollyVCamBwd) CameraBwd = dollyVCamBwd.GetComponent<CinemachineSplineDolly>();
    }

    

    /// <summary>
    ///    ------  Avvia il viaggio verso 'stop' scegliendo il verso più corto  ------
    /// </summary>
    
    public void BeginTravelTo(TeleportStop stop)
    {
        // se i campi non sono assegnai non partire
        if (!stop || spline == null || moveVCam == null || dollyVCamFwd == null || dollyVCamBwd == null
            || CameraFwd == null || CameraBwd == null || playerTransform == null)
            return;

        //assegnazione telecamera principale, se non c'è esci
        var cam = Camera.main;
        if (!cam) return;

        // 1) t di partenza: se sei già su una Dolly, usa la sua CameraPosition; altrimenti punto più vicino alla POV
        float tStart;
        if (dollyVCamFwd.Priority > moveVCam.Priority && dollyVCamFwd.Priority >= dollyVCamBwd.Priority && CameraFwd != null)
            tStart = CameraFwd.CameraPosition;
        else if (dollyVCamBwd.Priority > moveVCam.Priority && dollyVCamBwd.Priority >= dollyVCamFwd.Priority && CameraBwd != null)
            tStart = CameraBwd.CameraPosition;
        else
            tStart = SplineNearest.ClosestOnSpline(spline, samples, cam.transform.position).t;

        //Mathf.Clamp01(x) forza il valore x nell’intervallo [0,1].
        //Qui garantisce che sia il punto di partenza (_tCurrent) sia il target (_tTarget) siano parametri validi
        //per la spline (le spline si valutano su t normalizzato 0..1).
        _tCurrent = Mathf.Clamp01(tStart); //tStart è la posizione sulla spline più vicina a dove si trova la camera
        _tTarget = Mathf.Clamp01(stop.timeOnSpline); //stop.T è la posizione del TeleportStop

        // 2) verso più corto
        // --- rimosso il ramo per spline aperte: assumiamo sempre spline chiusa (wrap) nel gioco ---
        // calcola la distanza tra la posizione di TeleportStop e la posizione iniziale a cui mi trovo (entrambi valori di posizione sulla spline) in una direzione e nell'altra
        float forward = Mathf.Repeat(_tTarget - _tCurrent, 1f);
        float backward = Mathf.Repeat(_tCurrent - _tTarget, 1f);
        _dir = (forward <= backward) ? +1 : -1;  //scegli il verso con distanza minore; in caso di parità sceglie forward (+1) per via della condizione <=
        _tripRemainingStartT = Mathf.Max((_dir > 0) ? forward : backward); 

        // 3) imposta stessa spline su entrambe e sync iniziale
        SetCameraPositionBoth(_tCurrent);
        //SyncPlayerAndPivotToDolly();             //?? allinea subito Player&Pivot al t di partenza

        // 4) attiva il rig giusto, disattiva input se richiesto
        ActivateRig(_dir > 0); // Attiva la Dolly forward o backward via Priority; Move più bassa durante il travel
        SetInputsEnabled(false); // Dis/abilita i tuoi script di input durante il travel

        _isTravelling = true;
    }

    void Update()
    {
        if (!_isTravelling) return;

        // distanza residua lungo il verso scelto
        float remaining = (_dir > 0)
            ? Mathf.Repeat(_tTarget - _tCurrent, 1f)
            : Mathf.Repeat(_tCurrent - _tTarget, 1f);

        // passo costante
        float maxStep = travelSpeedT * Time.deltaTime;

        // se supero il target (TravelStop) ferma la camera attiva e cambia camera
        if (remaining <= maxStep)
        {
            _tCurrent = _tTarget;
            SetCameraPositionBoth(_tCurrent);
            SyncPlayerAndPivotToDolly();         //?? ultima sync (sono già allineati)
            FinishAndHandoffToMove();            //?
            return;
        }

        // altrimenti avanza con segno
        float stepSigned = (_dir > 0 ? +maxStep : -maxStep);
        _tCurrent = Mathf.Repeat(_tCurrent + stepSigned, 1f);

        // muovi le Dolly e sincronizza Player&Pivot ad ogni frame → nessuno scarto allo switch
        SetCameraPositionBoth(_tCurrent);
    }

    /// Scrive CameraPosition = t su entrambe le Dolly (sempre).
    private void SetCameraPositionBoth(float t)
    {
        if (CameraFwd) CameraFwd.CameraPosition = t;
        if (CameraBwd) CameraBwd.CameraPosition = t;
    }

    /// Attiva la Dolly forward o backward via Priority; Move più bassa durante il travel
    private void ActivateRig(bool forward)
    {
        if (forward)
        {
            if (dollyVCamFwd) dollyVCamFwd.Priority = 20;
            if (dollyVCamBwd) dollyVCamBwd.Priority = 10;
        }
        else
        {
            if (dollyVCamFwd) dollyVCamFwd.Priority = 10;
            if (dollyVCamBwd) dollyVCamBwd.Priority = 20;
        }
        if (moveVCam) moveVCam.Priority = 5;
    }

    /// Dis/abilita i tuoi script di input durante il travel
    private void SetInputsEnabled(bool enabled)
    {
        if (disableDuringTravel == null) return;
        foreach (var mb in disableDuringTravel)
            if (mb) mb.enabled = enabled;
    }

    /// Sincronizza Player (posizione + yaw) e Pivot (pitch) alla Dolly attiva.
    private void SyncPlayerAndPivotToDolly()
    {
        var activeDolly = (_dir > 0) ? dollyVCamFwd : dollyVCamBwd;
        if (!activeDolly || playerTransform == null) return;

        Transform d = activeDolly.transform;

        // assegna al player solo X,Z dalla dolly, conserva Y corrente del player
        Vector3 playerPos = playerTransform.position;
        playerTransform.position = new Vector3(d.position.x, playerPos.y, d.position.z);

        // assegna al pivot la posizione completa della dolly (X,Y,Z) rispettando la gerarchia
        if (cameraPivotTransform != null)
        {
            var parent = cameraPivotTransform.parent;
            if (parent != null)
                cameraPivotTransform.localPosition = parent.InverseTransformPoint(d.position);
            else
                cameraPivotTransform.position = d.position;

            // SYNC VIEW sul pivot: pitch (X) dalla componente verticale di forward, roll (Z) dalla dolly
            float pitchDeg = Mathf.Asin(Mathf.Clamp(d.forward.y, -1f, 1f)) * Mathf.Rad2Deg;
            float rollDeg = Mathf.DeltaAngle(0f, d.rotation.eulerAngles.z);

            // Se il pivot è figlio del player, applichiamo pitch/roll in locale (yaw gestito dal player)
            if (cameraPivotTransform.IsChildOf(playerTransform))
            {
                cameraPivotTransform.localEulerAngles = new Vector3(pitchDeg, 0f, rollDeg);
            }
            else
            {
                // pivot indipendente: combiniamo yaw del player con pitch/roll della dolly
                float playerYaw = playerTransform != null ? playerTransform.eulerAngles.y : d.rotation.eulerAngles.y;
                cameraPivotTransform.rotation = Quaternion.Euler(pitchDeg, playerYaw, rollDeg);
        }
    }

    /// Conclude il travel: riabilita input e passa alla MoveVCamera (che è già allineata).
    private void FinishAndHandoffToMove()
    {
        SetInputsEnabled(true);

        SyncPlayerAndPivotToDolly();

        // switch POV (Brain: meglio "Cut" o blend corto per test)
        if (moveVCam) moveVCam.Priority = 30;
        if (dollyVCamFwd) dollyVCamFwd.Priority = 10;
        if (dollyVCamBwd) dollyVCamBwd.Priority = 10;

        _isTravelling = false;

       }
}