using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class UnitInfoListbattler : ListItem, IListViewItem
    {
        [SerializeField] private BattlerInfoComponent component;
        public void UpdateViewItem()
        {
            return;
            if (ListData == null)
            {
                return;
            }
            var data = ListItemData<BattlerInfo>();
            if (data == null)
            {
                return;
            }
            component.UpdateInfo(data);
            component.RefreshStatus();
        }
    }
}
