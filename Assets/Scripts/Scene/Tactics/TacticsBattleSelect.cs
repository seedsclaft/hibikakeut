using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace Ryneus
{
    public class TacticsBattleSelect : ListItem ,IListViewItem 
    {
        [SerializeField] private TextMeshProUGUI partyNames;
        [SerializeField] private TextMeshProUGUI enemyNames;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var battleSceneInfo = ListItemData<BattleSceneInfo>();
            partyNames.SetText(PartyName(battleSceneInfo.ActorInfos));
            enemyNames.SetText(EnemyName(battleSceneInfo.EnemyInfos));
        }

        private string PartyName(List<ActorInfo> actorInfos)
        {
            var text = "";
            for (int i = actorInfos.Count-1;i >= 0;i--)
            {
                text += actorInfos[i].Master.Name + "隊";
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
