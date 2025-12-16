using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]

public class Selectable : MonoBehaviour, IPointerDownHandler
{
    [Header("Camera di interazione (fronte frigo)")]
    [SerializeField] private Camera interactionCamera; 

    // Camera usata per convertire coordinate schermo → mondo
    private Camera cam;

    // Posizione dell'oggetto in coordinate schermo al momento del click/inizio drag
    private Vector3 screenPoint;

    // Offset tra il punto cliccato e la posizione dell'oggetto,
    // così l'oggetto non "salta" sotto il cursore ma resta agganciato
    // esattamente dove è stato preso.
    private Vector3 offset;

    private Rigidbody rb;
    private Collider myCol;

    // Trasforn dell'oggetto visivo principale.
    private Transform visualRoot;

    // Eventuale Animator che controlla il modello (idle, animazioni, ecc.)
    private Animator visualAnimator;
    private bool visualAnimatorWasEnabled;

    // Layer in cui si trovano le mensole (per il raycast verticale)
    public LayerMask shelfLayerMask;

    // Layer delle "snap zone" (zone nelle quali l'oggetto può essere agganciato)
    public LayerMask snapZoneLayerMask;

    // Quanto avvicinare l'oggetto alla camera durante il drag (in metri).
    // Utile per tenerlo leggermente "staccato" dallo sfondo.
    public float dragTowardsCamera = 0.1f;

    // Zona evidenziata correntemente (se il puntatore è sopra una FridgeSnapZone)
    private FridgeSnapZone highlightedZone;

    //Boleano per sapere se l'oggetto è stato selezionato
    bool isSelected = false;
    public static Selectable CurrentSelected;

    bool isLocked = false; // nuovo: oggetto “definitivamente posizionato”
    public static bool GlobalInteractionBlocked = false; // per blocco totale gioco

    public static System.Action<Selectable> OnAnySelected;

    private Vector3 originalScale;

    // Posizione/rotazione al momento della selezione (per poter tornare "dov'era")
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Awake()
    {
        // Se non è impostata una camera, usa la main camera
        if (interactionCamera == null)
            interactionCamera = Camera.main;

        cam = interactionCamera;

        rb = GetComponent<Rigidbody>();
        myCol = GetComponent<Collider>();

        // Trova un Renderer figlio per identificare il nodo visivo principale.
        // Se non lo trova, usa direttamente il transform dell'oggetto.
        var rend = GetComponentInChildren<Renderer>();
        visualRoot = rend != null ? rend.transform : transform;

        // Se c'è un Animator associato al modello visivo, lo salviamo.
        visualAnimator = visualRoot.GetComponentInParent<Animator>();

        originalScale = visualRoot.localScale;
    }

    // =====================================================================
    //  POINTER EVENTS (COMPATIBILI CON MOUSE, TOUCH, LIDAR)
    // =====================================================================

    /// <summary>
    /// Chiamato quando il puntatore preme sull'oggetto (mouse down / touch begin / Lidar touch down).
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (GlobalInteractionBlocked) return;
        if (isLocked) return;
        
        if (cam == null) cam = Camera.main;

        // 1. Verifica che il "click" sia davvero su questo oggetto
        Ray ray = cam.ScreenPointToRay(eventData.position);
        if (!Physics.Raycast(ray, out var hit, 100f))
            return; // niente colpito → ignora

        // Se il collider colpito NON è il mio (né un figlio del mio), esco
        if (hit.collider != myCol && !hit.collider.transform.IsChildOf(transform))
            return;

        // 2. A questo punto è davvero un click su di me: gestisco toggle selezione
        Debug.Log($"[Selectable] OnPointerDown su {name} | pos: {eventData.position} | pointerId: {eventData.pointerId}");

        // Se questo oggetto è già selezionato, un secondo tap annulla la selezione
        // e lo riporta alla posa originale (dov'era).
        if (isSelected)
        {
            CancelSelection();
            return;
        }

        // Se un altro Selectable è già selezionato, NON permettiamo la selezione di questo
        if (CurrentSelected != null && CurrentSelected != this)
        {
            Debug.Log($"[Selectable] {CurrentSelected.name} è già selezionato, ignoro click su {name}.");
            return;
        }

