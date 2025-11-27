using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI minimale che riceve rivelazioni via evento statico e popola il Content del ScrollView.
/// Assegnare in Inspector: contentParent (RectTransform) e entryPrefab (prefab con Text).
/// </summary>
public class ClueWallUI : MonoBehaviour
{
    [Tooltip("RectTransform Content del ScrollView")]
    public RectTransform contentParent;

    [Tooltip("Prefab per ogni entry: deve avere un componente Text o child Text")]
    public GameObject entryPrefab;

    HashSet<string> shownIds = new HashSet<string>();

    void OnEnable()
    {
        ClueTarget.OnClueRevealed += OnClueRevealed;
    }

    void OnDisable()
    {
        ClueTarget.OnClueRevealed -= OnClueRevealed;
    }

    void OnClueRevealed(Clue clue)
    {
        Debug.Log($"[ClueWallUI] OnClueRevealed called id='{clue?.id}'");
        AddEntry(clue);
    }

    public void AddEntry(Clue clue)
    {
        Debug.Log($"[ClueWallUI] AddEntry called (entryPrefab={(entryPrefab == null ? "NULL" : "OK")}, contentParent={(contentParent == null ? "NULL" : "OK")})");

        if (clue == null) { Debug.Log("[ClueWallUI] clue==null"); return; }
        if (!string.IsNullOrWhiteSpace(clue.id) && shownIds.Contains(clue.id))
        {
            Debug.Log($"[ClueWallUI] id {clue.id} già mostrato");
            return;
        }
        if (entryPrefab == null || contentParent == null)
        {
            Debug.LogWarning("[ClueWallUI] entryPrefab o contentParent non assegnati nell'Inspector");
            return;
        }

        var go = Instantiate(entryPrefab, contentParent);
        go.name = "ClueEntry_" + (clue.id ?? "n_a");

        var txt = go.GetComponent<Text>();
        if (txt == null) txt = go.GetComponentInChildren<Text>();

        if (txt != null)
        {
            string title = $"{clue.categoria} • {clue.tipo}";
            string body = (clue.testo ?? "").Replace("\n", " ");
            txt.text = $"{title}\n{body}";
        }
        else
        {
            Debug.LogWarning("[ClueWallUI] entryPrefab non contiene Text child");
        }

        // se presente, forza ricalcolo altezza tramite ClueEntryAutoSize
        var auto = go.GetComponent<ClueEntryAutoSize>();
        if (auto != null) auto.Refresh();
        else Canvas.ForceUpdateCanvases();

        if (!string.IsNullOrWhiteSpace(clue.id)) shownIds.Add(clue.id);

        Debug.Log($"[ClueWallUI] Entry aggiunta: {go.name}");
    }

    public void ClearAll()
    {
        shownIds.Clear();
        if (contentParent == null) return;
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(contentParent.GetChild(i).gameObject);
    }

    // Metodo di test visibile in Inspector (context menu)
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
        AddEntry(dummy);
    }
}