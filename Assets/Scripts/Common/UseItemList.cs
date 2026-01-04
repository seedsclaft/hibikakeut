using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class UseItemList : BaseList
    {
        [SerializeField] private ItemInfoComponent itemInfoComponent;
        public new void Initialize()
        {
            base.Initialize();
            SetSelectedHandler(() => UpdateItemHelp());
        }

        public void UpdateItemHelp()
        {
            if (itemInfoComponent == null)
            {
                return;
            }
            var listData = ListData;
            if (listData != null)
            {
                var itemInfo = (ItemInfo)listData.Data;
                itemInfoComponent.UpdateInfo(itemInfo);
            }
            else
            {
                itemInfoComponent.Clear();
            }
        }

        public override void UpdateHelpWindow()
        {
            UpdateItemHelp();
        }
    }
}
