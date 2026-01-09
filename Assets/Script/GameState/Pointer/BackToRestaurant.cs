using UnityEngine;
using UnityEngine.EventSystems;

public class BackToRestaurant : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    
    public void OnPointerDown(PointerEventData eventData)
    {
        

        Debug.Log(">>> CLICK RILEVATO SULLA FRECCIA! <<<");

        if (GameManager.Instance != null)
        {
            Debug.Log("GameManager trovato, cambio scena in corso...");
            GameManager.Instance.ExitMinigame(); // Assicurati di aver aggiunto questa funzione nel GameManager
        }
        else
        {
            Debug.LogError("ERRORE: Non trovo il GameManager in questa scena!");
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("Caricamento scena ristorante...");
            GameManager.Instance.ExitMinigame();
        }

        else
        {
            Debug.LogWarning("GameManager.Instance è null. Impossibile tornare al ristorante.");
        }
    }
}
