using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class DungeonMapModel : BaseModel
    {
        private HexRoute _hexRoute;
        private List<HexPath> _hexPaths = new();
        public DungeonMapModel()
        {
            var dungeonFloor = DataSystem.FindDungeonFloor(CurrentStage.StageId.Value);
            _hexRoute = new HexRoute(dungeonFloor.floorSizeHorizontal, dungeonFloor.floorSizeHorizontal, MapCellInfos());
        }

        public List<MapCellInfo> MapCellInfos()
        {
            var list = new List<MapCellInfo>();
            var dungeonFloor = DataSystem.FindDungeonFloor(CurrentStage.StageId.Value);
            if (dungeonFloor != null)
            {
                foreach (var mapInfo in dungeonFloor.mapInfo)
                {
                    var hexField = new HexField
                    {
                        X = mapInfo.eventId % dungeonFloor.floorSizeHorizontal,
                        Y = mapInfo.eventId / dungeonFloor.floorSizeHorizontal
                    };
                    var info = new MapCellInfo
                    {
                        MapInfo = mapInfo,
                        HexField = hexField,
                        IsPathSelect = _hexPaths.Find(a => a.X == hexField.X && a.Y == (hexField.Y)) != null
                    };
                    list.Add(info);
                }
            }
            return list;
        }

        public void FindPath(MapCellInfo mapCellInfo)
        {
            var startHex = new HexField
            {
                X = CurrentDeckInfo.PositionX.Value,
                Y = CurrentDeckInfo.PositionY.Value
            };
            var dungeonFloor = DataSystem.FindDungeonFloor(CurrentStage.StageId.Value);
            var goalHex = new HexField
            {
                X = mapCellInfo.MapInfo.eventId % dungeonFloor.floorSizeHorizontal,
                Y = mapCellInfo.MapInfo.eventId / dungeonFloor.floorSizeHorizontal
            };
            _hexRoute.FindRoute(MoveType.Normal, startHex, goalHex, false);
            _hexPaths = _hexRoute.Pathlist;
        }
    }
}