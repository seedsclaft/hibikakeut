using System.Collections.Generic;

namespace Ryneus
{
    public partial class BaseModel
    {
        public List<StageInfo> StageInfos()
        {
            var list = new List<StageInfo>();
            foreach (var stageData in DataSystem.Stages)
            {
                if (!stageData.Selectable)
                {
                    continue;
                }
                if (stageData.DisplayRank > PartyInfo.MissionRank.Value)
                {
                    continue;
                }
                if (stageData.Chapter > PartyInfo.Chapter.Value)
                {
                    continue;
                }
                if (stageData.OnlyOnce && PartyInfo.GetDungeonTraverse(stageData.Id) != null)
                {
                    continue;
                }
                var cleared = PartyInfo.IsClaeredStage(stageData.StageNo);
                var stageInfo = new StageInfo(stageData.Id,cleared);
                list.Add(stageInfo);
            }
            return list;
        }

        public void MakeStageInfo(int stageId,bool startStage,int clearCount = 0)
        {
            SaveDungeonPlayerData();
            var stageInfo = new StageInfo(stageId);

            // ダンジョンターン数を設定
            if (startStage)
            {
                PartyInfo.TurnCount.SetValue(50);
            }
            CurrentGameInfo.SetStageInfo(stageInfo);
            PartyInfo.StageId.SetValue(stageId);
        }
    }
}
