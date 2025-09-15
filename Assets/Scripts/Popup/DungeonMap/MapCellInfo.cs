using System;
using Ariadne;

namespace Ryneus
{
    public class MapCellInfo
    {
        public MapInfo MapInfo;

        public HexField HexField;
        public bool IsUnit => GameSystem.GameInfo.PartyInfo.CurrentDeckInfo.ExistPlayerPosition(MapInfo.eventId);
        public bool IsWall => MapInfo != null ? MapInfo.mapAttr == 1 || MapInfo.mapAttr == 99 : false;
        public bool IsPathSelect;
        public bool Opened = false;
        public bool IsPlayerPosition = false;
        public bool IsRoad()
        {
            if (MapInfo != null)
            {
                return MapInfo.mapAttr == 0 || (MapInfo.mapAttr > 1 && MapInfo.mapAttr < 99);
            }
            return false;
        }
        public bool IsCellImage()
        {
            if (MapInfo != null)
            {
                return MapInfo.mapAttr > 1 && MapInfo.mapAttr < 99;
            }
            return false;
        }
        public bool OnField(HexField hexField)
        {
            return true;
        }
    }
}
