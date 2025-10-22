using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singolo asset che contiene tutti gli indizi (usa la classe serializzabile Clue).
/// </summary>
[CreateAssetMenu(fileName = "ClueDatabase", menuName = "Cluedo/ClueDatabase")]
public class ClueDatabase : ScriptableObject
{
    [Tooltip("Lista di indizi caricati (serializzabili)")]
    public List<Clue> indizi = new List<Clue>();
}