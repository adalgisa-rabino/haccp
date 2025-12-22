using UnityEngine;
using UnityEngine.EventSystems;

public class KitchenPointer : MonoBehaviour, IPointerUpHandler
{
    public enum Target
    {
        Frigo,
        Lavandino
    }

    [SerializeField] private Target target;
    [SerializeField] private GameManager gameManager;

    public void OnPointerUp(PointerEventData eventData)
    {
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
