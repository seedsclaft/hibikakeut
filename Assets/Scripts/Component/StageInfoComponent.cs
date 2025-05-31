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
        [SerializeField] private Image stageImage;
        [SerializeField] private GameObject cleared;

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
            if (cleared != null)
            {
                cleared.SetActive(stageInfo.Cleared.Value);
            }
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
            stageLv?.SetText(stageData.StageLv.ToString());
            stageNoText?.SetText(DataSystem.GetReplaceText(15010,stageData.StageNo.ToString()));
            if (stageImage != null)
            {
                stageImage.sprite = ResourceSystem.LoadBackGround(stageData.BackGround);
            }
        }
    }
}