using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class FridgeCheckButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Fridge1State fridgeState;
    [SerializeField] private TMP_Text feedbackText; // testo UI dove mostrare il risultato

    public void OnPointerDown(PointerEventData eventData)
    {
        if (fridgeState == null)
            fridgeState = Fridge1State.Instance;

        if (fridgeState == null)
            return;

        string result = fridgeState.CheckChallengeAndApplyScore();

        if (feedbackText != null)
            feedbackText.text = result;
        else
            Debug.Log(result);
    }
}
