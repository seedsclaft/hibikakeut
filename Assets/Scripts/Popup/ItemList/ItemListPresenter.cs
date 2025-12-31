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

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize();
        }

        private void Initialize()
        {
            _model = new ItemListModel();
            SetModel(_model);
            _view.SetItemList(MakeListData(_model.ItemInfos(), 0));
            _view.OpenAnimation();
            _view.ActivateItemList(true);
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
                case CommandType.Initialize:
                    Initialize();
                    break;
                case CommandType.DecideItem:
                    CommandDecideItem();
                    break;
                case CommandType.PlusUseNum:
                    if (viewEvent.Template == null)
                    {
                        return;
                    }
                    CommandPlusUseNum((int)viewEvent.Template);
                    break;
                case CommandType.MinusUseNum:
                    if (viewEvent.Template == null)
                    {
                        return;
                    }
                    CommandMinusUseNum((int)viewEvent.Template);
                    break;
                case CommandType.DetailItem:
                    CommandDetailItem((ItemInfo)viewEvent.Template);
                    break;
            }
        }

        private void CommandDecideItem()
        {
            if (_model.CanPresent())
            {
                _view.ActivateItemList(false);
                _busy = true;
                CallConfirmView(DataSystem.GetText(34040), (a) =>
                {
                    if (a == ConfirmCommandType.Yes)
                    {
                        var getItemInfos = _model.PresentGetItemInfos();
                        if (getItemInfos.Count > 0)
                        {
                            _model.PartyInfo.PartyStatInfo.PresentCommandCount.GainValue(1);
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
                    _view.ActivateItemList(true);
                });
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Deny);
            CommandCautionInfo(DataSystem.GetText(34050));
        }

        private void CommandPlusUseNum(int itemId)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeUseNum(itemId, true);
            CommandRefresh();
        }

        private void CommandMinusUseNum(int itemId)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeUseNum(itemId, false);
            CommandRefresh();
        }

        private void CommandDetailItem(ItemInfo itemInfo)
        {
            if (itemInfo == null)
            {
                return;
            }
            switch (itemInfo.Master.ItemType)
            {
                case ItemType.RandumAddSkill:
                    CommandDetailSkill(itemInfo);
                    break;
            }
        }

        private void CommandDetailSkill(ItemInfo itemInfo)
        {
            _busy = true;
            _view.ActivateItemList(false);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var skillInfos = _model.GetRandumAddSkillInfos(itemInfo);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(34060), (a) =>
            {
                _busy = false;
                _view.ActivateItemList(true);
            }, ConfirmType.SkillDetail);
            confirmInfo.SetBackEvent(() =>
            {
                _busy = false;
                _view.ActivateItemList(true);
            });
            confirmInfo.SetSkillInfo(skillInfos);
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandRefresh()
        {
            _view.SetItemList(MakeListData(_model.ItemInfos()));
            _view.CheckItemDetailButtonActive();
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}