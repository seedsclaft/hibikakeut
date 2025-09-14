using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class HexUnitInfo
    {
        public HexField HexField;
        public bool IsUnit = true;
        public bool IsWall = true;
        public bool OnField(HexField hexField)
        {
            return true;
        }
    }
}
