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
            var hexUnit = GameSystem.GameInfo.StageInfo.HexUnitList.Find(a => a.HexField.X == hexPosition.Item1 && a.HexField.Y == hexPosition.Item2);
            if (hexUnit != null)
            {
                hexUnitComponent.UpdateInfo(hexUnit);
            } else
            {
                hexUnitComponent.Clear();
            }
        }
    }
}
