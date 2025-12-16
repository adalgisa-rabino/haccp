using UnityEngine;

/// <summary>
/// Test loader che usa ClueDatabase (ScriptableObject) invece del JSON.
/// Assegna l'asset ClueDatabase in Inspector su questo componente per testare in Play mode.
/// </summary>
public class ClueTestLoaderFromDB : MonoBehaviour
{
    public ClueDatabase clueDatabase;

    void Awake() { Debug.Log("Awake() chiamato ✅"); }
    void OnEnable() { Debug.Log("OnEnable() chiamato ✅"); }

    void Start()
    {
        if (clueDatabase == null || clueDatabase.indizi == null || clueDatabase.indizi.Count == 0)
        {
            Debug.LogWarning("⚠️ ClueDatabase non assegnato o vuoto. Usa l'importer per popolare l'asset.");
            return;
        }

        Debug.Log($"✅ ClueDatabase caricato! Totale indizi: {clueDatabase.indizi.Count}");

        var c0 = clueDatabase.indizi[0];
        Debug.Log($"🔎 [0] [{c0.tipo}] {c0.categoria} → {c0.testo}  (id={c0.id})");
        Debug.Log($"     bersagli → colpevole={c0.bersaglioColpevole} | arma={c0.bersaglioArma} | luogo={c0.bersaglioLuogo}");

        if (clueDatabase.indizi.Count > 1)
        {
            var c1 = clueDatabase.indizi[1];
            Debug.Log($"🔎 [1] [{c1.tipo}] {c1.categoria} → {c1.testo}  (id={c1.id})");
            Debug.Log($"     bersagli → colpevole={c1.bersaglioColpevole} | arma={c1.bersaglioArma} | luogo={c1.bersaglioLuogo}");
        }
    }
}