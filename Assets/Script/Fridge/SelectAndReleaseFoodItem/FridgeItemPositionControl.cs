using UnityEngine;

/// <summary>
/// controllo del gioco del frigo 1: gestione punteggio HACCP in base al posizionamento degli oggetti nel frigo.
/// </summary>
public class FridgeItemPositionControl : MonoBehaviour
{
    public static FridgeItemPositionControl Instance;

    [Header("HACCP - Punteggi Snap")]
    [SerializeField] private int pointsCorrectFirstTime = 10;
    [SerializeField] private int pointsCorrectAfterAlreadyRewarded = 0;
    [SerializeField] private int pointsWrongShelf = -5;
    [SerializeField] private int pointsSameZone = -1;
    public static System.Action<FoodItem, FridgeSnapZone> OnAnySnapped;



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

        // Se è già stato snappato almeno una volta e il ripiano è lo stesso → nessuna variazione punti
        //if (item.hasBeenSnappedAtLeastOnce && item.lastSnappedArea == zone.area)
        //{
        //    Debug.Log($"[Frigo1] STESSO RIPIANO: {item.displayName} su {zone.area} → nessuna variazione HACCP");
        //    return;
        //}

        //if (item.hasBeenSnappedAtLeastOnce && item.lastSnappedArea == zone.area)
        //{
        //    int deltaSameZone = pointsSameZone; // es. -1 o -2 configurabile
        //    HaccpScoreState.Instance?.AddScore(deltaSameZone);

        //    if (logDebug)
        //        Debug.Log($"[Frigo1] STESSO RIPIANO: {item.displayName} su {zone.area} (delta {deltaSameZone})");

        //    return;
        //}


        bool isCorrectPosition = IsPlacementCorrect(item, zone);

        // aggiorna stato posizione
        item.isCorrectlyPlaced = isCorrectPosition;

        // Punteggio: +10 solo la prima volta che va nel ripiano giusto, -5 se sbagliato
        int delta;

        if (isCorrectPosition)
        {
            if (!item.hasReceivedCorrectPlacementReward)
            {
                delta = pointsCorrectFirstTime;
                item.hasReceivedCorrectPlacementReward = true;
            }
            else
            {
                delta = pointsCorrectAfterAlreadyRewarded;
            }
        }
        else
        {
            delta = pointsWrongShelf;
        }


        if (HaccpScoreState.Instance != null)
            HaccpScoreState.Instance.AddScore(delta);

        Debug.Log($"[Frigo1] {(isCorrectPosition ? "POSIZIONE OK" : "POSIZIONE ERRATA")}: {item.displayName} su {zone.area} (delta {delta})");

        // Aggiorna tracking ripiano
        item.hasBeenSnappedAtLeastOnce = true;
        item.lastSnappedArea = zone.area;

        // Aggiorna indicatori
        item.UpdateIndicators();

        // Se ora è “HACCP ok”, allora si finalizza (lock + conteggio vittoria)
        if (Fridge1State.Instance != null)
            Fridge1State.Instance.TryFinalizeItem(item);

        OnAnySnapped?.Invoke(item, zone);

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
