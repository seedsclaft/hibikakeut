using System.Collections;
using System.Collections.Generic;

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

    }
}