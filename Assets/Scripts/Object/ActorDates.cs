using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class ActorDates : ScriptableObject
    {
        public List<ActorData> Data = new();
    }

    [Serializable]
    public class ActorData : MasterData
    {
        public string Name;
        public string EnglishName;
        public string SubName;
        public string Relief;
        public string EnglishRelief;
        public string Profile;
        public int Rank;
        //public UnitType UnitType;
        public string ImagePath;
        public int InitLv;
        public int MaxLv;
        public AttributeType AttributeType;
        public StatusInfo InitStatus;
        public StatusInfo PlusStatus;
        public StatusInfo NeedStatus;
        public List<AttributeRank> Attribute;
        public int X;
        public int Y;
        public float Scale;
        public int AwakenX;
        public int AwakenY;
        public float AwakenScale;
        public List<KindType> Kinds;
        public List<LearningData> LearningSkills = new();
        public List<SkillTriggerActorData> SkillTriggerDates = new();

        public string GetName()
        {
            if (GameSystem.GetLanguage() == Language.English)
            {
                return EnglishName;
            }
            return Name;
        }

        public string GetRelief()
        {
            if (GameSystem.GetLanguage() == Language.English)
            {
                return EnglishRelief;
            }
            return Relief;
        }
    }

    public enum UnitType
    {
        None = 0,
        Attacker,
        Defender,
        Enhancer,
        Healer,
        Jammer
    }

    [Serializable]
    public class LearningData
    {
        public int SkillId;
        public int Level;
        public int Weight;
        public List<SkillData.TriggerData> TriggerDates;
    }

    [Serializable]
    public class SkillTriggerActorData
    {
        public int SkillId;
        public int Trigger1;
        public int Trigger2;
    }
}