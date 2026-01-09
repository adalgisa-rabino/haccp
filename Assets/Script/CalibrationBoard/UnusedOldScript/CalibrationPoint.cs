using UnityEngine;
using UnityEngine.EventSystems;

public class CalibrationPoint : MonoBehaviour, IPointerDownHandler
{
    public CalibrationManager manager;

    public void OnPointerDown(PointerEventData eventData)
    {
        manager?.RegisterCurrentPoint();
    }
}
