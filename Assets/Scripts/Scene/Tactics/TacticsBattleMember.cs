using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class TacticsBattleBattler : ListItem ,IListViewItem 
    {
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private GameObject swapSelect;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var battlerInfo = ListItemData<ActorInfo>();
            actorInfoComponent.UpdateInfo(battlerInfo,null);
            swapSelect?.SetActive(ListData.Selected);
        }
    }
}