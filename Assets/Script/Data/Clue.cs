using System;
using UnityEngine;

/// <summary>
/// Rappresenta un singolo indizio caricato da clues.json.
/// </summary>
[Serializable]
public class Clue
{
    [Header("Identificatore univoco (es. indizio_001)")]
    public string id;

    [Header("Tipo: Escludente / Ambiguo / Positivo")]
    public string tipo;

    [Header("Categoria: Colpevole / Arma / Luogo")]
    public string categoria;

    [Header("Effetto logico: Esclude / Ambiguo / Supporta (opzionale)")]
    public string effetto;

    [Header("Se categoria=Colpevole")]
    public string bersaglioColpevole;

    [Header("Se categoria=Arma")]
    public string bersaglioArma;

    [Header("Se categoria=Luogo")]
    public string bersaglioLuogo;

    [TextArea(3, 5)]
    [Header("Testo narrativo mostrato al giocatore")]
    public string testo;
}
