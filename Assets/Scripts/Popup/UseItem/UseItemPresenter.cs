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
            /*
            if (!_model.CanUseItem(itemInfo))
            {
                return;
            }
            */
            _model.PartyInfo.ConsuneItemNum(itemInfo.Id.Value, 1);

            switch (itemInfo.Master.Param1)
            {
                case (int)UseItemType.EncountRate:
                    UseItemEncountRate(itemInfo);
                    break;
                case (int)UseItemType.DungeonTurn:
                    UseItemDungeonTurn(itemInfo);
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
            _view.SetBusy(true);

            var textId = encountRate > 100 ? 42010 : 42011;
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(textId), (a) =>
            {
                _busy = false;
                _view.SetBusy(false);
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UseItemDungeonTurn(ItemInfo itemInfo)
        {
            var turns = itemInfo.Master.Param2;
            _model.ChangeDungeonTurn(turns);
            _busy = true;
            _view.SetBusy(true);

            var confirmInfo = new ConfirmInfo(DataSystem.GetText(42020), (a) =>
            {
                _busy = false;
                _view.SetBusy(false);
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
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