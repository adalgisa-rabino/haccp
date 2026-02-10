using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class Question
{
    public string testoDomanda;
    public string[] risposte; // 4 opzioni
    public int indiceRispostaCorretta; // da 0 a 3
}

public class QuizManager : MonoBehaviour
{
    public Question[] databaseDomande; // Riempi questo nell'Inspector con la dispensa
    private List<Question> domandeRimaste;
    private Question domandaCorrente;

    [Header("UI Elements (Display 2)")]
    public TMP_Text testoDomanda;
    public Button[] bottoniRisposta;
    public GameObject pannelloVittoria;

    void Start()
    {
        domandeRimaste = new List<Question>(databaseDomande);
        ProssimaDomanda();
    }

    public void ProssimaDomanda()
    {
        if (domandeRimaste.Count > 0)
        {
            // Scelta casuale (Random)
            int index = Random.Range(0, domandeRimaste.Count);
            domandaCorrente = domandeRimaste[index];
            
            // Aggiorna UI
            testoDomanda.text = domandaCorrente.testoDomanda;
            for (int i = 0; i < bottoniRisposta.Length; i++)
            {
                bottoniRisposta[i].GetComponentInChildren<TMP_Text>().text = domandaCorrente.risposte[i];
                int bi = i; // Closure per il bottone
                bottoniRisposta[i].onClick.RemoveAllListeners();
                bottoniRisposta[i].onClick.AddListener(() => ControllaRisposta(bi));
            }
            domandeRimaste.RemoveAt(index);
        }
        else
        {
            VittoriaFinale();
        }
    }

    void ControllaRisposta(int indiceScelto)
    {
        if (indiceScelto == domandaCorrente.indiceRispostaCorretta)
        {
            Debug.Log("Esatto!");
            ProssimaDomanda();
        }
        else
        {
            Debug.Log("Sbagliato! Ricomincia la sanificazione.");
            // Qui potresti resettare la Fase 2 o togliere punti
        }
    }

    void VittoriaFinale()
    {
        testoDomanda.text = "CERTIFICAZIONE HACCP OTTENUTA!";
        pannelloVittoria.SetActive(true);
    }
}