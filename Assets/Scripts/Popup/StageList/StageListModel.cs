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
            var resumeInfo = PartyInfo.DungeonResumeInfos.Find(a => a.DungeonId.Value == stageId);
            // 復帰処理
            if (resumeInfo != null)
            {
                CurrentDeckInfo.SetPosition(resumeInfo.DungeonId.Value, resumeInfo.PositionX.Value, resumeInfo.PositionY.Value, resumeInfo.Direction.Value);
            } else
            {
                // 初期位置に設定
                var floor = DataSystem.FindDungeonFloor(stageId);
                CurrentDeckInfo.SetPosition(stageId, floor.entrancePos.x, floor.entrancePos.y, (int)floor.enteringDir);
            }
            CurrentDeckInfo.StageNo.SetValue(stageId);
            CurrentDeckInfo.Encount.SetValue(0);
            MakeStageInfo(stageId, true);
        }

        public bool IsLimitedRank(StageInfo stageInfo)
        {
            return PartyInfo.MissionRank.Value < stageInfo.Master.DisplayRank;
        }

    }
}