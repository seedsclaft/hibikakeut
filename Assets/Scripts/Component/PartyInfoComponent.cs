using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class PartyInfoComponent : BaseInfoComponent
    {
        [SerializeField] private TextMeshProUGUI period;
        [SerializeField] private TextMeshProUGUI chapter;
        [SerializeField] private TextMeshProUGUI currency;
        [SerializeField] private TextMeshProUGUI evaluationValue;
        [SerializeField] private TextMeshProUGUI turnCount;
        [SerializeField] private TextMeshProUGUI missionRank;
        [SerializeField] private StatusGaugeAnimation evaluationValueGauge;
        [SerializeField] private TextMeshProUGUI victoryBonus;
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
                var periodValue = partyInfo.Period.Value > 6 ? 6 :partyInfo.Period.Value;
                // 6超えてても6で表示
                period.SetText(periodValue.ToString());
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
                victoryBonus.SetText("x" + (1 + (0.1f * partyInfo.VictoryBonusCount.Value)).ToString());
            }
        }
    }
}
