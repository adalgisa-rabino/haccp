using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Attachment;

public class TemperatureController : MonoBehaviour
{
    public System.Action OnTemperatureCompleted;

    [Header("Valori da indovinare")]
    [SerializeField] private float _correctMinTemperature = 45f; // es. 45°
    [SerializeField] private float _correctMaxTemperature = 60f; // es. 60°
    [SerializeField] private float _tolerance = 0.5f;            // quanto puoi sbagliare

    [Header("UI – Slider / Termometro")]
    [SerializeField] private Slider _minSlider;        // slider temperatura minima
    [SerializeField] private Slider _maxSlider;        // slider temperatura
    //[SerializeField] private Im _correctImage; //triangolino verde
    [SerializeField] private RectTransform _correctMinHandle; //triangolino verde temp min
    [SerializeField] private RectTransform _correctMaxHandle; //triangolino verde temp max
    [SerializeField] private GameObject _sliderGroup;  // pannello della UI del termometro

    [Header("UI – Valore numerico")]
    [SerializeField] private TextMeshProUGUI _temperatureLabel;   // unico Text: "xx°C"

    [Header("Tick (tacche ogni 10)")]
    [SerializeField] private RectTransform _tickPrefab; // immagine sottile disattivata
    [SerializeField] private RectTransform _maxTrack;   // rect che copre l'altezza del termometro

    private bool _minCorrect = false;
    private bool _maxCorrect = false;
    private bool _alreadyWon = false;

    private void Start()
    {
        // Minigioco nascosto all'inizio
        if (_sliderGroup != null)
            _sliderGroup.SetActive(false);

        ResetHandleStates();

        _minCorrect = false;
        _maxCorrect = false;
        _alreadyWon = false;

        // genera le tacche 0–100 ogni 10
        GenerateTicksEvery10(_maxTrack, _tickPrefab);
    }

    // chiamata dal GameFlowManager quando parte il minigioco
    public void StartTemperatureMinigame()
    {
        Debug.Log("Minigioco delle temperature iniziato.");

        if (_sliderGroup != null)
            _sliderGroup.SetActive(true);

        _minCorrect = false;
        _maxCorrect = false;
        _alreadyWon = false;

        ResetHandleStates();

        // MIN attivo, MAX nascosto
        _minSlider.interactable = true;

        _maxSlider.interactable = false;
        _maxSlider.gameObject.SetActive(false);

        // il text mostra il valore attuale del minimo (di solito 0°)
        UpdateTemperatureLabel(_minSlider.value);
    }

    // --------------------------------------------------
    // SLIDER MIN (triangolino rosso)
    // --------------------------------------------------
    public void OnMinSliderChanged(float newValue)
    {
        // se è già stato bloccato perché corretto, ignora
        if (!_minSlider.interactable)
            return;

        // non blocchiamo in base al range: l'utente è libero di scegliere
        _minCorrect = Mathf.Abs(newValue - _correctMinTemperature) <= _tolerance;

        // aggiorna il valore numerico in tempo reale
        UpdateTemperatureLabel(newValue + 5f);

        if (_minCorrect)
        {
            Debug.Log("Temperatura MIN indovinata!");

            // blocca il triangolino rosso alla temperatura trovata
            _minSlider.interactable = false;
            // mostra il triangolino verde alla temperatura corretta

            if (_correctMinHandle != null) 
            {
                // salva la posizione del triangolino rosso
                Vector3 oldPos = _minSlider.handleRect.localPosition;

                // nasconde il triangolino rosso
                _minSlider.handleRect.gameObject.SetActive(false);

                // posiziona il triangolino verde nella stessa posizione
                _correctMinHandle.localPosition = oldPos;

                // mostra il triangolino verde
                _correctMinHandle.gameObject.SetActive(true);
            }

            
            if (_maxSlider != null)
            {
                Debug.Log("Sbloccato slider MAX");
                _maxSlider.gameObject.SetActive(true);
                _maxSlider.interactable = true;

                // parto dal fondo
                _maxSlider.value = _maxSlider.minValue;

                // mi assicuro che il triangolino rosso del MAX sia acceso
                if (_maxSlider.handleRect != null)
                {

                    Debug.Log("Attivato triangolino rosso MAX");
                    _maxSlider.handleRect.gameObject.SetActive(true);

                }
                else
                {

                    Debug.LogWarning("HandleRect del MAX Slider è null!");

                }
            }

        }

        CheckWin();
    }

