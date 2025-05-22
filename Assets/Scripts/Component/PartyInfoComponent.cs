using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class PartyInfoComponent : BaseInfoComponent
    {
        [SerializeField] private TextMeshProUGUI limitCount;
        [SerializeField] private TextMeshProUGUI currency;
        [SerializeField] private TextMeshProUGUI evaluationValue;
        [SerializeField] private StatusGaugeAnimation evaluationValueGauge;
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
            if (limitCount != null)
            {
                limitCount.SetText(partyInfo.LimitCount.Value.ToString());
            }
            if (evaluationValue != null)
            {
                evaluationValue.SetText(partyInfo.EvaluationValue.Value.ToString());
            }
            if (evaluationValueGauge != null)
            {
                evaluationValueGauge.UpdateGauge(partyInfo.EvaluationValue.Value * 0.01f);
            }
        }
    }
}
