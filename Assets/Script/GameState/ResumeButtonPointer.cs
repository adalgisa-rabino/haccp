using UnityEngine;
using UnityEngine.EventSystems;

public class ResumeButtonPointer : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.IsPaused)
            GameManager.Instance.SetPause(false);
    }
}
