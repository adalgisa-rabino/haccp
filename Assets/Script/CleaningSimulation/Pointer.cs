using UnityEngine;
using UnityEngine.EventSystems;

public class Pointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    
    private bool isAnimating = false;

    [SerializeField] private FoodWasteController _foodWasteController;
    [SerializeField] private GameFlowManager _gameFlowManager;
    public int tapTouchCount = 0;

    public void OnPointerDown(PointerEventData eventData)

    {

        if (gameObject.CompareTag("Waste"))
        {
            Debug.Log("Pointer DOWN sulla spazzatura!");
            gameObject.SetActive(false); //nascondo l'oggetto spazzatura toccato
            _foodWasteController.NotifyWasteRemoved();

        }

        else if (gameObject.name == "SM_Water_Control"
         && gameObject.CompareTag("Touchable"))
        {
            if (_gameFlowManager.GetCurrentState() == GameFlowManager.WashGameState.FaucetTouchToStart)
            {
                Debug.Log("Pointer DOWN sull'acqua!");
                GameFlowManager.Instance.OnFaucetTouched();
            }

            else if (_gameFlowManager.GetCurrentState() == GameFlowManager.WashGameState.WaterRunning)
            {
                Debug.Log("Pointer DOWN sull'acqua in WaterRunning!");
                GameFlowManager.Instance.OnFaucetTouched();
                
            }
        }



    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
        if (gameObject.name == "SM_Water_Control" && gameObject.CompareTag("Touchable"))
        {
            Debug.Log("Pointer UP sull'acqua!");
            //quando l'animazione finisce, nascondo il termometro e fermo l'animazione dell'acqua
            return;
        }
        
    }
}
