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
        public BaseList LearningDateList => learningDateList;
        public void UpdateInfo(EquipmentInfo equipmentInfo)
        {
            UpdateData(equipmentInfo.Master);
            if (learningDateList != null)
            {
                learningDateList.SetData(ListData.MakeListData(equipmentInfo.LearningInfos));
            }
        }

        public void UpdateData(EquipmentData equipmentData)
        {
            UIComponent.SetText(nameText, equipmentData.Name);
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
    }
}
