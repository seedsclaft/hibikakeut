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
            _view.SetItemList(MakeListData(_model.ItemInfos(), 0));
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
                _busy = true;
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(34040), (a) =>
                {
                    if (a == ConfirmCommandType.Yes)
                    {
                        var getItemInfos = _model.PresentGetItemInfos();
                        if (getItemInfos.Count > 0)
                        {
                            _model.PartyInfo.PresentCommandCount.GainValue(1);
                            CheckAchievements();
                            _view.CallSystemCommand(Base.CommandType.ClosePopup);
                            var sceneParam = new MainMenuSceneInfo
                            {
                                CommandIndex = 4
                            };
                            var strategySceneInfo = new StrategySceneInfo
                            {
                                ActorInfos = _model.PartyInfo.CurrentDeckActorInfos(),
                                InBattle = false,
                                GetItemInfos = getItemInfos,
                                ReturnMainMenuSceneParam = sceneParam
                            };
                            _view.CommandSceneChange(Scene.Strategy, strategySceneInfo);
                        }
                    }
                    _busy = false;
                });
                _view.CommandCallConfirm(confirmInfo);
            } else
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(34050));
                _view.CommandCallCaution(cautionInfo);
            }
        }

        private void CommandPlusUseNum(int itemId)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeUseNum(itemId,true);
            CommandRefresh();
        }

        private void CommandMinusUseNum(int itemId)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeUseNum(itemId,false);
            CommandRefresh();
        }

        private void CommandRefresh()
        {
            _view.SetItemList(MakeListData(_model.ItemInfos()));
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}