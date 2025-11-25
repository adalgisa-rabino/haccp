using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Carica la lista di indizi dal file JSON in StreamingAssets.
/// </summary>
[System.Serializable]
public class ClueLoader
{
    public List<Clue> indizi;

    public static ClueLoader Load()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "clues.json");

        if (!File.Exists(path))
        {
            Debug.LogError($"❌ File non trovato: {path}");
            return new ClueLoader { indizi = new List<Clue>() };
        }

        string json = File.ReadAllText(path);
        var db = JsonUtility.FromJson<ClueLoader>(json);

        if (db == null || db.indizi == null)
        {
            Debug.LogError("⚠️ JSON non valido o chiave 'indizi' mancante.");
            return new ClueLoader { indizi = new List<Clue>() };
        }

        Debug.Log($"✅ Caricati {db.indizi.Count} indizi da clues.json");
        return db;
    }
}
