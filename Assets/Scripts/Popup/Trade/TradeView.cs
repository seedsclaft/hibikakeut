using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ryneus.Trade;

namespace Ryneus
{
    public class TradeView : BaseView
    {
        [SerializeField] private BaseList tradeList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        [SerializeField] private PartyInfoComponent partyInfoComponent;
        [SerializeField] private TextMeshProUGUI afterCurrency;
        [SerializeField] private OnOffButton tradeButton = null;
        [SerializeField] private OnOffButton detailButton = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Trade);
            InitializeTrade();
            SetBaseAnimation(popupAnimation);
            if (tradeButton != null)
            {
                tradeButton.OnClickAddListener(() => CallViewEvent(CommandType.DecideTrade, tradeList.ListItemData<TradeItemInfo>()));
            }
            if (detailButton != null)
            {
                detailButton.OnClickAddListener(() => CallViewEvent(CommandType.TradeItemDetail, tradeList.ListItemData<TradeItemInfo>()));
            }
            _ = new TradePresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => {});
        }

        private void InitializeTrade()
        {
            tradeList.Initialize();
            tradeList.SetInputHandler(InputKeyType.Cancel, () => CallViewEvent(CommandType.CommandBack));
            tradeList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CommandType.SelectTradeItem, tradeList.ListItemData<TradeItemInfo>()));
            tradeList.SetInputHandler(InputKeyType.Option1, () => CallViewEvent(CommandType.TradeItemDetail, tradeList.ListItemData<TradeItemInfo>()));
            tradeList.SetInputHandler(InputKeyType.Start, () => CallViewEvent(CommandType.DecideTrade));
            //tradeList.SetInputHandler(InputKeyType.Right, () => CallViewEvent(CommandType.AddTradeItem, tradeList.ListItemData<TradeItemInfo>()));
            //tradeList.SetInputHandler(InputKeyType.Left, () => CallViewEvent(CommandType.RemoveTradeItem, tradeList.ListItemData<TradeItemInfo>()));
            SetInputHandler(tradeList.gameObject);
        }

        public void SetTrade(List<ListData> getItemInfos)
        {
            tradeList.SetData(getItemInfos, true, () =>
            {
                foreach (var itemPrefab in tradeList.ItemPrefabList)
                {
                    var comp = itemPrefab.GetComponent<TradeItemInfoComponent>();
                    if (comp != null)
                    {
                        comp.SetDetailEvent((a) =>
                        {
                            CallViewEvent(CommandType.TradeItemDetail, a);
                        });
                    }
                }
            });
            tradeList.Activate();
            partyInfoComponent.UpdateCurrentInfo();
        }

        public void RefreshTradeList(List<ListData> getItemInfos)
        {
            tradeList.RefreshListData(getItemInfos);
        }

        public void UpdateAfterCurrency(int currency)
        {
            afterCurrency.SetText(currency.ToString());
        }
    }

    namespace Trade
    {
        public enum CommandType
        {
            DecideTrade = 0,
            TradeItemDetail,
            SelectTradeItem,
            CommandBack
        }
    }
}
