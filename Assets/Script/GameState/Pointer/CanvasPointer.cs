using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasPointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum Target { Play, Calibration, Menu}
    [SerializeField] private Target target;


    public void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
        }
        
    }



    public void OnPointerDown(PointerEventData eventData)
    {


    }

    public void OnPointerUp(PointerEventData eventData)
    {
        switch (target)
        {
            case Target.Play:
                GameManager.Instance.StartNewGame();
                break;

            case Target.Calibration:
                GameManager.Instance.GoToCalbrationScene();
                break;

            case Target.Menu:
                GameManager.Instance.GoToMenu();
                break;
        }
    }
}