using UnityEngine;

using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    [Header("Componenti")]
    public CharacterController controller;
    public Transform playerCamera;

    float pitch = 0f; // rotazione verticale (su/giù)

    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = Camera.main.transform;

        // blocca il cursore al centro della finestra
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- movimento con WASD ---
        float x = Input.GetAxis("Horizontal"); // A/D
        float z = Input.GetAxis("Vertical");   // W/S

        Vector3 move = transform.right * x + transform.forward * z;
        controller.SimpleMove(move * moveSpeed);

        // --- rotazione con mouse ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // rotazione orizzontale del player
        transform.Rotate(Vector3.up * mouseX);

        // rotazione verticale della camera
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f); // limite su/giù
        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}

