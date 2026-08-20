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
    public class ItemData : MasterData
    {
        public int IconIndex;
        public ItemType ItemType;
        public int Param1;
        public int Param2;
        public int Param3;
        public int Cost;
        public string Name;
        public string Help;
        public string EnglishName;
        public string EnglishHelp;
        public string GetName()
        {
            if (GameSystem.GetLanguage() == Language.English)
            {
                return EnglishName;
            }
            return Name;
        }

        public string GetHelp()
        {
            if (GameSystem.GetLanguage() == Language.English)
            {
                return EnglishHelp;
            }
            return Help;
        }

        public bool IsAddEquipment()
        {
            return ItemType == ItemType.RandumAddEquipment || ItemType == ItemType.SelectAddEquipment;
        }

        public bool IsPresentItem()
        {
            return ItemType == ItemType.Currency || IsAddEquipment();
        }
    }

    [Serializable]
    public enum ItemType
    {
        RandumAddEquipment = 10,
        SelectAddEquipment = 11,
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
        Heal = 70,
    }
}
