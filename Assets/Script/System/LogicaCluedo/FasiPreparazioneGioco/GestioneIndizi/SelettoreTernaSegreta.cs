using System.Collections.Generic;

/// <summary>
/// Seleziona casualmente la terna segreta (colpevole, arma, luogo)
/// a partire dai cataloghi disponibili.
/// </summary>
public class SelettoreTernaSegreta
{
    private readonly List<string> _colpevoli, _armi, _luoghi;

    public SelettoreTernaSegreta(List<string> colpevoli, List<string> armi, List<string> luoghi)
    {
        _colpevoli = colpevoli;
        _armi = armi;
        _luoghi = luoghi;
    }

    public (string colpevole, string arma, string luogo) Seleziona()
    {
        string c = FunzioniAusiliarie.PescaACaso(_colpevoli);
        string a = FunzioniAusiliarie.PescaACaso(_armi);
        string l = FunzioniAusiliarie.PescaACaso(_luoghi);
        return (c, a, l);
    }
}
