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
    public Transform playerRootForYaw;           // Root del player (ruota Y e prende la posizione)
    public ViewActions viewActions;              // Per impostare il pitch del pivot in modo immediato
    public Transform cameraPivotFallback;        // Solo se non usi ViewActions: pivot della camera (figlio del player)

    [Header("Travel settings (t/sec, Units=Normalized)")]
    [Range(10, 500)] public int samples = 150;   // campioni per trovare tStart
    public bool closedSpline = true;             // spline chiusa → wrap su [0..1)
    public float travelSpeedT = 0.35f;           // velocità costante in t/s
    public float stopEpsilonT = 0.005f;          // tolleranza di arrivo in t

    [Header("Facoltativo: disattiva input durante il travel")]
    public MonoBehaviour[] disableDuringTravel;  // es. UIWalkController, UIRotateController, ViewActions, ecc.

    // ---- stato interno ----
    private CinemachineSplineDolly _extFwd;
    private CinemachineSplineDolly _extBwd;
    private bool _isTravelling;
    private int _dir = +1;                     // +1 avanti, -1 indietro
    private float _tCurrent;
    private float _tTarget;
    private float _tripRemainingStartT = 1f;

    void Awake()
    {
        if (dollyVCamFwd) _extFwd = dollyVCamFwd.GetComponent<CinemachineSplineDolly>();
        if (dollyVCamBwd) _extBwd = dollyVCamBwd.GetComponent<CinemachineSplineDolly>();

        if (spline && spline.Splines != null && spline.Splines.Count > 0)
            closedSpline = spline.Splines[0].Closed;
    }

    /// <summary>
    /// Avvia il viaggio verso 'stop' scegliendo il verso più corto.
    /// </summary>
    public void BeginTravelTo(TeleportStop stop)
    {
        if (!stop || spline == null || moveVCam == null || dollyVCamFwd == null || dollyVCamBwd == null
            || _extFwd == null || _extBwd == null || playerRootForYaw == null)
            return;

        var cam = Camera.main;
        if (!cam) return;

        // 1) t di partenza: se sei già su una Dolly, usa la sua CameraPosition; altrimenti punto più vicino alla POV
        float tStart = SplineNearest.ClosestOnSpline(spline, samples, cam.transform.position).t;
        if (dollyVCamFwd.Priority > moveVCam.Priority && dollyVCamFwd.Priority >= dollyVCamBwd.Priority && _extFwd != null)
            tStart = _extFwd.CameraPosition;
        else if (dollyVCamBwd.Priority > moveVCam.Priority && dollyVCamBwd.Priority >= dollyVCamFwd.Priority && _extBwd != null)
            tStart = _extBwd.CameraPosition;

        _tCurrent = Mathf.Clamp01(tStart);
        _tTarget = Mathf.Clamp01(stop.T);

        // 2) verso più corto
        if (closedSpline)
        {
            float forward = Mathf.Repeat(_tTarget - _tCurrent, 1f);
            float backward = Mathf.Repeat(_tCurrent - _tTarget, 1f);
            _dir = (forward <= backward) ? +1 : -1;
            _tripRemainingStartT = Mathf.Max((_dir > 0) ? forward : backward, 1e-5f);
        }
        else
        {
            _dir = (_tTarget >= _tCurrent) ? +1 : -1;
            _tripRemainingStartT = Mathf.Max(Mathf.Abs(_tTarget - _tCurrent), 1e-5f);
        }

        // 3) imposta stessa spline su entrambe e sync iniziale
        _extFwd.Spline = spline;
        _extBwd.Spline = spline;
        SetCameraPositionBoth(_tCurrent);
        SyncPlayerAndPivotToDolly();             // allinea subito Player&Pivot al t di partenza

        // 4) attiva il rig giusto, disattiva input se richiesto
        ActivateRig(_dir > 0);
        SetInputsEnabled(false);

        _isTravelling = true;
    }

    void Update()
    {
        if (!_isTravelling) return;

        // distanza residua lungo il verso scelto (in t normalizzato)
        float remaining = closedSpline
            ? (_dir > 0 ? Mathf.Repeat(_tTarget - _tCurrent, 1f)
                        : Mathf.Repeat(_tCurrent - _tTarget, 1f))
            : Mathf.Abs(_tTarget - _tCurrent);

        // passo costante in t (senza ease)
        float maxStep = travelSpeedT * Time.deltaTime;

        // overshoot-safe: se supereremmo il target, snappa e chiudi
        if (remaining <= maxStep + stopEpsilonT)
        {
            _tCurrent = _tTarget;
            SetCameraPositionBoth(_tCurrent);
            SyncPlayerAndPivotToDolly();         // ultima sync (sono già allineati)
            FinishAndHandoffToMove();
            return;
        }

        // altrimenti avanza con segno
        float stepSigned = (_dir > 0 ? +maxStep : -maxStep);
        _tCurrent = closedSpline
            ? Mathf.Repeat(_tCurrent + stepSigned, 1f)
            : Mathf.Clamp01(_tCurrent + stepSigned);

        // muovi le Dolly e sincronizza Player&Pivot ad ogni frame → nessuno scarto allo switch
        SetCameraPositionBoth(_tCurrent);
        SyncPlayerAndPivotToDolly();
    }

    // -------- Helpers --------

    /// Scrive CameraPosition = t su entrambe le Dolly (sempre).
    private void SetCameraPositionBoth(float t)
    {
        if (_extFwd) _extFwd.CameraPosition = t;
        if (_extBwd) _extBwd.CameraPosition = t;
    }

    /// Attiva la Dolly forward o backward via Priority; Move più bassa durante il travel.
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

    /// Dis/abilita i tuoi script di input durante il travel (facoltativo ma consigliato).
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
        if (!activeDolly || !playerRootForYaw) return;

        Transform d = activeDolly.transform;

        // POSIZIONE al Player root
        playerRootForYaw.position = d.position;

        // YAW al Player root (solo direzione orizzontale)
        Vector3 fwdOnPlane = Vector3.ProjectOnPlane(d.forward, Vector3.up);
        if (fwdOnPlane.sqrMagnitude > 1e-6f)
        {
            Quaternion yawOnly = Quaternion.LookRotation(fwdOnPlane.normalized, Vector3.up);
            playerRootForYaw.rotation = yawOnly;
        }

        // PITCH al pivot
        float pitchDeg = Mathf.Asin(Mathf.Clamp(d.forward.y, -1f, 1f)) * Mathf.Rad2Deg;
        if (viewActions != null)
        {
            viewActions.SetPitchImmediate(pitchDeg); // usa il tuo metodo (clamping integrato)
        }
        else if (cameraPivotFallback != null)
        {
            // fallback: imponi localEuler X, azzera yaw/roll locali
            var e = cameraPivotFallback.localEulerAngles;
            // converti in [-180,180] e imposta X = pitchDeg
            float normX = Mathf.DeltaAngle(0f, e.x);
            normX = pitchDeg;
            cameraPivotFallback.localEulerAngles = new Vector3(normX, 0f, 0f);
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