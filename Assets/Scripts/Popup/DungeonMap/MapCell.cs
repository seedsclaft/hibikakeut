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
            UIComponent.SetActive(roadImage, data.Opened && data.IsRoad());
            UIComponent.SetActive(cellImage, data.Opened && data.IsCellImage());
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
                UIComponent.SetActive(playerPointer, open);
            }
            UIComponent.SetActive(pathImage, data.Opened && data.IsPathSelect);
            UIComponent.SetActive(Disable, !ListData.Enable.Value);
        }
    }
}
