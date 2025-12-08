using UnityEngine;
using UnityEngine.EventSystems;

public class TrashBinButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // Deve esserci un oggetto selezionato (quindi davanti al frigo + preview attiva)
        var selected = Selectable.CurrentSelected;
        if (selected == null)
            return;

        // Recupero il FoodItem associato
        var food = selected.GetComponent<FoodItem>();
        if (food == null)
            return;

        // Può essere buttato solo se è scaduto (o se vuoi: se isExpired == true)
        if (!food.isExpired)
            return;

        // Lo marchiamo come buttato per la logica di vittoria
        food.MarkDiscarded();

        // Effetto fisico già pronto nella Selectable
        selected.ThrowToTrash();

        // Volendo qui potresti anche togliere punti o aggiungerne via HaccpScoreState
        // HaccpScoreState.Instance?.AddScore(...)
    }
}
