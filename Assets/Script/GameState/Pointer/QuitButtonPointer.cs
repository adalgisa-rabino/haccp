using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class QuitButtonPointer : MonoBehaviour, IPointerUpHandler
{
   

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Quit button pressed.");

        // Avvia il gioco passando dal GameManager, che gestisce stato/coin/progressi
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }

    }
}
