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
        // DATI FISSI: Questi appariranno sempre nella tua board Checklist
        Colpevoli.AddRange(new[] { 
            "Cuoco", "Pasticcere", "Cameriere", "Scaffalista Fattorino", "Lavapiatti", "Operatore pulizie" 
        });

        Armi.AddRange(new[] { 
            "Contaminanti fisici", "Contaminanti chimici", "Contaminanti biologici", 
            "Allergeni", "Contaminazione crociata", "Proliferazione microbica" 
        });

        Luoghi.AddRange(new[] { 
            "Magazzino", "Frigorifero", "Lavandino", "Area cottura", "Banco freddo", "Armadio abiti" 
        });
    }
}