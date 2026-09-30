using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class AchievementModel : BaseModel
    {
        public AchievementModel()
        {
        }

        public List<AchievementInfo> AchivementDates()
        {
            // 達成済みは後詰め
            var list = new List<AchievementInfo>();
            foreach (var achievementInfo in PartyInfo.AchievementInfos)
            {
                if (achievementInfo.Master.Rank != PartyInfo.MissionRank.Value)
                {
                    continue;
                }
                list.Add(achievementInfo);
            }
            list.Sort((a, b) => b.SortKey.CompareTo(a.SortKey));
            return list;
        }
    }
}