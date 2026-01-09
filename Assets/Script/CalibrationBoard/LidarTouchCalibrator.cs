using LidarTouch.Core.Tracking;
using LidarTouch.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LidarTouchCalibrator : MonoBehaviour, INeedsCalibration
{
    //attributi pubblici che definiscono lo stato della calibrazione
    //public CalibrationOrder CurrentCalibrationPoint { get; private set; }
    //public Dictionary<CalibrationOrder, Vector2> CalibrationPoints { get; set; }

    //indica se la calibrazione è valida (tutti i punti sono stati acquisiti)
    private bool _isValidCalibration;
    public bool IsValidCalibration => _isValidCalibration; //implementazione dell'interfaccia che espone lo stato della calibrazione,
                                                           //se si usasse solo una variabile pubblica ci sarebbe il rischio che venga modificata dall'esterno
    [SerializeField]
    private CalibrationUIController _uiController;

    public List<CalibrationSample> Samples = new List<CalibrationSample>(20);
    private Vector2 _currentTargetScreenPx;

    protected void OnEnable()
    {
        // Inizializzo il dizionario dei punti di calibrazione
        CalibrationPoints = new Dictionary<CalibrationOrder, Vector2>();
        CurrentCalibrationPoint = CalibrationOrder.TopLeft;

        _isValidCalibration = false;

        _uiController?.HideStatusMessage();

        // Notifica subito il punto corrente da acquisire (TopLeft) alla UI
        OnCalibration?.Invoke(CurrentCalibrationPoint);
    }

    void Update()
    {
        // ESC cancella la calibrazione in corso sneza salvarla
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCalibration();
        }

        //R reset manuale della calibrazione senza salvare (utile se si sbagli un punto)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCalibration();
        }

        //Q esci solo se la calibrazione è valida (completa e salvata)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_isValidCalibration)
            {
                QuitApp();
            }
            else
            {
                Debug.Log("Cannot exit calibration mode: calibration not valid.");
            }
        }
    }

    [Serializable]
    public sealed class CalibrationEvent : UnityEngine.Events.UnityEvent<CalibrationOrder> //evento che quando invocato passa il punto di calibrazione corrente al listener
    { }

    [Serializable]
    public sealed class CalibrationFinishedEvent : UnityEngine.Events.UnityEvent
    { }

    public CalibrationEvent OnCalibration;
    public CalibrationFinishedEvent OnCalibrationFinished;

    // metodo chiamato quando si riceve un tocco dal Lidar
    public void HandleTouch(LidarTouchUnityDriver.UnityGestureEvent evt)
    {
        // conto solo i TouchDown per acquisire i punti di calibrazione
        if (evt.Type != GestureType.TouchDown) return;

        // Salva il punto nella chiave corrispondente al punto corrente da acquisire
        CalibrationPoints[CurrentCalibrationPoint] = evt.Position;

        // Avanza al prossimo punto
        CurrentCalibrationPoint++;

        // Se ho finito, salvo e segnalo completamento
        if (CurrentCalibrationPoint == CalibrationOrder.Finished)
        {
            LidarConstants.SaveCalibration(CalibrationPoints);
            _isValidCalibration = true; // ora la calibrazione è valida, tutti i punti sono stati acquisiti
            OnCalibrationFinished?.Invoke();
            _uiController?.ShowStatusMessage("Calibrazione completata.\nPremi Q per uscire.");
        }
        else
        {
            // Notifica il nuovo punto corrente da acquisire (il prossimo marker)
            OnCalibration?.Invoke(CurrentCalibrationPoint);
        }
    }

    private void CancelCalibration()
    {
        Debug.Log("Calibration canceled by user.");
        ResetCalibration();
        _uiController?.ShowStatusMessage("Calibrazione annullata.\nPremi R per riprovare.");
    }

    private void ResetCalibration()
    {
        Debug.Log("Calibration reset, restarting from first point.");

        if (CalibrationPoints == null)
            CalibrationPoints = new Dictionary<CalibrationOrder, Vector2>();
        else
            CalibrationPoints.Clear();

        CurrentCalibrationPoint = CalibrationOrder.TopLeft;
        _isValidCalibration = false;

        _uiController?.ShowStatusMessage("Calibrazione resettata.\nRiparti dal primo punto.");

        // Aggiorna subito la UI per mostrare il punto corrente da acquisire (TopLeft)
        OnCalibration?.Invoke(CurrentCalibrationPoint);
    }

    private void QuitApp()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
