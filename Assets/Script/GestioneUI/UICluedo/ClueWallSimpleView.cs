using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class ClueWallSimpleView : MonoBehaviour
{
    [Tooltip("TMP Text che mostra tutti gli indizi (il codice forzerà il word wrapping)")]
    public TMP_Text targetText;

    [Tooltip("Separatore tra entry")]
    public string separator = "\n\n";

    [Tooltip("Spaziatura tra righe (valore aggiuntivo in unità testo, TMP usa unità in px)")]
    public float lineSpacing = 0f;

    [Tooltip("Se true mostra la categoria (es. Colpevole/Arma/Luogo) prima del testo; se false mostra solo il testo")]
    public bool showCategory = false;

    HashSet<string> shownIds = new HashSet<string>();
    List<string> entries = new List<string>();

    void OnEnable()
    {
        ClueTarget.OnClueRevealed += OnClueRevealed;
        ApplyLineSpacing();
    }

    void OnDisable()
    {
        ClueTarget.OnClueRevealed -= OnClueRevealed;
    }

    void OnClueRevealed(Clue clue)
    {
        if (clue == null) return;
        if (!string.IsNullOrWhiteSpace(clue.id) && shownIds.Contains(clue.id)) return;

        string body = (clue.testo ?? "").Replace("\n", " ");

        // Se showCategory è true mostriamo anche la categoria, altrimenti solo il testo
        string entry = showCategory && !string.IsNullOrWhiteSpace(clue.categoria)
            ? $"{clue.categoria}\n{body}"
            : body;

        entries.Add(entry);
        if (!string.IsNullOrWhiteSpace(clue.id)) shownIds.Add(clue.id);

        RefreshText();
    }

    void RefreshText()
    {
        if (targetText == null)
        {
            Debug.LogWarning("[ClueWallSimpleView] targetText non assegnato");
            return;
        }

        ApplyLineSpacing();

        var sb = new StringBuilder();
        for (int i = 0; i < entries.Count; i++)
        {
            if (i > 0) sb.Append(separator);
            sb.Append(entries[i]);
        }

        targetText.text = sb.ToString();

        // Aggiorna TMP immediatamente (meno costoso di Canvas.ForceUpdateCanvases)
        targetText.ForceMeshUpdate();
    }

    void ApplyLineSpacing()
    {
        if (targetText == null) return;

        // Assicura word-wrapping su TMP
        targetText.enableWordWrapping = true;

        // Imposta spaziatura fra righe (valore aggiuntivo in unità TMP)
        targetText.lineSpacing = lineSpacing;
    }

    public void ClearAll()
    {
        entries.Clear();
        shownIds.Clear();
        if (targetText != null) targetText.text = "";
    }

    [ContextMenu("Add Test Entry")]
    void AddTestEntry()
    {
        var dummy = new Clue
        {
            id = "test_001",
            tipo = "Ambiguo",
            categoria = "Colpevole",
            testo = "Testo di prova per verificare la UI. Questo testo dovrebbe andare a capo se è lungo."
        };
        OnClueRevealed(dummy);
    }
}