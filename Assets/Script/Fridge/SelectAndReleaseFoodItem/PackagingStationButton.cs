using UnityEngine;
using UnityEngine.EventSystems;

public class PackagingStationButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // Deve esserci un oggetto selezionato ? è in avanti rispetto al frigo
        var selected = Selectable.CurrentSelected;
        if (selected == null)
            return;

        var food = selected.GetComponent<FoodItem>();
        if (food == null)
            return;

        // Deve essere "unpackaged" secondo la logica del tuo inspector
        if (!food.isUnpackaged)
            return;

        // Lo confezioniamo: aggiornamento visuale + stato logico
        food.SetPackaged(true);

        // Se vuoi, qui puoi aumentare un punteggio, o loggare qualcosa.
    }
}
