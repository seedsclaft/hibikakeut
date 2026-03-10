using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class PartyInfoComponent : BaseInfoComponent
    {
        [SerializeField] private TextMeshProUGUI period;
        [SerializeField] private TextMeshProUGUI periodLimit;
        [SerializeField] private TextMeshProUGUI chapter;
        [SerializeField] private TextMeshProUGUI currency;
        [SerializeField] private TextMeshProUGUI evaluationValue;
        [SerializeField] private TextMeshProUGUI evaluationAddictValue;
        [SerializeField] private TextMeshProUGUI turnCount;
        [SerializeField] private TextMeshProUGUI missionRank;
        [SerializeField] private StatusGaugeAnimation evaluationValueGauge;
        [SerializeField] private TextMeshProUGUI victoryBonus;
        [SerializeField] private AchievementInfoComponent achievementInfoComponent;
        [SerializeField] private TextMeshProUGUI dungeonCompletionRate;
        [SerializeField] private GameObject encountRateRoot;
        [SerializeField] private TextMeshProUGUI encountRate;
        [SerializeField] private GameObject routeModeRoot;
        [SerializeField] private TextMeshProUGUI routeMode;
        [SerializeField] private TextMeshProUGUI recoveryCount;
        public void UpdateCurrentInfo()
        {
            var current = PartyInfo;
            if (current != null)
            {
                UpdateInfo(current);
            }
        }

        public void UpdateInfo(PartyInfo partyInfo)
        {
            UIComponent.SetText(currency, partyInfo.Currency);

            var periodValue = Mathf.Min(partyInfo.Period.Value, DataSystem.System.PeriodTurns);
            // 表示ターン数限度は超えないように表示
            UIComponent.SetText(period, periodValue.ToString());

            UIComponent.SetText(periodLimit, DataSystem.System.PeriodTurns);
            UIComponent.SetText(chapter, partyInfo.Chapter);
            UIComponent.SetText(evaluationValue, partyInfo.EvaluationValue);
            if (evaluationAddictValue != null)
            {
                var evaluationAddict = partyInfo.EvaluationAddictValue();
                UIComponent.SetActive(evaluationAddictValue.gameObject, evaluationAddict != 0);
                UIComponent.SetText(evaluationAddictValue, "(" + evaluationAddict.ToString() + ")");
            }
            if (evaluationValueGauge != null)
            {
                evaluationValueGauge.UpdateGauge(partyInfo.EvaluationValue.Value * 0.01f);
            }
            UIComponent.SetText(missionRank, partyInfo.MissionRank);
            UIComponent.SetText(victoryBonus, (partyInfo.PartyStatInfo.BattleScore.Value * 0.01f).ToString("F2") + "%");

            if (achievementInfoComponent != null)
            {
                achievementInfoComponent.UpdateInfo(partyInfo.NearAchievementInfo());
            }
            UIComponent.SetText(dungeonCompletionRate, partyInfo.DungeonCompletionRate().ToString("F2") + "%");

            UIComponent.SetActive(encountRateRoot, partyInfo.CurrentDeckInfo.EncountRate.Value != 1);
            UIComponent.SetText(encountRate, partyInfo.CurrentDeckInfo.EncountRateText());
            UIComponent.SetActive(routeModeRoot, partyInfo.CurrentDeckInfo.RoutePaths.Count > 0);
            UIComponent.SetText(recoveryCount, partyInfo.CurrentDeckInfo.RecoveryCount.Value);
        }
    }
}
