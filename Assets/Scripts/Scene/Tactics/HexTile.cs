using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class HexTile : ListItem, IListViewItem
    {
        [SerializeField] private RectTransform layoutRect;
        [SerializeField] private GameObject baseObj;
        [SerializeField] private HexUnitComponent filedHexUnit;
        [SerializeField] private HexUnitComponent unitHexUnit;
        [SerializeField] private TextMeshProUGUI position;

        private List<HexUnitInfo> _lastHexUnitInfos = new();
        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
        }
    }
}
