using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // Importa il namespace per il VideoPlayer
using System.Collections.Generic;

public class DialogManager : MonoBehaviour
{
    [Header("UI References")]
    public Image actorImage;
    public TextMeshProUGUI actorName;
    public TextMeshProUGUI messageText;
    public RectTransform backgroundBox;
    public Button startGameButton;
    public Button nextDialogButton;

    [Header("Input")]
    public KeyCode advanceKey = KeyCode.E;

    [Header("Video Player Reference")]
    public VideoPlayer displayTwoVideoPlayer; // Riferimento al componente VideoPlayer

    [Header("Typing")]
    [Tooltip("Seconds per character. Example: 0.03 = fast typing.")]
    public float typeSpeed = 0.03f;

    [Tooltip("If true, pressing advanceKey while typing will instantly show the whole line.")]
    public bool allowSkipTyping = true;

    [Header("Optional")]
    [Tooltip("Optional: disable a gameplay controller script while the dialogue is active.")]
    public Behaviour controllerToDisable;

    public bool startGame = false;

    public bool IsActive => isActive;

    DialogTrigger.Actor[] currentActors;
    DialogTrigger.Message[] currentMessages;
    Dictionary<int, DialogTrigger.Actor> actorLookup;

    int activeIndex;
    bool isActive;

    Coroutine typingCoroutine;
    bool isTyping;
    string currentFullLine = "";

    private bool buttonPressed = false;

    public void Start()
    {
    }

    void Awake()
    {
        SetVisible(true);

        if (displayTwoVideoPlayer != null)
        {
            displayTwoVideoPlayer.Stop(); // Assicurati che il video sia fermo all'avvio
            Debug.Log("VideoPlayer fermato all'avvio.");
        }

        /*

        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(false); // Disattiva il bottone all'avvio
            Debug.Log("startGameButton assegnato correttamente.");
        }
        else
        {
            Debug.LogWarning("startGameButton non è stato assegnato!");
        }
        */

        /*

        if (nextDialogButton != null)
            nextDialogButton.onClick.AddListener(OnNextDialogButtonPressed);
        */
    }

    void Update()
    {
        if (!isActive) return;

        /*devo modificarlo inserendo la presenza di un bottone 

        if (Input.GetKeyDown(advanceKey))
        {
            AdvanceOrSkip(forceSkipTyping: false);
        }
        */
    }

// logica se voglio il bottone per andare avanti 
    /*

    public void OnNextDialogButtonPressed()
    {
        if (buttonPressed) return; // Evita chiamate multiple
        buttonPressed = true;

        Debug.Log($"OnNextDialogButtonPressed called. isTyping: {isTyping}, activeIndex: {activeIndex}");

        if (isTyping)
        {
            ShowFullCurrentLine();
        }
        else
        {
            AdvanceOrSkip(forceSkipTyping: false);
        }

        // Riabilita il bottone dopo un frame
        StartCoroutine(ResetButtonPress());
    }

    private IEnumerator ResetButtonPress()
    {
        yield return null; // Aspetta un frame
        buttonPressed = false;
    }

    */

    //inizio il dialogo 
    public void OpenDialogue(DialogTrigger.Actor[] actors, DialogTrigger.Message[] messages)
    {
        if (messages == null || messages.Length == 0)
        {
            Debug.LogWarning("DialogManager.OpenDialogue called with empty messages.");
            return;
        }

        currentActors = actors;
        currentMessages = messages;
        actorLookup = BuildActorLookup(actors);

        activeIndex = 0;
        isActive = true;

        if (controllerToDisable != null)
            controllerToDisable.enabled = false;

        SetVisible(true);
        //if (nextDialogButton != null) nextDialogButton.gameObject.SetActive(true);
        ShowCurrent();
    }

    public void CloseDialogue()
    {
        if (!isActive) return;
        EndDialogue();
    }

    void ShowCurrent()
    {
        if (currentMessages == null || activeIndex < 0 || activeIndex >= currentMessages.Length)
        {
            EndDialogue();
            return;
        }

        ApplyActorForIndex(activeIndex);

        string line = currentMessages[activeIndex].text ?? "";
        StartTyping(line);

        // Avvia il video al quarto messaggio (indice 3)
        if (activeIndex == 3)
        {
            PlayDisplay2Video();
        }
    }

    void PlayDisplay2Video()
    {
        if (displayTwoVideoPlayer != null)
        {
            displayTwoVideoPlayer.Stop(); // Ferma il video se è già in riproduzione
            displayTwoVideoPlayer.time = 0; // Resetta il tempo del video
            displayTwoVideoPlayer.Play(); // Avvia il video da capo
            Debug.Log("Video del display 2 avviato da capo.");
        }
        else
        {
            Debug.LogWarning("Il VideoPlayer per il display 2 non è stato assegnato.");
        }
    }

