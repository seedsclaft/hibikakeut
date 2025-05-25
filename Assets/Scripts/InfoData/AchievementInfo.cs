using System.Collections.Generic;

namespace Ryneus
{
    [System.Serializable]
    public class AchievementInfo
    {
        public AchievementData Master => DataSystem.Achievements.Find(a => a.Id == Id.Value);
        public ParameterInt Id = new();

        // 達成値
        public ParameterInt Count = new();
        public ParameterInt AchieveCount = new();

        public ParameterBool Achieved = new(false);
        public ParameterBool Presented = new(false);

        private List<GetItemInfo> _getItemInfos = new();
        public List<GetItemInfo> GetItemInfos => _getItemInfos;

        public float AchievePer => AchieveCount.Value > 0 ? Count.Value / AchieveCount.Value : 0;

        public AchievementInfo(AchievementData achievementData)
        {
            Id.SetValue(achievementData.Id);
            var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == achievementData.PriseSetId);
            if (prizeSets != null)
            {
                foreach (var prizeSet in prizeSets)
                {
                    var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                    _getItemInfos.Add(getItemInfo);
                }
            }
        }

        public void SetCondition(int count,int achieveCount)
        {
            Count.SetValue(count);
            AchieveCount.SetValue(achieveCount);
            Achieved.SetValue(Count.Value >= AchieveCount.Value);
        }
    }
}
