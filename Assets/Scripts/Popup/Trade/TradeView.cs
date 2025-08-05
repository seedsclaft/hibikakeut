using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ryneus.Trade;
using Unity.VisualScripting;

namespace Ryneus
{
    public class TradeView : BaseView
    {
        [SerializeField] private BaseList tradeList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        [SerializeField] private PartyInfoComponent partyInfoComponent;
        [SerializeField] private TextMeshProUGUI afterCurrency;
        [SerializeField] private OnOffButton tradeButton = null;
        [SerializeField] private InputInfoComponent tradeButtonKey = null;

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
            if (tradeButtonKey != null)
            {
                tradeButtonKey.UpdateGuideIcon(InputKeyType.Start);
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
            tradeList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            tradeList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CommandType.SelectTradeItem, tradeList.ListItemData<TradeItemInfo>()));
            tradeList.SetInputHandler(InputKeyType.Start, () => CallViewEvent(CommandType.DecideTrade));
            //tradeList.SetInputHandler(InputKeyType.Right, () => CallViewEvent(CommandType.AddTradeItem, tradeList.ListItemData<TradeItemInfo>()));
            //tradeList.SetInputHandler(InputKeyType.Left, () => CallViewEvent(CommandType.RemoveTradeItem, tradeList.ListItemData<TradeItemInfo>()));
            SetInputHandler(tradeList.gameObject);
        }

        public void SetTrade(List<ListData> getItemInfos)
        {
            tradeList.SetData(getItemInfos);
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
            SelectTradeItem,
        }
    }
}
