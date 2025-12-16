using UnityEngine;
using UnityEngine.EventSystems;

public class TrashBinButton : MonoBehaviour, IPointerDownHandler
{
    [Header("Punteggio HACCP")]
    [SerializeField] private int gainIfExpired = 10;
    [SerializeField] private int loseIfNotExpired = 10;

    [Header("Animazione Cestino")]
    [SerializeField] private Animator binAnimator;
    [SerializeField] private string playTriggerName = "PlayBin";

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    public static System.Action<FoodItem, bool> OnTrashAttempt; // item, wasExpired


    public void OnPointerDown(PointerEventData eventData)
    {
        if (logDebug) Debug.Log("[TrashBinButton] CLICK");

        var selected = Selectable.CurrentSelected;
        if (selected == null)
        {
            if (logDebug) Debug.LogWarning("[TrashBinButton] Nessun oggetto selezionato.");
            return;
        }

        var food = selected.GetComponent<FoodItem>();

        OnTrashAttempt?.Invoke(food, food.isExpired);

        if (food == null)
        {
            if (logDebug) Debug.LogWarning("[TrashBinButton] Oggetto selezionato senza FoodItem.");
            return;
        }

        // Se già buttato, non fare nulla
        if (food.isDiscarded)
        {
            if (logDebug) Debug.Log("[TrashBinButton] Item già buttato.");
            return;
        }

        // CASO 2: SBAGLIATO BUTTARLO (non expired) → resta selected, ma perdi punti
        if (!food.isExpired)
        {
            int deltaWrong = -loseIfNotExpired;
            HaccpScoreState.Instance?.AddScore(deltaWrong);

            if (logDebug) Debug.Log($"[TrashBinButton] SBAGLIATO: '{food.name}' non è expired → delta {deltaWrong}. Resta selezionato.");

            // niente MarkDiscarded, niente animazione: è un errore, resta davanti alla camera
            return;
        }

        // CASO 1: GIUSTO BUTTARLO (expired) → guadagni, lo marchi e lo fai sparire
        int deltaOk = gainIfExpired;
        HaccpScoreState.Instance?.AddScore(deltaOk);

        // Trigger animazione (solo caso valido)
        if (binAnimator != null && !string.IsNullOrEmpty(playTriggerName))
        {
            if (logDebug) Debug.Log("[TrashBinButton] Riproduco animazione cestino.");
            binAnimator.SetTrigger(playTriggerName);
        }
        else if (logDebug)
            Debug.LogWarning("[TrashBinButton] binAnimator non assegnato o triggerName vuoto: animazione non riprodotta.");

        food.MarkDiscarded();
        food.UpdateIndicators();

        if (SelectedFoodPanelController.Instance != null)
            SelectedFoodPanelController.Instance.Refresh();

        if (logDebug) Debug.Log($"[TrashBinButton] OK: '{food.name}' expired → delta {deltaOk}. Lo rimuovo.");

        // sparisce, non cade
        selected.ConsumeToTrash();
    }
}
