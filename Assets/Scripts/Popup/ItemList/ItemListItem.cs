using System;
using ES3Types;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class ItemListItem : ListItem, IListViewItem
    {
        [SerializeField] private ItemInfoComponent component;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button minusButton;

        private Action<bool> _useCountEvent = null;

        public void SetUseCountEvent(Action<bool> useCountEvent)
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
            _useCountEvent = useCountEvent;
        }

        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var data = ListItemData<ItemInfo>();
            component.UpdateInfo(data);
            if (Disable != null)
            {
                Disable.SetActive(!ListData.Enable);
            }
        }
    }
}
