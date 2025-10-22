using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]
[CreateAssetMenu]


//rappresenta dati del livello non logica esecutiva, per questo deve essere trattato come asset indipendente
public class BoardData : ScriptableObject

{

    //inserisco diverse classi dentro questa classe
    [System.Serializable]

//classe per le parole da cercare
    public class SearchingWord
    {
        public string word;

    }

    [System.Serializable]

//classe per le righe della board
    public class BoardRow
    {
        //specifiche della riga
        public int Size;
        public string[] Row;

        public BoardRow(int size)
        {

            CreateRow(size);
        }

        //funzione per creare una riga della dimensione desiderata
        public void CreateRow(int size)
        {
            Size = size;
            Row = new string[Size];
            ClearRow();

        }

        public void ClearRow()
        {
            for (int i = 0; i < Size; i++)
            {
                Row[i] = "";
            }
        }
    }



    public float timeInSeconds; // tempo per completare il livello
    public int Columns = 0;
    public int Rows = 0;

    public BoardRow[] Board; //array di righe

    public List<SearchingWord> SearchWords = new List<SearchingWord>(); //lista di parole da cercare

//funzione per resettare la board

    public void ClearWithEmptyString()
    {
        for (int i = 0; i < Columns; i++)
        {
            Board[i].ClearRow();
        }
    }

//funzione per creare una nuova board
    public void CreateNewBoard()
    {
        Board = new BoardRow[Columns];
        for (int i = 0; i < Columns; i++)
        {
            Board[i] = new BoardRow(Rows);
        }
    }
}