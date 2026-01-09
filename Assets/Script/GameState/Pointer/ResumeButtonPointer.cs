using UnityEngine;
using UnityEngine.EventSystems;

public class ResumeUIPointer : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] private GameObject pauseButton;  // Bottone "Pause" da riattivare
    [SerializeField] private GameObject resumeButton; // Bottone "Resume" da nascondere (di solito questo stesso oggetto)

    public void OnPointerUp(PointerEventData e)
    {
        Debug.Log("OnPointerDown called. Resuming the game.");

        // Sblocca il gioco passando dal GameManager se disponibile
        if (GameManager.Instance != null)
            GameManager.Instance.SetPause(false);
        
        /*
        else
            Time.timeScale = 1f;
            */

        // Riattiva il bottone "Pause"
        if (pauseButton != null)
            pauseButton.SetActive(true);
        else
            Debug.LogWarning("Pause button non assegnato nell'Inspector.");

        // Nasconde il bottone "Resume"
        if (resumeButton != null)
            resumeButton.SetActive(false);
        else
            Debug.LogWarning("Resume button non assegnato nell'Inspector.");
    }
}
