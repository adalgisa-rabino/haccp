using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Mescola il pool e seleziona i primi N indizi richiesti.
/// </summary>
public class EstrattoreIndizi
{
    public List<Clue> Estrai(List<Clue> sorgente, int n)
    {
        var tmp = new List<Clue>(sorgente);
        FunzioniAusiliarie.Mescola(tmp);
        if (n >= tmp.Count) return tmp;
        return tmp.Take(n).ToList();
    }
}
