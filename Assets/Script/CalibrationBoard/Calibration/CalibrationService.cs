using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CalibrationBoard.Calibration
{
    /// <summary>
    /// CalibrationService
    /// ----------------
    /// Obiettivo:
    /// - Stimare una trasformazione proiettiva (omografia / homography) che mappa:
    ///     punto Lidar "raw" (Vector2) -> punto schermo normalizzato (Vector2 in [0..1])
    /// - Salvare la trasformazione su file in Application.persistentDataPath
    /// - Fornire un metodo Map() che converte un punto Lidar in pixel schermo/canvas
    ///
    /// Scelte pratiche:
    /// - JSON: usiamo JsonUtility (Unity) per compatibilità massima con le versioni Unity/C#.
    /// - Coordinate target: salviamo/risolviamo in NORMALIZZATO [0..1] così la calibrazione è stabile
    ///   anche se cambia la risoluzione (poi in Map() riconvertiamo in pixel).
    /// </summary>
    public sealed class CalibrationService
    {
        // File dove viene persistita la matrice (9 valori).
        // Nota: per tua richiesta il nome NON contiene "_" (underscore).
        public const string DefaultFileName = "lidarCalibration.json";

        private readonly string _filePath;

        // Se null => nessuna calibrazione caricata/salvata.
        private Homography _homography;

        public CalibrationService(string fileName = DefaultFileName)
        {
            // persistentDataPath è il posto "sicuro" di Unity per salvare file per l'app
            // (funziona tra scene e tra sessioni).
            _filePath = Path.Combine(Application.persistentDataPath, fileName);

            // Provo subito a caricare una calibrazione già salvata.
            _homography = Load();
        }

        /// <summary>
        /// True se abbiamo una calibrazione valida caricata in memoria.
        /// </summary>
        public bool HasCalibration => _homography != null;

        /// <summary>
        /// Cancella la calibrazione sia in memoria sia su disco.
        /// </summary>
        public void Clear()
        {
            _homography = null;
            TryDelete();
        }

        /// <summary>
        /// Calcola una nuova omografia usando i campioni.
        /// Richiede almeno 4 campioni (il minimo matematico), ma 15-30 è meglio in pratica.
        /// </summary>
        public void ApplyNewCalibration(IReadOnlyList<CalibrationSample> samples)
        {
            if (samples == null || samples.Count < 4)
                throw new ArgumentException("At least four calibration samples are required.", nameof(samples));

            // Stima la matrice 3x3 (9 valori) in least squares.
            var homo = Homography.ComputeLeastSquares(samples);
            _homography = homo ?? throw new InvalidOperationException("Failed to compute calibration transform.");

            // Persistenza su disco.
            Save(_homography);
        }

        /// <summary>
        /// Mappa un punto Lidar raw in pixel schermo/canvas.
        /// - input: lidarRaw (coordinate raw del sensore)
        /// - canvasSizePx: dimensione in pixel dello spazio target (tipicamente Screen.width/height)
        /// - output: pixel schermo
        /// </summary>
        public Vector2 Map(Vector2 lidarRaw, Vector2 canvasSizePx)
        {
            if (_homography == null)
                return default;

            // Applico l'omografia: output in NORMALIZZATO [0..1] (idealmente).
            var normalized = _homography.Transform(lidarRaw);

            // Protezione: clamp in [0..1] per evitare click "fuori schermo".
            var x = Mathf.Clamp01(normalized.x) * canvasSizePx.x;
            var y = Mathf.Clamp01(normalized.y) * canvasSizePx.y;

            return new Vector2(x, y);
        }

        // --------------------------------------------------------------------
        // Persistenza (JsonUtility)
        // --------------------------------------------------------------------

        [Serializable]
        private sealed class HomographyDto
        {
            // JsonUtility supporta bene array e tipi base; qui basta un array di double.
            public double[] matrix;
        }

        private Homography Load()
        {
            if (!File.Exists(_filePath))
                return null;

            try
            {
                var json = File.ReadAllText(_filePath);
                var dto = JsonUtility.FromJson<HomographyDto>(json);

                if (dto == null || dto.matrix == null || dto.matrix.Length != 9)
                    return null;

                return new Homography(dto.matrix);
            }
            catch
            {
                // In caso di file corrotto o errori IO: semplicemente "nessuna calibrazione".
                return null;
            }
        }

        private void Save(Homography homography)
        {
            var dto = new HomographyDto { matrix = homography.Matrix };
            var json = JsonUtility.ToJson(dto, prettyPrint: true);
            File.WriteAllText(_filePath, json);

#if UNITY_EDITOR
            Debug.Log($"[CalibrationService] Calibrazione salvata in: {_filePath}");
#endif
        }

        private void TryDelete()
        {
            try
            {
                if (File.Exists(_filePath))
                    File.Delete(_filePath);
            }
            catch
            {
                // cleanup best-effort: se non riesce, pazienza.
            }
        }

        // --------------------------------------------------------------------
        // Modello dati campioni
        // --------------------------------------------------------------------

        /// <summary>
        /// Un campione di calibrazione:
        /// - World: coordinate raw del sensore (evt.Position)
        /// - ScreenNormalized: coordinate target normalizzate [0..1] (screenPx / Screen.width,height)
        /// </summary>
        [Serializable]
        public sealed class CalibrationSample
        {
            public Vector2 World;
            public Vector2 ScreenNormalized;

            public CalibrationSample(Vector2 world, Vector2 screenNormalized)
            {
                World = world;
                ScreenNormalized = screenNormalized;
            }
        }

        // --------------------------------------------------------------------
        // Core matematico: omografia
        // --------------------------------------------------------------------

        private sealed class Homography
        {
            // Matrice 3x3 salvata come array row-major:
            // [ m00 m01 m02
            //   m10 m11 m12
            //   m20 m21 m22 ]
            public Homography(double[] matrix)
            {
                if (matrix == null || matrix.Length != 9)
                    throw new ArgumentException("Homography matrix must have 9 elements.", nameof(matrix));

                Matrix = matrix;
            }

            public double[] Matrix { get; }

            /// <summary>
            /// Applica la trasformazione proiettiva al punto src.
            /// </summary>
            public Vector2 Transform(Vector2 src)
            {
                var m = Matrix;
                var x = src.x;
                var y = src.y;

                // Denominatore (proiettivo).
                var w = (m[6] * x) + (m[7] * y) + m[8];
                if (Math.Abs(w) < 1e-6)
                    return Vector2.zero;

                var tx = ((m[0] * x) + (m[1] * y) + m[2]) / w;
                var ty = ((m[3] * x) + (m[4] * y) + m[5]) / w;

                return new Vector2((float)tx, (float)ty);
            }

            /// <summary>
            /// Stima l'omografia con DLT + least squares.
            /// Nota: questa implementazione è pensata per essere "autosufficiente" in Unity
            /// (niente dipendenze esterne).
            /// </summary>
            public static Homography ComputeLeastSquares(IReadOnlyList<CalibrationSample> samples)
            {
                if (samples == null || samples.Count < 4)
                    return null;

                // Matrice A (2N x 9): per ogni corrispondenza aggiungo 2 righe.
                var rows = samples.Count * 2;
                var cols = 9;
                var a = new double[rows, cols];

                for (var i = 0; i < samples.Count; i++)
                {
                    var world = samples[i].World;
                    var screen = samples[i].ScreenNormalized;

                    var x = world.x;
                    var y = world.y;
                    var u = screen.x;
                    var v = screen.y;

                    var r1 = i * 2;
                    var r2 = r1 + 1;

                    // Riga per u
                    a[r1, 0] = -x;
                    a[r1, 1] = -y;
                    a[r1, 2] = -1;
                    a[r1, 3] = 0;
                    a[r1, 4] = 0;
                    a[r1, 5] = 0;
                    a[r1, 6] = u * x;
                    a[r1, 7] = u * y;
                    a[r1, 8] = u;

                    // Riga per v
                    a[r2, 0] = 0;
                    a[r2, 1] = 0;
                    a[r2, 2] = 0;
                    a[r2, 3] = -x;
                    a[r2, 4] = -y;
                    a[r2, 5] = -1;
                    a[r2, 6] = v * x;
                    a[r2, 7] = v * y;
                    a[r2, 8] = v;
                }

                // Trovo il vettore h che minimizza ||A h|| con vincolo ||h||=1:
                // uso l'autovettore associato al più piccolo autovalore di (A^T A).
                var ata = MultiplyTranspose(a);
                var eigen = JacobiSmallestEigenVector(ata);
                if (eigen == null)
                    return null;

                // Normalizzo imponendo h[8] = 1 per una rappresentazione consistente.
                var h = eigen;
                if (Math.Abs(h[8]) > 1e-9)
                {
                    var scale = 1.0 / h[8];
                    for (var k = 0; k < h.Length; k++)
                        h[k] *= scale;
                }

                return new Homography(h);
            }

            private static double[,] MultiplyTranspose(double[,] a)
            {
                var rows = a.GetLength(0);
                var cols = a.GetLength(1);
                var result = new double[cols, cols];

                for (var i = 0; i < cols; i++)
                {
                    for (var j = i; j < cols; j++)
                    {
                        double sum = 0;
                        for (var r = 0; r < rows; r++)
                            sum += a[r, i] * a[r, j];

                        result[i, j] = sum;
                        result[j, i] = sum;
                    }
                }

                return result;
            }

            /// <summary>
            /// Jacobi per autovettori di matrice simmetrica:
            /// ritorna l'autovettore relativo al più piccolo autovalore.
            /// </summary>
            private static double[] JacobiSmallestEigenVector(double[,] matrix)
            {
                var n = matrix.GetLength(0);
                const int maxIterations = 100;
                const double tolerance = 1e-10;

                var a = (double[,])matrix.Clone();
                var v = new double[n, n];

                // inizializza v come identità
                for (var i = 0; i < n; i++)
                    v[i, i] = 1.0;

                for (var iter = 0; iter < maxIterations; iter++)
                {
                    // Cerco l'elemento fuori diagonale più grande.
                    var p = 0;
                    var q = 1;
                    double max = 0;

                    for (var i = 0; i < n - 1; i++)
                    {
                        for (var j = i + 1; j < n; j++)
                        {
                            var val = Math.Abs(a[i, j]);
                            if (val > max)
                            {
                                max = val;
                                p = i;
                                q = j;
                            }
                        }
                    }

                    if (max < tolerance)
                        break;

                    var app = a[p, p];
                    var aqq = a[q, q];
                    var apq = a[p, q];

                    var tau = (aqq - app) / (2 * apq);
                    var t = Math.Sign(tau) / (Math.Abs(tau) + Math.Sqrt(1 + (tau * tau)));
                    var c = 1.0 / Math.Sqrt(1 + (t * t));
                    var s = t * c;

                    // Ruoto righe/colonne p,q
                    for (var i = 0; i < n; i++)
                    {
                        if (i == p || i == q) continue;

                        var aip = a[i, p];
                        var aiq = a[i, q];

                        a[i, p] = (c * aip) - (s * aiq);
                        a[p, i] = a[i, p];

                        a[i, q] = (c * aiq) + (s * aip);
                        a[q, i] = a[i, q];
                    }

                    var appNew = (c * c * app) - (2 * s * c * apq) + (s * s * aqq);
                    var aqqNew = (s * s * app) + (2 * s * c * apq) + (c * c * aqq);

                    a[p, p] = appNew;
                    a[q, q] = aqqNew;
                    a[p, q] = 0;
                    a[q, p] = 0;

                    // aggiorno la matrice degli autovettori
                    for (var i = 0; i < n; i++)
                    {
                        var vip = v[i, p];
                        var viq = v[i, q];
                        v[i, p] = (c * vip) - (s * viq);
                        v[i, q] = (s * vip) + (c * viq);
                    }
                }

                // Trovo l'indice del più piccolo autovalore (sulla diagonale di a).
                var minIndex = 0;
                var minValue = a[0, 0];
                for (var i = 1; i < n; i++)
                {
                    if (a[i, i] < minValue)
                    {
                        minValue = a[i, i];
                        minIndex = i;
                    }
                }

                // Estraggo l'autovettore corrispondente.
                var eigenVector = new double[n];
                for (var i = 0; i < n; i++)
                    eigenVector[i] = v[i, minIndex];

                // Normalizzo.
                var norm = Math.Sqrt(eigenVector.Sum(e => e * e));
                if (norm < 1e-9)
                    return null;

                for (var i = 0; i < eigenVector.Length; i++)
                    eigenVector[i] /= norm;

                return eigenVector;
            }
        }
    }
}
