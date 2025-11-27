using UnityEngine;

/// <summary>
/// Macrocategorie che corrispondono alle macro-aree del frigo.
/// Ogni valore qui avrà una corrispondenza con un FridgeArea.
/// </summary>
public enum FoodPlacementCategory
{
    TopShelfGroup,        // va sul ripiano alto
    UpperMidShelfGroup,   // va sul ripiano medio-alto
    LowerMidShelfGroup,   // va sul ripiano medio-basso
    BottomShelfGroup,     // va sul ripiano basso
    DoorGroup             // va sulla porta
}

public class FoodItem : MonoBehaviour
{
    [Header("Dati logici dell'alimento")]
    public string displayName;

    // Macrocategoria di collocazione (corrisponde a un'area del frigo)
    public FoodPlacementCategory placementCategory;

    [Header("Stato HACCP base")]
    public bool isExpired;       // scaduto
    public bool isUnpackaged;    // non confezionato
}
