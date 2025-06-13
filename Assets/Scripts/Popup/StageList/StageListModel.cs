using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Ryneus
{
    public class StageListModel : BaseModel
    {
        public StageListModel()
        {
        }

        public void MakeStageInfoDepature(int stageId)
        {
            var dungeonId = CurrentDeckInfo.DungeonId.Value;
            var resumeStage = DataSystem.FindStage(dungeonId);
            // 復帰処理
            if (DataSystem.FindStage(stageId).StageNo == resumeStage.StageNo)
            {
                stageId = dungeonId;
            } else
            {
                // 初期位置に設定
                var floor = DataSystem.FindDungeonFloor(stageId);
                CurrentDeckInfo.SetPosition(stageId,floor.entrancePos.x,floor.entrancePos.y,(int)floor.enteringDir);
                CurrentDeckInfo.StageNo.SetValue(stageId);
            }
            MakeStageInfo(stageId,true);
        }

        public bool IsLimitedRank(StageInfo stageInfo)
        {
            return PartyInfo.MissionRank.Value < stageInfo.Master.DisplayRank;
        }

        public List<StageInfo> StageInfos()
        {
            var list = new List<StageInfo>();
            foreach (var stageData in DataSystem.Stages)
            {
                if (!stageData.Selectable)
                {
                    continue;
                }
                var cleared = PartyInfo.IsClaeredStage(stageData.StageNo);
                var stageInfo = new StageInfo(stageData.Id,cleared);
                list.Add(stageInfo);
            }
            return list;
        }
    }
}