    void ApplyActorForIndex(int index)
    {
        if (currentActors != null && currentActors.Length > 0)
        {
            var a = GetActorForMessage(index);

            if (actorName != null) actorName.text = a != null ? a.name : "";
            if (actorImage != null)
            {
                actorImage.sprite = a != null ? a.ActorSprite : null;
                actorImage.enabled = (actorImage.sprite != null);
            }
        }
        else
        {
            if (actorName != null) actorName.text = "";
            if (actorImage != null)
            {
                actorImage.sprite = null;
                actorImage.enabled = false;
            }
        }
    }

    void StartTyping(string line)
    {
        currentFullLine = line;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;

        if (messageText != null)
            messageText.text = "";

        for (int i = 0; i < line.Length; i++)
        {
            if (messageText != null)
                messageText.text += line[i];

            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        typingCoroutine = null;

        // Avanza automaticamente al prossimo messaggio dopo un breve ritardo
        yield return new WaitForSeconds(1f); // Attendi 1 secondo prima di avanzare
        AdvanceOrSkip();
    }

    void AdvanceOrSkip()
    {
        if (isTyping)
        {
            /*if (forceSkipTyping || allowSkipTyping)
            {
                ShowFullCurrentLine();
                return;
            }

            // Se non si può saltare la digitazione, ignoro l'input finché non ha finito.
            return;

            */

            ShowFullCurrentLine();
                return;
        }

        activeIndex++;

        if (currentMessages == null || activeIndex >= currentMessages.Length)
        {
            startGame = true;
            EndDialogue();
            return;
        }

        ShowCurrent();
    }

    void ShowFullCurrentLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = null;
        isTyping = false;

        if (messageText != null)
        {
            messageText.text = currentFullLine;
        }

        /*
        if (backgroundBox != null)
        {
            // Adjust the size of the background box to fit the full text
            //LayoutRebuilder.ForceRebuildLayoutImmediate(messageText.rectTransform);
            //backgroundBox.sizeDelta = new Vector2(backgroundBox.sizeDelta.x, messageText.rectTransform.sizeDelta.y + 20); // Add padding if needed
        }
        */
    }

    void EndDialogue()
    {
        Debug.Log(startGame);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = null;
        isTyping = false;
        currentFullLine = "";

        isActive = false;
        SetVisible(false);

        if (controllerToDisable != null)
            controllerToDisable.enabled = true;

        currentActors = null;
        currentMessages = null;
        actorLookup = null;
        activeIndex = 0;

        if (startGame)
        {
            startGameButton.gameObject.SetActive(true);

            backgroundBox.gameObject.SetActive(false);
            messageText.gameObject.SetActive(false);
            actorName.gameObject.SetActive(false);
            actorImage.gameObject.SetActive(false);
            if (nextDialogButton != null) nextDialogButton.gameObject.SetActive(false);

            // Ferma il video al termine del dialogo
            if (displayTwoVideoPlayer != null)
            {
                displayTwoVideoPlayer.Stop();
                Debug.Log("Video del display 2 fermato al termine del dialogo.");
            }
        }  
    }

    //visibilità del dialogo
    void SetVisible(bool visible)
    {
        if (backgroundBox != null) backgroundBox.gameObject.SetActive(visible);
        if (actorName != null) actorName.gameObject.SetActive(visible);
        if (messageText != null) messageText.gameObject.SetActive(visible);
        if (actorImage != null) actorImage.gameObject.SetActive(visible);
    }

    DialogTrigger.Actor GetActorForMessage(int messageIndex)
    {
        if (currentMessages != null &&
            messageIndex >= 0 &&
            messageIndex < currentMessages.Length)
        {
            int actorId = currentMessages[messageIndex]?.actorId ?? -1;

            if (actorLookup != null && actorLookup.TryGetValue(actorId, out var actorById))
                return actorById;
        }

        // Fallback to previous index-based behaviour for compatibility.
        int actorIdx = Mathf.Clamp(messageIndex, 0, currentActors.Length - 1);
        return currentActors[actorIdx];
    }

    Dictionary<int, DialogTrigger.Actor> BuildActorLookup(DialogTrigger.Actor[] actors)
    {
        if (actors == null || actors.Length == 0)
            return null;

        var map = new Dictionary<int, DialogTrigger.Actor>();
        for (int i = 0; i < actors.Length; i++)
        {
            var actor = actors[i];
            if (actor == null) continue;

            if (!map.ContainsKey(actor.id))
                map[actor.id] = actor;
        }

        return map;
    }


}
