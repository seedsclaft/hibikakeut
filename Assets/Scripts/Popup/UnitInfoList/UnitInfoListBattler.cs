using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class UnitInfoListbattler : ListItem ,IListViewItem  
    {   
        [SerializeField] private BattlerInfoComponent component;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<BattlerInfo>();
            if (data == null) return;
            component.UpdateInfo(data);
            component.RefreshStatus();
        }
    }
}
