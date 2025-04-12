using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class HexTile : ListItem ,IListViewItem
    {        
        [SerializeField] private RectTransform layoutRect;
        [SerializeField] private GameObject baseObj;
        [SerializeField] private HexUnitComponent filedHexUnit;
        [SerializeField] private HexUnitComponent unitHexUnit;
        [SerializeField] private TextMeshProUGUI position;

        private List<HexUnitInfo> _lastHexUnitInfos = new();
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var hexPosition = ListItemData<HexField>();
            var hexUnits = GameSystem.GameInfo.StageInfo.FindUnitInfos(hexPosition.X,hexPosition.Y);
            var halfsize = hexPosition.X % 2 == 1 ? -48 : 0;
            layoutRect.localPosition = new Vector2(0,halfsize);
            //position.SetText(hexPosition.X + ":" + hexPosition.Y);
            // マス更新がない
            if (_lastHexUnitInfos.Count == 0 && hexUnits.Count == 0)
            {
                return;
            }
            _lastHexUnitInfos = hexUnits;
            if (hexUnits.Find(a => a.HexUnitType == HexUnitType.None) != null)
            {
                // 存在しないマス
                baseObj?.SetActive(false);
                filedHexUnit.Clear();
                unitHexUnit.Clear();
                return;
            }
            baseObj?.SetActive(true);
            filedHexUnit.Clear();
            unitHexUnit.Clear();
            foreach (var hexUnit in hexUnits)
            {
                var compnent = hexUnit.IsUnit ? unitHexUnit : filedHexUnit;
                compnent.UpdateInfo(hexUnit);
            }
        }

        public void LostUnit()
        {
            filedHexUnit.Clear();
            unitHexUnit.LostUnit();
        }

        public void InitLost()
        {
            unitHexUnit.InitUnit();
        }
    }
}
