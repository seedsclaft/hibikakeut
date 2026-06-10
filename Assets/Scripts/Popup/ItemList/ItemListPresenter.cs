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
                        SelectEquipment(selectEquipmentInfos);
                        return;
                    }
                    PresentGetItemInfos();
                }
                _busy = false;
                _view.ActivateItemList(true);
            });
        }

        private void SelectEquipment(List<ItemData> itemDates)
        {
            // 全て入手済みの場合
            if (_model.SelectEquipmentZero(itemDates[0]))
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
            var count = 0;
            foreach (var itemDate in itemDates)
            {
                if (itemDate.Id == itemDates[0].Id)
                {
                    count++;
                }
            }
            var selectEquipmentSceneInfo = new SelectEquipmentSceneInfo
            {
                SelectCount = count,
                SelectEquipments = _model.SelectEquipmentInfos(itemDates[0]),
                SelectedEquipments = new()
            };
            CallPopupView(PopupType.SelectEquipment, () =>
            {
                if (selectEquipmentSceneInfo.SelectedEquipments.Count == count)
                {
                    PresentGetEquipmentInfos(selectEquipmentSceneInfo.SelectedEquipments, itemDates);
                } else
                {                
                    CommandRefresh();
                }
            }, selectEquipmentSceneInfo);
        }

        private void PresentGetEquipmentInfos(List<EquipmentInfo> equipmentInfos, List<ItemData> itemDates)
        {
            var getItemInfos = _model.PresentGetEquipmentInfos(equipmentInfos, itemDates[0]);
            if (getItemInfos.Count > 0)
            {
                _model.PartyInfo.PartyStatInfo.PresentCommandCount.GainValue(1);
                CheckAchievements();
            }
            CallEquipmentDetailView(DataSystem.GetText(10171) , equipmentInfos, () =>
            {
                itemDates.RemoveAt(0);
                if (itemDates.Count > 0)
                {
                    SelectEquipment(itemDates);
                    return;
                }
                _busy = false;
                CommandRefresh();
                PresentGetItemInfos();
            });
        }

        private void PresentGetItemInfos()
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