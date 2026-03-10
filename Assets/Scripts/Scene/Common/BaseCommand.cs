using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class BaseCommand : ListItem, IListViewItem
    {
        [SerializeField] private TextMeshProUGUI commandName;

        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }

            var data = ListItemData<SystemData.CommandData>();
            UIComponent.SetText(commandName, data.Name);
            UIComponent.SetActive(Disable, !ListData.Enable.Value);
            UIComponent.SetActive(Batch, ListData.Batch.Value);
        }
    }
}