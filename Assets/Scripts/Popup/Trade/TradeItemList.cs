using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class TradeItemList : BaseList
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private ItemInfoComponent itemInfoComponent;
        [SerializeField] private EquipmentInfoComponent equipmentInfoComponent;
        public new void Initialize()
        {
            base.Initialize();
            SetSelectedHandler(() => UpdateItemHelp());
            equipmentInfoComponent.LearningDateList.Initialize();
        }

        public void UpdateItemHelp()
        {
            if (skillInfoComponent == null)
            {
                return;
            }
            var listData = ListData;
            if (listData != null)
            {
                var tradeItemInfo = (TradeItemInfo)listData.Data;
                var skillInfo = tradeItemInfo.SkillInfo();
                var itemInfo = tradeItemInfo.ItemInfo();
                var equipmentInfo = tradeItemInfo.EquipmentInfo();
                if (skillInfo != null)
                {
                    skillInfoComponent.UpdateInfo(skillInfo);
                } else
                {
                    skillInfoComponent.Clear();
                }
                if (itemInfo != null)
                {
                    itemInfoComponent.UpdateInfo(itemInfo);
                } else
                {
                    itemInfoComponent.Clear();
                }
                if (equipmentInfo != null)
                {
                    equipmentInfoComponent.UpdateInfo(equipmentInfo);
                } else
                {
                    equipmentInfoComponent.Clear();
                }
            } else
            {
                skillInfoComponent.Clear();
                itemInfoComponent.Clear();
                equipmentInfoComponent.Clear();
            }
        }

        public override void UpdateHelpWindow()
        {
            UpdateItemHelp();
        }
    }
}