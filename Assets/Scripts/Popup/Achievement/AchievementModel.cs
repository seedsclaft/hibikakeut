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
            return PartyInfo.AchievementInfos;
        }
    }
}