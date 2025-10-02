using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class KitchenMapUI : MonoBehaviour
{
    [Serializable]
    public class AreaRefs
    {
        // Punto di arrivo nel MONDO (Empty/marker): usato per posizione + yaw del player
        public GameObject bodyGO;

        // Accessor comodo al Transform (null se non assegnato)
        public Transform Body => bodyGO ? bodyGO.transform : null;
    }

    [Header("Player (trascina il GO che ha PlayerMovement)")]
    [SerializeField] private PlayerMovement player;   // Riferimento allo script di movimento del player

    [Header("Aree (GameObject nel MONDO, NON sotto Canvas)")]
    [SerializeField] private List<AreaRefs> areas = new();  // Lista dei punti di teletrasporto

    private void Awake()
    {
        // Se non assegnato in Inspector, prova a trovarlo:
        // 1) GameObject con tag "Player" → PlayerMovement
        // 2) Primo PlayerMovement trovato in scena
        if (player == null)
        {
            var tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged) player = tagged.GetComponent<PlayerMovement>();
            if (player == null) player = FindFirstObjectByType<PlayerMovement>();
        }
    }

    /// <summary>
    /// Teletrasporta il player all'area indicata (usa SOLO il body; la view è ignorata).
    /// Collega questo metodo ai pulsanti della mappa (OnClick).
    /// </summary>
    public void GoToArea(int index)
    {
        // Controllo minimo sull'indice
        if (index < 0 || index >= areas.Count) return;
        if (player == null) return;

        var targetBody = areas[index]?.Body;
        if (targetBody == null) return;

        // Chiama il tuo PlayerMovement: passiamo NULL come view perché la ignoriamo
        player.ApplyPose(targetBody, null);
    }

#if UNITY_EDITOR
    // (Facoltativo) Piccolo aiuto in Editor: avvisa se dimentichi il bodyGO
    private void OnValidate()
    {
        if (areas == null) return;
        for (int i = 0; i < areas.Count; i++)
        {
            if (areas[i] != null && areas[i].bodyGO == null)
            {
                Debug.LogWarning($"[KitchenMapUI] bodyGO mancante per area {i}", this);
            }
        }
    }
#endif
}
