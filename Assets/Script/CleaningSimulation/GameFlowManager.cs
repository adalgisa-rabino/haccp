using TMPro;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{

    public enum WashGameState
    {
        FoodWasteRemoval,   // scarti sul piano
        FaucetTouchToStart, // tocco rubinetto per iniziare temp
        TemperatureGame,    // slider min/max
        WaterRunning,       // acqua aperta
        HandWashing,        // lavaggio mani
        SurfaceCleaning,    // pulizia piano di lavoro
        DisinfectionQuiz,   // quiz finale
        Finished            // fine
    }
    public static GameFlowManager Instance { get; private set; }

    [SerializeField] private WashGameState _currentState = WashGameState.FoodWasteRemoval;

    [Header("Controllers")]
    [SerializeField] private FoodWasteController _wasteController;
    [SerializeField] private TemperatureController _temperatureController;
    //[SerializeField] private HandWashController _handWashController;
   //[SerializeField] private SurfaceCleaningController _surfaceController;
    //[SerializeField] private QuizController _quizController;

    [Header("Animators")]
    [SerializeField] private Animator _faucetAnimator;
    [SerializeField] private Animator _waterAnimator;

    [Header("UI")]
    [SerializeField] private GameObject _temperatureUI;
    [SerializeField] private GameObject _handHintUI;
    //[SerializeField] private GameObject _surfaceUI;
    [SerializeField] private GameObject _quizUI;
    [SerializeField] private GameObject _timerUI;
    [SerializeField] private TextMeshProUGUI _timerLabel;

    [Header("Timer and Scoring")]
    private float _temperatureTimer = 0f;
    private bool _temperatureTimerRunning = false;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // ALL'INIZIO: solo la fase scarti è attiva
        _temperatureUI.SetActive(false);
        if (_timerUI != null)
            _timerUI.SetActive(false);
        UpdateTimerLabel();
        // _handHintUI.SetActive(false);
        //_surfaceUI.SetActive(false);
        //_quizUI.SetActive(false);

        // registriamo callback
        _wasteController.OnAllWasteRemoved += OnFoodWasteRemoved;
        _temperatureController.OnTemperatureCompleted += OnTemperatureSolved;
        //_handWashController.OnHandWashCompleted = OnHandWashCompleted;
        //_surfaceController.OnSurfaceCleaned += OnSurfaceCleaned;
        //_quizController.OnQuizCompleted = OnQuizCompleted;

        _currentState = WashGameState.FoodWasteRemoval;
    }

    private void Update()
    {
        // Timer per il minigioco della temperatura
        if (_temperatureTimerRunning)
        {
            _temperatureTimer += Time.deltaTime;
            UpdateTimerLabel();
            //Debug.Log("Timer temperatura: " + _temperatureTimer);
        }
    }

    // --------------------------------------------------
    // 1) SCARTI RIMOSSI
    // --------------------------------------------------
    private void OnFoodWasteRemoved()
    {
        Debug.Log("Scarti rimossi → Tocca il rubinetto");
        _currentState = WashGameState.FaucetTouchToStart;
    }

    // --------------------------------------------------
    // 2) TOCCO RUBINETTO (viene chiamato dal tuo script pointer)
    // --------------------------------------------------
    public void OnFaucetTouched()
    {
        Debug.Log("Rubinetto toccato in stato: " + _currentState);

        if (_currentState == WashGameState.FaucetTouchToStart)
        {
            StartTemperatureGame();
        }
        else if (_currentState == WashGameState.WaterRunning)
        {
            StopWaterAndStartWashing();
        }
    }

    private void StartTemperatureGame()
    {
        Debug.Log("Avvio minigioco temperatura");
        _currentState = WashGameState.TemperatureGame;

        _temperatureUI.SetActive(true);
        _temperatureController.StartTemperatureMinigame();
        //faccio partire un timer e collego al calcolo dei punti; 

        _temperatureTimer = 0f;
        _temperatureTimerRunning = true;
        if (_timerUI != null)
            _timerUI.SetActive(true);
        UpdateTimerLabel();
        


    }

    // --------------------------------------------------
    // 3) COMPLETATA TEMPERATURA
    // --------------------------------------------------
    private void OnTemperatureSolved()
    {
        if (_currentState != WashGameState.TemperatureGame) return;

        Debug.Log("Temperatura corretta → apri acqua");
        _temperatureUI.SetActive(false);
        _temperatureTimerRunning = false;
        UpdateTimerLabel();
        Debug.Log("Tempo impiegato per temperatura: " + _temperatureTimer + " secondi");
        

        _faucetAnimator.SetBool("Open", true);

        _currentState = WashGameState.WaterRunning;
        
    }

    // --------------------------------------------------
    // 4) UTENTE RITOCCA RUBINETTO PER CHIUDERE L’ACQUA
    // --------------------------------------------------
    private void StopWaterAndStartWashing()
    {
        Debug.Log("Chiudo acqua → lavaggio superficie");
        _faucetAnimator.SetBool("Open", false);

        _currentState = WashGameState.SurfaceCleaning;

        //_surfaceController.StartSurfaceCleaning();
    }

    // --------------------------------------------------
    // 5) SUPERFICIE PULITA
    // --------------------------------------------------

    private void OnSurfaceCleaned()
    {
        Debug.Log("Superficie pulita → quiz disinfezione");
        

        _currentState = WashGameState.DisinfectionQuiz;

        _quizUI.SetActive(true);
        //_quizController.StartQuiz();
        
    }

    // --------------------------------------------------
    // 6) QUIZ COMPLETATO
    // --------------------------------------------------
    private void OnQuizCompleted()
    {
        Debug.Log("Quiz completato → processo finito");
        _quizUI.SetActive(false);

        _currentState = WashGameState.Finished;
    }
    
    public WashGameState GetCurrentState()
    {
        return _currentState;
    }

    private void UpdateTimerLabel()
    {
        if (_timerLabel == null)
            return;

        float totalSeconds = Mathf.Max(0f, _temperatureTimer);
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        int centiseconds = Mathf.FloorToInt((totalSeconds - Mathf.Floor(totalSeconds)) * 100f);

        _timerLabel.text = $"{minutes:00}:{seconds:00}.{centiseconds:00}";
    }
}
