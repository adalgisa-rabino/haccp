using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CalibrationPoint : MonoBehaviour
{
    [Tooltip("Indice del punto (0..8) per l'ordine di calibrazione")]
    public int index;

    [Header("Sprite da colorare")]
    public SpriteRenderer spriteRenderer;

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

    private void OnMouseDown()
    {
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