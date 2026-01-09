using UnityEngine;
using UnityEngine.EventSystems;

public class KitchenPointer : MonoBehaviour, IPointerDownHandler
{
    public enum Target
    {
        Frigo,
        Lavandino
    }

    [SerializeField] private Target target;
    [SerializeField] private GameManager gameManager;

    private void Start()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // --- AGGIUNGI QUESTO CONTROLLO ---
        // Se il puntatore è sopra un elemento della UI (come la tua Board), interrompi l'esecuzione.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Click bloccato: il cursore è sopra la UI.");
            return;
        }
        // ---------------------------------

        if (gameManager == null) gameManager = GameManager.Instance;

        if (target == Target.Frigo)
        {
            Debug.Log("Pointer DOWN sul frigo!");
            gameManager.EnterFrigoMinigame();
        }
        else if (target == Target.Lavandino)
        {
            Debug.Log("Pointer DOWN sul lavandino!");
            gameManager.EnterLavandinoMinigame();
        }
    }
}