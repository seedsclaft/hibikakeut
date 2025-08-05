using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class TradeItemInfoComponent : ListItem, IListViewItem
    {
        [SerializeField] private GetItem getItem;
        [SerializeField] private TextMeshProUGUI destriction;
        [SerializeField] private TextMeshProUGUI cost;
        [SerializeField] private GameObject selected;
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
            cost.SetText(data.Cost.Value.ToString() + DataSystem.GetText(1000));
            selected.SetActive(data.Selected.Value);
        }
    }
}
