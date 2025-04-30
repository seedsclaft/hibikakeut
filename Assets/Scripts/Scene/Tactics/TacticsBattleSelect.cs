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
            partyNames.SetText(PartyName(battleSceneInfo.ActorBattlerInfos));
            enemyNames.SetText(EnemyName(battleSceneInfo.EnemyInfos));
        }

        private string PartyName(List<BattlerInfo> battlerInfos)
        {
            var text = "";
            for (int i = battlerInfos.Count-1;i >= 0;i--)
            {
                if (battlerInfos[i].ActorInfo == null)
                {
                    continue;
                }
                text += battlerInfos[i].ActorInfo.Master.Name + "隊";
                if (i != 0)
                {
                    text += "・";
                }
            }
            return text;
        }

        private string EnemyName(List<BattlerInfo> enemyInfos)
        {
            var text = "";
            for (int i = 0;i < enemyInfos.Count;i++)
            {
                if (enemyInfos[i].EnemyData == null)
                {
                    continue;
                }
                text += enemyInfos[i].EnemyData.Name + "隊";
                if (i != 0)
                {
                    text += "・";
                }
            }
            return text;
        }
    }
}
