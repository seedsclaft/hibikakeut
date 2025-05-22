using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class AchievementDates : ScriptableObject
    {
        [SerializeField] public List<AchievementData> Data = new();
    }

    [Serializable]
    public class AchievementData
    {
        public int Id;
        public int Rank;
        public AchievementConditionType ConditionType;
        public int Param1;
        public int Param2;
        public int PriseSetId;
        public string Text;
        public string Help;
    }

    public enum AchievementConditionType
    {
        DepartureCount = 1010,
    }
}
