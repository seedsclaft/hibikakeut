using System.Linq;
using UnityEngine;

namespace Ryneus
{
    public class PartyInfoChecker : SingletonMonoBehaviour<PartyInfoChecker>
    {
        [SerializeField] private bool encountZero = false;
        [SerializeField] private bool allLearnSkills = false;
        [SerializeField] private bool getAllItems = false;
        [SerializeField] private bool clearAchivements = false;
        [SerializeField] private PartyInfo partyInfo = null;
        [SerializeField] private DeckInfo deckInfo = null;
        public void UpdateInfo()
        {
            partyInfo = GameSystem.GameInfo.PartyInfo;
            deckInfo = partyInfo.CurrentDeckInfo;
        }

        private void Update()
        {
            UpdateEncountZero();
            UpdateAllLearnSkills();
            UpdateGetAllItems();
            UpdateClearAchivements();
        }

        private void UpdateEncountZero()
        {
            if (deckInfo != null && encountZero)
            {
                deckInfo.Encount.SetValue(0);
            }
        }

        private void UpdateAllLearnSkills()
        {
            if (partyInfo == null)
            {
                return;
            }
            if (allLearnSkills)
            {
                var skills = DataSystem.Skills.Where(a => a.Value.Id > 1000 && a.Value.Rank > 0);
                foreach (var skill in skills)
                {
                    partyInfo.AddLearningSkill(skill.Value.Id);
                }
                allLearnSkills = false;
            }
        }

        private void UpdateGetAllItems()
        {
            if (partyInfo == null)
            {
                return;
            }
            if (getAllItems)
            {
                foreach (var item in DataSystem.Items)
                {
                    var getItemData = new GetItemData
                    {
                        Type = GetItemType.Item,
                        Param1 = item.Id,
                        Param2 = 99
                    };
                    var getitemInfo = new GetItemInfo(getItemData);
                    partyInfo.AddGetItemInfo(getitemInfo);
                }
                getAllItems = false;
            }
        }

        private void UpdateClearAchivements()
        {
            if (partyInfo == null)
            {
                return;
            }
            if (clearAchivements)
            {
                var achievementInfos = partyInfo.AchievementInfos;
                foreach (var achievementInfo in achievementInfos)
                {
                    if (achievementInfo.Master.Rank != partyInfo.MissionRank.Value)
                    {
                        continue;
                    }
                    achievementInfo.Achieved.SetValue(true);
                }
                clearAchivements = false;
            }
        }
    }
}
