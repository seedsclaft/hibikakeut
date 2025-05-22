using UnityEngine;

namespace Ryneus
{
    public class AchievementListItem : ListItem, IListViewItem
    {
        [SerializeField] private AchievementInfoComponent component;
        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var data = ListItemData<AchievementInfo>();
            component.UpdateInfo(data);
            if (Disable != null)
            {
                Disable.SetActive(!ListData.Enable);
            }
        }
    }
}
