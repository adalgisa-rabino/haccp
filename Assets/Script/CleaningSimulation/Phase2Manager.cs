using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Attachment;

public class Phase2Manager : MonoBehaviour
{
    [Header("UI & TEMPERATURA")]
    public Slider tempSlider;
    public TMP_Text tempText;
    public float minTarget = 45f;
    public float maxTarget = 60f;

    [Header("DIFFICOLTÀ (CALDAIA)")]
    [Tooltip("Velocità con cui la temperatura scende da sola")]
    public float coolingSpeed = 10f; 
    [Tooltip("Se vero, la temperatura oscilla in modo casuale")]
    public bool enableJitter = true;

    [Header("PULIZIA")]
    public float cleaningSpeed = 0.5f;

    private bool isTempOK = false;

    void Update()
    {
        // 1. GESTIONE CALDAIA (La temperatura cala sempre)
        SimulateBoilerBehavior();

        // 2. CONTROLLO RANGE
        UpdateTemperatureCheck();

        // 3. LOGICA SFREGAMENTO
        if (isTempOK && Input.GetMouseButton(0))
        {
            TryToScrub();
        }
    }

    void SimulateBoilerBehavior()
    {
        if (tempSlider == null) return;

        // Effetto Raffreddamento Costante
        if (tempSlider.value > tempSlider.minValue)
        {
            tempSlider.value -= coolingSpeed * Time.deltaTime;
        }

        // Effetto Jitter (piccoli sbalzi casuali per simulare instabilità)
        if (enableJitter)
        {
            float jitter = Random.Range(-0.5f, 0.5f);
            tempSlider.value += jitter;
        }
    }

    void UpdateTemperatureCheck()
    {
        float currentTemp = tempSlider.value;
        tempText.text = "TEMP ACQUA: " + currentTemp.ToString("F1") + "°C";

        // Se siamo nel range verde
        if (currentTemp >= minTarget && currentTemp <= maxTarget)
        {
            isTempOK = true;
            tempText.color = Color.green;
            tempText.text += " (PRONTA)";
        }
        else
        {
            isTempOK = false;
            tempText.color = Color.red;
            tempText.text += (currentTemp < minTarget) ? " (TROPPO FREDDA)" : " (TROPPO CALDA)";
        }
    }

    void TryToScrub()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("Waste"))
            {
                // Rimpicciolisce lo sporco
                hit.transform.localScale -= Vector3.one * Time.deltaTime * cleaningSpeed;

                // Debug per testare la velocità
                Debug.Log("Pulizia in corso... Scala: " + hit.transform.localScale.x);

                if (hit.transform.localScale.x <= 0.02f)
                {
                    Destroy(hit.transform.gameObject);

                    if(GameObject.FindGameObjectsWithTag("Waste").Length == 0)
                    {
                        Debug.Log("TUTTO PULITO! Passa alla fase successiva.");
                        GameManagerDishWater.Instance.ChangeState(GameManagerDishWater.WashGameState.DisinfectionQuiz);
                    }
                   
                }
            }
        }
    }
}