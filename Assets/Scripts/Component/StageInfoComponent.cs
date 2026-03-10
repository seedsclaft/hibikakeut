using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

namespace Ryneus
{
    public class StageInfoComponent : BaseInfoComponent
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI stageNoText;
        [SerializeField] private TextMeshProUGUI help;
        [SerializeField] private TextMeshProUGUI stageLv;
        [SerializeField] private TextMeshProUGUI needStageRank;
        [SerializeField] private Image stageImage;
        [SerializeField] private Image bossImage;
        [SerializeField] private TextMeshProUGUI bossLv;
        [SerializeField] private TextMeshProUGUI bossName;
        [SerializeField] private GameObject cleared;
        [SerializeField] private GameObject mainStage;
        [SerializeField] private GameObject subStage;
        [SerializeField] private GameObject battleFieldStage;
        [SerializeField] private GameObject dungeonEnemySymbolRoot;
        [SerializeField] private TextMeshProUGUI dungeonEnemySymbolNum;

        public void UpdateCurrent()
        {
            var current = CurrentStage;
            if (current != null)
            {
                UpdateInfo(current);
            }
        }

        public void UpdateInfo(StageInfo stageInfo)
        {
            if (stageInfo == null)
            {
                return;
            }
            var stageData = stageInfo.Master;
            UpdateData(stageData);
            UIComponent.SetText(help, stageData.Help.Replace("\\p", CurrentData.PlayerInfo.PlayerName.Value));
            UIComponent.SetActive(cleared, stageInfo.Cleared.Value);
            var bossEnemyData = stageInfo.BossEnemyData();
            if (bossEnemyData != null && !stageInfo.Cleared.Value)
            {
                UIComponent.SetImage(bossImage, ResourceSystem.EnemySpritePath(stageInfo.BossImage()));
                UIComponent.SetText(bossName, bossEnemyData.Name);
                UIComponent.SetText(bossLv, DataSystem.GetText(3010) + stageInfo.BossLv().ToString());
            }
            else
            {
                UIComponent.ClearText(bossName);
                UIComponent.ClearText(bossLv);
            }
            /*
            if (clearCount != null){
                clearCount.text = stageInfo.ClearCount.ToString();
            }
            */

            if (dungeonEnemySymbolNum != null && dungeonEnemySymbolRoot != null)
            {
                var symbolNum = CurrentStage.DungeonEnemySymbolNum(CurrentGameInfo.ReadEventKeys);
                if (CurrentStage.Master.EncountTimes != -1)
                {
                    symbolNum += CurrentDeckInfo.RemainEncountTimes();
                }
                UIComponent.SetActive(dungeonEnemySymbolRoot, symbolNum > 0);
                UIComponent.SetText(dungeonEnemySymbolNum, "x" + symbolNum);
            }
        }

        public void UpdateData(StageData stageData)
        {
            if (stageData == null)
            {
                return;
            }
            UIComponent.SetText(nameText, stageData.Name);
            UIComponent.SetText(stageLv, stageData.StageLv);
            UIComponent.SetText(stageNoText, DataSystem.GetReplaceText(15010, stageData.StageNo.ToString()));
            UIComponent.SetText(needStageRank, stageData.DisplayRank.ToString() + "～");
            UIComponent.SetImage(stageImage, ResourceSystem.BackGroundPath(stageData.BackGround));

            UIComponent.SetActive(mainStage, stageData.Category == StageCategory.Main);
            UIComponent.SetActive(subStage, stageData.Category == StageCategory.Sub);
            UIComponent.SetActive(battleFieldStage, stageData.Category == StageCategory.BattleField);
        }
    }
}