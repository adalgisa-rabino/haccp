using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Raccolta di funzioni di supporto comuni:
/// - Estrazione casuale da liste
/// - Mescolamento casuale
/// - Confronto di stringhe senza distinzione di maiuscole/minuscole
/// </summary>
public static class FunzioniAusiliarie
{
    /// <summary>
    /// Restituisce un elemento casuale da una lista.
    /// </summary>
    public static T PescaACaso<T>(List<T> lista)
    {
        if (lista == null || lista.Count == 0) return default;
        int i = UnityEngine.Random.Range(0, lista.Count);
        return lista[i];
    }

    /// <summary>
    /// Mescola gli elementi di una lista in ordine casuale.
    /// </summary>
    public static void Mescola<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Confronta due stringhe ignorando maiuscole, minuscole e spazi.
    /// </summary>
    public static bool SonoUguali(string a, string b)
    {
        if (a == null || b == null) return false;
        return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
