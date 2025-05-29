using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class DungeonDates : ScriptableObject
    {
        public DungeonData Data;
        public List<Ariadne.MapInfo> FloorData = new();
    }

    [Serializable]
    public class DungeonData
    {
        public int Id;
        public string Name;
        public string Help;
        public int FloorId;
        public int Width;
        public int Height;
        public int InitX;
        public int InitY;
        public int InitDir;
    }
}
