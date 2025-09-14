using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class MapCell : ListItem, IListViewItem
    {
        [SerializeField] private Image roadImage;
        [SerializeField] private Image cellImage;
        [SerializeField] private GameObject playerPointer;
        [SerializeField] private Image pathImage;

        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var data = ListItemData<MapCellInfo>();
            if (roadImage != null)
            {
                roadImage.gameObject.SetActive(data.MapInfo.mapAttr == 0 || data.MapInfo.mapAttr > 1 && data.MapInfo.mapAttr < 99);
            }
            if (cellImage != null)
            {
                cellImage.gameObject.SetActive(data.MapInfo.mapAttr > 1 && data.MapInfo.mapAttr < 99);
            }
            if (playerPointer != null)
            {
                playerPointer.SetActive(GameSystem.GameInfo.PartyInfo.CurrentDeckInfo.ExistPlayerPosition(data.MapInfo.eventId));
            }
            if (pathImage != null)
            {
                pathImage.gameObject.SetActive(data.IsPathSelect);
            }
            if (Disable != null)
            {
                Disable.SetActive(!ListData.Enable.Value);
            }
        }
    }
}
