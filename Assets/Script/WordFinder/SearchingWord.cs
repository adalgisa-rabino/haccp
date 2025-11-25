using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchingWord : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    public Image crossLine;
    private string _word;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


    }



    public void OnEnable()
    {
        GameEvents.OnCorrectWord += CorrectWord;

    }

    public void OnDisable()
    {
         GameEvents.OnCorrectWord -= CorrectWord;

    }

    public void SetWord(string word)
    {
        _word = word;
        displayText.text = _word;

    }

    private void CorrectWord(string word, List<int> squareIndexes)
    {
        if (word == _word)
        {
            // Strike through the word
            crossLine.gameObject.SetActive(true);
            // Additional logic for marking squares can be added here
        }


    }
    


}
