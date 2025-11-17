using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// Esegue la rotazione orizzontale (yaw) del Player in modo semplice e robusto.

public class RotateActions : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Oggetto da ruotare in orizzontale. Di solito il Player (root con CharacterController).")]
    public Transform targetToRotate;

    [Header("Rotazione")]
    [Tooltip("Velocità di rotazione orizzontale (gradi/secondo).")]
    public float rotationSpeed = 180f;

    // stato dei due pulsanti da aggiornare
    private bool _leftHeld;
    private bool _rightHeld;

    public void SetRotation(int direction, bool active)
    {
        if (direction > 0) _rightHeld = active;
        else if (direction < 0) _leftHeld = active;
    }

    private void Update()
    {

        if (!targetToRotate) return;

        // Direzione risultante: -1 = sinistra, +1 = dest   ra, 0 = fermo
        float dir = 0f;
        if (_leftHeld) dir -= 1f;
        if (_rightHeld) dir += 1f;

        //calcola e applica la rotazione locale al gameobject targetToRotate
        float yawDelta = dir * rotationSpeed * Time.deltaTime;
        targetToRotate.Rotate(0f, yawDelta, 0f, Space.Self);
    }
}
