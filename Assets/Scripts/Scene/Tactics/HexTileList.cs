using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class HexTileList : ListItem ,IListViewItem
    {
        [SerializeField] private GameObject blankObj;
        [SerializeField] private GameObject blankRoot;
        [SerializeField] private BaseList hexList;
        private bool _isInitialized = false;
        public void Initialize()
        {
            hexList.Initialize();
        }
    
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            if (_isInitialized == false)
            {
                Initialize();
                _isInitialized = true;
            }
            var lineX = ListItemData<int>();
            if (lineX % 2 == 1)
            {
                var prefab = Instantiate(blankObj);
                prefab.transform.SetParent(blankRoot.transform,false);
            }
            var stageData = GameSystem.GameInfo.PartyInfo.StageMaster;
            var list = new List<(int,int)>();
            for (int i = 0;i < stageData.Height;i++)
            {
                list.Add((lineX,i));
            }
            var data = ListData.MakeListData(list);
            hexList.SetData(data);
        }

        public void SetSelectIndex(int selectIndex)
        {
            hexList.UpdateSelectIndex(selectIndex);
        }
    }
}
