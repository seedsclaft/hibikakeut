using Unity.VisualScripting;
using UnityEngine;

namespace Ryneus
{
    public class PartyInfoChecker : SingletonMonoBehaviour<PartyInfoChecker>
    {
        [SerializeField] private bool encountZero = false;
        [SerializeField] private PartyInfo partyInfo = null;
        [SerializeField] private DeckInfo deckInfo = null;
        public void UpdateInfo()
        {
            partyInfo = GameSystem.GameInfo.PartyInfo;
            deckInfo = partyInfo.CurrentDeckInfo;
        }

        private void Update()
        {
            if (deckInfo != null && encountZero)
            {
                deckInfo.Encount.SetValue(0);
            }
        }
    }
}
