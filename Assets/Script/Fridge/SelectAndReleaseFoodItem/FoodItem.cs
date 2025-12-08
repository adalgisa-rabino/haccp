using UnityEngine;

public enum FoodPlacementCategory
{
    TopShelfGroup,
    UpperMidShelfGroup,
    LowerMidShelfGroup,
    BottomShelfGroup,
    DoorGroup,
    NoneGroup
}

public class FoodItem : MonoBehaviour
{
    [Header("Dati logici dell'alimento")]
    public string displayName;

    [TextArea]
    public string description;

    // Macrocategoria di collocazione (corrisponde a un'area del frigo)
    public FoodPlacementCategory placementCategory;

    [Header("Stato HACCP base")]
    public bool isExpired;       // scaduto
    public bool isUnpackaged;    // true = non confezionato, e quindi da confezionare

    [Header("Stato avanzato (gioco)")]
    public bool isDiscarded;     // true quando è stato buttato nel cestino

    [Header("Visuali confezionamento (opzionali)")]
    public GameObject unpackagedVisualRoot; // modello/sprite versione "sfusa"
    public GameObject packagedVisualRoot;   // modello/sprite versione "confezionata"

    [Header("Anteprima 3D (opzionale)")]
    public GameObject previewPrefab;
    // se lasci null, useremo il modello corrente come fallback

    // --- METODI DI SUPPORTO ---

    /// <summary>
    /// Imposta lo stato confezionato/non confezionato e aggiorna i modelli visivi.
    /// </summary>
    public void SetPackaged(bool packaged)
    {
        // nel tuo schema: isUnpackaged = true significa "deve essere confezionato"
        isUnpackaged = !packaged;

        if (unpackagedVisualRoot != null)
            unpackagedVisualRoot.SetActive(!packaged);

        if (packagedVisualRoot != null)
            packagedVisualRoot.SetActive(packaged);
    }

    /// <summary>
    /// Marca l'oggetto come buttato (da chiamare quando finisce nel cestino).
    /// </summary>
    public void MarkDiscarded()
    {
        isDiscarded = true;
    }
}
