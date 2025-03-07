﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class ActorDates : ScriptableObject
    {
        [SerializeField] public List<ActorData> Data = new();
    }

    [Serializable]
    public class ActorData 
    {   
        public int Id;
        public string Name;
        public string SubName;
        public string Profile;
        public int ClassId;
        //public UnitType UnitType;
        public string ImagePath;
        public int InitLv;
        public int MaxLv;
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