using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class WordChecker : MonoBehaviour
{
    public GameData currentGameDate;
    private string _word;
    private int _assignedPoints = 0;
    private int _completedWords = 0;

    // Rays for all directions
    private Ray _rayUp, _rayDown;
    private Ray _rayRight, _rayLeft;
    private Ray _rayDiagonalLeftUp, _rayDiagonalLeftDown;
    private Ray _rayDiagonalRightUp, _rayDiagonalRightDown;
    private Ray _currentRay = new Ray();

    private Vector3 _raystartPosition;
    private List<int> _correctSquareList = new List<int>();

    private void OnEnable()
    {
        GameEvents.OnCheckSquare += SquareSelectded;
        GameEvents.OnClearSelection += ClearSelection;


    }
    private void OnDisable()
    {
        GameEvents.OnCheckSquare -= SquareSelectded;
        GameEvents.OnClearSelection -= ClearSelection;

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _assignedPoints = 0;
        _completedWords = 0;


    }

    // Update is called once per frame
    void Update()
    {
        if (_correctSquareList.Count > 0 && Application.isEditor)
        {
            Debug.DrawRay(_rayUp.origin, _rayUp.direction * 4);
            Debug.DrawRay(_rayDown.origin, _rayDown.direction * 4);
            Debug.DrawRay(_rayRight.origin, _rayRight.direction * 4);
            Debug.DrawRay(_rayLeft.origin, _rayLeft.direction * 4);
            Debug.DrawRay(_rayDiagonalLeftUp.origin, _rayDiagonalLeftUp.direction * 4);
            Debug.DrawRay(_rayDiagonalLeftDown.origin, _rayDiagonalLeftDown.direction * 4);
            Debug.DrawRay(_rayDiagonalRightUp.origin, _rayDiagonalRightUp.direction * 4);
            Debug.DrawRay(_rayDiagonalRightDown.origin, _rayDiagonalRightDown.direction * 4);
        }
    }

    private void SquareSelectded(string letter, Vector3 squarePosition, int squareIndex)
    {

        if (_correctSquareList.Count == 0)

        {
            GameEvents.SelectSquareMethod(squarePosition);
            _raystartPosition = squarePosition;
            _correctSquareList.Add(squareIndex);
            _word += letter;


            Vector3 origin = new Vector3(squarePosition.x, squarePosition.y, squarePosition.z);

            _rayUp = new Ray(origin, new Vector3(0f, 1f, 0f));
            _rayDown = new Ray(origin, new Vector3(0f, -1f, 0f));
            _rayRight = new Ray(origin, new Vector3(1f, 0f, 0f));
            _rayLeft = new Ray(origin, new Vector3(-1f, 0f, 0f));
            _rayDiagonalLeftDown = new Ray(origin, new Vector3(-1f, -1f, 0f).normalized);
            _rayDiagonalLeftUp = new Ray(origin, new Vector3(-1f, 1f, 0f).normalized);
            _rayDiagonalRightDown = new Ray(origin, new Vector3(1f, -1f, 0f).normalized);
            _rayDiagonalRightUp = new Ray(origin, new Vector3(1f, 1f, 0f).normalized);
        }

        // second selected square
        else if (_correctSquareList.Count == 1)
        {


            _currentRay = SelectRay(_raystartPosition, squarePosition); ;
            _correctSquareList.Add(squareIndex);

            GameEvents.SelectSquareMethod(squarePosition);
            _word += letter;
            CheckWord();
        }

        else
        {
            //check if the selected square is on the ray

            if (IsPointOnTheRay(_currentRay, squarePosition))
            {
                _correctSquareList.Add(squareIndex);
                GameEvents.SelectSquareMethod(squarePosition);
                _word += letter;
                CheckWord();

            }
            else {
                Debug.Log("Square not on the ray" );
            }
            

        }
        
        

        
  
    }

    private void CheckWord()
    {
        foreach (var searchingWord in currentGameDate.selectedBoardData.SearchWords)
        {
            if (_word == searchingWord.word)
            {
                GameEvents.CorrectWordMethod(_word, _correctSquareList);
                _word = string.Empty;
                _correctSquareList.Clear();
                return;

            }
        }

    }

    private bool IsPointOnTheRay(Ray currentRay, Vector3 point)
    {
        //lenght of the ray
        var hits = Physics.RaycastAll(currentRay, 100.0f);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.position == point)
            {
                return true;
            }
        }

        return false;
    }
    
    /*
    private bool IsPointOnTheRayGeometric(Ray currentRay, Vector3 point, float tolerance = 0.001f)
    {
        // 1. Il punto deve essere in avanti rispetto all'origine del raggio.
        Vector3 vectorFromOrigin = point - currentRay.origin;

        // Se il prodotto scalare è negativo, il punto è dietro all'origine del raggio.
        if (Vector3.Dot(vectorFromOrigin, currentRay.direction) < 0f)
        {
            return false;
        }

        // 2. Il punto deve essere allineato con la direzione del raggio (cross product vicino a zero).
        // Calcoliamo la distanza perpendicolare (o l'area del parallelogramma) dal raggio al punto.
        // L'uso di .sqrMagnitude è più performante della distanza normale (evita la radice quadrata).
        float sqrDistanceToRay = Vector3.Cross(currentRay.direction.normalized, vectorFromOrigin).sqrMagnitude;

        // Se la distanza perpendicolare è minore di una tolleranza, il punto è sul raggio.
        return sqrDistanceToRay < tolerance * tolerance;
    }
    */

    private Ray SelectRay(Vector2 firstPosition, Vector2 secondPosition)


    // first square = firstPosition second square = secondPosition ..... function return teh right ray of the selection
    {

        var direction = (secondPosition - firstPosition).normalized; // get direction vector, normalized to get only direction info UP/DOWN ...
        float tollerance = 0.01f; //tollerance for diagonal selection


        //CASO UP
        if (Mathf.Abs(direction.x) < tollerance && Mathf.Abs(direction.y - 1f) < tollerance)
        {
            return _rayUp;
        }

        //CASO DOWN
        if (Mathf.Abs(direction.x) < tollerance && Mathf.Abs(direction.y - (-1f)) < tollerance)
        {
            return _rayDown;
        }

        if (Mathf.Abs(direction.x - 1f) < tollerance && Mathf.Abs(direction.y) < tollerance)
        {
            return _rayRight;

        }
        if (Mathf.Abs(direction.x - (-1f)) < tollerance && Mathf.Abs(direction.y) < tollerance)
        {
            return _rayLeft;

        }

        if (direction.x < 0f && direction.y > 0f)
        {
            return _rayDiagonalLeftUp;
        }

        if (direction.x < 0f && direction.y < 0f)
        {
            return _rayDiagonalLeftDown;
        }

        if (direction.x > 0f && direction.y > 0f)
        {
            return _rayDiagonalRightUp;
        }

        if (direction.x > 0f && direction.y < 0f)
        {
            return _rayDiagonalRightDown;
        }


        return _rayDown; //default return


    }

    private void ClearSelection()
    {
        _assignedPoints = 0;
        _correctSquareList.Clear();
        _word = string.Empty;
    }

}