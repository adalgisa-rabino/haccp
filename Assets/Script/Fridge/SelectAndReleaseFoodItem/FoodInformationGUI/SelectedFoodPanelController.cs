using System.Collections;
using UnityEngine;
using TMPro;

public class SelectedFoodPanelController : MonoBehaviour
{
    public static SelectedFoodPanelController Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text infoText;

    [Header("Preview 3D")]
    [SerializeField] private Transform previewAnchor;   // un empty davanti alla GUICamera, vicino al testo
    [SerializeField] private string previewLayerName = "SelectedFood3D";
    [SerializeField] private float previewRotationSpeed = 30f;

    [Header("Popup animation")]
    [SerializeField] private float popupDuration = 0.15f;
    [SerializeField] private float popupScale = 1.0f;
    [SerializeField] private float popupScaleStart = 0.8f;

    private FoodItem currentFood;
    private Selectable currentSelectable;
    private GameObject currentPreviewInstance;
    private int previewLayer = -1;
    private Coroutine popupRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panelRoot == null)
            panelRoot = gameObject;

        // layer per l'anteprima 3D (opzionale)
        if (!string.IsNullOrEmpty(previewLayerName))
        {
            previewLayer = LayerMask.NameToLayer(previewLayerName);
            if (previewLayer == -1)
            {
                Debug.LogWarning($"[SelectedFoodPanel] Layer '{previewLayerName}' non trovato. Verifica in Project Settings > Tags and Layers.");
            }
        }
    }

    private void Start()
    {
        HideImmediate();
    }

    private void Update()
    {
        // Rotazione lenta della preview
        if (currentPreviewInstance != null)
        {
            currentPreviewInstance.transform.Rotate(Vector3.up * previewRotationSpeed * Time.deltaTime, Space.World);
        }
    }

    /// <summary>
    /// Mostra il popup con nome+descrizione e crea l'anteprima 3D.
    /// </summary>
    public void Show(FoodItem food, Selectable selectable)
    {
        currentFood = food;
        currentSelectable = selectable;

        if (nameText != null)
            nameText.text = food != null ? food.displayName : string.Empty;

        if (infoText != null)
            infoText.text = food != null ? food.description : string.Empty;

        SpawnPreview(food);

        panelRoot.SetActive(true);
        StartPopupAnimation(opening: true);
    }

    /// <summary>
    /// Nasconde popup e preview con animazione.
    /// </summary>
    public void Hide()
    {
        if (!panelRoot.activeSelf && currentPreviewInstance == null)
            return;

        StartPopupAnimation(opening: false);
    }

    /// <summary>
    /// Nasconde subito senza animazione (usato a Start o come fallback).
    /// </summary>
    public void HideImmediate()
    {
        if (popupRoutine != null)
        {
            StopCoroutine(popupRoutine);
            popupRoutine = null;
        }

        panelRoot.SetActive(false);

        if (currentPreviewInstance != null)
        {
            Destroy(currentPreviewInstance);
            currentPreviewInstance = null;
        }

        currentFood = null;
        currentSelectable = null;
    }

    private void SpawnPreview(FoodItem food)
    {
        if (previewAnchor == null)
        {
            Debug.LogWarning("[SelectedFoodPanel] previewAnchor non assegnato.");
            return;
        }

        // distruggi eventuale preview precedente
        if (currentPreviewInstance != null)
        {
            Destroy(currentPreviewInstance);
            currentPreviewInstance = null;
        }

        GameObject prefab = null;
        if (food != null)
        {
            if (food.previewPrefab != null)
            {
                prefab = food.previewPrefab;
            }
            else
            {
                // fallback: usa il GameObject dell'alimento
                prefab = food.gameObject;
            }
        }

        if (prefab == null)
            return;

        currentPreviewInstance = Instantiate(prefab, previewAnchor.position, previewAnchor.rotation, previewAnchor);

        // se stai clonando l'oggetto di scena, rimuovi componenti che non servono (Rigidbody, Selectable, ecc.)
        var rb = currentPreviewInstance.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        var selectable = currentPreviewInstance.GetComponent<Selectable>();
        if (selectable != null) Destroy(selectable);

        // metti tutta la preview su un layer dedicato (se il layer esiste)
        if (previewLayer != -1)
        {
            SetLayerRecursively(currentPreviewInstance, previewLayer);
        }

        // reset scala locale a 1 (la regoli tu in editor sulla anchor)
        currentPreviewInstance.transform.localScale = Vector3.one;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void StartPopupAnimation(bool opening)
    {
        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(PopupCoroutine(opening));
    }

    private IEnumerator PopupCoroutine(bool opening)
    {
        float t = 0f;
        Vector3 startScale;
        Vector3 endScale;

        if (opening)
        {
            startScale = Vector3.one * popupScaleStart;
            endScale = Vector3.one * popupScale;
            panelRoot.transform.localScale = startScale;
        }
        else
        {
            startScale = panelRoot.transform.localScale;
            endScale = Vector3.one * popupScaleStart;
        }

        while (t < popupDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / popupDuration);
            // easing semplice tipo "ease out"
            float ease = opening ? 1f - Mathf.Pow(1f - k, 3f) : Mathf.Pow(1f - k, 3f);

            panelRoot.transform.localScale = Vector3.Lerp(startScale, endScale, ease);
            yield return null;
        }

        if (opening)
        {
            panelRoot.transform.localScale = Vector3.one * popupScale;
        }
        else
        {
            // chiusura definitiva
            HideImmediate();
        }

        popupRoutine = null;
    }
}
