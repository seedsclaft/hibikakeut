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
        [SerializeField] private OnOffButton autoDeckButton = null;
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
            if (autoDeckButton != null)
            {
                autoDeckButton.OnClickAddListener(() => CallViewEvent(CommandType.AutoDeck));
            }
            SetBaseAnimation(popupAnimation);
            _ = new DeckEditPresenter(this);
            SetBackEvent(() => CallViewEvent(CommandType.Back));
        }

        public void OpenAnimation(Action initializeAfter)
        {
            popupAnimation.OpenAnimation(UiRoot.transform, initializeAfter);
        }

        private void InitializePartyUnitList()
        {
            partyUnitList.Initialize();
            partyUnitList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CommandType.SelectBattler, partyUnitList.Index));
            partyUnitList.SetInputHandler(InputKeyType.Option1, () => CallViewEvent(CommandType.AutoDeck));
            partyUnitList.SetInputHandler(InputKeyType.Cancel, () => CallViewEvent(CommandType.Back));
            partyUnitList.SetSelectedHandler(() => CallViewEvent(CommandType.SelectingBattlerInfo, partyUnitList.ListItemData<BattlerInfo>()?.ActorInfo));
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
            actorInfoList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CommandType.DecideBattlerInfo, actorInfoList.ListItemData<ActorInfo>()));
            actorInfoList.SetInputHandler(InputKeyType.Cancel, () => CallViewEvent(CommandType.Back));
            actorInfoList.SetSelectedHandler(() =>
            {
                if (!actorInfoList.Active)
                {
                    return;
                }
                CallViewEvent(CommandType.SelectingActorInfo, actorInfoList.ListItemData<ActorInfo>());
            });
            //unitInfoList.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(UnitInfoList.CommandType.DecideUnit,unitInfoList.ListItemData<UnitInfo>()));
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

        public void CandidateSelectIndex(int selectIndex)
        {
            partyUnitList.UpdateSelectIndexList(new List<int>(){selectIndex});
        }

        public void EndSelectChangeBattler()
        {
            SetActivate(partyUnitList);
            CandidateSelectIndex(-1);
        }

        public void UpdateActorInfo(ActorInfo actorInfo)
        {
            actorInfoComponent.UpdateInfo(actorInfo, null);
        }

        public void EndPopup()
        {
            partyUnitList.UpdateSelectIndex(0);
            actorInfoList.UpdateSelectIndex(-1);
            CandidateSelectIndex(-1);
            PopupClose();
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
            AutoDeck,
            Back,
        }
    }
}
