//using UnityEngine;
//using UnityEngine.EventSystems;

//[RequireComponent(typeof(Collider))]
//[RequireComponent(typeof(Rigidbody))]

//public class DraggableBasic : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
//{
//    // Camera usata per convertire coordinate schermo → mondo
//    private Camera cam;

//    // Posizione dell'oggetto in coordinate schermo al momento del click/inizio drag
//    private Vector3 screenPoint;

//    // Offset tra il punto cliccato e la posizione dell'oggetto,
//    // così l'oggetto non "salta" sotto il cursore ma resta agganciato
//    // esattamente dove è stato preso.
//    private Vector3 offset;

//    private Rigidbody rb;
//    private Collider myCol;

//    // Trasforn dell'oggetto visivo principale.
//    private Transform visualRoot;

//    // Eventuale Animator che controlla il modello (idle, animazioni, ecc.)
//    private Animator visualAnimator;
//    private bool visualAnimatorWasEnabled;

//    // Layer in cui si trovano le mensole (per il raycast verticale)
//    public LayerMask shelfLayerMask;

//    // Layer delle "snap zone" (zone nelle quali l'oggetto può essere agganciato)
//    public LayerMask snapZoneLayerMask;

//    // Quanto avvicinare l'oggetto alla camera durante il drag (in metri).
//    // Utile per tenerlo leggermente "staccato" dallo sfondo.
//    public float dragTowardsCamera = 0.1f;

//    // Zona evidenziata correntemente (se il puntatore è sopra una FridgeSnapZone)
//    private FridgeSnapZone highlightedZone;

//    void Awake()
//    {
//        // Se non è impostata una camera, usa la main camera
//        cam = Camera.main;

//        rb = GetComponent<Rigidbody>();
//        myCol = GetComponent<Collider>();

//        // Trova un Renderer figlio per identificare il nodo visivo principale.
//        // Se non lo trova, usa direttamente il transform dell'oggetto.
//        var rend = GetComponentInChildren<Renderer>();
//        visualRoot = rend != null ? rend.transform : transform;

//        // Se c'è un Animator associato al modello visivo, lo salviamo.
//        visualAnimator = visualRoot.GetComponentInParent<Animator>();
//    }

//    // =====================================================================
//    //  POINTER EVENTS (COMPATIBILI CON MOUSE, TOUCH, LIDAR)
//    // =====================================================================

//    /// <summary>
//    /// Chiamato quando il puntatore preme sull'oggetto (mouse down / touch begin / Lidar touch down).
//    /// </summary>
//    public void OnPointerDown(PointerEventData eventData)
//    {
//        BeginDrag(eventData.position);
//    }

//    /// <summary>
//    /// Chiamato ad ogni frame in cui il puntatore si muove con il pulsante/dito premuto.
//    /// </summary>
//    public void OnDrag(PointerEventData eventData)
//    {
//        ContinueDrag(eventData.position);
//    }

//    /// <summary>
//    /// Chiamato quando il puntatore viene rilasciato (mouse up / touch end / Lidar touch up).
//    /// </summary>
//    public void OnPointerUp(PointerEventData eventData)
//    {
//        EndDrag(eventData.position);
//    }

//    // =====================================================================
//    //  LOGICA COMUNE DI DRAG (INDIPENDENTE DALLA SORGENTE: MOUSE / LIDAR)
//    // =====================================================================

//    /// <summary>
//    /// Inizializza il drag: calcola offset e blocca la fisica.
//    /// Quando trascino un oggetto 3D usando coordinate dello schermo (mouse, touch, Lidar…), il puntatore è in 2D, ma l’oggetto sta in 3D.
//    /// Quindi devo dire a Unity a che distanza dalla camera deve collocare l’oggetto mentre lo seguo con il puntatore.
//    /// </summary>
//    public void BeginDrag(Vector2 screenPos) //Stabilisce il punto dello spazio da cui inizia il trascinamento, così l’oggetto resta “agganciato” esattamente dove hai cliccato.
//    {
//        if (cam == null) cam = Camera.main;

//        // Converto la posizione word del modello 3D in coordinate schermo, cioè rispetto alla camera (dove screenPoint.z è la profondità rispetto alla camera)
//        screenPoint = cam.WorldToScreenPoint(visualRoot.position);

//        // Converto il puntatore 2D in un punto 3D del mondo mantenendo la stessa profondità dalla camera dell'oggetto 3D
//        // Così determino come l’utente ha “preso” l’oggetto cioè in quale punto lo ha afferrato
//        var worldUnderPointer = cam.ScreenToWorldPoint(new Vector3(
//            screenPos.x,
//            screenPos.y,
//            screenPoint.z));

//        // Offset tra il centro dell'oggetto e il punto cliccato, serve poi a mantenere “agganciato” il punto di presa.
//        offset = visualRoot.position - worldUnderPointer;

//        // Blocca la fisica durante il drag in modo da muovere l'oggetto a mano.
//        rb.isKinematic = true;

//        // Azzeriamo velocità lineare e angolare.
//        // (Se usi Rigidbody classico di Unity, assicurati che la proprietà sia "velocity".)
//        rb.linearVelocity = Vector3.zero;
//        rb.angularVelocity = Vector3.zero;

//        // Se l'oggetto ha un Animator, lo disabilitiamo temporaneamente:
//        // evita che l'animazione sposti il modello mentre lo trasciniamo.
//        if (visualAnimator != null)
//        {
//            visualAnimatorWasEnabled = visualAnimator.enabled;
//            visualAnimator.enabled = false;
//        }
//    }

//    /// <summary>
//    /// Aggiorna la posizione dell'oggetto mentre si trascina.
//    /// </summary>

//    public void ContinueDrag(Vector2 screenPos)
//    {
//        if (cam == null) cam = Camera.main;

