using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// CalibrationUIController
/// ----------------------
/// Si occupa SOLO dell'interfaccia di calibrazione:
/// - piazzare un marker (RectTransform) in un punto random dello schermo
/// - mostrare/nascondere marker e messaggi di stato
/// </summary>
public class CalibrationUIController : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [SerializeField] public RectTransform singleMarker;
    // Testo di stato (TMP)
    [SerializeField] public TMP_Text statusText;
    // RectTransform del canvas (se lasciato vuoto lo ricavo dal marker)
    [SerializeField] public RectTransform canvasRect;

    //[Header("Button di fine calibrazione")]
    //[SerializeField] private Button menuButton;
    //[SerializeField] private Button calibrateButton;

    [SerializeField] private TMP_Text shortcutsText;

    private void Awake()
    {
        // Se canvasRect non è assegnato, provo a ricavarlo dal Canvas padre del marker.
        if (canvasRect == null && singleMarker != null)
        {
            var canvas = singleMarker.GetComponentInParent<Canvas>();
            canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
        }


    }

    //private void OnEnable()
    //{
    //    if (statusText != null)
    //        statusText.gameObject.SetActive(false);

    //    //HideShortcutsText();          // 👈 aggiungi
    //    //SetEndButtonsVisible(false);
    //}



    /// <summary>
    /// 1) Calcola un punto random in pixel schermo (screenPx)
    /// 2) Converte screenPx nelle coordinate locali del canvas per spostare il marker
    /// 3) Ritorna screenPx (questo è il dato usato dalla calibrazione)
    /// </summary>
    public Vector2 PlaceMarkerRandom()
    {
        if (singleMarker == null || canvasRect == null)
            return Vector2.zero;

        Vector2 screenPx = new Vector2(
            UnityEngine.Random.Range(0f, Screen.width),
            UnityEngine.Random.Range(0f, Screen.height)

        );

        // Conversione da pixel schermo -> coordinate locali canvas.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPx,
            null, // Canvas overlay => camera null
            out Vector2 localPoint
        );

        // Posiziono e mostro il marker
        singleMarker.anchoredPosition = localPoint;
        singleMarker.gameObject.SetActive(true);

#if UNITY_EDITOR
        Debug.Log($"[CalibrationUIController] Marker: screenPx={screenPx} localPoint={localPoint}");
#endif

        return screenPx;
    }

    public void HideMarker()
    {
        if (singleMarker != null)
            singleMarker.gameObject.SetActive(false);
    }

    public void ShowStatusMessage(string message)
    {
        if (statusText == null) return;

        statusText.text = message;
        statusText.gameObject.SetActive(true);
    }

    public void HideStatusMessage()
    {
        if (statusText == null) return;

        statusText.gameObject.SetActive(false);
    }

    public void ShowShortcutsText()
    {
        if (shortcutsText != null)
            shortcutsText.gameObject.SetActive(true);
    }

    public void HideShortcutsText()
    {
        if (shortcutsText != null)
            shortcutsText.gameObject.SetActive(false);
    }


    //public void SetEndButtonsVisible(bool visible)
    //{
    //    if (menuButton != null) menuButton.gameObject.SetActive(visible);
    //    if (calibrateButton != null) calibrateButton.gameObject.SetActive(visible);
    //}

    //public void BindEndButtons(Action onMenu, Action onCalibrateAgain)
    //{
    //    if (menuButton != null)
    //    {
    //        menuButton.onClick.RemoveAllListeners();
    //        menuButton.onClick.AddListener(() => onMenu?.Invoke());
    //    }

    //    if (calibrateButton != null)
    //    {
    //        calibrateButton.onClick.RemoveAllListeners();
    //        calibrateButton.onClick.AddListener(() => onCalibrateAgain?.Invoke());
    //        Debug.Log("[CalibrationUIController] Calibrate Again button bound.");
    //    }
    //}

}
