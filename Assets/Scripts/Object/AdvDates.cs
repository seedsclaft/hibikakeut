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
    public class AdvData : MasterData
    {
        public string AdvName;
        public EventTiming Timing;
        public int Param1;
        public int Param2;
        public int Param3;
        public bool ReadFlag;
        public int PrizeSetId;
        public string EventKey;
    }
}