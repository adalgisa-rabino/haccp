using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ChecklistItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public TextMeshProUGUI nomeTesto;
    public Image croceRossa;
    public bool puoSegnareConX = true;

    public void Setup(string nome) {
        nomeTesto.text = nome;
        if (croceRossa != null) croceRossa.enabled = false;
        
        // Se è un indizio, adatta l'altezza (aggiungi qui la chiamata se serve)
        // if (!puoSegnareConX) AdattaAltezzaPostIt(); 
    }

    public void OnPointerDown(PointerEventData eventData) {
        // Se non è abilitato il segno o manca la X, non fare nulla
        if (!puoSegnareConX || croceRossa == null) return;

        // Inverte lo stato della X (Acceso -> Spento / Spento -> Acceso)
        croceRossa.enabled = !croceRossa.enabled;
        
        // La porta in primo piano rispetto ad altri elementi della card
        croceRossa.transform.SetAsLastSibling(); 
        
        Debug.Log("X impostata su: " + croceRossa.enabled);
    }

    // Lasciamo OnPointerUp vuoto: così quando rilasci il mouse non succede nulla
    public void OnPointerUp(PointerEventData eventData) {
        // Rimosso ToggleX() da qui
    }

    // Puoi anche eliminare del tutto ToggleX se non lo usi altrove
}