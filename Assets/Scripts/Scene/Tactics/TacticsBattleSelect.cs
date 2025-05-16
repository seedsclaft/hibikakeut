using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace Ryneus
{
    public class TacticsBattleSelect : ListItem, IListViewItem
    {
        [SerializeField] private TextMeshProUGUI partyNames;
        [SerializeField] private TextMeshProUGUI enemyNames;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var battleSceneInfo = ListItemData<BattleSceneInfo>();
            partyNames.SetText(PartyName(battleSceneInfo.ActorUnitInfos));
            enemyNames.SetText(EnemyName(battleSceneInfo.EnemyUnitInfos));
        }

        private string PartyName(List<UnitInfo> unitInfos)
        {
            var text = "";
            foreach (var unitInfo in unitInfos)
            {
                for (int i = unitInfo.BattlerInfos.Count-1;i >= 0;i--)
                {
                    if (unitInfo.BattlerInfos[i].ActorInfo == null)
                    {
                        continue;
                    }
                    text += unitInfo.BattlerInfos[i].ActorInfo.Master.Name;
                    if (i != 0)
                    {
                        text += "・";
                    }
                }
                text += "隊";
            }
            return text;
        }

        private string EnemyName(List<UnitInfo> unitInfos)
        {
            var text = "";
            foreach (var unitInfo in unitInfos)
            {
                for (int i = 0;i < unitInfo.BattlerInfos.Count;i++)
                {
                    if (unitInfo.BattlerInfos[i].EnemyData == null)
                    {
                        continue;
                    }
                    text += unitInfo.BattlerInfos[i].EnemyData.Name;
                    if (i != 0)
                    {
                        text += "・";
                    }
                }
                text += "隊";
            }
            return text;
        }
    }
}
