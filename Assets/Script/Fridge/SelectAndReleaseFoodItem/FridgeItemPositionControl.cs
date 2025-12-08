using UnityEngine;

/// <summary>
/// controllo del gioco del frigo 1: gestione punteggio HACCP in base al posizionamento degli oggetti nel frigo.
/// </summary>
public class FridgeItemPositionControl : MonoBehaviour
{
    public static FridgeItemPositionControl Instance; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void OnItemSnapped(FoodItem item, FridgeSnapZone zone)
    {
        if (item == null || zone == null)
            return;

        bool isCorrect = IsPlacementCorrect(item, zone);

        int delta = isCorrect ? +10 : -5;

        if (HaccpScoreState.Instance != null)
        {
            HaccpScoreState.Instance.AddScore(delta);
            Debug.Log($"[Frigo1] {(isCorrect ? "CORRETTO" : "ERRORE")}: {item.displayName} su {zone.area}");
        }
        else
        {
            Debug.LogWarning("[Frigo1] HaccpScoreState.Instance è null, score non aggiornato.");
        }

            //se posizionamento corretto, questo alimento non è più selezionabile
        if (isCorrect)
        {
            var selectable = item.GetComponent<Selectable>();
            if (selectable != null)
            {
                selectable.LockSelection();
            }

            // 🔥 Qui notifichiamo a Fridge1State che un item corretto è stato posizionato
            if (Fridge1State.Instance != null)
            {
                Fridge1State.Instance.NotifyCorrectPlacement(item);
            }
        }

    }


    private bool IsPlacementCorrect(FoodItem item, FridgeSnapZone zone)
    {
        switch (zone.area)
        {
            case FridgeArea.ShelfTop:
                return item.placementCategory == FoodPlacementCategory.TopShelfGroup;
            case FridgeArea.ShelfUpperMid:
                return item.placementCategory == FoodPlacementCategory.UpperMidShelfGroup;
            case FridgeArea.ShelfLowerMid:
                return item.placementCategory == FoodPlacementCategory.LowerMidShelfGroup;
            case FridgeArea.ShelfBottom:
                return item.placementCategory == FoodPlacementCategory.BottomShelfGroup;
            case FridgeArea.Door:
                return item.placementCategory == FoodPlacementCategory.DoorGroup;
            default:
                return false;
        }
    }
}
