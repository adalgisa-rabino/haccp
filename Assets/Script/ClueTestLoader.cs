using UnityEngine;
using System.IO;

public class ClueTestLoader : MonoBehaviour
{
    void Awake() { Debug.Log("Awake() chiamato ✅"); }
    void OnEnable() { Debug.Log("OnEnable() chiamato ✅"); }

    void Start()
    {
        Debug.Log("Start() chiamato ✅");

        // Mostra dove sta cercando il file
        string path = Path.Combine(Application.streamingAssetsPath, "clues.json");
        Debug.Log("📁 Percorso JSON: " + path + "  | Esiste? " + File.Exists(path));

        // Carica il database
        ClueLoader loader = ClueLoader.Load();

        if (loader == null) { Debug.LogError("❌ loader == null"); return; }
        if (loader.indizi == null) { Debug.LogError("❌ loader.indizi == null"); return; }
        if (loader.indizi.Count == 0) { Debug.LogWarning("⚠️ Nessun indizio nel JSON."); return; }

        Debug.Log($"✅ Database caricato! Totale indizi: {loader.indizi.Count}");

        // Stampa i primi 2 indizi usando i NOMI ITALIANI della tua Clue.cs
        var c0 = loader.indizi[0];
        Debug.Log($"🔎 [0] [{c0.tipo}] {c0.categoria} → {c0.testo}  (id={c0.id})");
        Debug.Log($"     bersagli → colpevole={c0.bersaglioColpevole} | arma={c0.bersaglioArma} | luogo={c0.bersaglioLuogo}");

        if (loader.indizi.Count > 1)
        {
            var c1 = loader.indizi[1];
            Debug.Log($"🔎 [1] [{c1.tipo}] {c1.categoria} → {c1.testo}  (id={c1.id})");
            Debug.Log($"     bersagli → colpevole={c1.bersaglioColpevole} | arma={c1.bersaglioArma} | luogo={c1.bersaglioLuogo}");
        }
    }
}