        // Se arrivo qui, posso selezionare questo oggetto
        SelectObject(eventData.position);
        isSelected = true;
        CurrentSelected = this;
        OnAnySelected?.Invoke(this);

    }


    /// <summary>
    /// Inizializza il drag: calcola offset e blocca la fisica.
    /// Quando trascino un oggetto 3D usando coordinate dello schermo (mouse, touch, Lidar…), il puntatore è in 2D, ma l’oggetto sta in 3D.
    /// Quindi devo dire a Unity a che distanza dalla camera deve collocare l’oggetto mentre lo seguo con il puntatore.
    /// </summary>
    public void SelectObject(Vector2 screenPos) //Stabilisce il punto dello spazio da cui inizia il trascinamento, così l’oggetto resta “agganciato” esattamente dove hai cliccato.
    {
        Debug.Log($"ObjectSelected");

        if (cam == null) cam = Camera.main;

        // Salvo la posa originale per poterci tornare se la selezione viene annullata
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Converto la posizione word del modello 3D in coordinate schermo, cioè rispetto alla camera
        // e porto l'oggetto un po' avanti verso la camera
        Vector3 pulledForwardPos = visualRoot.position - cam.transform.forward * dragTowardsCamera;
        Vector3 pulledForwardScreen = cam.WorldToScreenPoint(pulledForwardPos);

        // Centro l'oggetto nella vista della camera mantenendo la stessa profondità
        Vector3 centeredScreenPoint = new Vector3(
            Screen.width * 0.5f,
            Screen.height * 0.5f,
            pulledForwardScreen.z);

        Vector3 centeredWorldPos = cam.ScreenToWorldPoint(centeredScreenPoint);

        // Sposto subito l’oggetto nella posizione centrata e avanzata
        transform.position = centeredWorldPos;
        if (visualRoot != null && visualRoot != transform)
            visualRoot.position = centeredWorldPos;

        // Blocca la fisica durante il drag in modo da muovere l'oggetto a mano.
        rb.isKinematic = true;

        // Azzeriamo velocità lineare e angolare.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Se l'oggetto ha un Animator, lo disabilitiamo temporaneamente:
        // evita che l'animazione sposti il modello mentre lo trasciniamo.
        if (visualAnimator != null)
        {
            visualAnimatorWasEnabled = visualAnimator.enabled;
            visualAnimator.enabled = false;
        }

        if (visualRoot != null)
        {
            visualRoot.localScale = originalScale * 1.2f;
        }

        // >>> QUI: mostra pannello oggetto selezionato, se è un FoodItem
        var food = GetComponent<FoodItem>();
        if (food != null && SelectedFoodPanelController.Instance != null)
        {
            SelectedFoodPanelController.Instance.Show(food, this);
        }

   }

    public void ConsumeToTrash()
    {
        Debug.Log($"[Selectable] ConsumeToTrash su {name}");

        // Disattivo la selezione
        isSelected = false;
        if (CurrentSelected == this)
            CurrentSelected = null;

        // Chiudo subito il pannello (evita animazioni che poi si interrompono)
        if (SelectedFoodPanelController.Instance != null)
            SelectedFoodPanelController.Instance.HideImmediate();

        // Stop fisica
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Disabilito collider per evitare interazioni
        if (myCol != null)
            myCol.enabled = false;

        // Disabilito renderers (sparisce)
        var rends = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++)
            rends[i].enabled = false;

        // Disabilito lo script per sicurezza
        this.enabled = false;

        // Se preferisci: puoi proprio disattivare il GO
        // gameObject.SetActive(false);
    }


    /// <summary>
    /// Annulla la selezione e rimette l'oggetto nella posa originale (posizione/rotazione/scala).
    /// </summary>
    private void CancelSelection()
    {
        // Ripristina posa originale
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (visualRoot != null && visualRoot != transform)
        {
            visualRoot.position = originalPosition;
            visualRoot.rotation = originalRotation;
        }

        // Ripristina scala originale
        if (visualRoot != null)
            visualRoot.localScale = originalScale;

        // Riattiva la fisica
        rb.isKinematic = false;

        // Ripristina Animator
        if (visualAnimator != null)
            visualAnimator.enabled = visualAnimatorWasEnabled;

        // Rimuove highlight
        SetHighlightedZone(null);

        isSelected = false;
        if (CurrentSelected == this)
            CurrentSelected = null;

        if (SelectedFoodPanelController.Instance != null)
            SelectedFoodPanelController.Instance.Hide();
    }

    /// <summary>
    /// Termina il drag: prova ad agganciare l'oggetto a una SnapZone sotto il puntatore,
    /// riattiva la fisica e ripristina l'Animator.
    /// </summary>
    public void ReleaseSelectedObjectToZone(FridgeSnapZone zone, Vector2 screenPos)
    {
        Debug.Log("[Selectable] ReleaseSelectedObjectToZone CHIAMATO");

        if (zone == null)
        {
            CancelSelection();
            return;
        }

        if (cam == null) cam = Camera.main;

        // Posizione di snap definita dalla SnapZone + shelf + X del click
        Vector3 snapPos = zone.GetSnapWorldPosition(cam, screenPos, 0.001f);

        // Applichiamo la posizione di snap all'oggetto.
        transform.position = snapPos;

        if (visualRoot != null && visualRoot != transform)
            visualRoot.position = snapPos;

        // Riattiva la fisica
        rb.isKinematic = false;

        // Rimuove highlight
        SetHighlightedZone(null);

        // Ripristina Animator
        if (visualAnimator != null)
            visualAnimator.enabled = visualAnimatorWasEnabled;

        // Ripristina scala originale
        if (visualRoot != null)
            visualRoot.localScale = originalScale;

        isSelected = false;

        if (CurrentSelected == this)
            CurrentSelected = null;

        if (SelectedFoodPanelController.Instance != null)
            SelectedFoodPanelController.Instance.Hide();


        // >>> QUI: notifica logica di gioco Frigo 1 <<<
        var food = GetComponent<FoodItem>();
        if (FridgeItemPositionControl.Instance != null && food != null)
        {
            FridgeItemPositionControl.Instance.OnItemSnapped(food, zone);
        }
    }


    public void ThrowToTrash()
    {
        Debug.Log($"[Selectable] ThrowToTrash su {name}");

        // Disattivo la selezione
        isSelected = false;
        if (CurrentSelected == this)
            CurrentSelected = null;

        // Ripristino scala originale
        if (visualRoot != null)
            visualRoot.localScale = originalScale;

        // Riattivo fisica e gravità in modo che "cada"
        rb.isKinematic = false;
        rb.useGravity = true;

        // impulso leggero verso il basso/avanti, giusto per feedback visivo
        rb.AddForce(Vector3.down * 2f + cam.transform.forward * 1.5f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

        // opzionale: disabilito selezione futura
        this.enabled = false;
    }





    // =====================================================================
    //  GESTIONE SNAP ZONE (HIGHLIGHT + SNAP FINALE)
    // =====================================================================

    /// <summary>
    /// Esegue un raycast dal puntatore per verificare se stiamo "puntando" una SnapZone
    /// e aggiorna l'evidenziazione di conseguenza.
    /// </summary>
    void UpdateSnapZoneHighlight(Vector2 screenPos)
    {
        if (cam == null) cam = Camera.main;

        // Ray dalla camera passando per la posizione del puntatore sullo schermo.
        Ray ray = cam.ScreenPointToRay(screenPos);

        // Raycast limitato al layer delle SnapZone, fino a 10 unità, trigger inclusi.
        if (Physics.Raycast(ray, out RaycastHit hit, 10f, snapZoneLayerMask, QueryTriggerInteraction.Collide))
        {
            // Prova a prendere una FridgeSnapZone dal collider colpito o dai suoi genitori.
            var zone = hit.collider.GetComponent<FridgeSnapZone>()
                       ?? hit.collider.GetComponentInParent<FridgeSnapZone>();

            SetHighlightedZone(zone);
        }
        else
        {
            SetHighlightedZone(null);
        }
    }

    /// <summary>
    /// Imposta la SnapZone evidenziata corrente (attivando/disattivando highlight).
    /// </summary>
    void SetHighlightedZone(FridgeSnapZone zone)
    {
        // Se è la stessa zona di prima, non fare nulla.
        if (highlightedZone == zone)
            return;

        // Disattiva highlight dalla zona precedente, se c'era.
        if (highlightedZone != null)
            highlightedZone.SetHighlighted(false);

        highlightedZone = zone;

        // Attiva highlight sulla nuova zona, se presente.
        if (highlightedZone != null)
            highlightedZone.SetHighlighted(true);
    }

    /// <summary>
    /// Al rilascio, prova ad agganciare l'oggetto a una SnapZone sotto il puntatore.
    /// Regola la Z secondo la SnapZone e la Y in base alla mensola più vicina sotto l'oggetto.
    /// </summary>
    void SnapIntoZoneUnderPointer(Vector2 screenPos)
    {
        if (cam == null) cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f, snapZoneLayerMask, QueryTriggerInteraction.Collide))
        {
            var zone = hit.collider.GetComponent<FridgeSnapZone>()
                       ?? hit.collider.GetComponentInParent<FridgeSnapZone>();

            if (zone == null)
                return;

            // Lasciamo che sia la SnapZone a calcolare la posizione corretta
            Vector3 snapPos = zone.GetSnapWorldPosition(cam, screenPos, 0.001f);

            // (Opzionale) rifinitura sull'asse Y usando la mensola sotto
            Vector3 rayOrigin = snapPos + Vector3.up * 0.5f;
            float maxDistance = 2f;

            if (Physics.Raycast(rayOrigin,
                                Vector3.down,
                                out RaycastHit shelfHit,
                                maxDistance,
                                shelfLayerMask,
                                QueryTriggerInteraction.Ignore))
            {
                snapPos.y = shelfHit.point.y + 0.001f;
            }

            // Applichiamo la posizione di snap all'oggetto
            transform.position = snapPos;

            if (visualRoot != null && visualRoot != transform)
                visualRoot.position = snapPos;
        }
        else
        {
            Debug.Log("[Selectable] Nessuna SnapZone colpita al rilascio.");
        }
    }

    public void LockSelection()
    {
        isLocked = true;
        // opzionale: ripristina scala, disabilita pannello, ecc.
    }

}
