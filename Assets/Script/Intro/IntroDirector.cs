using UnityEngine;

public class IntroDirector : MonoBehaviour
{
    public Animator animatorCam2;
    public string stateName = "Move"; // nome ESATTO dello state

    public void StartAnim()
    {
        animatorCam2.enabled = true;                 // riaccende l'Animator
        animatorCam2.Play(stateName, 0, 0f);          // parte da 0
        animatorCam2.Update(0f);                      // applica subito
    }
}
