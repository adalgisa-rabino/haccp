using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Usa UNA sola Virtual Camera (singleVcam) per due modalità:
/// - GUI mode: la camera è in modalità "move" / interazione con la GUI (Transposer/Follow attivi)
/// - Dolly mode: la stessa camera usa CinemachineSplineDolly per muoversi sulla spline
///
/// Alla fine del viaggio (solo un verso supportato) la camera passa dalla modalità Dolly alla modalità Move
/// senza cambiare GameObject della Virtual Camera: si disabilita il componente Dolly e si abilita il Follow/HardLock ecc.
/// </summary>
public class SingleDollyTravelController : MonoBehaviour
{
    [Header("Spline & Virtual Camera (single)")]
    public SplineContainer spline;
    public CinemachineVirtualCameraBase singleVcam;        // la vcam unica che contiene anche il Dolly component
    public Transform cameraPivot;                          // tracking target (CameraPivot) usato dalla Move mode

    [Header("Player rig")]
    public Transform playerRootForYaw;                     // root usata per yaw/posizione (come prima)
    public ViewActions viewActions;
    public Transform cameraPivotFallback;

    [Header("Travel settings")]
    public float travelSpeedT = 0.35f;                     // t / s (normalizzato)
    public bool closedSpline = true;
    public float stopEpsilonT = 0.005f;

    [Header("Optional: disable input during travel")]
    public MonoBehaviour[] disableDuringTravel;

    [Header("Components on the same vcam (assign in inspector)")]
    public Behaviour hardLockToTarget;                     // componente "Hard Lock to Target" (assegnalo dall'Inspector)
    public Behaviour rotateWithFollowTarget;               // componente "Rotate With Follow Target" (assegnalo dall'Inspector)

    // stato interno
    private Component _splineDollyComp;
    private CinemachineTransposer _transposer;
    private CinemachineComposer _composer;
    private bool _isTravelling;
    private float _tCurrent;
    private float _tTarget;
    private bool _handoffInProgress;

    void Awake()
    {
        if (singleVcam != null)
        {
            // otteniamo componenti in modo compatibile senza usare API deprecate
            _splineDollyComp = singleVcam.GetComponent(typeof(CinemachineSplineDolly));

            // prendi il componente del Body stage e castalo a Transposer
            var bodyComp = singleVcam.GetCinemachineComponent(CinemachineCore.Stage.Body);
            _transposer = bodyComp as CinemachineTransposer;

            // prendi il componente dell'Aim stage e castalo a Composer
            var aimComp = singleVcam.GetCinemachineComponent(CinemachineCore.Stage.Aim);
            _composer = aimComp as CinemachineComposer;
        }

        if (spline && spline.Splines != null && spline.Splines.Count > 0)
            closedSpline = spline.Splines[0].Closed;
    }

    // ---------- Mode control API ----------

    // Entra in modalità GUI (move): disabilita il Dolly, abilita Follow/Transposer/HardLock (se presenti)
    public void EnterGuiMode()
    {
        if (singleVcam == null) return;

        // Disattiva la procedura Dolly e abilita i componenti Move
        ApplyMoveProcedural();

        // follow target che useremo in modalità GUI
        var followTarget = cameraPivot != null ? cameraPivot : playerRootForYaw;

        // calcola e applica l'offset del Transposer in modo che la vcam mantenga la stessa posizione world
        var cam = Camera.main;
        if (cam != null && followTarget != null && _transposer != null)
        {
            Vector3 worldOffset = cam.transform.position - followTarget.position;
            Vector3 localOffset = Quaternion.Inverse(followTarget.rotation) * worldOffset;
            _transposer.m_FollowOffset = localOffset;
        }

        // assegna il follow target
        singleVcam.Follow = followTarget;

        // abilita i Behaviour di Move (se presenti)
        if (hardLockToTarget != null) hardLockToTarget.enabled = true;
        if (rotateWithFollowTarget != null) rotateWithFollowTarget.enabled = true;
    }

    // Entra in modalità Dolly (disabilita Follow/HardLock e abilita il Dolly)
    // startT: posizione t normalizzata iniziale sulla spline
    public void EnterDollyMode(float startT = 0f)
    {
        if (singleVcam == null || spline == null) return;
        if (_splineDollyComp == null) _splineDollyComp = singleVcam.GetComponent(typeof(CinemachineSplineDolly));
        if (_splineDollyComp == null) return;

        // Disattiva i Behaviour di Move per evitare feedback e setta la vcam a usare la procedura Dolly
        if (hardLockToTarget != null) hardLockToTarget.enabled = false;
        if (rotateWithFollowTarget != null) rotateWithFollowTarget.enabled = false;

        // disable follow target to let spline drive the camera
        singleVcam.Follow = null;

        // abilita la procedura Dolly e disabilita Transposer/Composer
        ApplyDollyProcedural();

        // assegna spline e posizione iniziale (usiamo reflection-safe access)
        var sd = _splineDollyComp as CinemachineSplineDolly;
        if (sd != null)
        {
            sd.Spline = spline;
            sd.CameraPosition = Mathf.Clamp01(startT);
            sd.enabled = true;
        }

        _tCurrent = Mathf.Clamp01(startT);
        _isTravelling = false;
        _handoffInProgress = false;
    }

    // Inizia il travel dolly verso tTarget (solo un verso)
    public void BeginTravelTo(float tTarget)
    {
        if (singleVcam == null) return;

        EnterDollyMode(_tCurrent);

        _tTarget = Mathf.Clamp01(tTarget);
        _isTravelling = true;

        // disabilita input se necessario
        SetInputsEnabled(false);
    }

