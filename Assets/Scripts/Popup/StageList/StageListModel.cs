using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class StageListModel : BaseModel
    {
        public StageListModel()
        {
        }

        public DungeonResumeInfo GetDungeonResumeInfo(int stageNo)
        {
            return PartyInfo.DungeonResumeInfos.Find(a => a.StageNo.Value == stageNo);
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
            CurrentDeckInfo.EncountRate.SetValue(1);
            CurrentDeckInfo.EncountRateTurn.SetValue(0);
            MakeStageInfo(stageId, true);
            CurrentDeckInfo.SetEncountTimes(CurrentStage.Master.EncountTimes);
            PartyInfo.PartyStatInfo.DepartureCount.GainValue(1);
            if (CurrentStage.Master.Category == StageCategory.BattleField)
            {
                PartyInfo.PartyStatInfo.DepartureBattleFieldCount.GainValue(1);
            }
        }

        public bool IsLimitedRank(StageInfo stageInfo)
        {
            return PartyInfo.MissionRank.Value < stageInfo.Master.DisplayRank;
        }

    }
}