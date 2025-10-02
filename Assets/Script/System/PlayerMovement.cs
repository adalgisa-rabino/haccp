using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    // ====== PARAMETRI DI MOVIMENTO ======
    [Header("Movement")]
    [Tooltip("Velocità avanti/indietro (m/s).")]
    public float moveSpeed = 3.5f;

    [Tooltip("Velocità di rotazione orizzontale (gradi/sec).")]
    public float turnSpeedDeg = 180f;

    [Tooltip("Accelerazione di gravità (valore negativo).")]
    public float gravity = -9.81f;

    [Tooltip("Leggera spinta verso il basso per restare ancorati al suolo quando grounded.")]
    public float groundStick = -2f;

    // ====== CAMERA / VISTA ======
    [Header("Camera / View")]
    [Tooltip("Empty padre della Main Camera: ruota in X (pitch) e definisce l'altezza occhi.")]
    public Transform cameraPivot;      // Assegna l’Empty genitore della camera

    [Tooltip("Altezza degli occhi rispetto al player (Y locale del cameraPivot).")]
    public float eyeHeight = 1.7f;

    // ====== RIFERIMENTI COMPONENTI ======
    private CharacterController cc;    // Controller fisico del player
    private PlayerInput pi;            // Input System (nuovo)

    // ====== INPUT ACTIONS (nuovo Input System) ======
    // Richiede che nell'Asset di Input esistano:
    // - Action "Move" di tipo Vector2 (W/S sul Y, A/D sul X)
    // - Action "Interact" di tipo Button
    private InputAction moveAction;
    private InputAction interactAction;

    // ====== STATO INTERNO ======
    private Vector2 moveInput;         // input WASD/analog stick (x=rotazione, y=avanti/indietro)
    private float verticalVel;         // velocità verticale accumulata (gravità)
    private bool skipOneFrame = false; // salta 1 frame dopo il teletrasporto per evitare scatti CC

    private void Awake()
    {
        // Cache componenti obbligatori (garantiti da RequireComponent)
        cc = GetComponent<CharacterController>();
        pi = GetComponent<PlayerInput>();

        // Prendi le actions dall'asset del PlayerInput
        // (i nomi devono combaciare nell'InputActionAsset)
        moveAction = pi.actions["Move"];
        interactAction = pi.actions["Interact"];
    }

    private void OnEnable()
    {
        // Abilita le actions quando l'oggetto si attiva
        moveAction?.Enable();
        interactAction?.Enable();
    }

    private void OnDisable()
    {
        // Disabilita le actions quando l'oggetto si disattiva
        moveAction?.Disable();
        interactAction?.Disable();
    }

    private void Update()
    {
        // Dopo un teletrasporto saltiamo un frame di movimento:
        // evita che il CC reagisca con contatti/pendenze e “trascini” il player.
        if (skipOneFrame)
        {
            skipOneFrame = false;
            return;
        }

        // Leggi input (x = rotazione, y = avanti/indietro)
        moveInput = moveAction.ReadValue<Vector2>();

        // ROTAZIONE YAW (A/D o stick orizzontale)
        float yawDelta = moveInput.x * turnSpeedDeg * Time.deltaTime;
        transform.Rotate(0f, yawDelta, 0f);

        // GRAVITÀ: quando a terra, tieni il player “incollato” con groundStick
        if (cc.isGrounded && verticalVel < 0f)
            verticalVel = groundStick;
        // Integra la gravità
        verticalVel += gravity * Time.deltaTime;

        // TRASLAZIONE: avanti/indietro lungo la forward del player
        Vector3 move = transform.forward * (moveInput.y * moveSpeed);
        move.y = verticalVel;

        // Muovi con CharacterController (gestisce collisioni/pendenze)
        cc.Move(move * Time.deltaTime);

        // INPUT DI INTERAZIONE (placeholder per tua logica)
        if (interactAction.WasPerformedThisFrame())
        {
            Debug.Log("Interact premuto!");
            // TODO: logica HACCP / interazioni
        }
    }

    /// <summary>
    /// Teletrasporta il player:
    /// - usa targetBody per posizione e yaw (rotazione Y),
    /// - applica il pitch X della camera da targetView (se presente),
    /// - riallinea l’altezza occhi, azzera caduta e salta 1 frame.
    /// </summary>
    /// <param name="targetBody">Transform di destinazione (pos + yaw)</param>
    /// <param name="targetView">Transform che contiene il pitch X desiderato (opzionale)</param>
    public void ApplyPose(Transform targetBody, Transform targetView)
    {
        if (targetBody == null)
        {
            Debug.LogWarning("[PlayerMovement] targetBody nullo: teletrasporto annullato.");
            return;
        }

        // Disattiva il CC per impostare liberamente posizione/rotazione
        bool hadCC = cc != null && cc.enabled;
        if (hadCC) cc.enabled = false;

        // Imposta posizione e YAW (ignora roll/pitch del corpo destinazione)
        transform.SetPositionAndRotation(
            targetBody.position,
            Quaternion.Euler(0f, targetBody.eulerAngles.y, 0f)
        );

        // Applica PITCH e altezza della vista se abbiamo un cameraPivot
        if (cameraPivot != null)
        {
            // Se targetView esiste, usa il suo angolo X (pitch); altrimenti lascia il pitch attuale
            float pitchX = targetView ? targetView.eulerAngles.x : cameraPivot.localEulerAngles.x;

            // Applica pitch e riallinea altezza occhi
            cameraPivot.localEulerAngles = new Vector3(pitchX, 0f, 0f);
            cameraPivot.localPosition = new Vector3(0f, eyeHeight, 0f);
        }

        // Riattiva il CC
        if (hadCC) cc.enabled = true;

        // Resetta la velocità verticale (niente “caduta” residua)
        verticalVel = 0f;

        // Salta un frame di movimento per evitare scatti post-teletrasporto
        skipOneFrame = true;

        Debug.Log($"[PlayerMovement] ApplyPose → pos {transform.position}, yaw {transform.eulerAngles.y}");
    }
}
