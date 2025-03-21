using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class AdvDates : ScriptableObject
    {
        public List<AdvData> Data = new();
    }

    [Serializable]
    public class AdvData
    {   
        public int Id;
        public string AdvName;
        public Scene EndJump;
        public int PrizeSetId;
    }
}