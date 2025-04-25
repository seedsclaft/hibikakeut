using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.UnitInfoList;

namespace Ryneus
{
    public class UnitInfoListView : BaseView
    {
        [SerializeField] private BaseList unitInfoList = null;
        public UnitInfo SelectUnitInfoList => unitInfoList.ListItemData<UnitInfo>();
        public BattlerInfo SelectBattlerInfo()
        {
            var unitInfoItems = unitInfoList.GetComponentsInChildren<UnitInfoListItem>();
            foreach (var unitInfoItem in unitInfoItems)
            {
                var selectBattlerInfo = unitInfoItem.SelectBattlerInfo();
                if (selectBattlerInfo != null)
                {
                    return selectBattlerInfo;
                }
            }
            return null;
        }
        [SerializeField] private PopupAnimation popupAnimation = null;
        
        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.UnitInfoList);
            InitializeUnitInfoList();
            SetBaseAnimation(popupAnimation);
            _ = new UnitInfoListPresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,() => CallViewEvent(CommandType.EndOpenAnimation));
        }

        private void InitializeUnitInfoList()
        {
            unitInfoList.Initialize();
            unitInfoList.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(CommandType.DecideBattlerInfo,unitInfoList.ListItemData<UnitInfo>()));
            unitInfoList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            unitInfoList.SetInputHandler(InputKeyType.Option1,CallStatus);
            unitInfoList.SetInputHandler(InputKeyType.Right,() => CallViewEvent(CommandType.SelectSubBattler));
            unitInfoList.SetInputHandler(InputKeyType.Left,() => CallViewEvent(CommandType.SelectMainBattler));
            unitInfoList.SetSelectedHandler(() => CallViewEvent(CommandType.InputSelectUnitInfo));
            //unitInfoList.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(UnitInfoList.CommandType.DecideUnit,unitInfoList.ListItemData<UnitInfo>()));
            SetInputHandler(unitInfoList.gameObject);
        }

        public void SetUnitInfoList(List<ListData> unitInfoLists,bool battlerListActivate)
        {
            unitInfoList.SetData(unitInfoLists,false,() =>
            {
                var unitInfoItems = unitInfoList.GetComponentsInChildren<UnitInfoListItem>();
                foreach (var unitInfoItem in unitInfoItems)
                {
                    unitInfoItem.SetDecideBattlerEvent(
                        () => CallViewEvent(CommandType.DecideBattlerInfo,unitInfoList.ListItemData<UnitInfo>()),
                        () => CallViewEvent(CommandType.SelectUnitInfo),
                        CallStatus
                    );
                    unitInfoItem.SetBattlerListActivate(battlerListActivate);
                    unitInfoItem.SetBattlerSelectIndex(-1);
                }
                //unitInfoItems[0].SetBattlerSelectIndex(0);
            });
            //unitInfoList.Activate();
        }

        public void SetDepatureUnitInfoList(List<ListData> unitInfoLists)
        {
            unitInfoList.SetData(unitInfoLists,false,() =>
            {
                var unitInfoItems = unitInfoList.GetComponentsInChildren<UnitInfoListItem>();
                foreach (var unitInfoItem in unitInfoItems)
                {
                    unitInfoItem.UnselectAll();
                    unitInfoItem.SetBattlerListActivate(false);
                    unitInfoItem.SetDecideBattlerEvent(
                        () => {},
                        () => {},
                        CallStatus
                    );
                }
            });
            //unitInfoList.Activate();
        }

        public void SetActiveUnitInfoList()
        {
            var unitInfoItems = unitInfoList.GetComponentsInChildren<UnitInfoListItem>();
            foreach (var unitInfoItem in unitInfoItems)
            {
                unitInfoItem.SetBattlerListActivate(true);
            }
        }

        public void UnselectAll()
        {
            var unitInfoItems = unitInfoList.GetComponentsInChildren<UnitInfoListItem>();
            foreach (var unitInfoItem in unitInfoItems)
            {
                unitInfoItem.UnselectAll();
            }
        }

        public void CommandSelectMainBattler()
        {
            var unitInfoItems = unitInfoList.GetComponentsInChildren<UnitInfoListItem>();
            foreach (var unitInfoItem in unitInfoItems)
            {
                unitInfoItem.SetBattlerSelectIndex(-1);
            }
            unitInfoItems[unitInfoList.Index].SetBattlerSelectIndex(0);
        }

        public void CommandSelectSubBattler()
        {
            var unitInfoItems = unitInfoList.GetComponentsInChildren<UnitInfoListItem>();
            foreach (var unitInfoItem in unitInfoItems)
            {
                unitInfoItem.SetBattlerSelectIndex(-1);
            }
            unitInfoItems[unitInfoList.Index].SetBattlerSelectIndex(1);
        }

        public void CommandInputSelectUnitInfo(BattlerInfo battlerInfo)
        {
            var unitInfoItems = unitInfoList.GetComponentsInChildren<UnitInfoListItem>();
            foreach (var unitInfoItem in unitInfoItems)
            {
                unitInfoItem.SetBattlerSelectIndex(-1);
            }
            unitInfoItems[unitInfoList.Index].SetBattlerSelectIndex(battlerInfo);
        }

        public void CallStatus()
        {
            var unitInfo = unitInfoList.ListItemData<UnitInfo>();
            if (unitInfo != null)
            {
                CallViewEvent(CommandType.CallStatus,unitInfo.BattlerInfos);
            }
        }

    }

    namespace UnitInfoList
    {
        public enum CommandType
        {
            None = 0,
            DecideUnit = 1,
            EndOpenAnimation = 2,
            DecideBattlerInfo = 3,
            SelectUnitInfo = 4,
            SelectMainBattler = 5,
            SelectSubBattler = 6,
            InputSelectUnitInfo = 7,
            CallStatus = 8,
        }
    }
}
