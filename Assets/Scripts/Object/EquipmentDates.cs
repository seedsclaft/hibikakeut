using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class EquipmentDates : ScriptableObject
    {
        public List<EquipmentData> Data = new();
    }

    [Serializable]
    public class EquipmentData : MasterData
    {
        public string Name;
        public string EnglishName;
        public MagicIconType IconIndex;
        public int Rank;
        public AttributeType Attribute;
        public List<EquipmentLearningData> LearningDates;
        public string GetName()
        {
            if (GameSystem.GetLanguage() == Language.English)
            {
                return EnglishName;
            }
            return Name;
        }

    }

    [Serializable]
    public class EquipmentLearningData : MasterData
    {
        public int SkillId;
        public int Rate;
        public bool EquipmentOnly;
    }
}