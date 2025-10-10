using UnityEngine;

/// <summary>
/// RotateActions
/// ---------------------------
/// Esegue la rotazione orizzontale (yaw) del Player in modo semplice e robusto.
/// Riceve comandi da uno o più UIRotateController (StartRotation/StopRotation).
///
/// Obiettivo: ruotare attorno all'ASSE VERTICALE DEL PLAYER,
/// evitando che lo yaw "segua la vista" (Camera/CameraPivot).
///
/// Consigli di setup:
/// - Assegna come 'targetToRotate' il Player (root con CharacterController).
/// - NON assegnare la Camera né il CameraPivot.
/// - Se il pivot del modello non è centrato e noti "orbite strane",
///   attiva 'useRotateAround' per ruotare attorno alla posizione del root.
/// </summary>
public class RotateActions : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Oggetto da ruotare in orizzontale. Di solito il Player (root con CharacterController).")]
    public Transform targetToRotate;

    [Header("Rotazione")]
    [Tooltip("Velocità di rotazione orizzontale (gradi/secondo).")]
    public float rotationSpeed = 180f;

    [Header("Opzioni avanzate")]
    [Tooltip("Se attivo, ruota attorno alla POSIZIONE del root (utile se il pivot del modello non è centrato).")]
    public bool useRotateAround = false;

    [Tooltip("Root per l'asse di rotazione. Se vuoto, prova a usare il Transform con CharacterController.")]
    public Transform rotateAroundRoot;

    // --- Stato interno: gestiamo i due pulsanti come flag separati ---
    private bool _leftHeld;
    private bool _rightHeld;

    private void Awake()
    {
        // Se il target non è assegnato, prova ad auto-riconoscere il Player (con CharacterController)
        if (!targetToRotate)
        {
            var cc = GetComponent<CharacterController>();
            if (!cc) cc = GetComponentInParent<CharacterController>();
            targetToRotate = cc ? cc.transform : transform;
        }

        // Se non è definito il root per RotateAround, prova con il Player (CharacterController)
        if (!rotateAroundRoot)
        {
            var cc = GetComponent<CharacterController>();
            if (!cc) cc = GetComponentInParent<CharacterController>();
            rotateAroundRoot = cc ? cc.transform : targetToRotate;
        }

        if (!targetToRotate)
            Debug.LogWarning("[RotateActions] Nessun 'targetToRotate' valido. Assegna il Player (root).");
    }

    // ----------------------------------------------------------------
    // COMPATIBILITÀ con UIRotateController esistente:
    // UIRotateController chiama StartRotation(+1/-1) e StopRotation(+1/-1).
    // Qui traduciamo i valori int in due flag separati (left/right).
    // ----------------------------------------------------------------
    public void StartRotation(int direction)
    {
        if (direction > 0) _rightHeld = true;
        else if (direction < 0) _leftHeld = true;
        // se direction == 0, non facciamo nulla
    }

    public void StopRotation(int direction)
    {
        if (direction > 0) _rightHeld = false;
        else if (direction < 0) _leftHeld = false;
    }

    private void Update()
    {

        if (_leftHeld || _rightHeld)
            Debug.Log($"[RotateActions] rotating dir={(_rightHeld ? 1 : 0) - (_leftHeld ? 1 : 0)}");


        if (!targetToRotate) return;

        // Direzione risultante: -1 = sinistra, +1 = dest   ra, 0 = fermo
        float dir = 0f;
        if (_leftHeld) dir -= 1f;
        if (_rightHeld) dir += 1f;

        if (Mathf.Approximately(dir, 0f)) return;

        float yawDelta = dir * rotationSpeed * Time.deltaTime;

        if (useRotateAround && rotateAroundRoot)
        {
            // Ruota attorno alla POSIZIONE del root (asse Up mondo): utile se il pivot non è centrale
            targetToRotate.RotateAround(rotateAroundRoot.position, Vector3.up, yawDelta);
        }
        else
        {
            // Ruota attorno al pivot locale del target (asse Y locale)
            targetToRotate.Rotate(0f, yawDelta, 0f, Space.Self);
        }
    }
}
