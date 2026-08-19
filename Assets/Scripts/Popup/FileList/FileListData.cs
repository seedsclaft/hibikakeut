using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class FileListData : ListItem, IListViewItem
    {
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private StageInfoComponent stageInfoComponent;
        [SerializeField] private TextMeshProUGUI saveNo;
        [SerializeField] private TextMeshProUGUI saveTime;
        [SerializeField] private TextMeshProUGUI playTime;
        [SerializeField] private TextMeshProUGUI chapter;
        [SerializeField] private TextMeshProUGUI period;
        [SerializeField] private TextMeshProUGUI rank;
        [SerializeField] private TextMeshProUGUI state;
        [SerializeField] private GameObject clearData;
        [SerializeField] private GameObject saveData;
        [SerializeField] private GameObject newGame;


        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var data = ListItemData<SaveFileInfo>();
            UIComponent.SetActive(saveData, data.ActorId > 0);
            UIComponent.SetActive(newGame, data.ActorId <= 0);
            var saveNoText = data.SaveNo == 0 ? DataSystem.GetText(31080) : data.SaveNo.ToString();
            UIComponent.SetText(saveNo, saveNoText);
            if (data.ActorId > 0)
            {
                actorInfoComponent.UpdateData(DataSystem.FindActor(data.ActorId));
                stageInfoComponent.UpdateData(DataSystem.FindStage(data.StageNo));
                UIComponent.SetText(saveTime, data.SaveTime);
                var hours = data.PlayTime / 3600;
                var minutes = (data.PlayTime / 60) % 60;
                var seconds = data.PlayTime % 60;
                UIComponent.SetText(playTime, hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00"));
                //clearData?.SetActive(data.ClearCount > 0);
            }
            if (data.Chapter > 0)
            {
                UIComponent.SetText(chapter, data.Chapter.ToString());
            }
            if (data.Period > 0)
            {
                UIComponent.SetText(period, data.Period > DataSystem.System.PeriodTurns ? DataSystem.System.PeriodTurns.ToString() : data.Period.ToString());
            }
            if (data.Rank > 0)
            {
                UIComponent.SetText(rank, data.Rank.ToString());
            }
            if (state != null)
            {
                var stateText = data.Scene == Scene.Dungeon ? DataSystem.GetReplaceText(31060, DataSystem.FindStage(data.StageNo).Name) : DataSystem.GetText(31061);
                UIComponent.SetText(state, stateText);
            }
        }
    }
}
