using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class HexTile : ListItem ,IListViewItem
    {        
        [SerializeField] private GameObject blank;
        [SerializeField] private HexUnitComponent filedHexUnit;
        [SerializeField] private HexUnitComponent unitHexUnit;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var hexPosition = ListItemData<HexField>();
            var hexUnits = GameSystem.GameInfo.StageInfo.HexUnitList.FindAll(a => a.HexField.X == hexPosition.X && a.HexField.Y == hexPosition.Y);
            
            filedHexUnit.Clear();
            unitHexUnit.Clear();
            foreach (var hexUnit in hexUnits)
            {
                var compnent = hexUnit.IsUnit ? unitHexUnit : filedHexUnit;
                compnent.UpdateInfo(hexUnit);
            }
            blank?.SetActive(hexPosition.X % 2 == 1);
        }
    }
}
