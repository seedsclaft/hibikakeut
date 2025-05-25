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
        Complete = 10,
        DepartureCount = 1010,
        BattleVictory = 1020,
        CharacterLevel = 1030,
        TacticsLvupCount = 1040,
        BattleScore = 1050,
        TotalDamage = 1060,
    }
}
