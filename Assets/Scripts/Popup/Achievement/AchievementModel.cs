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
                if (!achievementInfo.Achieved.Value)
                {
                    list.Add(achievementInfo);
                }
            }
            list.Sort((a, b) => a.Master.Rank - b.Master.Rank > 0 ? -1 : 1);
            //list.Sort((a, b) => a.SortKey() - b.SortKey() > 0 ? 1 : -1);
            foreach (var achievementInfo in PartyInfo.AchievementInfos)
            {
                if (achievementInfo.Achieved.Value)
                {
                    list.Add(achievementInfo);
                }
            }
            return list;
        }
    }
}