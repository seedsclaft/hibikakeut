using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
            help?.SetText(stageData.Help.Replace("\\p", CurrentData.PlayerInfo.PlayerName.Value));
            if (cleared != null)
            {
                cleared.SetActive(stageInfo.Cleared.Value);
            }
            var bossEnemyData = stageInfo.BossEnemyData();
            if (bossEnemyData != null && !stageInfo.Cleared.Value)
            {
                if (bossImage != null)
                {
                    bossImage.sprite = ResourceSystem.LoadEnemySprite(stageInfo.BossImage());
                }
                if (bossName != null)
                {
                    bossName.SetText(bossEnemyData.Name);
                }
                if (bossLv != null)
                {
                    bossLv.SetText(DataSystem.GetText(3010) + stageInfo.BossLv().ToString());
                }
            } else
            {
                if (bossName != null)
                {
                    bossName.SetText("");
                }
                if (bossLv != null)
                {
                    bossLv.SetText("");
                }
            }
            /*
            if (clearCount != null){
                clearCount.text = stageInfo.ClearCount.ToString();
            }
            */

            if (dungeonEnemySymbolNum != null && dungeonEnemySymbolRoot != null)
            {
                var symbolNum = CurrentStage.DungeonEnemySymbolNum(CurrentGameInfo.ReadEventKeys);
                dungeonEnemySymbolRoot.SetActive(symbolNum > 0);
                dungeonEnemySymbolNum.SetText("x" + symbolNum.ToString());
            }
        }

        public void UpdateData(StageData stageData)
        {
            if (stageData == null)
            {
                return;
            }
            nameText?.SetText(stageData.Name);
            stageLv?.SetText(stageData.StageLv.ToString());
            stageNoText?.SetText(DataSystem.GetReplaceText(15010, stageData.StageNo.ToString()));
            needStageRank?.SetText(stageData.DisplayRank.ToString() + "～");
            if (stageImage != null)
            {
                stageImage.sprite = ResourceSystem.LoadBackGround(stageData.BackGround);
            }
            if (mainStage != null)
            {
                mainStage.SetActive(stageData.Category == StageCategory.Main);
            }
            if (subStage != null)
            {
                subStage.SetActive(stageData.Category == StageCategory.Sub);
            }
            if (battleFieldStage != null)
            {
                battleFieldStage.SetActive(stageData.Category == StageCategory.BattleField);
            }
        }
    }
}