    // --------------------------------------------------
    // SLIDER MAX (triangolino verde)
    // --------------------------------------------------
    public void OnMaxSliderChanged(float newValue)
    {
        // se non è ancora stato sbloccato (min non trovato), ignora
        if (!_maxSlider.interactable)
            return;


        _maxCorrect = Mathf.Abs(newValue - _correctMaxTemperature) <= _tolerance;

        if(_maxCorrect)

        {
            Debug.Log("Temperatura MAX indovinata!");

            // blocca il triangolino rosso alla temperatura trovata
            _maxSlider.interactable = false;

            if(_correctMaxHandle != null)
            
            {

                // salva la posizione del triangolino rosso
                Vector3 oldPos = _maxSlider.handleRect.localPosition;

                // nasconde il triangolino rosso
                _maxSlider.handleRect.gameObject.SetActive(false);
                // posiziona il triangolino verde nella stessa posizione
                _correctMaxHandle.localPosition = oldPos;

                // mostra il triangolino verde
                _correctMaxHandle.gameObject.SetActive(true);

            }

        
        }

        // il text ora mostra la temperatura massima che stai scegliendo
        UpdateTemperatureLabel(newValue + 5f);
        CheckWin();
    }

    // --------------------------------------------------
    // LOGICA DI VITTORIA
    // --------------------------------------------------
    private void CheckWin()
    {
        if (_alreadyWon)
            return;

        if (_minCorrect && _maxCorrect)
        {

            _alreadyWon = true;
            _temperatureLabel.text = "CORRETTO!";
            Debug.Log("Hai indovinato MIN e MAX!");
            
            OnTemperatureCompleted?.Invoke();
        }
    }

    // --------------------------------------------------
    // LABEL NUMERICA
    // --------------------------------------------------
    private void UpdateTemperatureLabel(float value)
    {
        if (_temperatureLabel != null)
        {
            _temperatureLabel.text = value.ToString("0") + "°C";
        }
    }

    // --------------------------------------------------
    // TACCHETTE OGNI 10 (0–100)
    // --------------------------------------------------
    private void GenerateTicksEvery10(RectTransform track, RectTransform prefab)
    {
        if (track == null || prefab == null) return;

        int tickCount = 11; // 0,10,20,...,100

        for (int i = 0; i < tickCount; i++)
        {
            float normalized = i / 10f;   // 0 → 0.0, 10 → 0.1, ... 100 → 1.0

            RectTransform tick = Instantiate(prefab, track);
            tick.gameObject.SetActive(true);

            Vector2 aMin = tick.anchorMin;
            Vector2 aMax = tick.anchorMax;

            aMin.x = normalized;
            aMax.x = normalized;

            tick.anchorMin = aMin;
            tick.anchorMax = aMax;

            tick.anchoredPosition = new Vector2(tick.anchoredPosition.y, 0f);
        }
    }

    private void ResetHandleStates()
    {
        if (_correctMinHandle != null)
            _correctMinHandle.gameObject.SetActive(false);

        if (_correctMaxHandle != null)
            _correctMaxHandle.gameObject.SetActive(false);

        if (_minSlider != null)
        {
            _minSlider.value = _minSlider.minValue;

            if (_minSlider.handleRect != null)
                _minSlider.handleRect.gameObject.SetActive(true);
        }

        if (_maxSlider != null)
        {
            _maxSlider.value = _maxSlider.minValue;

            if (_maxSlider.handleRect != null)
                _maxSlider.handleRect.gameObject.SetActive(false);
        }
    }
}
