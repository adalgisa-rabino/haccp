using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class PolaroidData
{
    public string id;        // deve corrispondere al nome / testo dell'indizio
    [TextArea] public string testo;
    public Sprite immagine;
}

public class ChecklistManager : MonoBehaviour
{
    [Header("Riferimenti")]
    public CluedoGameController cluedoController;
    public GameObject prefabCarta;
    public GameObject indizioPrefab;

    [Header("Contenitori (Grid Layout)")]
    public Transform containerColpevoli;
    public Transform containerArmi;
    public Transform containerLuoghi;

    [Header("Dove finiscono gli indizi rivelati")]
    public Transform containerIndiziEsterno; // nuovo: qui vanno gli indizi (al posto di dx)

    [Header("Layout Indizi (se li vuoi sparsi anche lì)")]
    public float margineIndizi = 80f;
    public float rotazioneIndizi = 15f;

    [Header("Dati Polaroid (FISSI)")]
    [SerializeField] private List<PolaroidData> polaroids;

    void OnEnable()
    {
        if (cluedoController != null)
            cluedoController.OnSetupComplete += IniziaPopolamento;

        ClueTarget.OnClueRevealed += AggiungiIndizio;
    }

    void OnDisable()
    {
        if (cluedoController != null)
            cluedoController.OnSetupComplete -= IniziaPopolamento;

        ClueTarget.OnClueRevealed -= AggiungiIndizio;
    }

    void IniziaPopolamento()
    {
        PopolaCategoria(cluedoController.tuttiColpevoli, containerColpevoli);
        PopolaCategoria(cluedoController.tutteArmi, containerArmi);
        PopolaCategoria(cluedoController.tuttiLuoghi, containerLuoghi);
    }

    void PopolaCategoria(List<string> listaNomi, Transform container)
    {
        if (container == null || prefabCarta == null) return;

        foreach (string nome in listaNomi)
        {
            GameObject nuovaCarta = Instantiate(prefabCarta, container);

            float rotazioneCasuale = Random.Range(-5f, 5f);
            nuovaCarta.transform.localRotation = Quaternion.Euler(0, 0, rotazioneCasuale);

            var data = TrovaPolaroid(nome);
            Sprite sprite = data != null ? data.immagine : null;

            nuovaCarta.GetComponent<ChecklistItem>().Setup(nome, sprite);
        }
    }

    PolaroidData TrovaPolaroid(string id)
    {
        foreach (var p in polaroids)
        {
            if (p.id == id)
                return p;
        }

        Debug.LogWarning("Nessuna PolaroidData trovata per id: " + id);
        return null;
    }

    void AggiungiIndizio(Clue indizio)
    {
        if (containerIndiziEsterno == null || indizioPrefab == null) return;

        GameObject nuovaCard = Instantiate(indizioPrefab, containerIndiziEsterno);

        // Se vuoi che gli indizi siano “sparsi” anche nel nuovo posto:
        RectTransform rectCard = nuovaCard.GetComponent<RectTransform>();
        RectTransform rectContainer = containerIndiziEsterno.GetComponent<RectTransform>();

        if (rectCard != null && rectContainer != null)
        {
            rectCard.anchorMin = new Vector2(0.5f, 0.5f);
            rectCard.anchorMax = new Vector2(0.5f, 0.5f);
            rectCard.pivot = new Vector2(0.5f, 0.5f);

            float limiteX = (rectContainer.rect.width / 2) - margineIndizi;
            float limiteY = (rectContainer.rect.height / 2) - margineIndizi;

            float posX = Random.Range(-limiteX, limiteX);
            float posY = Random.Range(-limiteY, limiteY);
            rectCard.anchoredPosition = new Vector2(posX, posY);
        }

        nuovaCard.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-rotazioneIndizi, rotazioneIndizi));

        var data = TrovaPolaroid(indizio.testo);
        Sprite sprite = data != null ? data.immagine : null;

        nuovaCard.GetComponent<ChecklistItem>().Setup(indizio.testo, sprite);
        StartCoroutine(AnimazioneComparsa(nuovaCard.transform));
    }

    IEnumerator AnimazioneComparsa(Transform target)
    {
        float durata = 0.3f;
        float tempo = 0;

        target.localScale = Vector3.zero;

        while (tempo < durata)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / durata;

            float scala = Mathf.Lerp(0, 1.1f, progresso);
            if (progresso > 0.8f) scala = Mathf.Lerp(1.1f, 1.0f, (progresso - 0.8f) * 5);

            target.localScale = new Vector3(scala, scala, 1);
            yield return null;
        }

        target.localScale = Vector3.one;
    }
}
