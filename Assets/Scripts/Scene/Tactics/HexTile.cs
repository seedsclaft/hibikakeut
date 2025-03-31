using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class HexTile : ListItem ,IListViewItem
    {        
        [SerializeField] private HexUnitComponent hexUnitComponent;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var hexPosition = ListItemData<(int,int)>();
            var hexData = GameSystem.GameInfo.StageInfo.HexUnitList.Find(a => a.PositionX.Value == hexPosition.Item1 && a.PositionY.Value == hexPosition.Item2);
            if (hexData != null)
            {
                hexUnitComponent.UpdateInfo(hexData);
            } else
            {
                hexUnitComponent.Clear();
            }
        }
    }
}
