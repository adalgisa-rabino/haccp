using UnityEngine;
using UnityEngine.EventSystems;

public class FridgeThermometerButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Fridge1State fridgeState;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("CLICK TERMOMETRO RICEVUTO");

        if (fridgeState != null)
            fridgeState.ToggleTemperatureFreeze();
        else
            Debug.LogError("FridgeState NON assegnato nel termometro!");
    }

}
