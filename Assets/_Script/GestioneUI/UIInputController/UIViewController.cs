using UnityEngine;
using UnityEngine.EventSystems;

public class UIViewController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Riferimenti")]
    [Tooltip("Componente sul CameraPivot che esegue la rotazione verticale.")]
    public ViewActions viewActions; // oppure Project.Actions.ViewActions se hai il namespace

    [Header("Direzione")]
    [Tooltip("+1 = guarda in su, -1 = guarda in giù")]
    [Range(-1, 1)] public int axisSign = +1;

    private bool holding;

    public void OnPointerDown(PointerEventData e) { holding = true; Apply(true); }
    public void OnPointerUp(PointerEventData e) { holding = false; Apply(false); }
    public void OnPointerExit(PointerEventData e) { if (holding) { holding = false; Apply(false); } }

    private void Apply(bool active)
    {
        if (!viewActions) return;
        if (axisSign >= 0) viewActions.SetLookUp(active);
        else viewActions.SetLookDown(active);
    }
}
