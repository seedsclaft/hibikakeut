using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class SymbolItem : ListItem ,IListViewItem
    {
        [SerializeField] private SymbolComponent symbolComponent;
        private bool _isClickInitialized = false;
        private bool _isSelectInitialized = false;
        private bool _scrollEventInitialized = false;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<SymbolInfo>();
            symbolComponent.UpdateInfo(data);
        }

        public void SetClickHandler(int? seek,System.Action<int> handler)
        {
            if (_isClickInitialized)
            {
                return;
            }
            _isClickInitialized = true;
            clickButton.onClick.AddListener(() => 
            {
                if (ListData == null) return;
                var data = ListItemData<SymbolInfo>();
                handler(data.Master.SeekIndex);
            });
        }

        public void SetSelectedHandler(int? seek,System.Action<int> handler)
        {
            if (_isSelectInitialized)
            {
                return;
            }
            _isSelectInitialized = true;
            var enterListener = clickButton.gameObject.AddComponent<ContentEnterListener>();
            enterListener.SetEnterEvent(() => 
            {
                if (ListData == null) return;
                var data = ListItemData<SymbolInfo>();
                handler(data.Master.SeekIndex);
            });
        }

        public void SetScrollEvent(ScrollRect scrollRect)
        {
            if (_scrollEventInitialized)
            {
                return;
            }
            _scrollEventInitialized = true;
            var scrolls = GetComponentsInChildren<MultiScroller>();
            foreach (var scroll in scrolls)
            {
                scroll.SetScrollEvent(scrollRect);
            }
        }
    }
}
