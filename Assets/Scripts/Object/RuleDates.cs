using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class RuleDates : ScriptableObject
    {
        public List<RuleData> Data = new();
    }

    [Serializable]
    public class RuleData : MasterData
    {
        public string Name;
        public string Help;
        public int Category;
    }
}