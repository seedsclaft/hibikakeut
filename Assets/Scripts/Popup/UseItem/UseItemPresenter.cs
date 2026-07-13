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
            Initialize(true);
        }

        private void Initialize(bool first)
        {
            _model = new UseItemModel();
            SetModel(_model);
             _view.OpenAnimation(first ? InitializeAfter : null);
            if (!first)
            {
                InitializeAfter();
            }
            _busy = false;
        }

        private void InitializeAfter()
        {
            CommandRefresh();
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
                    Initialize(false);
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
            if (!_model.EnableUse(itemInfo) || !_model.CanUseItem(itemInfo))
            {
                CommandCautionInfo(DataSystem.GetText(42040));
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                return;
            }
            if (_model.CanUseItem(itemInfo))
            {
                _model.PartyInfo.ConsuneItemNum(itemInfo.Id.Value, 1);
            }

            switch ((UseItemType)itemInfo.Master.Param1)
            {
                case UseItemType.EncountRate:
                    UseItemEncountRate(itemInfo);
                    break;
                case UseItemType.DungeonTurn:
                    UseItemDungeonTurn(itemInfo);
                    break;
                case UseItemType.Heal:
                    UseItemHeal(itemInfo);
                    break;
                case UseItemType.Exp:
                    UseItemExp(itemInfo);
                    break;
                case UseItemType.AttributeUp:
                    UseItemAttributeUp(itemInfo);
                    break;
                case UseItemType.StatusUp:
                    UseItemStatusUp(itemInfo);
                    break;
                case UseItemType.ClassChange:
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

            CallConfirmNoChoiceView(DataSystem.GetText(42020), (a) =>
            {
                _busy = false;
            });
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
            CallCommandOther(ViewCommandSceneType.Dungeon, Dungeon.CommandType.UseItemHeal, heal);
        }

        private void UseItemExp(ItemInfo itemInfo)
        {
            var getExpValue = _model.GetExpValue(itemInfo);
            _busy = true;
            _model.PartyInfo.PartyStatInfo.TacticsLvupCount.GainValue(1);
            CommandExpUp(_model.CurrentActor, getExpValue, () =>
            {
                CheckAchievements();
                _busy = false;
            });
        }

        private void UseItemAttributeUp(ItemInfo itemInfo)
        {
            var getAttibute = (AttributeType)itemInfo.Master.Param2;
            _busy = true;
            CommandAttributeUp(_model.CurrentActor, getAttibute, () =>
            {
                CheckAchievements();
                _busy = false;
            });
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
            Func<ItemInfo, bool> enable = (itemInfo) =>
            {
                // 使用可能か
                return _model.EnableUse(itemInfo) && _model.CanUseItem(itemInfo);
            };
            _view.SetUseItem(MakeListData(_model.UseItemInfos(), enable, null));
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}