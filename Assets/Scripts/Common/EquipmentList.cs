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
    }
}
