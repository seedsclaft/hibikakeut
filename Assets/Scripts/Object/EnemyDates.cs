﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class EnemyDates : ScriptableObject
    {
        public List<EnemyData> Data = new();
    }

    [Serializable]
    public class EnemyData
    {   
        public int Id;
        public string Name;
        public string ImagePath;
        public float ImageScale;
        public StatusInfo BaseStatus;
        public List<KindType> Kinds;
        public int HpGrowth;
        public int MpGrowth;
        public int AtkGrowth;
        public int DefGrowth;
        public int SpdGrowth;
        public List<LearningData> LearningSkills = new();
        public List<SkillTriggerActorData> SkillTriggerDates = new();
        
        public int CurrentParam(StatusParamType growType,int level)
        {
            return 0;
        }

        public StatusInfo LevelUpStatus(int level)
        {
            StatusInfo upStatus = new StatusInfo();
            foreach (StatusParamType growType in Enum.GetValues(typeof(StatusParamType)))
            {
                int currentParam = CurrentParam(growType,level);
                int nextParam = CurrentParam(growType,level + 1);
                if (currentParam < nextParam){
                    int upParam = nextParam - currentParam;
                    upStatus.AddParameter(growType,upParam);
                }
            }
            return upStatus;
        }
    }


    public enum KindType
    {
        None = 0,
        Fire = 1,
        Thunder = 2,
        Ice = 3,
        Shine = 4,
        Dark = 5,
        Normal = 11,
        Boss = 12,
        Undead = 21,
        Air = 22,
        Demon = 23,
        Creature = 24,
        Animal = 25,
        UnKnown = 26,
    }

    public enum LineType
    {
        Front = 0,
        Back = 1
    }
}