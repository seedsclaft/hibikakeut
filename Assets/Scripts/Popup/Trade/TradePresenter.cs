using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.Trade;

namespace Ryneus
{
    public class TradePresenter : BasePresenter
    {
        TradeModel _model = null;
        TradeView _view = null;

        private bool _busy = true;
        public TradePresenter(TradeView view)
        {
            _view = view;

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize();
            _busy = false;
        }

        private void Initialize()
        {
            _model = new TradeModel();
            SetModel(_model);
            _view.SetTrade(MakeListData(_model.TradeGetItemInfos(), 0));
            _view.OpenAnimation();
            CommandRefresh();
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Trade)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.Initialize:
                    Initialize();
                    break;
                case CommandType.DecideTrade:
                    CommandDecideTrade();
                    break;
                case CommandType.TradeItemDetail:
                    CommandTradeItemDetail((TradeItemInfo)viewEvent.Template);
                    break;
                case CommandType.PlusTradeItem:
                    if (viewEvent.Template == null)
                    {
                        return;
                    }
                    CommandPlusTradeItem((TradeItemInfo)viewEvent.Template);
                    break;
                case CommandType.MinusTradeItem:
                    if (viewEvent.Template == null)
                    {
                        return;
                    }
                    CommandMinusTradeItem((TradeItemInfo)viewEvent.Template);
                    break;
                case CommandType.SelectIndex:
                    _view.UpdateItemOwnCount(_model.ItemOwnCount(_view.CurrentTradeItemInfo));
                    break;
                case CommandType.CommandBack:
                    CommandBack((TradeItemInfo)viewEvent.Template);
                    break;
            }
        }

        private void CommandDecideTrade()
        {
            if (_model.IsNotSelectTradeItem())
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(39030));
                return;
            }

            _busy = true;
            CallConfirmView(DataSystem.GetText(39040), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    var getItemInfos = _model.GetTradeGetItemInfos();
                    if (getItemInfos.Count > 0)
                    {
                        _model.PayCostTrade();
                        _model.PartyInfo.PartyStatInfo.TradeCommandCount.GainValue(1);
                        CheckAchievements();
                        _view.CallSystemCommand(Base.CommandType.ClosePopup);
                        var sceneParam = new MainMenuSceneInfo
                        {
                            CommandIndex = 6
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
        }

        private void CommandTradeItemDetail(TradeItemInfo tradeItemInfo)
        {
            if (tradeItemInfo == null)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            _view.SetActivateItemList(false);
            if (tradeItemInfo.GetItemInfo.Master.Type == GetItemType.Skill)
            {
                var skillInfo = new SkillInfo(tradeItemInfo.GetItemInfo.Param1);
                CallConfirmSkillDetailView("", new List<SkillInfo>(){skillInfo}, (a) =>
                {
                    _busy = false;
                    _view.SetActivateItemList(true);
                });
            }
            else
            if (tradeItemInfo.GetItemInfo.Master.Type == GetItemType.Item)
            {
                var itemInfo = new ItemInfo(tradeItemInfo.GetItemInfo.Param1, 1);
                if (itemInfo.Master.ItemType == ItemType.RandumAddSkill)
                {
                    var skillInfos = _model.GetRandumAddSkillInfos(itemInfo);
                    CallConfirmSkillDetailView(DataSystem.GetText(34060), skillInfos, (a) =>
                    {
                        _view.SetActivateItemList(true);
                        _busy = false;
                    });
                    return;
                }
                CallConfirmItemDetailView("", new List<ItemInfo>(){itemInfo}, (a) =>
                {
                    _busy = false;
                    _view.SetActivateItemList(true);
                });
            }
        }

        private void CommandPlusTradeItem(TradeItemInfo tradeItemInfo)
        {
            if (!_model.CanPayCost(tradeItemInfo))
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(39050));
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeTradeItemNum(tradeItemInfo, true);
            CommandRefresh();
        }

        private void CommandMinusTradeItem(TradeItemInfo tradeItemInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeTradeItemNum(tradeItemInfo, false);
            CommandRefresh();
        }

        private void CommandBack(TradeItemInfo tradeItemInfo)
        {
            if (tradeItemInfo != null && _model.GetTradeItems.ContainsKey(tradeItemInfo) && _model.GetTradeItems[tradeItemInfo] > 0)
            {
                CommandRemoveTradeItem(tradeItemInfo);
                return;
            }
            _view.BackEvent();
        }

        private void CommandAddTradeItem(TradeItemInfo getItemInfo)
        {
            if (!_model.CanPayCost(getItemInfo))
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(39050));
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.AddTradeItem(getItemInfo);
            CommandRefresh();
        }

        private void CommandRemoveTradeItem(TradeItemInfo getItemInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.RemoveTradeItem(getItemInfo);
            CommandRefresh();
        }

        private void CommandRefresh()
        {
            Func<TradeItemInfo, bool> enable = (tradeItemInfo) =>
            {
                // コスト足りているか
                return !_model.CanPayCost(tradeItemInfo);
            };
            _view.RefreshTradeList(MakeListDataFunc(_model.TradeGetItemInfos(), 0, enable));
            _view.UpdateAfterCurrency(_model.AfterCurrency());
            _view.UpdateItemOwnCount(_model.ItemOwnCount(_view.CurrentTradeItemInfo));
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}