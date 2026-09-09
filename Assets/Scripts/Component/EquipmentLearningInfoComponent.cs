using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class EquipmentLearningInfoComponent : MonoBehaviour
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private GameObject learningRateRoot;
        [SerializeField] private TextMeshProUGUI learningRate;
        [SerializeField] private TextMeshProUGUI learningExp;
        [SerializeField] private StatusGaugeAnimation skillExpGauge;
        [SerializeField] private bool miniDisplay;

        public void UpdateInfo(EquipmentLearningInfo equipmentLearningInfo)
        {
            UIComponent.SetText(learningRate, "x" + equipmentLearningInfo.LearningRate.Value);
            if (equipmentLearningInfo.EquipmentOnly.Value)
            {
                var textId = miniDisplay ? 14231 : 14230;
                UIComponent.SetText(learningRate, DataSystem.GetText(textId));
            }
            UIComponent.SetActive(learningRateRoot, !equipmentLearningInfo.EquipmentOnly.Value);
            UIComponent.SetText(learningExp, equipmentLearningInfo.LearningExp.Value + "%");
            if (skillExpGauge != null)
            {
                skillExpGauge.UpdateGauge(equipmentLearningInfo.LearningExp.Value * 0.01f);
            }
            UpdateData(equipmentLearningInfo.SkillData);
        }

        public void UpdateData(SkillData skillData)
        {
            skillInfoComponent.UpdateData(skillData.Id);
        }
    }
}
