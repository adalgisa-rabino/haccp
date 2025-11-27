using System.Collections.Generic;

/// <summary>
/// Rimuove dal pool gli indizi ESCLUDENTI che andrebbero
/// a "negare" la terna segreta scelta (colpevole/arma/luogo).
/// Ambigui e Positivi restano sempre.
/// </summary>
public class FiltroEscludenti
{
    private readonly string _colpevole, _arma, _luogo;

    public FiltroEscludenti(string colpevoleSegreto, string armaSegreta, string luogoSegreto)
    {
        _colpevole = colpevoleSegreto;
        _arma = armaSegreta;
        _luogo = luogoSegreto;
    }

    public List<Clue> Applica(List<Clue> tutti)
    {
        var risultato = new List<Clue>(tutti.Count);

        foreach (var c in tutti)
        {
            bool escludente = FunzioniAusiliarie.SonoUguali(c.tipo, "Escludente");

            if (!escludente)
            {
                // Ambigui/Positivi → li teniamo
                risultato.Add(c);
                continue;
            }

            if (FunzioniAusiliarie.SonoUguali(c.categoria, "Colpevole"))
            {
                if (!FunzioniAusiliarie.SonoUguali(c.bersaglioColpevole, _colpevole))
                    risultato.Add(c);
            }
            else if (FunzioniAusiliarie.SonoUguali(c.categoria, "Arma"))
            {
                if (!FunzioniAusiliarie.SonoUguali(c.bersaglioArma, _arma))
                    risultato.Add(c);
            }
            else if (FunzioniAusiliarie.SonoUguali(c.categoria, "Luogo"))
            {
                if (!FunzioniAusiliarie.SonoUguali(c.bersaglioLuogo, _luogo))
                    risultato.Add(c);
            }
            else
            {
                // categoria non riconosciuta → non rischio di eliminare
                risultato.Add(c);
            }
        }

        return risultato;
    }
}
