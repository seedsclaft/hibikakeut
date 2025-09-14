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
            commandName.SetText(data.Name);
            if (Disable != null)
            {
                Disable.SetActive(!ListData.Enable.Value);
            }
            if (Batch != null)
            {
                Batch.SetActive(ListData.Batch.Value);
            }
        }
    }
}