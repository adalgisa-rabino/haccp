using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

public class DirtGridController : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private RectTransform surfaceRect;     // SurfacePanel
    [SerializeField] private RectTransform gridRoot;        // DirtGrid (figlio)
    [SerializeField] private int cols = 40;
    [SerializeField] private int rows = 25;
    [SerializeField] private float eraseRadiusCells = 2.2f; // raggio in celle

    [Header("Visual")]
    [SerializeField] private Color dirtColor = new Color(0.35f, 0.15f, 0.05f, 1f);
    [SerializeField] private Sprite cellSprite;             // opzionale, altrimenti quadrato default

    [Header("Progress")]
    [Range(0f, 1f)]
    [SerializeField] private float percentToWin = 0.85f;

    private Image[,] cells;
    private bool[,] cleared;
    private int clearedCount;
    private int totalCount;

    [SerializeField] private float eraseStrengthPerHit = 0.22f;   // quanto togli al centro per “passata”
    [SerializeField] private float smoothSpeed = 12f;             // quanto velocemente l’alpha segue il target
    [SerializeField] private float neighborBlur = 0.08f;           // 0 = niente blur, 0.05–0.15 = leggero
    private float[,] dirt;        // stato reale (0..1)
    private float[,] dirtTarget;  // target dopo l’erase
    private float cleaningPercent  = 0.0f;

    


    void Awake()
    {
        if (surfaceRect == null) surfaceRect = GetComponentInParent<RectTransform>();
        GenerateGrid();
    }

    void Update()
    {
        // Mouse erase demo
        if (Input.GetMouseButton(0))
        {
            EraseAtScreenPoint(Input.mousePosition);
        }

        cleaningPercent = Mathf.Clamp01(cleaningPercent);
        if (cleaningPercent > percentToWin) {

            Debug.Log("Surface cleaned!");
            //voglio che tutte le celle spariscano

            //aggiungo animazione che fa sparire tutte le celle



            for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
            {
                dirtTarget[x, y] = 0f;
            }

        }
    }

    void LateUpdate()
    {
        // 1) blur leggero tra vicini (opzionale)
        if (neighborBlur > 0f)
            ApplyNeighborBlur();

        // 2) smooth verso il target (fade)
        float k = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);

        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            dirt[x, y] = Mathf.Lerp(dirt[x, y], dirtTarget[x, y], k);
            SetCellAlpha(x, y, dirt[x, y]);
        }
    }

    private void ApplyNeighborBlur()
    {
        // Un blur leggero tipo diffusione: ogni cella tende verso la media dei vicini
        // neighborBlur 0.05–0.15 è già realistico
        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            float sum = dirtTarget[x, y];
            int n = 1;

            if (x > 0) { sum += dirtTarget[x - 1, y]; n++; }
            if (x < cols - 1) { sum += dirtTarget[x + 1, y]; n++; }
            if (y > 0) { sum += dirtTarget[x, y - 1]; n++; }
            if (y < rows - 1) { sum += dirtTarget[x, y + 1]; n++; }

            float avg = sum / n;
            dirtTarget[x, y] = Mathf.Lerp(dirtTarget[x, y], avg, neighborBlur);
        }
    }



    public void GenerateGrid()
    {
        if (gridRoot == null)
        {
            Debug.LogError("DirtGridController: gridRoot non assegnato.");
            return;
        }

        // pulizia figli esistenti
        for (int i = gridRoot.childCount - 1; i >= 0; i--)
            Destroy(gridRoot.GetChild(i).gameObject);

        cells = new Image[cols, rows];
        cleared = new bool[cols, rows];
        clearedCount = 0;
        totalCount = cols * rows;

        // gridRoot deve matchare surfaceRect
        if (surfaceRect != null)
        {
            gridRoot.anchorMin = Vector2.zero;
            gridRoot.anchorMax = Vector2.one;
            gridRoot.offsetMin = Vector2.zero;
            gridRoot.offsetMax = Vector2.zero;
        }

        Rect r = gridRoot.rect;
        float cellW = r.width / cols;
        float cellH = r.height / rows;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                GameObject go = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(gridRoot, false);

                RectTransform rt = (RectTransform)go.transform;
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0, 0);

                rt.sizeDelta = new Vector2(cellW + 0.5f, cellH + 0.5f); // piccolo overlap per evitare gap
                rt.anchoredPosition = new Vector2(x * cellW, y * cellH);

                Image img = go.GetComponent<Image>();
                img.color = dirtColor;
                if (cellSprite != null) img.sprite = cellSprite;

                cells[x, y] = img;
                cleared[x, y] = false;
            }
        }

        dirt = new float[cols, rows];
        dirtTarget = new float[cols, rows];

        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            dirt[x,y] = 1f;        // sporco pieno
            dirtTarget[x,y] = 1f;
            SetCellAlpha(x, y, 1f);
        }

    }
        private void SetCellAlpha(int x, int y, float a)
    {
        var img = cells[x, y];
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }


    public void ResetDirt()
    {
        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            cleared[x, y] = false;
            if (cells[x, y] != null)
                cells[x, y].enabled = true;
        }

        clearedCount = 0;
    }

    public void EraseAtScreenPoint(Vector2 screenPoint)
    {
        if (gridRoot == null) return;

        // Converti screen -> local in gridRoot
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRoot, screenPoint, null, out Vector2 local))
            return;

        // local è centrato sul pivot, ma noi abbiamo pivot 0,0 sulle celle e gridRoot di default pivot 0.5,0.5
        // quindi lo riportiamo in coordinate 0..width/height
        Rect r = gridRoot.rect;
        Vector2 p = local + new Vector2(r.width * gridRoot.pivot.x, r.height * gridRoot.pivot.y);

        if (p.x < 0 || p.x > r.width || p.y < 0 || p.y > r.height) return;

        int cx = Mathf.FloorToInt((p.x / r.width) * cols);
        int cy = Mathf.FloorToInt((p.y / r.height) * rows);

        EraseCellsAround(cx, cy);

        // win check (opzionale)
        float pct = (float)clearedCount / totalCount;
        // Debug.Log($"Cleaned: {pct:P0}");
    }

    private void EraseCellsAround(int cx, int cy)
    {
        int rad = Mathf.CeilToInt(eraseRadiusCells);

        for (int y = cy - rad; y <= cy + rad; y++)
        for (int x = cx - rad; x <= cx + rad; x++)
        {
            if (x < 0 || x >= cols || y < 0 || y >= rows) continue;

            float dx = x - cx;
            float dy = y - cy;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            if (dist > eraseRadiusCells) continue;

            // falloff morbido: 1 al centro -> 0 al bordo
            float t = 1f - Mathf.Clamp01(dist / eraseRadiusCells);
            // curva più “soft”
            float falloff = t * t; // puoi provare t^3 se vuoi più concentrato

            float delta = eraseStrengthPerHit * falloff;

            float previousDirt = dirtTarget[x, y];
            dirtTarget[x, y] = Mathf.Max(0f, dirtTarget[x, y] - delta);

            // Calcola la differenza di sporco rimossa
            float dirtRemoved = previousDirt - dirtTarget[x, y];

            // Aggiorna la percentuale di pulizia
            cleaningPercent += dirtRemoved / totalCount;
        }
    }


    public float GetCleanPercent()
    {
        return totalCount == 0 ? 0f : (float)clearedCount / totalCount;
    }

    public bool IsCleanEnough()
    {
        return GetCleanPercent() >= percentToWin;
    }

    public void EraseAtScreenPoint(Vector2 screenPoint, Camera uiCam = null)
{
    if (gridRoot == null) return;

    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRoot, screenPoint, uiCam, out Vector2 local))
        return;

    Rect r = gridRoot.rect;
    Vector2 p = local + new Vector2(r.width * gridRoot.pivot.x, r.height * gridRoot.pivot.y);

    if (p.x < 0 || p.x > r.width || p.y < 0 || p.y > r.height) return;

    int cx = Mathf.FloorToInt((p.x / r.width) * cols);
    int cy = Mathf.FloorToInt((p.y / r.height) * rows);

    EraseCellsAround(cx, cy);
}

}
