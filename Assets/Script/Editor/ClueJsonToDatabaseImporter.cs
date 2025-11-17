using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Importer editor che crea/aggiorna un singolo ClueDatabase.asset a partire dal JSON.
/// Menu: Tools > Clue Importer > Import From JSON
/// </summary>
public static class ClueJsonToDatabaseImporter
{
    [MenuItem("Tools/Clue Importer/Import From JSON (single DB)")]
    public static void ImportFromJsonToDatabase()
    {
        var loader = ClueLoader.Load();
        if (loader == null || loader.indizi == null)
        {
            Debug.LogError("[ClueImporter] Nessun indizio caricato dal JSON.");
            return;
        }

        string targetFolder = "Assets/Data/Clues";
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(targetFolder))
            AssetDatabase.CreateFolder("Assets/Data", "Clues");

        var dbAssetPath = Path.Combine(targetFolder, "ClueDatabase.asset").Replace('\\', '/');
        ClueDatabase db = AssetDatabase.LoadAssetAtPath<ClueDatabase>(dbAssetPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<ClueDatabase>();
            AssetDatabase.CreateAsset(db, dbAssetPath);
            Debug.Log("[ClueImporter] Creato ClueDatabase.asset");
        }

        // copia indizi dal loader nel DB
        db.indizi = new System.Collections.Generic.List<Clue>(loader.indizi);

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ClueImporter] Import completato: {db.indizi.Count} indizi nel ClueDatabase.asset");
    }
}