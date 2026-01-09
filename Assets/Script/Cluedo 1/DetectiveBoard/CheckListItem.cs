using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ChecklistItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public TextMeshProUGUI infoText;
    public Image redCross;
    public bool puoSegnareConX = true;
    public Image polaroidImage;

    public void Setup(string nome)
    {
        infoText.text = nome;
        if (redCross != null) redCross.enabled = false;

        if (polaroidImage != null)
        {
            // Carica l'immagine dalla cartella Resources/Polaroids
            Sprite loadedSprite = Resources.Load<Sprite>("Polaroids/" + nome);
            if (loadedSprite != null)
            {
                polaroidImage.sprite = loadedSprite;
            }
                else
            {
                Debug.LogWarning("Immagine Polaroid non trovata per: " + nome);
            }
        }


    }

    public void OnPointerDown(PointerEventData eventData) {
        // Se non è abilitato il segno o manca la X, non fare nulla
        if (!puoSegnareConX || redCross == null) return;

        // Inverte lo stato della X (Acceso -> Spento / Spento -> Acceso)
        redCross.enabled = !redCross.enabled;
        
        // La porta in primo piano rispetto ad altri elementi della card
        redCross.transform.SetAsLastSibling(); 
        
        Debug.Log("X impostata su: " + redCross.enabled);
    }

    // Lasciamo OnPointerUp vuoto: così quando rilasci il mouse non succede nulla
    public void OnPointerUp(PointerEventData eventData) {
        // Rimosso ToggleX() da qui
    }

    // Puoi anche eliminare del tutto ToggleX se non lo usi altrove
}