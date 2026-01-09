using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayButtonPointer : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] private int fallbackSceneIndex = 1; // Se manca il GameManager, carica questa scena

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Play button pressed.");

        // Avvia il gioco passando dal GameManager, che gestisce stato/coin/progressi
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }
        else
        {
            // Fallback: carica direttamente la scena indicata
            SceneManager.LoadScene(fallbackSceneIndex);
        }
    }
}
