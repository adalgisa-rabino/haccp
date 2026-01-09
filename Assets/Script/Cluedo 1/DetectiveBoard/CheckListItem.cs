using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ChecklistItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public TextMeshProUGUI infoText;
    public Image redCross;
    public bool puoSegnareConX = true;
    public Image polaroidImage;
    
    public RectTransform clipTransform;

    public void Setup(string nome)
    {
        if (infoText != null)
        {
            infoText.text = nome;
            infoText.rectTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
            
        }
        
        if (redCross != null) redCross.enabled = false;
        else
        {
            Debug.LogWarning("RedCross Image reference is missing in ChecklistItem.");
        }

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

        if (clipTransform != null) 
        {
            // Ruota la puntina in modo casuale tra -15 e 15 gradi
            float rotazionePuntina = Random.Range(-15f, 15f);
            clipTransform.localRotation = Quaternion.Euler(0, 0, rotazionePuntina);
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