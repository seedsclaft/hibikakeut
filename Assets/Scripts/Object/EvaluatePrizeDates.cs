using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class EvaluatePrizeDates : ScriptableObject
    {
        [SerializeField] public List<EvaluatePrizeData> Data = new();
    }

    [Serializable]
    public class EvaluatePrizeData
    {
        public int Id;
        public int Chapter;
        public int Category;
        public AchievementConditionType ConditionType;
        public int Param1;
        public int Param2;
        public int PriseSetId;
    }

}
