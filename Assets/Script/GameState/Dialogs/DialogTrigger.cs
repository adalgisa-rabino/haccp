using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogTrigger : MonoBehaviour
{
    [Serializable]
    public class Actor
    {
        public string name;
        public int id;
        public Sprite ActorSprite;
    }

    [Serializable]
    public class Message
    {
        public int actorId;
        [TextArea(2, 6)]

        public string text;
    }

    [Header("Dialogue")]
    public Actor[] actors;
    public Message[] messages;

    [Header("Runtime")]
    [Tooltip("Optional: reference to the DialogManager that should display this dialogue.")]
    public DialogManager dialogManager;

    [Tooltip("If true, the dialogue will start automatically in Start() using the data above.")]
    public bool playOnStart;

    void Start()
    {
        //il dialogo parte automaticamente ==> devo 
        if (playOnStart)
            PlayDialogue();
    }

    //per far partire manualmente il dialogo 

    public void PlayDialogue()
    {
        if (dialogManager == null) return;
        if (dialogManager.IsActive) return;

        dialogManager.OpenDialogue(actors, messages);
    }
}
