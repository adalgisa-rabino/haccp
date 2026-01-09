using UnityEngine;

public sealed class CalibrationMapper
{
    // Origine locale: angolo Top-Left nello spazio Lidar
    private readonly Vector2 TL;

    // Base locale nello spazio Lidar
    private readonly Vector2 ex; // sinistra → destra
    private readonly Vector2 ey; // alto → basso

    // Inverso del determinante della base
    private readonly float invDet;

    private readonly int W, H;

    public CalibrationMapper(
        Vector2 topLeft,
        Vector2 topRight,
        Vector2 bottomLeft,
        int screenWidth,
        int screenHeight
    )
    {
        TL = topLeft;

        ex = topRight - topLeft;
        ey = bottomLeft - topLeft;

        /*
         * cambio di base
         * da coordinate globali (Lidar)
         * a coordinate locali (u, v)
         *
         * det = ex.x * ey.y - ex.y * ey.x
         *
         * È il determinante della matrice [ex ey]:
         * rappresenta l'area orientata del parallelogramma
         * generato dai due vettori base.
         *
         * Serve per calcolare l'inversa del cambio di base.
         * Se det = 0, la calibrazione non è invertibile.
         * 
         * L’inversa serve per tornare dalle coordinate globali alle coordinate locali.
         * Senza inversa:
         * sai costruire un punto dato 𝑢,𝑣 ma non sai leggere 𝑢,𝑣 da un punto reale
         */
        float det = ex.x * ey.y - ex.y * ey.x;
        invDet = 1f / det;

        W = screenWidth;
        H = screenHeight;
    }

    public Vector2 MapToScreenPixels(Vector2 raw)
    {
        // Porto il punto Lidar nello spazio della superficie calibrata corrispondente all'area dello schermo Unity.
        Vector2 p = raw - TL;

        // Calcolo dove cade il punto all'interno dell'area calibrata:
        // - u dice "quanto è a destra" (0 = sinistra, 1 = destra)
        // - v dice "quanto è in basso" (0 = alto, 1 = basso)
        float u = (p.x * ey.y - p.y * ey.x) * invDet;
        float v = (-p.x * ex.y + p.y * ex.x) * invDet;

        // Converto questa posizione relativa (u, v)
        // in una posizione assoluta sullo schermo di Unity.
        // A questo punto il punto è pronto per essere usato
        // come input (UI, raycast, spawn, ecc.).
        return new Vector2(
            u * W,
            (1f - v) * H
        );
    }

}
