using UnityEngine;
using UnityEngine.EventSystems;

public class UIRotateController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Tooltip("Il componente RotateActions sul Player.")]
    public RotateActions rotateActions;

    [Tooltip("+1 = destra, -1 = sinistra")]
    [Range(-1, 1)] public int axisSign = +1;

    public void OnPointerDown(PointerEventData e) { rotateActions?.SetRotation(axisSign , true); }
    public void OnPointerUp(PointerEventData e) { rotateActions?.SetRotation(axisSign , false); }
    public void OnPointerExit(PointerEventData e) { rotateActions?.SetRotation(axisSign , false); }
}
