using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ryneus.Trade;

namespace Ryneus
{
    public class TradeView : BaseView
    {
        [SerializeField] private TradeItemList itemList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        [SerializeField] private PartyInfoComponent partyInfoComponent;
        [SerializeField] private TextMeshProUGUI afterCurrency;
        [SerializeField] private OnOffButton tradeButton = null;
        [SerializeField] private OnOffButton detailButton = null;

        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Trade);
            InitializeTrade();
            SetBaseAnimation(popupAnimation);
            if (tradeButton != null)
            {
                tradeButton.OnClickAddListener(() => CallViewEvent(CommandType.DecideTrade, itemList.ListItemData<TradeItemInfo>()));
            }
            if (detailButton != null)
            {
                detailButton.OnClickAddListener(() => CallViewEvent(CommandType.TradeItemDetail, itemList.ListItemData<TradeItemInfo>()));
            }
            _ = new TradePresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => {});
        }

        private void InitializeTrade()
        {
            itemList.Initialize();
            itemList.SetInputHandler(InputKeyType.Cancel, () => CallViewEvent(CommandType.CommandBack, itemList.ListItemData<TradeItemInfo>()));
            itemList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CommandType.SelectTradeItem, itemList.ListItemData<TradeItemInfo>()));
            itemList.SetInputHandler(InputKeyType.Option1, () => CallViewEvent(CommandType.TradeItemDetail, itemList.ListItemData<TradeItemInfo>()));
            itemList.SetInputHandler(InputKeyType.Start, () => CallViewEvent(CommandType.DecideTrade));
            AddViewActives(itemList);
        }

        public void SetTrade(List<ListData> getItemInfos)
        {
            partyInfoComponent.UpdateCurrentInfo();

            itemList.SetData(getItemInfos, true, () =>
            {
                foreach (var itemPrefab in itemList.ItemPrefabList)
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
            SetActivateItemList(true);
        }

        public void RefreshTradeList(List<ListData> getItemInfos)
        {
            itemList.RefreshListData(getItemInfos);
            partyInfoComponent.UpdateCurrentInfo();
        }

        public void UpdateAfterCurrency(int currency)
        {
            afterCurrency.SetText(currency.ToString());
        }

        public void SetActivateItemList(bool isActivate)
        {
            if (isActivate)
            {
                SetActivate(itemList);
            } else
            {
                SetActivate(null);
            }
        }
    }

    namespace Trade
    {
        public enum CommandType
        {
            Initialize,
            DecideTrade,
            TradeItemDetail,
            SelectTradeItem,
            CommandBack
        }
    }
}
