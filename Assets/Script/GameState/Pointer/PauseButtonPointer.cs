using UnityEngine;
using UnityEngine.EventSystems;

public class PauseUIPointer : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] private GameObject resumeButton; // Riferimento al bottone "Resume"
    [SerializeField] private GameObject pauseButton;  // Riferimento al bottone "Pause" (di solito questo stesso oggetto)

    public void OnPointerUp(PointerEventData e)
    {
        Debug.Log("OnPointerDown called. Pausing the game.");

        // Blocca il gioco passando dal GameManager se disponibile
        if (GameManager.Instance != null)
            GameManager.Instance.SetPause(true);

            /*
        else
            Time.timeScale = 0f;
            */

        // Attiva il bottone "Resume"
        if (resumeButton != null)
        {
            
            
            resumeButton.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Resume button non assegnato nell'Inspector.");
        }

        // Nasconde il bottone "Pause" mentre il gioco è fermo
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Pause button non assegnato nell'Inspector.");
        }
    }
}
