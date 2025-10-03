using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.DeckEdit;

namespace Ryneus
{
    public class DeckEditView : BaseView
    {
        [SerializeField] private BattleBattlerList partyUnitList = null;
        [SerializeField] private BaseList actorInfoList = null;
        [SerializeField] private ActorInfoComponent actorInfoComponent = null;
        [SerializeField] private PopupAnimation popupAnimation = null;

        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.DeckEdit);
            InitializeActorInfoList();
            InitializePartyUnitList();
            SetBaseAnimation(popupAnimation);
            _ = new DeckEditPresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => CallViewEvent(CommandType.EndOpenAnimation));
        }

        private void InitializePartyUnitList()
        {
            partyUnitList.Initialize();
            partyUnitList.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(CommandType.SelectBattler,partyUnitList.Index));
            partyUnitList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            partyUnitList.SetSelectedHandler(() => CallViewEvent(CommandType.SelectingBattlerInfo,partyUnitList.ListItemData<BattlerInfo>()?.ActorInfo));
            AddViewActives(partyUnitList);
        }

        public void SetPartyUnitList(List<ListData> listDatas)
        {
            if (partyUnitList.Index > -1)
            {
                listDatas[partyUnitList.Index].Selected.SetValue(true);
            }
            partyUnitList.SetData(listDatas);
        }

        private void InitializeActorInfoList()
        {
            actorInfoList.Initialize();
            actorInfoList.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(CommandType.DecideBattlerInfo,actorInfoList.ListItemData<ActorInfo>()));
            actorInfoList.SetInputHandler(InputKeyType.Cancel,() => CallViewEvent(CommandType.Back));
            actorInfoList.SetSelectedHandler(() => CallViewEvent(CommandType.SelectingActorInfo,actorInfoList.ListItemData<ActorInfo>()));
            //unitInfoList.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(UnitInfoList.CommandType.DecideUnit,unitInfoList.ListItemData<UnitInfo>()));
            SetInputHandler(actorInfoList);
            AddViewActives(actorInfoList);
        }

        public void SetActorList(List<ListData> listDatas)
        {
            actorInfoList.SetData(listDatas);
        }

        public void SelectChangeBattler(int selectIndex)
        {
            SetActivate(actorInfoList);
            actorInfoList.UpdateSelectIndex(selectIndex);
        }

        public void EndSelectChangeBattler()
        {
            SetActivate(partyUnitList);
        }

        public void UpdateActorInfo(ActorInfo actorInfo)
        {
            actorInfoComponent.UpdateInfo(actorInfo,null);
        }

        public void CallStatus()
        {
            /*
            var unitInfo = unitInfoList.ListItemData<UnitInfo>();
            if (unitInfo != null)
            {
                CallViewEvent(CommandType.CallStatus,unitInfo.BattlerInfos);
            }
            */
        }
    }

    namespace DeckEdit
    {
        public enum CommandType
        {
            Initialize,
            SelectBattler,
            DecideBattlerInfo,
            SelectingActorInfo,
            SelectingBattlerInfo,
            EndOpenAnimation,
            Back,
        }
    }
}
