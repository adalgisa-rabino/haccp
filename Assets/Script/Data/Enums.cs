using UnityEngine;

// Assets/Scripts/Data/Enums.cs
namespace CluedoEdu
{
    public enum ClueDomain
    {
        Colpevole,
        Arma,
        Luogo,
        Trasversale // per indizi che toccano più assi (es. ambigui/scenari)
    }

    public enum ClueRole
    {
        Escludente,      // scagiona un'opzione specifica
        Ambiguo,         // suggerisce senza escludere
        ScenarioPositivo // colore narrativo, non prova
    }

    public enum Colpevole
    {
        None,
        Cuoco,
        Pasticcere,
        Cameriere,
        Scaffalista,
        Lavapiatti,
        Pulizie
    }

    public enum Arma
    {
        None,
        Fisici,
        Chimici,
        Biologici,
        Allergeni,
        Crociata,
        Proliferazione
    }

    public enum Luogo
    {
        None,
        Magazzino,
        Frigorifero,
        Lavandino,
        AreaCottura,
        BancoFreddo,
        ArmadioAbiti
    }
}

