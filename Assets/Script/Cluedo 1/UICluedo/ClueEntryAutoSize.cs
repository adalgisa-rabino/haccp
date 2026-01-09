using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Calcola e imposta LayoutElement.preferredHeight in base al Text figlio.
/// Chiamare Refresh() dopo aver impostato il testo (o lasciare che Start lo faccia una volta).
/// </summary>
[RequireComponent(typeof(LayoutElement))]
public class ClueEntryAutoSize : MonoBehaviour
{
    [Tooltip("Riferimento al Text che contiene il testo dell'indizio. Se vuoto verrà cercato nei figli.")]
    public Text bodyText;
    [Tooltip("Padding verticale totale applicato alla entry")]
    public float verticalPadding = 8f;

    LayoutElement _layoutEl;

    void Awake()
    {
        _layoutEl = GetComponent<LayoutElement>();
        if (bodyText == null) bodyText = GetComponentInChildren<Text>();
    }

    void Start()
    {
        // Primo refresh (utile quando il testo è già impostato immediatamente dopo l'istanza)
        Refresh();
    }

    /// <summary>
    /// Forza l'update della UI e calcola l'altezza preferita per il Text.
    /// </summary>
    public void Refresh()
    {
        if (bodyText == null || _layoutEl == null) return;

        // Forza il layout rebuild prima di leggere le misure
        Canvas.ForceUpdateCanvases();

        // ottiene l'altezza preferita del rect transform del testo
        float preferred = LayoutUtility.GetPreferredHeight(bodyText.rectTransform);

        // imposta preferredHeight (aggiunge padding)
        _layoutEl.preferredHeight = preferred + verticalPadding;
    }
}