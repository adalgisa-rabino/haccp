using UnityEngine;
using LidarTouch.Unity;
using TMPro;

public class CalibrationUIController : MonoBehaviour
{
    [Header("Assegna i 9 marker X in ordine TopLeft → BottomRight")]
    public GameObject[] markers; // length 9

    [Header("Testo informativo sullo stato della calibrazione")]
    public TMP_Text statusText;

    private void OnEnable()
    {
        // All’avvio mostra il primo marker (TopLeft)
        ShowMarkerIndex(0);
        if (statusText != null) statusText.gameObject.SetActive(false);
    }

    // Collega questo metodo a LidarTouchCalibrator.OnCalibration (UnityEvent<CalibrationOrder>)
    public void OnCalibrationStep(CalibrationOrder currentPointToAcquire)
    {
        // "currentPointToAcquire" è il punto corrente da acquisire.
        // Mostra esattamente quel marker. Se è Finished, spegni tutto.
        int index = (int)currentPointToAcquire;

        if (index >= 0 && index < 9) // 0..8 sono i 9 punti
        {
            ShowMarkerIndex(index);
        }
        else
        {
            HideAllMarkers();
        }

        // Quando riparti o avanzi, nascondi eventuali messaggi di stato
        HideStatusMessage();
    }

    // Collega questo metodo a LidarTouchCalibrator.OnCalibrationFinished
    public void OnCalibrationFinished()
    {
        HideAllMarkers();
        if (statusText != null)
        {
            statusText.text = "Calibrazione completata: premi Q per uscire.";
            statusText.gameObject.SetActive(true);
        }
    }

    private void ShowMarkerIndex(int index)
    {
        if (markers == null) return;

        for (int i = 0; i < markers.Length; i++)
            markers[i].SetActive(i == index);
    }

    private void HideAllMarkers()
    {
        if (markers == null) return;

        for (int i = 0; i < markers.Length; i++)
            markers[i].SetActive(false);
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
}
