using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.UseItem;

namespace Ryneus
{
    public class UseItemPresenter : BasePresenter
    {
        UseItemModel _model = null;
        UseItemView _view = null;

        private bool _busy = true;
        public UseItemPresenter(UseItemView view)
        {
            _view = view;

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize();
        }

        private void Initialize()
        {
            _model = new UseItemModel();
            SetModel(_model);
            _view.SetUseItem(MakeListData(_model.DungeonUseItemInfos(), 0));
            _view.OpenAnimation();
            CommandRefresh();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.UseItem)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.Initialize:
                    Initialize();
                    break;
                case CommandType.DecideUseItem:
                    CommandDecideUseItem((ItemInfo)viewEvent.Template);
                    break;
                case CommandType.CommandBack:
                    CommandBack();
                    break;
            }
        }

        private void CommandDecideUseItem(ItemInfo itemInfo)
        {
            if (itemInfo == null)
            {
                return;
            }
            if (_model.CanUseItem(itemInfo))
            {
                _model.PartyInfo.ConsuneItemNum(itemInfo.Id.Value, 1);
            }

            switch (itemInfo.Master.Param1)
            {
                case (int)UseItemType.EncountRate:
                    UseItemEncountRate(itemInfo);
                    break;
                case (int)UseItemType.DungeonTurn:
                    UseItemDungeonTurn(itemInfo);
                    break;
                case (int)UseItemType.Heal:
                    UseItemHeal(itemInfo);
                    break;
                case (int)UseItemType.Exp:
                    UseItemExp(itemInfo);
                    break;
                case (int)UseItemType.AttributeUp:
                    UseItemAttributeUp(itemInfo);
                    break;
                case (int)UseItemType.StatusUp:
                    UseItemStatusUp(itemInfo);
                    break;
                case (int)UseItemType.ClassChange:
                    UseItemClassChange(itemInfo);
                    break;
            }
            CommandRefresh();
        }

        private void UseItemEncountRate(ItemInfo itemInfo)
        {
            var encountRate = itemInfo.Master.Param2;
            var encountTurn = itemInfo.Master.Param3;
            _model.ChangeEncountRate(encountRate, encountTurn);
            _busy = true;

            var textId = encountRate > 100 ? 42010 : 42011;
            CallConfirmNoChoiceView(DataSystem.GetText(textId), (a) =>
            {
                _busy = false;
            });
        }

        private void UseItemDungeonTurn(ItemInfo itemInfo)
        {
            var turns = itemInfo.Master.Param2;
            _model.ChangeDungeonTurn(turns);
            _busy = true;

            var confirmInfo = new ConfirmInfo(DataSystem.GetText(42020), (a) =>
            {
                _busy = false;
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UseItemHeal(ItemInfo itemInfo)
        {
            if (!_model.CanUseRecoveryHeal())
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(10101));
                return;
            }
            var heal = itemInfo.Master.Param2;
            SoundManager.Instance.PlayStaticSe(SEType.Heal);
            _model.UseItemHeal(heal);
            _busy = true;

            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(42030, heal.ToString()), (a) =>
            {
                _busy = false;
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
            _view.UseItemHeal(heal);
        }

        private void UseItemExp(ItemInfo itemInfo)
        {
            var getExp = itemInfo.Master.Param2;
            if (_model.CurrentActor.Level <= itemInfo.Master.Param3)
            {
                getExp *= 2;
            }
            _busy = true;
            _model.PartyInfo.PartyStatInfo.TacticsLvupCount.GainValue(1);
            CommandExpUp(_model.CurrentActor, getExp, () =>
            {
                CheckAchievements();
                _busy = false;
                //CommandRefreshMagicList(false);
            });
            //CommandRefreshuseItemList();
        }

        private void UseItemAttributeUp(ItemInfo itemInfo)
        {
            var getAttibute = (AttributeType)itemInfo.Master.Param2;
            _busy = true;
            CommandAttributeUp(_model.CurrentActor, getAttibute, () =>
            {
                CheckAchievements();
                _busy = false;
                //CommandRefreshMagicList(false);
            });
            //CommandRefreshuseItemList();
        }

        private void UseItemStatusUp(ItemInfo itemInfo)
        {
            var statusType = (StatusParamType)itemInfo.Master.Param2;
            _busy = true;
            CommandStatusUp(_model.CurrentActor, statusType, itemInfo.Master.Param2, () =>
            {
                CheckAchievements();
                _busy = false;
                CommandRefresh();
            });
            //CommandRefreshuseItemList();
        }

        private void UseItemClassChange(ItemInfo itemInfo)
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.LevelUp);
            var beforeStatus = new StatusInfo();
            beforeStatus.SetParameter(_model.CurrentActor.CurrentStatus);
            var ClassChangeInfo = new ClassChangeInfo(_model.CurrentActor, beforeStatus);
            _model.CurrentActor.IsClassChenged.SetValue(true);
            CallPopupView(PopupType.ClassChange, () =>
            {
                CheckAchievements();
                _busy = false;
            }, ClassChangeInfo);
        }

        private void CommandBack()
        {
            _view.BackEvent();
        }

        private void CommandRefresh()
        {
            _view.SetUseItem(MakeListData(_model.DungeonUseItemInfos(), 0));
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}