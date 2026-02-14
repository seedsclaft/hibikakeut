using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    [Serializable]
    public class AchievementInfo
    {
        public AchievementData Master => DataSystem.Dates[DataType.Achievements].Find(Id.Value) as AchievementData;
        public ParameterInt Id = new();

        // 達成値
        public ParameterInt Count = new();
        public ParameterInt AchieveCount = new();

        public ParameterBool Achieved = new(false);
        public ParameterBool Presented = new(false);

        public List<PrizeSetData> PrizeSetsMaster => DataSystem.Dates[DataType.PrizeSets].FindAll<PrizeSetData>(a => a.Id == Master.PriseSetId);

        public float AchievePer => AchieveCount.Value > 0 ? (float)Count.Value / (float)AchieveCount.Value : 0;

        public int SortKey()
        {
            int sort = Id.Value;
            if (Achieved.Value)
            {
                sort += 10000000;
            }
            sort -= Master.Rank * 100000;
            return sort;
        }

        public AchievementInfo(AchievementData achievementData)
        {
            Id.SetValue(achievementData.Id);
        }

        public void SetCondition(int count, int achieveCount)
        {
            if (count > achieveCount)
            {
                count = achieveCount;
            }
            Count.SetValue(count);
            AchieveCount.SetValue(achieveCount);
            Achieved.SetValue(Count.Value >= AchieveCount.Value);
        }
    }
}
