using System.Collections.Generic;

/// <summary>
/// Dalla lista di indizi costruisce i cataloghi di possibili
/// Colpevoli, Armi e Luoghi, senza hardcode.
/// </summary>
public class CostruttoreCataloghiIndizi
{
    public List<string> Colpevoli { get; } = new();
    public List<string> Armi { get; } = new();
    public List<string> Luoghi { get; } = new();

    public CostruttoreCataloghiIndizi(List<Clue> indizi)
    {
        if (indizi == null) return;

        foreach (var c in indizi)
        {
            if (!string.IsNullOrWhiteSpace(c.bersaglioColpevole) && !Colpevoli.Contains(c.bersaglioColpevole))
                Colpevoli.Add(c.bersaglioColpevole);

            if (!string.IsNullOrWhiteSpace(c.bersaglioArma) && !Armi.Contains(c.bersaglioArma))
                Armi.Add(c.bersaglioArma);

            if (!string.IsNullOrWhiteSpace(c.bersaglioLuogo) && !Luoghi.Contains(c.bersaglioLuogo))
                Luoghi.Add(c.bersaglioLuogo);
        }

        // fallback se il JSON è parziale
        if (Colpevoli.Count == 0) Colpevoli.AddRange(new[] {
            "Cuoco","Pasticcere","Cameriere","Scaffalista Fattorino","Lavapiatti","Operatore pulizie"
        });
        if (Armi.Count == 0) Armi.AddRange(new[] {
            "Contaminanti fisici","Contaminanti chimici","Contaminanti biologici",
            "Allergeni","Contaminazione crociata","Proliferazione microbica"
        });
        if (Luoghi.Count == 0) Luoghi.AddRange(new[] {
            "Magazzino","Frigorifero","Lavandino","Area cottura","Banco freddo","Armadio abiti"
        });
    }
}
