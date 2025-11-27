using UnityEngine;

public class Fridge1GameManager : MonoBehaviour
{
    public static Fridge1GameManager Instance { get; private set; }

    [Header("Stato di gioco Frigo 1")]
    public int haccpScore = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Chiamato quando un oggetto viene posizionato in una SnapZone del Frigo 1.
    /// </summary>
    public void OnItemSnapped(FoodItem item, FridgeSnapZone zone)
    {
        if (item == null || zone == null)
            return;

        bool isCorrect = IsPlacementCorrect(item, zone);

        if (isCorrect)
        {
            haccpScore += 10;
            Debug.Log($"[Frigo1] CORRETTO: {item.displayName} su {zone.area}. Score = {haccpScore}");
        }
        else
        {
            haccpScore -= 5;
            Debug.Log($"[Frigo1] ERRORE: {item.displayName} su {zone.area}. Score = {haccpScore}");
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
