using UnityEngine;
using System.Collections.Generic;

public class WordsGrid : MonoBehaviour
{

    //variabili di cui ho bisogno
    public GameData currentGameData;
    public GameObject gridSquarePrefaf;
    public AlphabetData alphabetData;

    public float squareOffset = 0.0f;
    public float topPosition;

    private List<GameObject> _squareList = new List<GameObject>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
                        _squareList.Add(Instantiate(gridSquarePrefaf));
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

        var squareRect = gridSquarePrefaf.GetComponent<SpriteRenderer>().sprite.rect;
        var squareSize = new Vector2(0f, 0f);
        var startPosition = new Vector2(0f, 0f);

        //calcolo square size

        squareSize.y = (squareRect.width * targetScale.x) + squareOffset;
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
}
