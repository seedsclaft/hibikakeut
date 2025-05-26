using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.ItemList;

namespace Ryneus
{
    public class ItemListPresenter : BasePresenter
    {
        ItemListModel _model = null;
        ItemListView _view = null;

        private bool _busy = true;
        public ItemListPresenter(ItemListView view)
        {
            _view = view;
            _model = new ItemListModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("CHARACTER_LIST");
            _view.SetItemList(MakeListData(_model.ItemInfoss(), 0));
            _view.OpenAnimation();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.ItemList)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.DecideItem:
                    CommandDecideItem();
                    break;
                case CommandType.PlusUseNum:
                    CommandPlusUseNum((int)viewEvent.Template);
                    break;
                case CommandType.MinusUseNum:
                    CommandMinusUseNum((int)viewEvent.Template);
                    break;
            }
        }

        private void CommandDecideItem()
        {
            if (_model.CanPresent())
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var confirmInfo = new ConfirmInfo("献上しますか？", (a) =>
                {
                    if (a == ConfirmCommandType.Yes)
                    {
                        var getItemInfos = _model.PresentGetItemInfos();
                        if (getItemInfos.Count > 0)
                        {
                            _view.CallSystemCommand(Base.CommandType.ClosePopup);
                            var strategySceneInfo = new StrategySceneInfo
                            {
                                ActorInfos = _model.PartyInfo.CurrentDeckActorInfos(),
                                InBattle = false
                            };
                            strategySceneInfo.GetItemInfos = getItemInfos;
                            _view.CommandSceneChange(Scene.Strategy,strategySceneInfo);
                        }
                    }
                });
                _view.CommandCallConfirm(confirmInfo);
            } else
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle("献上するアイテムがありません!");
                _view.CommandCallCaution(cautionInfo);
            }
        }

        private void CommandPlusUseNum(int itemId)
        {
            _model.ChangeUseNum(itemId,true);
            CommandRefresh();
        }

        private void CommandMinusUseNum(int itemId)
        {
            _model.ChangeUseNum(itemId,false);
            CommandRefresh();
        }

        private void CommandRefresh()
        {
            _view.SetItemList(MakeListData(_model.ItemInfoss()));
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}