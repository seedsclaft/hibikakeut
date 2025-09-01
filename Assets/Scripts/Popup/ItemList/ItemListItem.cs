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
        [SerializeField] private Button detailButton;

        private Action<bool> _useCountEvent = null;
        private Action<ItemInfo> _detailEvent = null;
        private ItemInfo _itemInfo = null;

        public void SetUseCountEvent(Action<bool> useCountEvent, Action<ItemInfo> detailEvent)
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
                detailButton.onClick.AddListener(() => _detailEvent(_itemInfo));
            }
            _detailEvent = detailEvent;
            _useCountEvent = useCountEvent;
        }

        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var data = ListItemData<ItemInfo>();
            _itemInfo = data;
            component.UpdateInfo(data);
            if (Disable != null)
            {
                Disable.SetActive(!ListData.Enable);
            }
            if (detailButton != null)
            {
                detailButton.gameObject.SetActive(data.Master.ItemType == ItemType.RandumAddSkill);
            }
        }
    }
}
