using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class BuildingsDates : ScriptableObject
    {
        [SerializeField] public List<BuildingsData> Data = new();
    }

    [Serializable]
    public class BuildingsData
    {
        public int Id;
        public string Name;
        public string Help;
        public string ImagePath;
        public int Cost;
        public int Chapter;
        public int SkillId;
        public int NeedBuildingId;
    }

}
