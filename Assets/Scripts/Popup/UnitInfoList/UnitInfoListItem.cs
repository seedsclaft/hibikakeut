using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class UnitInfoListItem : ListItem ,IListViewItem  
    {   
        [SerializeField] private UnitInfoComponent component;
        [SerializeField] private BaseList battlerList;
        public BattlerInfo SelectBattlerInfo()
        {
            return battlerList.ListItemData<BattlerInfo>();
        }
        private bool _isInit = false;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<UnitInfo>();
            component.UpdateInfo(data);
            battlerList.Initialize();
            battlerList.SetData(ListData.MakeListData(data.BattlerInfos));
            battlerList.Activate();
        }

        public void SetDecideBattlerEvent(Action decideEvent,Action rightEvent)
        {
            if (_isInit)
            {
                return;
            }
            battlerList.SetInputHandler(InputKeyType.Decide,() => decideEvent?.Invoke());
            battlerList.SetSelectedHandler(() => 
            {
                if (battlerList.Index == 0)
                {
                    rightEvent?.Invoke();
                } else
                if (battlerList.Index == 1)
                {
                    rightEvent?.Invoke();
                }
            });
            _isInit = true;
        }


        public void SetBattlerSelectIndex(int selectIndex)
        {
            battlerList.UpdateSelectIndex(selectIndex);
        }

        public void SetBattlerSelectIndex(BattlerInfo battlerInfo)
        {
            battlerList.UpdateSelectIndex(battlerList.ListDates.FindIndex(a => a.Data != null && a.Data == battlerInfo));
        }

        public void UnselectAll()
        {
            battlerList.UnselectAll();
        }

        public void ListDeactivate()
        {
            battlerList.Deactivate();
        }
    }
}
