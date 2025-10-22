using UnityEngine;
using System.Collections.Generic;


public class WordsGrid : MonoBehaviour
{

    //variabili di cui ho bisogno
    public GameData currentGameData;
    public GameObject gridSquarePrefab;
    public AlphabetData alphabetData;

    public float squareOffset = 0.0f;
    public float topPosition;

    private List<GameObject> _squareList = new List<GameObject>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        SpawnGridSquares();
        SetSquarePosition();
    }

    // Update is called once per frame
    private void SpawnGridSquares()
    {
        if (currentGameData != null)

        {
            //calcolare  scale del rettangolo della griglia

            var squareScale = GetSquareScale(new Vector3(1.5f, 1.5f, 0.1f));

            foreach (var squares in currentGameData.selectedBoardData.Board)
            {

                foreach (var squareLetter in squares.Row)
                {
                    var normalLetterData = alphabetData.AlphabetNormal.Find(data => data.letter == squareLetter);
                    var selectedLetterData = alphabetData.AlphabetHighlighted.Find(data => data.letter == squareLetter);
                    var correctLetterData = alphabetData.AlphabetWrong.Find(data => data.letter == squareLetter);

                    // se non abbiamo le
                    if (normalLetterData.image == null || selectedLetterData.image == null)
                    {
                        Debug.LogError("Missing letter data for letter: " + squareLetter);


                        //SE SIAMO NELL'UNITI EDITOR
#if UNITY_EDITOR


                        if (UnityEditor.EditorApplication.isPlaying)
                        {
                             UnityEditor.EditorApplication.isPlaying = false;
                        }
#endif

                    }

                    else
                    {
                        _squareList.Add(Instantiate(gridSquarePrefab));

                        _squareList[_squareList.Count - 1].GetComponent<GridSquare>().SetSprite(normalLetterData, selectedLetterData, correctLetterData);

                        _squareList[_squareList.Count - 1].transform.SetParent(this.transform);
                        _squareList[_squareList.Count - 1].GetComponent<Transform>().position = new Vector3(0f, 0f, 0f);
                        _squareList[_squareList.Count - 1].transform.localScale = squareScale;

                    }


                }
            }

        }


    }


    private Vector3 GetSquareScale(Vector3 defaultScale)
    {
        var finalScale = defaultScale;
        var adjustment = 0.01f; // small adjustment to avoid overlapping
        while (ShouldScaleDown(finalScale))
        {
            finalScale.x -= adjustment;
            finalScale.y -= adjustment;

            if ((finalScale.x <= 0) || (finalScale.y <= 0))
            {
                finalScale.x = adjustment;
                finalScale.y = adjustment;
                return finalScale;
            }
        }

        return finalScale;

    }

    private bool ShouldScaleDown(Vector3 targetScale)
    {
        //check if our square will be fit in our screen

        var squareRect = gridSquarePrefab.GetComponent<SpriteRenderer>().sprite.rect;
        var squareSize = new Vector2(0f, 0f);
        var startPosition = new Vector2(0f, 0f);

        //calcolo square size

        squareSize.x = (squareRect.width * targetScale.x) + squareOffset;
        squareSize.y = (squareRect.height * targetScale.y) + squareOffset;

        var midWidhtPosition = ((currentGameData.selectedBoardData.Columns * squareSize.x) / 2) * 0.01f;

        var midWidhtHeight = ((currentGameData.selectedBoardData.Rows * squareSize.y) / 2) * 0.01f;


        //calcolo start position
        startPosition.x = (midWidhtPosition != 0) ? midWidhtPosition * -1 : midWidhtPosition;
        startPosition.y = midWidhtHeight;

        //CHECK IF GRID OUTSIDE OF THE SCREEEN
        return (startPosition.x < GetHalfScreenWidth() * -1 || startPosition.y > topPosition);
    }

    private float GetHalfScreenWidth()
    {
        float heigh = Camera.main.orthographicSize * 2;
        float width = (1.7f * heigh) * Screen.width / Screen.height;

        return width / 2;
    }

    private void SetSquarePosition()
    {
        //funzione per posizionare i quadrati nella griglia

        var squareRect = _squareList[0].GetComponent<SpriteRenderer>().sprite.rect;
        var squareTransform = _squareList[0].GetComponent<Transform>();

        var offset = new Vector2
        {
            x = (squareRect.width * squareTransform.localScale.x + squareOffset) * 0.01f,
            y = (squareRect.height * squareTransform.localScale.y + squareOffset) * 0.01f

        };

        var startPosition = GetFirstSquarePosition();

        int columnNumber = 0;
        int rowNumber = 0;

        foreach (var square in _squareList)
        {
            if (rowNumber + 1 > currentGameData.selectedBoardData.Rows)
            {
                rowNumber = 0;
                columnNumber++;
            }

            var positionX = startPosition.x + (offset.x * columnNumber);
            var positionY = startPosition.y - (offset.y * rowNumber);

            square.GetComponent<Transform>().position = new Vector2(positionX, positionY);

            rowNumber++;



        }

        CenterGrid();
    }

    private Vector2 GetFirstSquarePosition()
    {

        var startPosition = new Vector2(0f, transform.position.y);
        var squareRect = _squareList[0].GetComponent<SpriteRenderer>().sprite.rect;
        var squareTransform = _squareList[0].GetComponent<Transform>();
        var squareSize = new Vector2(0f, 0f);

        squareSize.x = (squareRect.width * squareTransform.localScale.x);
        squareSize.y = (squareRect.height * squareTransform.localScale.y);

        var midWidhtPosition = ((currentGameData.selectedBoardData.Columns - 1 * squareSize.x) / 2) * 0.01f;
        var midWidhtHeight = ((currentGameData.selectedBoardData.Rows - 1 * squareSize.y) / 2) * 0.01f;

        startPosition.x = (midWidhtPosition != 0) ? midWidhtPosition * -1 : midWidhtPosition;
        startPosition.y += midWidhtHeight;

        return startPosition;



    }

    private void CenterGrid()
    {
        if (_squareList.Count == 0) return;

        // 1) Calcola il centro geometrico della griglia
        Bounds gridBounds = new Bounds(_squareList[0].transform.position, Vector3.zero);
        foreach (var square in _squareList)
        {
            gridBounds.Encapsulate(square.GetComponent<SpriteRenderer>().bounds);
        }

        // 2) Ottieni il centro dello schermo in coordinate di mondo
        Camera cam = Camera.main;
        float z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 screenCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, z));

        // 3) Calcola di quanto spostare il parent (WordsGrid)
        Vector3 offset = screenCenter - gridBounds.center;
        transform.position += offset;
    }

    

}
