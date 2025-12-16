using UnityEngine;


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

        // 2) Calcola il vettore di movimento orizzontale e avvia il movimento
        Vector3 move = transform.forward * (axis * moveSpeed);
        _cc.Move(move * Time.deltaTime);
    }
}
