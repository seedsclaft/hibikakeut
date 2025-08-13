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
            } else
            {
                itemInfoComponent.Clear();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            itemInfoComponent.gameObject.SetActive(true);
            UpdateItemHelp();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            itemInfoComponent.gameObject.SetActive(false);
        }

        public override void UpdateHelpWindow()
        {
            UpdateItemHelp();
        }
    }
}
