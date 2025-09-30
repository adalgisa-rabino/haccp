using UnityEngine;

// Assets/Scripts/Data/ClueSO.cs
using UnityEngine;

namespace CluedoEdu
{   
    [CreateAssetMenu(menuName = "CluedoEdu/Clue", fileName = "Clue")]
    public class ClueSO : ScriptableObject
    {
        [TextArea(2, 6)]
        public string text;

        public ClueDomain domain = ClueDomain.Trasversale;
        public ClueRole role = ClueRole.Ambiguo;

        // Target per ESCLUDENTI (usa solo quello coerente col domain)
        public Colpevole targetColpevole = Colpevole.None;
        public Arma targetArma = Arma.None;
        public Luogo targetLuogo = Luogo.None;

        // Suggerimenti (per AMBIGUI): facoltativi, non vincolanti
        public Colpevole[] hintColpevoli;
        public Arma[] hintArmi;
        public Luogo[] hintLuoghi;

        // Aiuto in editor: controlli base di coerenza
        private void OnValidate()
        {
            // Pulisci target non coerenti con il dominio se è un ESCLUDENTE
            if (role == ClueRole.Escludente)
            {
                switch (domain)
                {
                    case ClueDomain.Colpevole:
                        if (targetColpevole == Colpevole.None)
                            Debug.LogWarning($"[Clue '{name}'] Escludente/Colpevole ma targetColpevole == None");
                        targetArma = Arma.None;
                        targetLuogo = Luogo.None;
                        break;

                    case ClueDomain.Arma:
                        if (targetArma == Arma.None)
                            Debug.LogWarning($"[Clue '{name}'] Escludente/Arma ma targetArma == None");
                        targetColpevole = Colpevole.None;
                        targetLuogo = Luogo.None;
                        break;

                    case ClueDomain.Luogo:
                        if (targetLuogo == Luogo.None)
                            Debug.LogWarning($"[Clue '{name}'] Escludente/Luogo ma targetLuogo == None");
                        targetColpevole = Colpevole.None;
                        targetArma = Arma.None;
                        break;

                    case ClueDomain.Trasversale:
                        Debug.LogWarning($"[Clue '{name}'] Escludente ma domain == Trasversale (evita: un escludente deve colpire un asse specifico)");
                        break;
                }
            }
            else
            {
                // Per Ambigui/ScenarioPositivo, i target non devono vincolare
                if (role != ClueRole.Escludente)
                {
                    // Lascia pure tag se vuoi, ma non necessari.
                    // Nessun warning: sono solo "odori" o "tag narrativi"
                }
            }
        }
    }
}
