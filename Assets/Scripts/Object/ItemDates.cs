using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class ItemDates : ScriptableObject
    {
        [SerializeField] public List<ItemData> Data = new();
    }

    [Serializable]
    public class ItemData
    {
        public int Id;
        public int ItemType;
        public int Param1;
        public int Param2;
        public string Text;
        public string Help;
    }

}
