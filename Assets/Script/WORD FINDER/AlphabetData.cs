using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu]
public class AlphabetData : ScriptableObject
{

    [System.Serializable] 
    public class LetterData
    {
        public string letter;
        public Sprite image;
    }

    public List<LetterData> AlphabatPlain= new List<LetterData>();
    public List<LetterData> AlphabatNormal = new List<LetterData>();
    public List<LetterData> AlphabatHighlighted = new List<LetterData>();
    public List<LetterData> AlphabatWrong = new List<LetterData>();
    


}
