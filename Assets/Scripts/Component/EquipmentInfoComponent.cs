using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Ryneus
{
    public class EquipmentInfoComponent : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image iconBack;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private BaseList learningDateList;
        [SerializeField] private GameObject selectEquipment;
        [SerializeField] private GameObject equipmentedObj;
        [SerializeField] private ActorInfoComponent equipmentedActor;
        public BaseList LearningDateList => learningDateList;
        public void UpdateInfo(EquipmentInfo equipmentInfo)
        {
            UpdateData(equipmentInfo.Master);
            if (learningDateList != null)
            {
                learningDateList.SetData(ListData.MakeListData(equipmentInfo.LearningInfos));
            }
            if (equipmentedActor != null)
            {
                UIComponent.SetActive(equipmentedObj, equipmentInfo.EquipmentActor != null);
                equipmentedActor.UpdateData(equipmentInfo.EquipmentActor?.Master);
            }
        }

        public void UpdateData(EquipmentData equipmentData)
        {
            UIComponent.SetText(nameText, equipmentData.Name);
            UpdateSkillIcon(equipmentData.IconIndex);
            UpdateSkillIconBack(equipmentData.Attribute);
        }

        private void UpdateSkillIcon(MagicIconType iconIndex)
        {
            UIComponent.SetActive(icon, true);
            var spriteAtlas = ResourceSystem.LoadSpellIcons();
            if (icon != null)
            {
                icon.sprite = spriteAtlas.GetSprite(iconIndex.ToString());
            }
        }

        private void UpdateSkillIconBack(AttributeType attributeType)
        {
            UIComponent.SetActive(iconBack, true);
            if (iconBack != null)
            {
                iconBack.sprite = ResourceSystem.LoadSpellIconBase(attributeType);
            }
        }

        public void Clear()
        {
            UIComponent.SetText(nameText, "");
            if (learningDateList != null)
            {
                var list = new List<EquipmentLearningInfo>();
                learningDateList.SetData(ListData.MakeListData(list));
            }
        }

        public void SetSelectEquipment(bool isSelect)
        {
            UIComponent.SetActive(selectEquipment, isSelect);
        }
    }
}
