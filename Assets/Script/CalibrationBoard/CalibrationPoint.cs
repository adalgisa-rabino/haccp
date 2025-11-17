using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CalibrationPoint : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Indice del punto (0..8) per l'ordine di calibrazione")]
    public int index;

    [Header("Sprite da colorare")]
    public Image spriteRenderer;

    [Header("Colori")]
    public Color idleColor = Color.white;
    public Color selectedColor = Color.green;

    private bool isSelected;

    void Awake()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = idleColor;
        }
        else
        {
            Debug.LogWarning($"CalibrationPoint {name}: nessun SpriteRenderer trovato.");
        }
    }

    public void OnPointerUp(PointerEventData e)
    {
        Debug.Log($"CalibrationPoint {name}: OnPointerUp called.");
    }

    public void OnPointerDown(PointerEventData e)
    {

        Debug.Log($"CalibrationPoint {name}: OnPointerDown called."); 

        // visual feedback
        SetSelected(true);

        // notify manager (null-check)
        if (CalibrationManager.Instance != null)
        {
            CalibrationManager.Instance.OnCalibrationPointClicked(this);
        }
        else
        {
            Debug.LogWarning($"CalibrationPoint {name}: CalibrationManager.Instance is null.");
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isSelected ? selectedColor : idleColor;
        }
    }
}