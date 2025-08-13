using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class StageListModel : BaseModel
    {
        public StageListModel()
        {
        }

        public DungeonResumeInfo GetDungeonResumeInfo(int stageId)
        {
            return PartyInfo.DungeonResumeInfos.Find(a => a.DungeonId.Value == stageId);
        }

        public void MakeStageInfoDepature(int stageId, bool resumeStart)
        {
            var resumeInfo = PartyInfo.DungeonResumeInfos.Find(a => a.DungeonId.Value == stageId);
            // 復帰処理
            if (resumeInfo != null && resumeStart)
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