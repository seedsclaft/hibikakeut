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
            UIComponent.SetText(count, achievementInfo.Count);
            UIComponent.SetText(achieveCount, achievementInfo.AchieveCount);
            if (achivePer != null)
            {
                achivePer.UpdateGauge(achievementInfo.AchievePer);
            }
            if (getItemInfoComponent != null && achievementInfo.Master.PriseSetId > 0)
            {
                UIComponent.SetActive(getItemInfoComponent.gameObject, true);
                getItemInfoComponent.UpdateInfo(new GetItemInfo(achievementInfo.PrizeSetsMaster[0].GetItem));
            }
        }

        private void UpdateDate(AchievementData achievementData)
        {
            UIComponent.SetActive(categoryMain, achievementData.Category == AchievementCategory.Main);
            UIComponent.SetActive(categoryNormal, achievementData.Category == AchievementCategory.Normal);
            UIComponent.SetText(rank, achievementData.Rank);
            UIComponent.SetText(description, achievementData.GetText());
        }
    }
}
