using UnityEngine;
using TMPro; // <--- QUESTA è la libreria fondamentale per TMP

public class DebrisCollector : MonoBehaviour
{
    [Header("Setup Display 2")]
    // Usa TMP_Text perché funziona sia per UI Canvas che per testi 3D
    public TMP_Text statusText; 
    
    // Variabili interne per il conteggio
    private int totalDebris;
    private int currentDebris;

    void Start()
    {
        // 1. Cerca e conta tutti gli oggetti che hanno il tag "Debris" nella scena
        GameObject[] debrisObjects = GameObject.FindGameObjectsWithTag("Waste");
        
        // Imposta i totali
        totalDebris = debrisObjects.Length;
        currentDebris = totalDebris;

        // Aggiorna subito il testo iniziale
        UpdateUI();
    }

    // Questa funzione scatta quando qualcosa entra nel Trigger (il pavimento invisibile)
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se l'oggetto che è caduto è sporcizia (ha il tag "Debris")
        if (other.CompareTag("Waste"))
        {
            // Riduci il conteggio
            currentDebris--;

            // Distruggi l'oggetto fisico (simula la raccolta/eliminazione)
            Destroy(other.gameObject);

            // Aggiorna lo schermo
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        // Controllo di sicurezza: se hai dimenticato di collegare il testo, non dà errore
        if (statusText != null)
        {

            if (currentDebris > 0)
            {
                statusText.text = "RESIDUI DA RIMUOVERE: " + currentDebris + " / " + totalDebris;
                statusText.color = Color.red; // Rosso finché c'è sporco
            }
            else
            {
                statusText.text = "FASE MECCANICA COMPLETATA!";
                statusText.color = Color.green; // Verde quando hai finito

                GameManagerDishWater.Instance.ChangeState(GameManagerDishWater.WashGameState.WaterOpening);
            }
        }
    }
}