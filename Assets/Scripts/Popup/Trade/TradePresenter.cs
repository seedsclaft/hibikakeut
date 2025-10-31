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
                case CommandType.CommandBack:
                    CommandBack((TradeItemInfo)viewEvent.Template);
                    break;
            }
        }

        private void CommandDecideTrade()
        {
            if (_model.GetTradeItems.Count == 0)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(39030));
                _view.CommandCallCaution(cautionInfo);
                return;
            }

            _busy = true;
            _view.SetBusy(true);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(39040), (a) =>
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
                _view.SetBusy(false);
            });
            _view.CommandCallConfirm(confirmInfo);
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
                var confirmInfo = new ConfirmInfo("", (a) =>
                {
                    _busy = false;
                    _view.SetActivateItemList(true);
                }, ConfirmType.SkillDetail);
                confirmInfo.SetBackEvent(() =>
                {
                    _busy = false;
                });
                confirmInfo.SetSkillInfo(new List<SkillInfo>(){skillInfo});
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
            } else
            if (tradeItemInfo.GetItemInfo.Master.Type == GetItemType.Item)
            {
                var itemInfo = new ItemInfo(tradeItemInfo.GetItemInfo.Param1, 1);
                if (itemInfo.Master.ItemType == ItemType.RandumAddSkill)
                {
                    var skillInfos = _model.GetRandumAddSkillInfos(itemInfo);
                    var rconfirmInfo = new ConfirmInfo(DataSystem.GetText(34060), (a) =>
                    {
                        _view.SetActivateItemList(true);
                        _busy = false;
                    }, ConfirmType.SkillDetail);
                    rconfirmInfo.SetBackEvent(() =>
                    {
                        _busy = false;
                    });
                    rconfirmInfo.SetSkillInfo(skillInfos);
                    rconfirmInfo.SetIsNoChoice(true);
                    _view.CommandCallConfirm(rconfirmInfo);
                    return;
                }
                var confirmInfo = new ConfirmInfo("", (a) =>
                {
                    _busy = false;
                    _view.SetActivateItemList(true);
                }, ConfirmType.ItemDetail);
                confirmInfo.SetBackEvent(() =>
                {
                    _busy = false;
                });
                confirmInfo.SetItemInfo(new List<ItemInfo>(){itemInfo});
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
            }
        }

        private void CommandPlusTradeItem(TradeItemInfo tradeItemInfo)
        {
            if (!_model.CanPayCost(tradeItemInfo))
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(39050));
                _view.CommandCallCaution(cautionInfo);
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
            if (tradeItemInfo != null && _model.GetTradeItems.ContainsKey(tradeItemInfo))
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
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(39050));
                _view.CommandCallCaution(cautionInfo);
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
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}