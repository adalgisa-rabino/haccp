using UnityEngine;

public class GridSquare : MonoBehaviour
{

    public int SquareIndex { get; set; }
    private AlphabetData.LetterData _normalLetterData;
    private AlphabetData.LetterData _selectedLetterData;
    private AlphabetData.LetterData _correctLetterData;

    private SpriteRenderer _displayedImage;

    private bool _selected;
    private bool _clicked;
    private int _index = -1;

    public void SetIndex(int index)
    {
        _index = index;
    }

    public int GetIndex()
    {
        return _index;
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _clicked = false;
        _selected = false;
        _displayedImage = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var cam = Camera.main;
            if (!cam) return;

            Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 p = new Vector2(world.x, world.y);

            // Richiede BoxCollider2D
            var hit = Physics2D.OverlapPoint(p);
            if (hit && hit.transform == transform)
            {
                Debug.Log("Raycast 2D hit " + name);
                OnMouseDown();   // richiama la tua logica
            }
        }
    }


    private void OnEnable()
    {
        GameEvents.OnEnableSquareSelection += OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection += OnDisableSquareSelection;
        GameEvents.OnSelectSquare += SelectSquare;

    }

    private void OnDisable()
    {
        GameEvents.OnEnableSquareSelection -= OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection -= OnDisableSquareSelection;
        GameEvents.OnSelectSquare -= SelectSquare;

    }

    public void OnEnableSquareSelection()
    {
        _clicked = true;
        _selected = false;
    }

    public void OnDisableSquareSelection()
    {
        _selected = false;
        _clicked = false;
    }

    
    private void SelectSquare(Vector3 position)
    {
        if ((transform.position - position).sqrMagnitude < 0.0001f) // confronto robusto
        {
            _displayedImage.sprite = _correctLetterData.image;
        }
    }



    public void SetSprite(AlphabetData.LetterData normalLetterData, AlphabetData.LetterData selectedLetterData, AlphabetData.LetterData correctLetterData)
    {
        _normalLetterData = normalLetterData;
        _selectedLetterData = selectedLetterData;
        _correctLetterData = correctLetterData;

        GetComponent<SpriteRenderer>().sprite = _normalLetterData.image;
    }


    private void OnMouseDown()
    {
        Debug.Log("Square clicked: " + _normalLetterData.letter);

        OnEnableSquareSelection();
        GameEvents.EnableSquareSelectionMethod();

        // INVIA la posizione del quadrato selezionato
        GameEvents.SelectSquareMethod(transform.position);

        CheckSquare();
        _displayedImage.sprite = _correctLetterData.image;
    }


    private void OnMouseEnter()
    {

        // se sto “trascinando” con il mouse premuto, colora anche al passaggio
        if (_clicked)
        {
            GameEvents.SelectSquareMethod(transform.position);
        }
        CheckSquare();

    }

    private void OnMouseUp()
    {
        
        GameEvents.ClearSelectionMethod();
        GameEvents.DisableSquareSelectionMethod();

    }

    public void CheckSquare()
    {
        if(_selected == false && _clicked == true)
        {
            _selected = true;
            GameEvents.CheckSquareMethod(_normalLetterData.letter, gameObject.transform.position, _index);
        }
    }
}
