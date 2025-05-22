using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class AchievementInfoComponent : BaseInfoComponent
    {
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI count;
        [SerializeField] private TextMeshProUGUI achieveCount;
        [SerializeField] private StatusGaugeAnimation achivePer;

        public void UpdateInfo(AchievementInfo achievementInfo)
        {
            UpdateDate(achievementInfo.Master);
            if (count != null)
            {
                count.SetText(achievementInfo.Count.Value.ToString());
            }
            if (achieveCount != null)
            {
                achieveCount.SetText(achievementInfo.AchieveCount.Value.ToString());
            }
            if (achivePer != null)
            {
                achivePer.UpdateGauge(achievementInfo.AchievePer);
            }
        }

        private void UpdateDate(AchievementData achievementData)
        {
            if (description != null)
            {
                description.SetText(achievementData.Text);
            }
        }
    }
}
