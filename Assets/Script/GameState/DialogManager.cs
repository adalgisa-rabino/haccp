using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogManager : MonoBehaviour
{
    [Header("UI References")]
    public Image actorImage;
    public TextMeshProUGUI actorName;
    public TextMeshProUGUI messageText;
    public RectTransform backgroundBox;

    [Header("Input")]
    public KeyCode advanceKey = KeyCode.E;

    [Header("Typing")]
    [Tooltip("Seconds per character. Example: 0.03 = fast typing.")]
    public float typeSpeed = 0.03f;

    [Tooltip("If true, pressing advanceKey while typing will instantly show the whole line.")]
    public bool allowSkipTyping = true;

    [Header("Optional")]
    [Tooltip("Optional: disable a gameplay controller script while the dialogue is active.")]
    public Behaviour controllerToDisable;

    public event Action OnDialogueEnded;

    public bool IsActive => isActive;

    DialogTrigger.Actor[] currentActors;
    DialogTrigger.Message[] currentMessages;
    Dictionary<int, DialogTrigger.Actor> actorLookup;

    int activeIndex;
    bool isActive;

    Coroutine typingCoroutine;
    bool isTyping;
    string currentFullLine = "";

    void Awake()
    {
        SetVisible(true);
    }

    void Update()
    {
        if (!isActive) return;

        if (Input.GetKeyDown(advanceKey))
        {
            AdvanceOrSkip();
        }
    }

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
        ShowCurrent();
    }

    public void CloseDialogue()
    {
        if (!isActive) return;
        EndDialogue(invokeEvent: false);
    }

    void ShowCurrent()
    {
        if (currentMessages == null || activeIndex < 0 || activeIndex >= currentMessages.Length)
        {
            EndDialogue(invokeEvent: true);
            return;
        }

        ApplyActorForIndex(activeIndex);

        string line = currentMessages[activeIndex].text ?? "";
        StartTyping(line);
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
    }

    void AdvanceOrSkip()
    {
        if (isTyping && allowSkipTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = null;
            isTyping = false;

            if (messageText != null)
                messageText.text = currentFullLine;

            return;
        }

        activeIndex++;

        if (currentMessages == null || activeIndex >= currentMessages.Length)
        {
            EndDialogue(invokeEvent: true);
            return;
        }

        ShowCurrent();
    }

    void EndDialogue(bool invokeEvent)
    {
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

        if (invokeEvent)
            OnDialogueEnded?.Invoke();
    }

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
