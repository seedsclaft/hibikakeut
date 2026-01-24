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
    public class HeroicData
    {
        public int Id;
        public string Name;
        public string Help;
        public int Param;
        public int MinLv;
        public int MaxLv;
    }
}
