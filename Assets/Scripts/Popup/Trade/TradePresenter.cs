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
            _model = new TradeModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetTrade(MakeListData(_model.TradeGetItemInfos(), 0));
            _view.OpenAnimation();
            _busy = false;
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
                case CommandType.DecideTrade:
                    CommandDecideTrade();
                    break;
                case CommandType.SelectTradeItem:
                    CommandSelectTradeItem((TradeItemInfo)viewEvent.Template);
                    break;
            }
        }

        private void CommandDecideTrade()
        {
            if (_model.GetItems.Count == 0)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle("取引する商品がありません");
                _view.CommandCallCaution(cautionInfo);
                return;
            }

            _busy = true;
            _view.SetBusy(true);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo("取引しますか？", (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    var getItemInfos = _model.GetTradeGetItemInfos();
                    if (getItemInfos.Count > 0)
                    {
                        _model.PartyInfo.TradeCommandCount.GainValue(1);
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

        private void CommandSelectTradeItem(TradeItemInfo getItemInfo)
        {
            if (_model.GetItems.Contains(getItemInfo))
            {
                CommandRemoveTradeItem(getItemInfo);
            } else
            {
                CommandAddTradeItem(getItemInfo);
            }
        }

        private void CommandAddTradeItem(TradeItemInfo getItemInfo)
        {
            if (!_model.CanPayCost(getItemInfo))
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle("Cost不足");
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