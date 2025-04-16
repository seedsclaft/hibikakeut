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
        private UnitInfo _unitInfo;
        private bool _isInit = false;
        private List<BattlerInfo> _battlerInfos = null;
        private bool _selectMain = true;
        public BattlerInfo SelectedBattlerInfo()
        {
            if (_selectMain && _battlerInfos.Count > 0)
            {
                return _battlerInfos[0];
            }
            if (!_selectMain && _battlerInfos.Count > 0)
            {
                return _battlerInfos[1];
            }
            return null;
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<UnitInfo>();
            _unitInfo = data;
            component.UpdateInfo(data);
            battlerList.Initialize();
            battlerList.SetData(ListData.MakeListData(data.BattlerInfos));
            battlerList.Activate();
            _battlerInfos = data.BattlerInfos;
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
