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
    public GameObject packagedPreviewPrefab; // anteprima quando l'item è confezionato

    [Header("Tracking ripiano (anti-penalità stesso ripiano)")]
    public bool hasBeenSnappedAtLeastOnce = false;
    public FridgeArea lastSnappedArea;

    [Header("Stato correttezza")]
    public bool isCorrectlyPlaced = false; // già lo hai? se sì non duplicare

    public bool hasBeenCountedCorrectly = false;   // per vittoria (una sola volta)
    public bool hasReceivedCorrectPlacementReward = false; // per +10 (una sola volta)

    [Header("Indicatori (opzionale)")]
    public GameObject indicatorWrong;
    public GameObject indicatorPending;   // es: punto esclamativo
    public GameObject indicatorOk;        // es: check
    public GameObject indicatorExpired;   // es: teschio/rosso



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


    public bool IsHaccpOk()
    {
        // “Corretto HACCP” = nel ripiano giusto + confezionato se richiesto + non scaduto (o non presente perché buttato)
        // Nota: isUnpackaged == true => richiede confezionamento
        if (isDiscarded) return true;            // se è stato buttato, non deve essere “ok nel frigo”
        if (!isCorrectlyPlaced) return false;    // posizione deve essere corretta
        if (isUnpackaged) return false;          // deve essere confezionato se richiesto
        if (isExpired) return false;             // se scaduto deve essere buttato (quindi non ok)
        return true;
    }

    public bool HasPendingIssues()
    {
        // “pendente” = in posizione corretta ma manca confezionamento oppure è scaduto e non buttato
        if (!isCorrectlyPlaced) return false;
        if (isDiscarded) return false;
        return isUnpackaged || isExpired;
    }

    public void UpdateIndicators()
    {
        if (indicatorWrong != null) indicatorWrong.SetActive(false);
        if (indicatorPending != null) indicatorPending.SetActive(false);
        if (indicatorOk != null) indicatorOk.SetActive(false);
        if (indicatorExpired != null) indicatorExpired.SetActive(false);

        if (isDiscarded) return;

        if (isExpired)
        {
            if (indicatorExpired != null) indicatorExpired.SetActive(true);
            return;
        }

        if (!isCorrectlyPlaced)
        {
            if (indicatorWrong != null) indicatorWrong.SetActive(true);
            return;
        }

        if (HasPendingIssues())
        {
            if (indicatorPending != null) indicatorPending.SetActive(true);
            return;
        }

        if (IsHaccpOk())
        {
            if (indicatorOk != null) indicatorOk.SetActive(true);
        }
    }

}
