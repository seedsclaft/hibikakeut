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
        public int IconIndex;
        public ItemType ItemType;
        public int Param1;
        public int Param2;
        public int Param3;
        public string Name;
        public string Help;
    }

    [Serializable]
    public enum ItemType
    {
        RandumAddSkill = 10,
        Artifact = 20,
        Currency = 30,
        UseItem = 40,
        RandumAddItem = 41,
        DungeonItem = 50,
    }

    [Serializable]
    public enum UseItemType
    {
        Exp = 10,
        AttributeUp = 20,
        StatusUp = 30,
        ClassChange = 40,
        EncountRate = 50,
        DungeonTurn = 60,
    }
}
