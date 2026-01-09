using UnityEngine;
using LidarTouch.Unity;
using TMPro;

public class CalibrationUIController : MonoBehaviour
{
    public RectTransform singleMarker;
    public TMP_Text statusText;

    private RectTransform _canvasRect;

    private void Awake()
    {
        if (singleMarker != null)
            _canvasRect = singleMarker.GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (statusText != null) statusText.gameObject.SetActive(false);

        // Qui la funzione server per piazzare il primo punto quando inizia la calibrazione
        PlaceMarkerRandom();
    }

    /// <summary>
    /// 1) calcola un punto random (pixel schermo)
    /// 2) sposta il marker in quel punto random (coordinate locali canvas)
    /// 3) ritorna le coordinate in pixel schermo di quel punto (Vecrot2 screenPx)
    /// </summary>
    public Vector2 PlaceMarkerRandom()
    {
        if (singleMarker == null || _canvasRect == null)
            return Vector2.zero;

        Vector2 screenPx = new Vector2(
            Random.Range(0f, Screen.width),
            Random.Range(0f, Screen.height)
        );

        //Posiziona il marker sul canva nel punto calcolato
        // Funzione di Unity che prende un punto in pixel schermo e indica dove cade nel sistema di coordinate locali di un RectTransform,
        // serve solo per posizionare il marker dal momento che è figlio di un canva, non influenza i valori necessari per la calibrazione
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            screenPx,
            null, // Overlay
            out Vector2 localPoint //out rende localPoint un valore di ritorno già definito
        );

        singleMarker.anchoredPosition = localPoint;
        singleMarker.gameObject.SetActive(true);

        //Ritorna le coordinate in pixel schermo del punto calcolato
        return screenPx;
    }
}
