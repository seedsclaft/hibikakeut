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
        [SerializeField] private GameObject achieve;
        [SerializeField] private TextMeshProUGUI achieveText;
        [SerializeField] private TextMeshProUGUI help;
        [SerializeField] private TextMeshProUGUI turnCount;
        [SerializeField] private TextMeshProUGUI clearCount;
        [SerializeField] private TextMeshProUGUI stageLv;
        [SerializeField] private TeamInfoComponent homeTeamInfoComponent;
        [SerializeField] private TeamInfoComponent awayTeamInfoComponent;

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
            help?.SetText(stageData.Help.Replace("\\p",CurrentData.PlayerInfo.PlayerName.Value));
            
            /*
            if (clearCount != null){
                clearCount.text = stageInfo.ClearCount.ToString();
            }
            */

        }

        public void UpdateData(StageData stageData)
        {
            if (stageData == null)
            {
                return;
            }
            nameText?.SetText(stageData.Name);
            if (achieve != null)
            {
                achieve?.SetActive(stageData.AchieveText != "");
            }
            if (stageData.AchieveText != "")
            {
                achieveText?.SetText(DataSystem.GetText(31) + stageData.AchieveText);
            } else
            {
                achieveText?.SetText(DataSystem.GetText(31) + DataSystem.GetText(10000));
            }
            stageLv?.SetText(stageData.StageLv.ToString());
            stageNoText?.SetText(DataSystem.GetReplaceText(15010,stageData.StageNo.ToString()));
        }
    }
}