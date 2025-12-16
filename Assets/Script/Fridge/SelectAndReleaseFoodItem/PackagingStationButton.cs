using UnityEngine;
using UnityEngine.EventSystems;

public class PackagingStationButton : MonoBehaviour, IPointerDownHandler
{

    [Header("Punteggio HACCP")]
    [SerializeField] private int rewardIfPackagedCorrectly = 5;
    [SerializeField] private int penaltyIfNotPackagable = 5;

    [Header("Animazione Package")]
    [SerializeField] private Animator packageAnimator;
    [SerializeField] private string playTriggerName = "PlayPackage";

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    public static System.Action<FoodItem, bool> OnPackageAttempt; // item, success


    public void OnPointerDown(PointerEventData eventData)
    {
        // Solo se è selezionato (quindi davanti alla camera)
        var selected = Selectable.CurrentSelected;
        if (selected == null)
            return;

        var food = selected.GetComponent<FoodItem>();
        if (food == null)
            return;

        // Non confezionabile → penalità (anti “a caso”)
        if (!food.isUnpackaged)
        {
            if (HaccpScoreState.Instance != null)
                HaccpScoreState.Instance.AddScore(-penaltyIfNotPackagable);
            return;
        }

        // Confezionamento corretto: aggiorna stato + visual (già gestito da FoodItem)
        food.SetPackaged(true);

        if (packageAnimator != null && !string.IsNullOrEmpty(playTriggerName))
        {
            if (logDebug) Debug.Log("[PackagingStationButton] Riproduco animazione package.");
            packageAnimator.SetTrigger(playTriggerName);
        }

        food.UpdateIndicators();

        // aggiorna pannello preview
        if (SelectedFoodPanelController.Instance != null)
            SelectedFoodPanelController.Instance.Refresh();

        // prova a finalizzare (se era già nel ripiano giusto, ora può diventare HACCP OK)
        if (Fridge1State.Instance != null)
            Fridge1State.Instance.TryFinalizeItem(food);



        if (HaccpScoreState.Instance != null)
            HaccpScoreState.Instance.AddScore(+rewardIfPackagedCorrectly);

        // Aggiorna subito la preview del pannello (se è aperto)
        if (SelectedFoodPanelController.Instance != null)
            SelectedFoodPanelController.Instance.Refresh();
    }
}