    // Overload compatibile con TeleportStop (se presente nel progetto)
    public void BeginTravelTo(object stop)
    {
        var tProp = stop?.GetType().GetProperty("T");
        if (tProp != null)
        {
            var val = tProp.GetValue(stop);
            if (val is float f) BeginTravelTo(f);
        }
    }

    void Update()
    {
        if (!_isTravelling) return;

        float remaining;
        if (closedSpline)
        {
            // differenza in [0..1)
            remaining = Mathf.Repeat(_tTarget - _tCurrent, 1f);
        }
        else
        {
            remaining = Mathf.Abs(_tTarget - _tCurrent);
        }

        float maxStep = travelSpeedT * Time.deltaTime;

        if (remaining <= maxStep + stopEpsilonT)
        {
            _tCurrent = _tTarget;
            var sd = _splineDollyComp as CinemachineSplineDolly;
            if (sd != null) sd.CameraPosition = _tCurrent;

            if (!_handoffInProgress)
            {
                _handoffInProgress = true;
                StartCoroutine(FinishAndSwitchToMoveCoroutine());
            }
            return;
        }

        float step = maxStep;
        // avanzamento positivo (un verso)
        _tCurrent = closedSpline ? Mathf.Repeat(_tCurrent + step, 1f) : Mathf.Clamp01(_tCurrent + step);

        var sd2 = _splineDollyComp as CinemachineSplineDolly;
        if (sd2 != null) sd2.CameraPosition = _tCurrent;
    }

    // Coroutine finale: aspetta EndOfFrame, poi applica una sola volta il Follow + followOffset al Transposer
    private IEnumerator FinishAndSwitchToMoveCoroutine()
    {
        // aspetta che Cinemachine Brain abbia applicato lo stato della Dolly
        yield return new WaitForEndOfFrame();

        var cam = Camera.main;

        // calcola la posizione/orientamento world reale della camera
        Vector3 camPos = cam ? cam.transform.position : Vector3.zero;
        Vector3 camFwd = cam ? cam.transform.forward : Vector3.forward;

        // imposta playerRootForYaw / cameraPivot in base allo stato attuale della camera (una sola volta)
        if (playerRootForYaw != null)
        {
            playerRootForYaw.position = camPos;

            Vector3 fwdOnPlane = Vector3.ProjectOnPlane(camFwd, Vector3.up);
            if (fwdOnPlane.sqrMagnitude > 1e-6f)
                playerRootForYaw.rotation = Quaternion.LookRotation(fwdOnPlane.normalized, Vector3.up);
        }

        float pitchDeg = Mathf.Asin(Mathf.Clamp(camFwd.y, -1f, 1f)) * Mathf.Rad2Deg;
        if (viewActions != null)
            viewActions.SetPitchImmediate(pitchDeg);
        else if (cameraPivotFallback != null)
            cameraPivotFallback.localEulerAngles = new Vector3(pitchDeg, 0f, 0f);

        // disabilita spline dolly
        if (_splineDollyComp != null) ((Behaviour)_splineDollyComp).enabled = false;

        // assegna follow target
        var followTarget = cameraPivot != null ? cameraPivot : playerRootForYaw;
        if (singleVcam != null) singleVcam.Follow = followTarget;

        // imposta i componenti procedurali corretti per la Move mode
        ApplyMoveProcedural();

        // se la vcam ha Transposer, calcola e scrivi l'offset locale in modo che la camera rimanga nella stessa world pos
        if (_transposer != null && followTarget != null)
        {
            Vector3 worldOffset = camPos - followTarget.position;
            Vector3 localOffset = Quaternion.Inverse(followTarget.rotation) * worldOffset;
            _transposer.m_FollowOffset = localOffset;
        }

        // abilita componenti Move (hard lock / rotate) per il comportamento desiderato
        if (hardLockToTarget != null) hardLockToTarget.enabled = true;
        if (rotateWithFollowTarget != null) rotateWithFollowTarget.enabled = true;

        // riabilita input
        SetInputsEnabled(true);

        _isTravelling = false;
        _handoffInProgress = false;
    }

    // Applica la configurazione procedurale per usare la Dolly (Position + Rotation dalla spline)
    private void ApplyDollyProcedural()
    {
        // abilita spline dolly
        if (_splineDollyComp != null) ((Behaviour)_splineDollyComp).enabled = true;

        // disabilita Transposer (Body) e Composer (Aim) se presenti
        var bodyComp = singleVcam.GetCinemachineComponent(CinemachineCore.Stage.Body);
        if (bodyComp is Behaviour bBody) bBody.enabled = false;
        var aimComp = singleVcam.GetCinemachineComponent(CinemachineCore.Stage.Aim);
        if (aimComp is Behaviour bAim) bAim.enabled = false;
    }

    // Applica la configurazione procedurale per la Move mode (Transposer + Composer)
    private void ApplyMoveProcedural()
    {
        // disabilita spline dolly
        if (_splineDollyComp != null) ((Behaviour)_splineDollyComp).enabled = false;

        // abilita Transposer (Body) e Composer (Aim) se presenti
        var bodyComp = singleVcam.GetCinemachineComponent(CinemachineCore.Stage.Body);
        if (bodyComp is Behaviour bBody) bBody.enabled = true;
        var aimComp = singleVcam.GetCinemachineComponent(CinemachineCore.Stage.Aim);
        if (aimComp is Behaviour bAim) bAim.enabled = true;
    }

    private void SetInputsEnabled(bool enabled)
    {
        if (disableDuringTravel == null) return;
        foreach (var mb in disableDuringTravel)
            if (mb) mb.enabled = enabled;
    }
}