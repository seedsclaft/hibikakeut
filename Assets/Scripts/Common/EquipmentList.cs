using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class EquipmentList : BaseList
    {
        [SerializeField] private EquipmentInfoComponent equipmentInfoComponent;
        public new void Initialize()
        {
            base.Initialize();
            equipmentInfoComponent.LearningDateList.Initialize();
            SetSelectedHandler(() => UpdateSkillHelp());
        }

        public void UpdateSkillHelp()
        {
            if (equipmentInfoComponent == null)
            {
                return;
            }
            var listData = ListData;
            if (listData != null)
            {
                var equipmentInfo = (EquipmentInfo)listData.Data;
                if (equipmentInfo.LearningInfos.Count > 0)
                {
                    equipmentInfoComponent.UpdateInfo(equipmentInfo);
                }
                else
                {
                    equipmentInfoComponent.Clear();
                }
            }
            else
            {
                equipmentInfoComponent.Clear();
            }
        }

        public override void UpdateHelpWindow()
        {
            UpdateSkillHelp();
        }

        public void UpdateSelectIndexList(List<int> indexes)
        {
            for (int i = 0; i < ItemPrefabList.Count; i++)
            {
                if (ItemPrefabList[i] == null)
                {
                    continue;
                }
                var listItem = ItemPrefabList[i].GetComponentInChildren<ListItem>();
                if (listItem == null || listItem.ListData == null)
                {
                    continue;
                }
                var infoComponent = ItemPrefabList[i].GetComponent<EquipmentInfoComponent>();
                EquipmentInfo equipmentInfo = (EquipmentInfo)ListDates[i].Data;
                if (equipmentInfo == null || infoComponent == null)
                {
                    continue;
                }
                if (indexes.Contains(equipmentInfo.EquipmentId.Value) && equipmentInfo.EquipmentId.Value > 10)
                {
                    infoComponent.SetSelectEquipment(true);
                }
                else
                {
                    infoComponent.SetSelectEquipment(false);
                }
            }
        }
    }
}
