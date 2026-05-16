using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class EquipmentLearningInfoComponent : MonoBehaviour
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private TextMeshProUGUI learningRate;
        [SerializeField] private TextMeshProUGUI learningExp;
        [SerializeField] private GameObject learnd;
        [SerializeField] private StatusGaugeAnimation skillExpGauge;

        public void UpdateInfo(EquipmentLearningInfo equipmentLearningInfo)
        {
            UIComponent.SetText(learningRate, "x" + equipmentLearningInfo.LearningRate.Value);
            UIComponent.SetText(learningExp, equipmentLearningInfo.LearningExp.Value + "%");
            if (skillExpGauge != null)
            {
                skillExpGauge.UpdateGauge(equipmentLearningInfo.LearningExp.Value * 0.01f);
            }
            UIComponent.SetActive(learnd, equipmentLearningInfo.LearningExp.Value >= 100);
            UpdateData(equipmentLearningInfo.Master);
        }

        public void UpdateData(SkillData skillData)
        {
            skillInfoComponent.UpdateData(skillData.Id);
        }
    }
}
