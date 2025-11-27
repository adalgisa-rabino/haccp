using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UITeleportController
/// ---------------------------
/// Adattatore di input: riceve i click dei pulsanti della mappa e
/// chiede a TeleportActions (sul Player) di eseguire il teletrasporto.
///
/// Come usare:
/// - Metti questo script su un GO UI (es. MapPanel).
/// - Assegna il riferimento a TeleportActions (Player).
/// - Compila la lista "areas" con i marker (Area_0, Area_1, ...).
/// - Nei Button → OnClick chiama GoToAreaIndex(int) con l'indice giusto.
/// </summary>
public class UITeleportControllerOldVersion : MonoBehaviour
{
    [Header("Azione di Teletrasporto (sul Player)")]
    [Tooltip("Riferimento al componente TeleportActions sul Player.")]
    public TeleportActions teleportActions;

    [Header("Aree di Teletrasporto (marker nella scena)")]
    [Tooltip("Trasforma dei segnaposto. La Z+ dell'area è la direzione di sguardo.")]
    public List<Transform> areas = new List<Transform>();

    /// <summary>
    /// Da collegare ai Button (OnClick) con parametro int.
    /// </summary>
    public void GoToAreaIndex(int index)
    {
        if (!teleportActions) return;
        if (index < 0 || index >= areas.Count) return;

        Transform marker = areas[index];
        if (!marker) return;
        teleportActions.ApplyPose(marker, null);
    }
}
