using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Ryneus
{
    public class TradeItemInfoComponent : ListItem, IListViewItem
    {
        [SerializeField] private GetItem getItem;
        [SerializeField] private TextMeshProUGUI destriction;
        [SerializeField] private TextMeshProUGUI cost;
        [SerializeField] private GameObject selected;
        [SerializeField] private Button detailButton;
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

            getItem.SetItemData(data.GetItemInfo);
            destriction.SetText(data.Destriction());
            var tradeCost = (int)(data.Cost.Value * GameSystem.GameInfo.PartyInfo.TradeDownRate());
            cost.SetText(tradeCost.ToString() + DataSystem.GetText(1000));
            selected.SetActive(data.Selected.Value);
        }

        public void SetDetailEvent(System.Action<TradeItemInfo> detailEvent)
        {
            if (detailButton == null)
            {
                return;
            }
            detailButton.onClick.AddListener(() => detailEvent.Invoke(ListItemData<TradeItemInfo>()));
        }
    }
}
