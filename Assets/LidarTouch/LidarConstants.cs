using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LidarTouch.Unity
{
    /// <summary>
    /// Ordine dei punti di calibrazione usati durante il processo di calibrazione.
    /// </summary>
    public enum CalibrationOrder
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        Finished
    }

    /// <summary>
    /// Gestisce il caricamento e il salvataggio della calibrazione Lidar.
    ///
    /// Scelta percorso:
    /// - cartella "Documenti Pubblici" (CommonDocuments), uguale per tutti gli utenti del PC
    /// - dentro una sottocartella stabile (es. "LidarTouch")
    ///
    /// Esempio Windows:
    /// C:\Users\Public\Documents\LidarTouch\calibration.json
    /// </summary>
    public static class LidarConstants
    {
        // Nome del file di calibrazione
        public const string CalibrationFileName = "calibration.json";

        // Sottocartella dentro "Documenti Pubblici"
        private const string SharedFolderName = "LidarTouch";

        private static string GetSharedFolder()
        {
            // Percorso base comune a tutti gli utenti (Windows: C:\Users\Public\Documents)
            string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            return Path.Combine(baseDir, SharedFolderName);
        }

        private static string GetCalibrationJsonPath()
        {
            return Path.Combine(GetSharedFolder(), CalibrationFileName);
        }

        public static Dictionary<CalibrationOrder, Vector2> LoadCalibration()
        {
            string filePath = GetCalibrationJsonPath();

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[Calibration] File not found at {filePath}");
                return new Dictionary<CalibrationOrder, Vector2>();
            }

            string json = File.ReadAllText(filePath);
            CalibrationWrapper wrapper = JsonUtility.FromJson<CalibrationWrapper>(json);

            Debug.Log($"[Calibration] Loaded from {filePath}");
            return wrapper.ToDictionary();
        }

        public static void SaveCalibration(Dictionary<CalibrationOrder, Vector2> points)
        {
            string folder = GetSharedFolder();
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string filePath = GetCalibrationJsonPath();

            string json = JsonUtility.ToJson(new CalibrationWrapper(points), true);
            File.WriteAllText(filePath, json);

            Debug.Log($"[Calibration] Saved to {filePath}");
        }
    }
}
