using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class TradeItemList : BaseList
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private ItemInfoComponent itemInfoComponent;
        public new void Initialize()
        {
            base.Initialize();
            SetSelectedHandler(() => UpdateItemHelp());
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
            } else
            {
                skillInfoComponent.Clear();
                itemInfoComponent.Clear();
            }
        }

        public override void UpdateHelpWindow()
        {
            UpdateItemHelp();
        }
    }
}