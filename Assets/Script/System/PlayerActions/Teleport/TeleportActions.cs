using UnityEngine;

/// <summary>
/// TeleportActions
/// ---------------------------
/// Unica logica di teletrasporto del Player:
/// - POSIZIONE  = marker.position
/// - YAW        = marker.eulerAngles.y  (direzione Z+ dell’area)
/// - VIEW (pivot)= pitch fisso 0° (dritto), Y=0, Z=0, altezza occhi impostata
/// </summary>
[DisallowMultipleComponent]
public class TeleportActions : MonoBehaviour
{
    [Header("Camera / View")]
    [Tooltip("Genitore della Main Camera (ruota SOLO in X = pitch).")]
    public Transform cameraPivot;

    [Tooltip("Altezza occhi dal suolo (metri).")]
    public float eyeHeight = 1.7f;

    [Header("Sicurezza")]
    [Tooltip("Intervallo minimo tra 2 teletrasporti per evitare doppi trigger.")]
    public float minInterval = 0.1f;

    private CharacterController _cc;    // opzionale: se presente, lo disattiviamo durante il TP
    private float _nextAllowedTime;     // anti-spam

    private void Awake()
    {
        _cc = GetComponent<CharacterController>(); // ok anche se è null
    }

    /// <summary>
    /// Teletrasporta il player su 'marker' e allinea la vista alla Z+ dell’area con pitch 0°.
    /// </summary>
    public void ApplyPose(Transform marker, Transform _unused = null)
    {
        if (!marker) return;

        // Anti-spam: ignora chiamate troppo ravvicinate (es. OnClick duplicati)
        if (Time.time < _nextAllowedTime) return;
        _nextAllowedTime = Time.time + minInterval;

        // 1) Disabilita temporaneamente il CC per evitare "spinte" dovute al cambio posizione
        bool hadCC = _cc && _cc.enabled;
        if (hadCC) _cc.enabled = false;

        // 2) POSIZIONE + YAW (solo asse Y) dal marker
        transform.position = marker.position;
        transform.rotation = Quaternion.Euler(0f, marker.eulerAngles.y, 0f);

        // 3) CAMERA PIVOT: vista dritta (pitch 0°), senza contaminare Y/Z, altezza occhi
        if (cameraPivot)
        {
            cameraPivot.localRotation = Quaternion.Euler(0f, 0f, 0f);
            cameraPivot.localPosition = new Vector3(0f, eyeHeight, 0f);
        }

        // 4) Sincronizza stato interno di ViewActions (se presente)
        var view = GetComponent<ViewActions>();
        if (view)
        {
            view.SetPitchImmediate(0f);
        }

        // 5) Riabilita il CC se lo avevamo disabilitato
        if (hadCC) _cc.enabled = true;
    }
}
