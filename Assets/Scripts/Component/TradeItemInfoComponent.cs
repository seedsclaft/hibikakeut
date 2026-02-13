using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Ryneus
{
    public class TradeItemInfoComponent : ListItem, IListViewItem
    {
        [SerializeField] private ItemInfoComponent itemInfoComponent;
        [SerializeField] private TextMeshProUGUI cost;
        [SerializeField] private TextMeshProUGUI getCount;
        [SerializeField] private GameObject selected;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button detailButton;
        private Action<bool> _useCountEvent = null;
        private Action<TradeItemInfo> _detailEvent = null;
        private TradeItemInfo _tradeItemInfo = null;
        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var data = ListItemData<TradeItemInfo>();
            if (data == null)
            {
                return;
            }
            _tradeItemInfo = data;

            itemInfoComponent.UpdateDate(DataSystem.FindItem(data.GetItemInfo.Param1));
            var tradeCost = (int)(data.Cost.Value * GameSystem.GameInfo.PartyInfo.TradeDownRate());
            cost.SetText(tradeCost.ToString() + DataSystem.GetText(1000));
            getCount.SetText(data.GetCount.Value.ToString());
            //selected.SetActive(data.Selected.Value);
        }

        public void SetUseCountEvent(Action<bool> useCountEvent, Action<TradeItemInfo> detailEvent)
        {
            if (_useCountEvent != null)
            {
                return;
            }
            if (plusButton != null)
            {
                plusButton.onClick.AddListener(() => _useCountEvent(true));
            }
            if (minusButton != null)
            {
                minusButton.onClick.AddListener(() => _useCountEvent(false));
            }
            if (detailButton != null)
            {
                detailButton.onClick.AddListener(() => _detailEvent(_tradeItemInfo));
            }
            _detailEvent = detailEvent;
            _useCountEvent = useCountEvent;
        }
    }
}
