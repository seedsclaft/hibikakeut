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
            saveData?.SetActive(data.ActorId > 0);
            newGame?.SetActive(data.ActorId <= 0);
            saveNo?.SetText(data.SaveNo.ToString());
            if (data.ActorId > 0)
            {
                actorInfoComponent.UpdateData(DataSystem.FindActor(data.ActorId));
                stageInfoComponent.UpdateData(DataSystem.FindStage(data.StageNo));
                saveTime?.SetText(data.SaveTime);
                var hours = data.PlayTime / 3600;
                var minutes = (data.PlayTime / 60) % 60;
                var seconds = data.PlayTime % 60;
                playTime?.SetText(hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00"));
                //clearData?.SetActive(data.ClearCount > 0);
            }
            if (data.Chapter > 0)
            {
                chapter.SetText(data.Chapter.ToString());
            }
            if (data.Period > 0)
            {
                period.SetText(data.Period.ToString());
            }
            if (data.Rank > 0)
            {
                rank.SetText(data.Rank.ToString());
            }
        }
    }
}
