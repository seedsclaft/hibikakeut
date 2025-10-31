using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Ryneus
{
    public class PartyInfoChecker : SingletonMonoBehaviour<PartyInfoChecker>
    {
        [SerializeField] private bool encountZero = false;
        [SerializeField] private bool allLearnSkills = false;
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
            if (partyInfo != null && allLearnSkills)
            {
                var skills = DataSystem.Skills.Where(a => a.Value.Id > 1000);
                foreach (var skill in skills)
                {
                    partyInfo.AddLearningSkill(skill.Value.Id);
                }
                allLearnSkills = false;
            }
        }
    }
}
