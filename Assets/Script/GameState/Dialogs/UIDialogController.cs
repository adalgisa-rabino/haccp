using UnityEngine;
using UnityEngine.EventSystems;

public class UIDialogController :  MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public DialogManager DialogManager;
    private bool holding;
    public void OnPointerDown(PointerEventData e)
    {
        Debug.Log($"OnPointerDown called.");
        holding = true;
        Apply(true);
    }

    public void OnPointerUp(PointerEventData e)
    {
        holding = false;
        Apply(false);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (holding)
        {
            holding = false;
            Apply(false);
        }
    }

    // Invia il comando 
    private void Apply(bool active)
    {
        if (active)
        {
            Debug.Log("gioco iniziato");
            GameManager.Instance.ContinueAfterIntroDialog();
        }
        
    }
}
