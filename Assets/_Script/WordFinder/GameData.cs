using UnityEngine;
using System.Collections;

[System.Serializable]
[CreateAssetMenu]

public class GameData : ScriptableObject
{

    public string selectedCategoryName;
    public BoardData selectedBoardData;
}
