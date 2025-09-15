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
                roadImage.gameObject.SetActive(data.Opened && data.IsRoad());
            }
            if (cellImage != null)
            {
                cellImage.gameObject.SetActive(data.Opened && data.IsCellImage());
            }
            if (playerPointer != null)
            {
                var open = data.IsPlayerPosition;
                if (open)
                {
                    var direction = Ariadne.PlayerPosition.Instance.direction;
                    switch (direction)
                    {
                        case Ariadne.DungeonDir.North:
                            playerPointer.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 180);
                            break;
                        case Ariadne.DungeonDir.East:
                            playerPointer.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 90);
                            break;
                        case Ariadne.DungeonDir.South:
                            playerPointer.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
                            break;
                        case Ariadne.DungeonDir.West:
                            playerPointer.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 270);
                            break;
                    }
                }
                playerPointer.SetActive(open);
            }
            if (pathImage != null)
            {
                pathImage.gameObject.SetActive(data.Opened && data.IsPathSelect);
            }
            if (Disable != null)
            {
                Disable.SetActive(!ListData.Enable.Value);
            }
        }
    }
}
