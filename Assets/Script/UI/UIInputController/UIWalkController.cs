using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UIWalkController
/// ---------------------------
/// Script da mettere su un pulsante UI per inviare comandi di camminata
/// avanti o indietro al componente <see cref="WalkActions"/> sul Player.
///
/// Come usarlo:
/// - Aggancia questo script ai pulsanti UI "Forward" e "Backward".
/// - Trascina nell'Inspector il Player (che ha lo script WalkActions).
/// - Imposta axisSign: +1 per avanti, -1 per indietro.
/// </summary>
public class UIWalkController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Riferimenti")]
    [Tooltip("Componente WalkActions sul Player.")]
    public WalkActions walkActions;

    [Header("Direzione")]
    [Tooltip("+1 = avanti, -1 = indietro")]
    [Range(-1, 1)]
    public int axisSign = +1;

    // Stato locale: true mentre il pulsante è tenuto premuto
    private bool holding;

    public void OnPointerDown(PointerEventData e)
    {
        holding = true;
        Apply(true);
    }

    public void OnPointerUp(PointerEventData e)
    {
        holding = false;
        Apply(false);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (holding)
        {
            holding = false;
            Apply(false);
        }
    }

    // Invia il comando al WalkActions
    private void Apply(bool active)
    {
        if (!walkActions) return;

        if (axisSign >= 0)
            walkActions.SetMoveForward(active);
        else
            walkActions.SetMoveBackward(active);
    }
}