//        // Converto il puntatore 2D in un punto 3D del mondo mantenendo la stessa profondità dalla camera dell'oggetto 3D
//        // Così aggiorno continuamente la posizione dell’oggetto nello spazio
//        Vector3 curScreenPoint = new Vector3(
//            screenPos.x,
//            screenPos.y,
//            screenPoint.z);

//        // Converto la posizione in coordinate mondo del puntatore
//        // mantenendo la stessa profondità e aggiungendo l'offset (differenza tra il centro dell'oggetto e il punto cliccato)
//        Vector3 curWorldPos =
//            cam.ScreenToWorldPoint(curScreenPoint) + offset;

//        transform.position = curWorldPos; // Spostamento del transform dell'oggetto 3d

//        // Se il nodo visivo non coincide con il transform principale, aggiorniamo anche lui.
//        if (visualRoot != null && visualRoot != transform)
//            visualRoot.position = curWorldPos;

//        // Aggiorna evidenziazione delle SnapZone sotto il puntatore.
//        UpdateSnapZoneHighlight(screenPos);
//    }



//    /// <summary>
//    /// Termina il drag: prova ad agganciare l'oggetto a una SnapZone sotto il puntatore,
//    /// riattiva la fisica e ripristina l'Animator.
//    /// </summary>
//    public void EndDrag(Vector2 screenPos)
//    {
//        // Tenta di agganciare l'oggetto alla SnapZone sotto il puntatore (se presente).
//        SnapIntoZoneUnderPointer(screenPos);

//        // Riattiva la fisica così l'oggetto torna ad essere governato dal Rigidbody.
//        rb.isKinematic = false;

//        // Rimuove eventuale evidenziazione residua.
//        SetHighlightedZone(null);

//        // Ripristina lo stato dell'Animator (se esisteva).
//        if (visualAnimator != null)
//            visualAnimator.enabled = visualAnimatorWasEnabled;
//    }

//    // =====================================================================
//    //  GESTIONE SNAP ZONE (HIGHLIGHT + SNAP FINALE)
//    // =====================================================================

//    /// <summary>
//    /// Esegue un raycast dal puntatore per verificare se stiamo "puntando" una SnapZone
//    /// e aggiorna l'evidenziazione di conseguenza.
//    /// </summary>
//    void UpdateSnapZoneHighlight(Vector2 screenPos)
//    {
//        if (cam == null) cam = Camera.main;

//        // Ray dalla camera passando per la posizione del puntatore sullo schermo.
//        Ray ray = cam.ScreenPointToRay(screenPos);

//        // Raycast limitato al layer delle SnapZone, fino a 10 unità, trigger inclusi.
//        if (Physics.Raycast(ray, out RaycastHit hit, 10f, snapZoneLayerMask, QueryTriggerInteraction.Collide))
//        {
//            // Prova a prendere una FridgeSnapZone dal collider colpito o dai suoi genitori.
//            var zone = hit.collider.GetComponent<FridgeSnapZone>()
//                       ?? hit.collider.GetComponentInParent<FridgeSnapZone>();

//            SetHighlightedZone(zone);
//        }
//        else
//        {
//            SetHighlightedZone(null);
//        }
//    }

//    /// <summary>
//    /// Imposta la SnapZone evidenziata corrente (attivando/disattivando highlight).
//    /// </summary>
//    void SetHighlightedZone(FridgeSnapZone zone)
//    {
//        // Se è la stessa zona di prima, non fare nulla.
//        if (highlightedZone == zone)
//            return;

//        // Disattiva highlight dalla zona precedente, se c'era.
//        if (highlightedZone != null)
//            highlightedZone.SetHighlighted(false);

//        highlightedZone = zone;

//        // Attiva highlight sulla nuova zona, se presente.
//        if (highlightedZone != null)
//            highlightedZone.SetHighlighted(true);
//    }

//    /// <summary>
//    /// Al rilascio, prova ad agganciare l'oggetto a una SnapZone sotto il puntatore.
//    /// Regola la Z secondo la SnapZone e la Y in base alla mensola più vicina sotto l'oggetto.
//    /// </summary>
//    void SnapIntoZoneUnderPointer(Vector2 screenPos)
//    {
//        if (cam == null) cam = Camera.main;

//        Ray ray = cam.ScreenPointToRay(screenPos);

//        if (Physics.Raycast(ray, out RaycastHit hit, 10f, snapZoneLayerMask, QueryTriggerInteraction.Collide))
//        {
//            var zone = hit.collider.GetComponent<FridgeSnapZone>()
//                       ?? hit.collider.GetComponentInParent<FridgeSnapZone>();

//            if (zone == null)
//                return;

//            // Partiamo dalla posizione attuale e modifichiamo solo alcuni assi.
//            Vector3 snapPos = transform.position;

//            // Imposta la Z target in base alla SnapZone.
//            float targetX = zone.GetTargetX();
//            snapPos.x = targetX;

//            // Ray verso il basso per trovare la mensola sottostante.
//            Vector3 rayOrigin = snapPos + Vector3.up * 0.5f;
//            float maxDistance = 2f;

//            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit shelfHit, maxDistance, shelfLayerMask, QueryTriggerInteraction.Ignore))
//            {
//                // Piazziamo l'oggetto appena sopra la superficie della mensola.
//                snapPos.y = shelfHit.point.y + 0.001f;
//            }

//            // Applichiamo la posizione di snap all'oggetto.
//            transform.position = snapPos;

//            if (visualRoot != null && visualRoot != transform)
//                visualRoot.position = snapPos;
//        }
//        else
//        {
//            Debug.Log("[Draggable] Nessuna SnapZone colpita al rilascio.");
//        }
//    }
//}
