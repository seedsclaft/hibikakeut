using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class AchievementInfoComponent : BaseInfoComponent
    {
        [SerializeField] private GameObject categoryMain;
        [SerializeField] private GameObject categoryNormal;
        [SerializeField] private TextMeshProUGUI rank;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI count;
        [SerializeField] private TextMeshProUGUI achieveCount;
        [SerializeField] private StatusGaugeAnimation achivePer;
        [SerializeField] private GetItemInfoComponent getItemInfoComponent;

        public void UpdateInfo(AchievementInfo achievementInfo)
        {
            UpdateDate(achievementInfo.Master);
            if (achievementInfo.Master.ConditionType == AchievementConditionType.BattleScore)
            {
                if (count != null)
                {
                    count.SetText((achievementInfo.Count.Value * 0.01f).ToString("F1"));
                }
                if (achieveCount != null)
                {
                    achieveCount.SetText((achievementInfo.AchieveCount.Value*0.01f).ToString());
                }
            }
            else
            {
                if (count != null)
                {
                    count.SetText(achievementInfo.Count.Value.ToString());
                }
                if (achieveCount != null)
                {
                    achieveCount.SetText(achievementInfo.AchieveCount.Value.ToString());
                }
            }
            if (achivePer != null)
            {
                achivePer.UpdateGauge(achievementInfo.AchievePer);
            }
            if (getItemInfoComponent != null && achievementInfo.Master.PriseSetId > 0)
            {
                getItemInfoComponent.gameObject.SetActive(true);
                getItemInfoComponent.UpdateInfo(new GetItemInfo(achievementInfo.PrizeSetsMaster[0].GetItem));
            }
        }

        private void UpdateDate(AchievementData achievementData)
        {
            if (categoryMain != null)
            {
                categoryMain.SetActive(achievementData.Category == AchievementCategory.Main);
            }
            if (categoryNormal != null)
            {
                categoryNormal.SetActive(achievementData.Category == AchievementCategory.Normal);
            }
            if (rank != null)
            {
                rank.SetText(achievementData.Rank.ToString());
            }
            if (description != null)
            {
                description.SetText(achievementData.Text);
            }
        }
    }
}
