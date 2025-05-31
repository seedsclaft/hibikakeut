using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class StageListModel : BaseModel
    {
        public StageListModel()
        {
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
                var cleared = PartyInfo.IsClaeredStage(stageData.Id);
                var stageInfo = new StageInfo(stageData.Id,cleared);
                list.Add(stageInfo);
            }
            return list;
        }
    }
}