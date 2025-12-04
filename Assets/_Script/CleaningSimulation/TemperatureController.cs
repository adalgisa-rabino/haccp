using UnityEngine;
using UnityEngine.UI;

public class TemperatureController : MonoBehaviour
{
    public System.Action OnTemperatureCompleted;

    [Header("Valori da indovinare")]
    [SerializeField] private float _correctMinTemperature = 45f;
    [SerializeField] private float _correctMaxTemperature = 60f;
    [SerializeField] private float _tolerance = 0.5f;

    [Header("UI")]
    [SerializeField] private Slider _minSlider;      // TMin
    [SerializeField] private Slider _maxSlider;      // TMax
    [SerializeField] private GameObject _sliderGroup; // l’oggetto che contiene i due slider (ad es. "Hand Washing Bar")

    private bool _minLocked = false;
    private bool _maxLocked = false;
    public bool win = false;

    private void Start()
    {
        // All’inizio il minigioco è nascosto
        if (_sliderGroup != null)
            _sliderGroup.SetActive(false);

        _minLocked = false;
        _maxLocked = false;
    }

    // Chiamata quando premi il rubinetto
    public void StartTemperatureMinigame()
    {
        Debug.Log("Minigioco delle temperature iniziato.");

        if (_sliderGroup != null)
            _sliderGroup.SetActive(true);

        _minLocked = false;
        _maxLocked = false;

        _minSlider.interactable = true;
        _maxSlider.interactable = true;
    }

    // --- SLIDER MIN ---

    public void OnMinSliderChanged(float newValue)
    {
        if (_minLocked) return;

        // Evita che la maniglia min superi quella max
        if (newValue > _maxSlider.value)
        {
            _minSlider.value = _maxSlider.value;
            newValue = _minSlider.value;
        }

        // Controllo indovinato
        if (Mathf.Abs(newValue - _correctMinTemperature) <= _tolerance)
        {
            _minLocked = true;
            _minSlider.interactable = false;
            Debug.Log("Temperatura MIN indovinata!");
            CheckWin();
        }
    }

    // --- SLIDER MAX ---

    public void OnMaxSliderChanged(float newValue)
    {
        if (_maxLocked) return;

        // Evita che la maniglia max scenda sotto quella min
        if (newValue < _minSlider.value)
        {
            _maxSlider.value = _minSlider.value;
            newValue = _maxSlider.value;
        }

        // Controllo indovinato
        if (Mathf.Abs(newValue - _correctMaxTemperature) <= _tolerance)
        {
            _maxLocked = true;
            _maxSlider.interactable = false;
            Debug.Log("Temperatura MAX indovinata!");
            CheckWin();
        }
    }

    private void CheckWin()
    {
        if (_minLocked && _maxLocked)
        {
            Debug.Log("Hai indovinato MIN e MAX!");
            OnTemperatureCompleted?.Invoke();
    
            
        }
    }
}
