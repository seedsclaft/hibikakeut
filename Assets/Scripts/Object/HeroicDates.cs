using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class HeroicDates : ScriptableObject
    {
        public List<HeroicData> Data = new();
    }

    [Serializable]
    public class HeroicData : MasterData
    {
        public string Name;
        public string Help;
        public int Param;
        public int MinLv;
        public int MaxLv;
        public string OneParamtext()
        {
            if (Param > 0)
            {
                return "(+" + Param + ")";
            }
            return "(" + Param + ")";
        }
    }
}
