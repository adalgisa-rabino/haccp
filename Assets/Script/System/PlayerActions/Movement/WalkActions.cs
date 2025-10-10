using UnityEngine;

/// <summary>
/// WalkActions
/// ---------------------------
/// Componente da mettere sul Player.
/// Gestisce lo spostamento avanti/indietro e la gravità in modo semplice,
/// usando un CharacterController.
/// I pulsanti UI comunicano con questo script tramite <see cref="UIWalkController"/>.
///
/// Funzionamento:
/// - Se "avanti" è premuto → Player si muove nella direzione forward.
/// - Se "indietro" è premuto → Player si muove nella direzione opposta.
/// - Puoi premere più pulsanti in contemporanea (il risultato si somma).
/// - Gestisce la gravità in modo basilare.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class WalkActions : MonoBehaviour
{
    [Header("Movimento")]
    [Tooltip("Velocità di camminata in metri al secondo.")]
    public float moveSpeed = 3.5f;

    [Header("Gravità")]
    [Tooltip("Applica gravità per tenere il player a terra.")]
    public bool useGravity = true;

    [Tooltip("Accelerazione di gravità (valore negativo).")]
    public float gravity = -9.81f;

    [Tooltip("Spinta verso il basso per rimanere incollati al terreno.")]
    public float groundStick = -2f;

    // Stati impostati dai pulsanti
    private bool forwardHeld;
    private bool backwardHeld;

    // Riferimento al CharacterController
    private CharacterController _cc;

    // Velocità verticale accumulata (per la gravità)
    private float _verticalVel;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();

        if (_cc == null)
            Debug.LogError("[WalkActions] Nessun CharacterController trovato sul Player.");
    }

    // Questi metodi vengono chiamati dai pulsanti UI
    public void SetMoveForward(bool active) => forwardHeld = active;
    public void SetMoveBackward(bool active) => backwardHeld = active;

    private void Update()
    {
        if (_cc == null) return;

        // 1) Determina la direzione di movimento dai pulsanti
        float axis = 0f;
        if (forwardHeld) axis += 1f;
        if (backwardHeld) axis -= 1f;

        // 2) Calcola il vettore di movimento orizzontale
        Vector3 move = transform.forward * (axis * moveSpeed);

        // 3) Gestione gravità opzionale
        if (useGravity)
        {
            // Se il Player è a terra e sta cadendo, resetta la velocità verticale
            if (_cc.isGrounded && _verticalVel < 0f)
                _verticalVel = groundStick;

            _verticalVel += gravity * Time.deltaTime;
        }
        else
        {
            _verticalVel = 0f;
        }

        // 4) Aggiungi la componente verticale e applica movimento
        move.y = _verticalVel;
        _cc.Move(move * Time.deltaTime);
    }
}
