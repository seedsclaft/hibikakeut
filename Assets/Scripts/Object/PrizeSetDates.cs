using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class PrizeSetDates : ScriptableObject
    {
        public List<PrizeSetData> Data = new();
    }

    [Serializable]
    public class PrizeSetData : MasterData
    {
        public GetItemData GetItem;
    }
}