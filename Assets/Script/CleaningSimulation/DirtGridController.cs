using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private Sprite cellSprite;

    [Header("Progress")]
    [Range(0f, 1f)]
    [SerializeField] private float percentToWin = 0.85f;

    [Header("Erase feel")]
    [SerializeField] private float eraseStrengthPerHit = 0.22f;

    [Header("Input (optional)")]
    [SerializeField] private bool alsoAllowMouse = true;

    private Image[,] cells;
    private float[,] dirtTarget;
    private int totalCount;
    private float cleaningPercent = 0f;

    void Awake()
    {
        if (surfaceRect == null) surfaceRect = GetComponent<RectTransform>();
        GenerateGrid();
    }

    void Update()
    {
        if (alsoAllowMouse && Input.GetMouseButton(0))
        {
            // Overlay: uiCam deve essere null
            EraseAtScreenPoint(Input.mousePosition, null);
        }

        cleaningPercent = Mathf.Clamp01(cleaningPercent);
        if (cleaningPercent >= percentToWin)
        {
            Debug.Log("Surface cleaned!");
        }
    }

    public void GenerateGrid()
    {
        if (gridRoot == null)
        {
            Debug.LogError("DirtGridController: gridRoot non assegnato.");
            return;
        }

        for (int i = gridRoot.childCount - 1; i >= 0; i--)
            Destroy(gridRoot.GetChild(i).gameObject);

        cells = new Image[cols, rows];
        dirtTarget = new float[cols, rows];

        totalCount = cols * rows;
        cleaningPercent = 0f;

        GridLayoutGroup gl = gridRoot.GetComponent<GridLayoutGroup>();
        if (gl == null) gl = gridRoot.gameObject.AddComponent<GridLayoutGroup>();

        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gl.constraintCount = cols;
        gl.childAlignment = TextAnchor.UpperLeft;
        gl.startAxis = GridLayoutGroup.Axis.Horizontal;
        gl.startCorner = GridLayoutGroup.Corner.UpperLeft;

        Rect r = gridRoot.rect;
        float cellW = r.width / cols;
        float cellH = r.height / rows;
        gl.cellSize = new Vector2(cellW, cellH);
        gl.spacing = Vector2.zero;

        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            GameObject go = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(gridRoot, false);

            Image img = go.GetComponent<Image>();
            img.color = dirtColor;

            if (cellSprite != null)
            {
                img.sprite = cellSprite;
                img.type = Image.Type.Simple;
                img.preserveAspect = false;
            }

            cells[x, y] = img;
            dirtTarget[x, y] = 1f; // 1 = sporco pieno (alpha 1)
        }
    }

    // Wrapper: serve per le chiamate che passano Vector3 (mousePosition è Vector3)
    public void EraseAtScreenPoint(Vector3 screenPoint, Camera uiCamera)
    {
        EraseAtScreenPoint(new Vector2(screenPoint.x, screenPoint.y), uiCamera);
    }

    // Metodo "vero": lavora in 2D (screen point) + camera opzionale
    public void EraseAtScreenPoint(Vector2 screenPoint, Camera uiCamera = null)
    {
        if (gridRoot == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRoot, screenPoint, uiCamera, out Vector2 local))
            return;

        Rect r = gridRoot.rect;

        // local è centrato sul pivot: riconverti in coordinate 0..width / 0..height
        Vector2 p = local + new Vector2(r.width * gridRoot.pivot.x, r.height * gridRoot.pivot.y);

        if (p.x < 0 || p.x > r.width || p.y < 0 || p.y > r.height) return;

        int cx = Mathf.FloorToInt((p.x / r.width) * cols);
        int cy = Mathf.FloorToInt((p.y / r.height) * rows);

        EraseCellsAround(cx, cy);
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

            float t = 1f - Mathf.Clamp01(dist / eraseRadiusCells);
            float falloff = t * t;

            float delta = eraseStrengthPerHit * falloff;

            float prev = dirtTarget[x, y];
            dirtTarget[x, y] = Mathf.Max(0f, dirtTarget[x, y] - delta);

            float removed = prev - dirtTarget[x, y];
            cleaningPercent += removed / totalCount;

            SetCellAlpha(x, y, dirtTarget[x, y]);
        }
    }

    private void SetCellAlpha(int x, int y, float a01)
    {
        if (cells == null) return;
        Image img = cells[x, y];
        Color c = img.color;
        c.a = a01;
        img.color = c;
    }

    public float GetCleanPercent01()
    {
        return cleaningPercent;
    }
}
