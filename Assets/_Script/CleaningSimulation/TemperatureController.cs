using UnityEngine;
using UnityEngine.UI;

public class TemperatureController : MonoBehaviour
{
    public System.Action OnTemperatureCompleted;

    [Header("Valori da indovinare")]
    [SerializeField] private float _correctMinTemperature = 45f;
    [SerializeField] private float _correctMaxTemperature = 60f;
    [SerializeField] private float _tolerance = 0.5f;

    [Header("UI Slider")]
    [SerializeField] private Slider _minSlider;       // Slider Tmin
    [SerializeField] private Slider _maxSlider;       // Slider Tmax
    [SerializeField] private GameObject _sliderGroup; // Pannello che contiene i due slider

    [Header("Valori numerici")]
    [SerializeField] private Text _minValueLabel;     // Testo vicino al termometro min
    [SerializeField] private Text _maxValueLabel;     // Testo vicino al termometro max

    [Header("Tick per Tmax")]
    [SerializeField] private RectTransform _tickPrefab; // Prefab della tacca (figlio di Background)
    [SerializeField] private RectTransform _maxTrack;   // RectTransform di Background

    private bool _minCorrect = false;
    private bool _maxCorrect = false;
    private bool _alreadyWon = false;

    private void Start()
    {
        if (_sliderGroup != null)
            _sliderGroup.SetActive(false);

        _minCorrect = false;
        _maxCorrect = false;
        _alreadyWon = false;

        // genera le tacche ogni 10 sulla barra di Tmax
        GenerateTicksEvery10(_maxTrack, _tickPrefab);

        UpdateLabels();
    }

    // chiamato dal GameFlowManager quando parte il minigioco
    public void StartTemperatureMinigame()
    {
        Debug.Log("Minigioco delle temperature iniziato.");

        if (_sliderGroup != null)
            _sliderGroup.SetActive(true);

        _minCorrect = false;
        _maxCorrect = false;
        _alreadyWon = false;

        // Prima l'utente sceglie la temperatura minima
        _minSlider.interactable = true;
        _maxSlider.interactable = false; // si sblocca solo dopo che Tmin è corretta

        UpdateLabels();
    }

    // --- SLIDER MIN ---

    public void OnMinSliderChanged(float newValue)
    {
        // opzionale: garantisci che Tmin non superi Tmax
        if (newValue > _maxSlider.value)
        {
            _minSlider.value = _maxSlider.value;
            newValue = _minSlider.value;
        }

        _minCorrect = Mathf.Abs(newValue - _correctMinTemperature) <= _tolerance;

        // appena l'utente ha trovato una Tmin corretta, permetti di scegliere Tmax
        if (_minCorrect && !_maxSlider.interactable)
        {
            Debug.Log("Temperatura MIN corretta, ora scegli la MAX.");
            _maxSlider.interactable = true;
        }

        UpdateLabels();
        CheckWin();
    }

    // --- SLIDER MAX ---

    public void OnMaxSliderChanged(float newValue)
    {
        // opzionale: Tmax non può scendere sotto Tmin
        if (newValue < _minSlider.value)
        {
            _maxSlider.value = _minSlider.value;
            newValue = _maxSlider.value;
        }

        _maxCorrect = Mathf.Abs(newValue - _correctMaxTemperature) <= _tolerance;

        UpdateLabels();
        CheckWin();
    }

    // --- LOGICA DI VITTORIA ---

    private void CheckWin()
    {
        if (_alreadyWon) return;

        if (_minCorrect && _maxCorrect)
        {
            _alreadyWon = true;
            Debug.Log("Hai indovinato MIN e MAX!");
            OnTemperatureCompleted?.Invoke();
        }
    }

    // --- LABEL NUMERICHE ---

    private void UpdateLabels()
    {
        if (_minValueLabel != null)
        {
            _minValueLabel.text = _minSlider.value.ToString("0") + "°C";
            _minValueLabel.color = _minCorrect ? Color.green : Color.white;
        }

        if (_maxValueLabel != null)
        {
            _maxValueLabel.text = _maxSlider.value.ToString("0") + "°C";
            _maxValueLabel.color = _maxCorrect ? Color.green : Color.white;
        }
    }

    // --- TACCHE OGNI 10 ---

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

            tick.anchoredPosition = new Vector2(0f, tick.anchoredPosition.y);
        }
    }
}
