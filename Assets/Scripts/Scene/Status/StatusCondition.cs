using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class StatusCondition : ListItem, IListViewItem
    {
        [SerializeField] private StateInfoComponent stateInfoComponent;

        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var data = ListItemData<StateInfo>();
            stateInfoComponent.UpdateInfo(data);
        }
    }
}