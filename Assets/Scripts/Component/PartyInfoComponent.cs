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
            if (currency != null)
            {
                currency.SetText(partyInfo.Currency.Value.ToString());
            }
            if (period != null)
            {
                var periodValue = Mathf.Min(partyInfo.Period.Value, DataSystem.System.PeriodTurns);
                // 表示ターン数限度は超えないように表示
                period.SetText(periodValue.ToString());
            }
            if (periodLimit != null)
            {
                periodLimit.SetText(DataSystem.System.PeriodTurns.ToString());
            }
            if (chapter != null)
            {
                chapter.SetText(partyInfo.Chapter.Value.ToString());
            }
            if (turnCount != null)
            {
                turnCount.SetText(partyInfo.TurnCount.Value.ToString());
            }
            if (evaluationValue != null)
            {
                evaluationValue.SetText(partyInfo.EvaluationValue.Value.ToString());
            }
            if (evaluationAddictValue != null)
            {
                var evaluationAddict = partyInfo.EvaluationAddictValue();
                evaluationAddictValue.gameObject.SetActive(evaluationAddict != 0);
                evaluationAddictValue.SetText("(" + evaluationAddict.ToString() + ")");
            }
            if (evaluationValueGauge != null)
            {
                evaluationValueGauge.UpdateGauge(partyInfo.EvaluationValue.Value * 0.01f);
            }
            if (missionRank != null)
            {
                missionRank.SetText(partyInfo.MissionRank.Value.ToString());
            }
            if (victoryBonus != null)
            {
                victoryBonus.SetText((partyInfo.BattleScore.Value * 0.01f).ToString("F2") + "%");
            }
            if (achievementInfoComponent != null)
            {
                achievementInfoComponent.UpdateInfo(partyInfo.NearAchievementInfo());
            }
            if (dungeonCompletionRate != null)
            {
                dungeonCompletionRate.SetText(partyInfo.DungeonCompletionRate().ToString("F2") + "%");
            }
            if (encountRateRoot != null)
            {
                encountRateRoot.SetActive(partyInfo.CurrentDeckInfo.EncountRate.Value != 1);
            }
            if (encountRate != null)
            {
                encountRate.SetText(partyInfo.CurrentDeckInfo.EncountRateText());
            }
            if (routeModeRoot != null)
            {
                routeModeRoot.SetActive(partyInfo.CurrentDeckInfo.RoutePaths.Count > 0);
            }
        }
    }
}
