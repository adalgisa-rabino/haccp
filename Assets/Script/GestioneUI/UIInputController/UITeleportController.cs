using System.Collections.Generic;
using UnityEngine;

public class UITeleportController : MonoBehaviour
{
    [Header("Dolly travel controller")]
    public DollyTravelController dollyTravel;

    [Header("Aree (Empty con TeleportStop)")]
    public List<TeleportStop> stops = new List<TeleportStop>();

    /// <summary>Da collegare ai Button (OnClick) con parametro int.</summary>
    public void GoToAreaIndex(int index)
    {
        if (!dollyTravel) return;
        if (index < 0 || index >= stops.Count) return;

        var stop = stops[index];
        if (!stop) return;

        dollyTravel.BeginTravelTo(stop);
    }
}
