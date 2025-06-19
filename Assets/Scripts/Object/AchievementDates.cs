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
        public AchievementCategory Category;
        public int Rank;
        public AchievementConditionType ConditionType;
        public int Param1;
        public int Param2;
        public int PriseSetId;
        public string Text;
        public string Help;
    }

    public enum AchievementCategory
    {
        None = 0,
        Main = 10, // 重要課題
        Normal = 20,
    }

    public enum AchievementConditionType
    {
        Complete = 10,
        DepartureCount = 1010,
        BattleVictory = 1020,
        CharacterLevel = 1030,
        CharacterLevelNum = 1031,
        TacticsLvupCount = 1040,
        BattleScore = 1050,
        TotalDamage = 1060,
        ClearStage = 2010,
        UseAwakeSkillCount = 3010,
        UseChangeLineCount = 3020,
        DeckEditCommandCount = 7020,
        PresentCommandCount = 7040,
        ReliefCommandCount = 7050,
        TransferCommandCount = 7060,
        StatusSkillChangeCount = 7080,
        MissionRank = 8010,
        ClearStageNum = 8020,
        PartyEvaluate = 8030,
    }
}
