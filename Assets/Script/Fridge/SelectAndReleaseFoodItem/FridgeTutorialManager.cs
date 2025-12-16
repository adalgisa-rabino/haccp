using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FridgeTutorialManager : MonoBehaviour
{
    [Header("UI Tutorial")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private GameObject backgroundPanel;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    [Header("Start Tutorial Button (DEVE stare FUORI dal panel)")]
    [SerializeField] private Button tutorialStartButton;

    [Header("Tutorial Items")]
    [SerializeField] private FoodItem healthyItem;
    [SerializeField] private FoodItem expiredItem;
    [SerializeField] private FoodItem unpackagedItem;

    [Header("Stato frigo")]
    [SerializeField] private Fridge1State fridgeState;
    [SerializeField] private GameObject[] objectsToDisableDuringTutorial;

    private int step = 0;
    private bool running = false;

    // step 2: per evitare doppi avanzamenti
    private bool step2Completed = false;

    // step 3: deve fare freeze ON e poi OFF per passare allo step 4
    private bool didFreezeOnStep3 = false;
    private bool didUnfreezeOnStep3 = false;

    private void Start()
    {
        // UI tutorial spenta a Play
        if (panel) panel.SetActive(false);
        if (backgroundPanel) backgroundPanel.SetActive(false);

        if (nextButton) nextButton.gameObject.SetActive(false);
        if (skipButton) skipButton.gameObject.SetActive(false);

        // Il pulsante tutorial deve essere visibile nel gioco (fuori dal panel)
        if (tutorialStartButton) tutorialStartButton.gameObject.SetActive(true);

        // Per sicurezza: nessun item “tutorial-only” deve restare acceso a Play
        SetOnlyItemVisible(null);
    }

    private void OnEnable()
    {
        Selectable.OnAnySelected += OnSelected;
        FridgeItemPositionControl.OnAnySnapped += OnSnapped;
        TrashBinButton.OnTrashAttempt += OnTrashAttempt;
        PackagingStationButton.OnPackageAttempt += OnPackageAttempt;
        FridgeThermometerButton.OnThermometerPressed += OnThermometerPressed;
    }

    private void OnDisable()
    {
        Selectable.OnAnySelected -= OnSelected;
        FridgeItemPositionControl.OnAnySnapped -= OnSnapped;
        TrashBinButton.OnTrashAttempt -= OnTrashAttempt;
        PackagingStationButton.OnPackageAttempt -= OnPackageAttempt;
        FridgeThermometerButton.OnThermometerPressed -= OnThermometerPressed;
    }

    public void StartTutorial()
    {
        Selectable.GlobalInteractionBlocked = false;

        running = true;
        step = 0;

        step2Completed = false;
        didFreezeOnStep3 = false;
        didUnfreezeOnStep3 = false;

        // Nascondi il pulsante tutorial durante il tutorial
        if (tutorialStartButton) tutorialStartButton.gameObject.SetActive(false);

        // Modalità tutorial: temperatura lenta + nessun fail alle soglie (dipende dalla tua implementazione)
        if (fridgeState != null)
            fridgeState.SetTutorialTemperatureMode(true, 0.01f);

        // Disattiva altri oggetti/UI che disturbano (assicurati che NON includa il TutorialStartButton o i suoi parent)
        if (objectsToDisableDuringTutorial != null)
        {
            foreach (var go in objectsToDisableDuringTutorial)
                if (go) go.SetActive(false);
        }

        // Mostra pannello tutorial
        if (panel) panel.SetActive(true);
        if (backgroundPanel) backgroundPanel.SetActive(true);

        if (nextButton) nextButton.gameObject.SetActive(true);
        if (skipButton) skipButton.gameObject.SetActive(true);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextStep);

        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(EndTutorial);

        ShowStep();
    }

    public void EndTutorial()
    {
        running = false;

        // Spegni UI tutorial
        if (panel) panel.SetActive(false);
        if (backgroundPanel) backgroundPanel.SetActive(false);
        if (nextButton) nextButton.gameObject.SetActive(false);
        if (skipButton) skipButton.gameObject.SetActive(false);

        // IMPORTANTISSIMO: spegni gli item mostrati dal tutorial
        SetOnlyItemVisible(null);

        // Riattiva il pulsante tutorial nel gioco
        if (tutorialStartButton) tutorialStartButton.gameObject.SetActive(true);

        // Ripristina modalità normale temperatura
        if (fridgeState != null)
            fridgeState.SetTutorialTemperatureMode(false);

        // Riattiva il resto
        if (objectsToDisableDuringTutorial != null)
        {
            foreach (var go in objectsToDisableDuringTutorial)
                if (go) go.SetActive(true);
        }

        PlayerPrefs.SetInt("FridgeTutorialDone", 1);
        PlayerPrefs.Save();
    }

    private void NextStep()
    {
        step++;
        ShowStep();
    }

    private void SetOnlyItemVisible(FoodItem itemToShow)
    {
        if (healthyItem) healthyItem.gameObject.SetActive(itemToShow != null && itemToShow == healthyItem);
        if (expiredItem) expiredItem.gameObject.SetActive(itemToShow != null && itemToShow == expiredItem);
        if (unpackagedItem) unpackagedItem.gameObject.SetActive(itemToShow != null && itemToShow == unpackagedItem);
    }

    private void ShowStep()
    {
        if (!panel) return;

        // di default “Avanti” nascosto
        if (nextButton) nextButton.gameObject.SetActive(false);

        if (backgroundPanel != null)
            backgroundPanel.SetActive(true);

        switch (step)
        {
            case 0:
                SetOnlyItemVisible(healthyItem);
                SetText(
                    "OBIETTIVO DELLA SFIDA: RIORDINARE IL FRIGORIFERO",
                    "Selezionare il cibo nel frigorifero e spostarlo nel posto giusto del frigo cliccando sul RIPIANO.\n" +
                    "Per non sbagliare bisogna tenere conto delle differenze di temperature nel frigo e delle possibili contaminazioni!\n" +
                    "Sbagliare comporta la perdita di punti HACCPP."
                );
                // Avanza con OnSnapped (healthy corretto)
                break;

            case 1:
                SetOnlyItemVisible(expiredItem);
                SetText(
                    "ATTENZIONE AL CIBO SCADUTO",
                    "Se un cibo è scaduto va buttato: selezionare il cibo e cliccare sul CESTINO nella scena per scartarlo.\n" +
                    "Sbagliare comporta la perdita di punti HACCPP."
                );
                // Avanza con OnTrashAttempt
                break;

            case 2:
                SetOnlyItemVisible(unpackagedItem);
                SetText(
                    "ATTENZIONE AL CIBO NON CONFEZIONATO",
                    "I cibi devono essere confezionati per stare nel frigo: selezionare il cibo non confezionato e cliccare sul CONTENITORE nella scena per impacchettarlo.\n" +
                    "Poi mettilo nel ripiano corretto."
                );
                // Avanza con OnSnapped quando l’item è confezionato (isUnpackaged=false) e posizionato correttamente
                break;

            case 3:
                // IMPORTANTISSIMO: da qui in poi nessun item deve restare visibile
                SetOnlyItemVisible(null);

                // reset flags step3 (nel caso tu ri-entri nello step 3 per qualche motivo)
                didFreezeOnStep3 = false;
                didUnfreezeOnStep3 = false;

                SetText(
                    "ATTENZIONE ALL'AUMENTO DELLA TEMPERATURA",
                    "Se il frigo resta troppo aperto la temperatura aumenta.\n" +
                    "Per bloccare l'aumento di temperatura premi sul TERMOMETRO in scena, poi ripremi per riattivarlo.\n" +
                    "Solo dopo passiamo al prossimo step."
                );
                // NON mostrare avanti: deve fare freeze ON e poi OFF
                break;

            case 4:
                SetOnlyItemVisible(null);
                SetText(
                    "ATTENZIONE AI PUNTI HACCP E... BUONA SFIDA!",
                    "Se i punti HACCP scendono sotto 0 la sfida finisce e si deve ricominciare.\n" +
                    "Per ripetere il tutorial premi il pulsante TUTORIAL."
                );

                // qui chiudiamo con il tasto “Avanti”
                ShowNext(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(EndTutorial);
                break;

            default:
                EndTutorial();
                break;
        }
    }

    private void SetText(string title, string body)
    {
        if (titleText) titleText.text = title;
        if (bodyText) bodyText.text = body;
    }

    private void ShowNext(bool show)
    {
        if (nextButton) nextButton.gameObject.SetActive(show);
    }

    // ===== Eventi =====

    private void OnSelected(Selectable sel)
    {
        if (!running) return;
        // non serve nulla qui
    }

    private void OnSnapped(FoodItem item, FridgeSnapZone zone)
    {
        if (!running) return;

        // STEP 0 — healthy item nel ripiano giusto
        if (step == 0)
        {
            if (item == healthyItem && item.isCorrectlyPlaced)
            {
                step++;
                ShowStep();
            }
            return;
        }

        // STEP 2 — oggetto confezionato (isUnpackaged=false) e in posizione corretta
        if (step == 2 && !step2Completed)
        {
            bool isPackaged = item != null && item.isUnpackaged == false;

            if (item != null && item.isCorrectlyPlaced && isPackaged)
            {
                step2Completed = true;

                // quando usciamo dallo step 2, spegniamo l’unpackaged item
                SetOnlyItemVisible(null);

                step++;
                ShowStep();
            }
            return;
        }
    }

    private void OnTrashAttempt(FoodItem item, bool wasExpired)
    {
        if (!running) return;

        // STEP 1 — butta lo scaduto
        if (step == 1 && item == expiredItem && wasExpired)
        {
            step++;
            ShowStep();
        }
    }

    private void OnPackageAttempt(FoodItem item, bool success)
    {
        if (!running) return;

        // Qui NON facciamo step++ per evitare doppio avanzamento,
        // perché lo step 2 viene completato nello snap quando l'item è confezionato e posizionato.
        if (step == 2 && item == unpackagedItem && success)
        {
            item.SetPackaged(true);
        }
    }

    private void OnThermometerPressed()
    {
        if (!running) return;
        if (step != 3) return;
        if (fridgeState == null) return;

        // stato dopo il toggle (perché il toggle avviene nel button prima dell'evento)
        bool frozen = fridgeState.IsTemperatureFrozen;

        if (frozen)
        {
            didFreezeOnStep3 = true;
            return;
        }

        // se ora è OFF e prima era stato ON -> sequenza completa
        if (!frozen && didFreezeOnStep3)
        {
            didUnfreezeOnStep3 = true;

            // passa a step 4 SOLO dopo ON poi OFF
            if (didFreezeOnStep3 && didUnfreezeOnStep3)
            {
                step++;
                ShowStep();
            }
        }
    }
}
