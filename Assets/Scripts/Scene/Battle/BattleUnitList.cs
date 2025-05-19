using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BattleUnitList : BaseList
    {
        [SerializeField] private List<GameObject> damageRoots;
        [SerializeField] private GameObject statusRoot;
        private Dictionary<int,BattleUnit> _battleBattler = new();
        private int _selectIndex = -1;
        public int SelectedIndex => _selectIndex;

        public void SetData(List<ListData> listDates)
        {
            SetData(listDates,true,null,true);
            SetBattlerInfoComp(listDates);
        }

        public void RefreshList(List<ListData> listDates)
        {
            base.SetData(listDates);
        }

        public void SetTargetListData(List<ListData> listDates)
        {
            base.SetData(listDates);
        }

        public void SetBattlerInfoComp(List<ListData> listDates)
        {
            //damageRoots.ForEach(a => a.SetActive(false));
            for (var i = 0;i < listDates.Count;i++)
            {
                var battleUnit = ItemPrefabList[i].GetComponent<BattleUnit>();
                var unitInfo = (UnitInfo)listDates[i].Data;
                if (battleUnit != null && unitInfo != null)
                {
                    _battleBattler[unitInfo.Index.Value] = battleUnit;
                    if (unitInfo.BattlerInfos.Count == 0)
                    {
                        battleUnit.SetDisable();
                    }
                    foreach (var battlerInfo in unitInfo.BattlerInfos)
                    {
                        battleUnit.SetDamageRoot(battlerInfo.Index.Value,damageRoots[(battlerInfo.Index.Value % 100) - 1]);
                        if (statusRoot != null)
                        {
                            battleUnit.SetStatusRoot(battlerInfo.Index.Value,statusRoot);
                        }
                    }
                }
            }
        }

        public BattlerInfoComponent GetBattlerInfoComp(int unitIndex,int battlerIndex)
        {
            var battleBattler = _battleBattler[unitIndex];
            return battleBattler != null ? battleBattler.FindBattlerInfoComponent(battlerIndex) : null;
        }


        public void UpdateSelectIndexList(List<int> indexes)
        {
            for (int i = 0; i < ItemPrefabList.Count;i++)
            {
                if (ItemPrefabList[i] == null) continue;
                var listItem = ItemPrefabList[i].GetComponentInChildren<ListItem>();
                if (listItem == null) continue;
                if (listItem.ListData == null) continue;
                var battler = (BattlerInfo)listItem.ListData.Data;
                if (indexes.Contains(battler.Index.Value))
                {
                    listItem.SetSelect();
                } else
                {
                    listItem.SetUnSelect();
                }
            }
        }

        public void ClearSelect()
        {
            SetSelectIndexes(new List<int>(){-1});
            UpdateSelectIndex(-1);
            /*
            foreach (var battleBattler in _battleBattler)
            {
                battleBattler.Value.SetDisable();
            }
            */
        }
    }
}
