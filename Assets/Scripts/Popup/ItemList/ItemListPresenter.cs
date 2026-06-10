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
            Initialize(true);
        }

        private void Initialize(bool first)
        {
            _model = new ItemListModel();
            SetModel(_model);
            _view.OpenAnimation(first ? InitializeAfter : null);
            if (!first)
            {
                InitializeAfter();
            }
        }

        private void InitializeAfter()
        {
            _view.SetItemList(MakeListData(_model.ItemInfos(), 0), true);
            _view.CheckItemDetailButtonActive();
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
                    Initialize(false);
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
            if (!_model.CanPresent())
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(34050));
            }
            _view.ActivateItemList(false);
            _busy = true;
            CallConfirmView(DataSystem.GetText(34040), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    // 選択装備を先に決定する
                    var selectEquipmentInfos = _model.SelectEquipmentItems();
                    if (selectEquipmentInfos.Count > 0)
                    {
                        // 全て入手済みの場合
                        if (_model.SelectEquipmentZero(selectEquipmentInfos[0]))
                        {
                            CallConfirmNoChoiceView(DataSystem.GetText(45020), (a) =>
                            {
                                if (a == ConfirmCommandType.Yes)
                                {
                                    PresentGetItemInfos();
                                    CommandRefresh();
                                }
                            });
                            return;
                        }
                        var selectEquipmentSceneInfo = new SelectEquipmentSceneInfo
                        {
                            SelectCount = selectEquipmentInfos.Count,
                            SelectEquipments = _model.SelectEquipmentInfos(),
                            SelectedEquipments = new()
                        };
                        CallPopupView(PopupType.SelectEquipment, () =>
                        {
                            if (selectEquipmentSceneInfo.SelectedEquipments.Count == selectEquipmentSceneInfo.SelectCount)
                            {
                                PresentGetItemInfos(selectEquipmentSceneInfo.SelectedEquipments, selectEquipmentInfos[0]);
                            }
                        }, selectEquipmentSceneInfo);
                        return;
                    }
                    PresentGetItemInfos();
                }
                _busy = false;
                _view.ActivateItemList(true);
            });
        }

        private void PresentGetItemInfos(List<EquipmentInfo> selectedEquipmentInfos = null, ItemData itemData = null)
        {
            var getItemInfos = _model.PresentGetItemInfos(selectedEquipmentInfos, itemData);
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

        private void CommandPlusUseNum(int itemId)
        {
            if (!_model.CanChangeUseNum(itemId, true))
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeUseNum(itemId, true);
            CommandRefresh();
        }

        private void CommandMinusUseNum(int itemId)
        {
            if (!_model.CanChangeUseNum(itemId, false))
            {
                return;
            }
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
                //case ItemType.RandumAddEquipment:
                case ItemType.SelectAddEquipment:
                    CommandDetailSkill(itemInfo);
                    break;
            }
        }

        private void CommandDetailSkill(ItemInfo itemInfo)
        {
            _busy = true;
            _view.ActivateItemList(false);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CallEquipmentDetailView("選択装具候補" , _model.SelectEquipmentInfos(itemInfo.Master), () =>
            {
                _busy = false;
                _view.ActivateItemList(true);
            });
        }

        private void CommandRefresh()
        {
            _view.SetItemList(MakeListData(_model.ItemInfos()), false);
            _view.CheckItemDetailButtonActive();
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}