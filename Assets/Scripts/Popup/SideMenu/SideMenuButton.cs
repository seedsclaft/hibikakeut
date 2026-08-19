using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SideMenuButton : ListItem, IListViewItem
    {
        [SerializeField] private TextMeshProUGUI commandName;

        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var data = ListItemData<SystemData.CommandData>();
            UIComponent.SetText(commandName, DataSystem.GetText(data.TextId));
            UIComponent.SetActive(Batch, ListData.Batch.Value);
        }
    }
}