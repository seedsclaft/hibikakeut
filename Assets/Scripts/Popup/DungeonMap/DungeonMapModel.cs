using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class DungeonMapModel : BaseModel
    {
        private HexRoute _hexRoute;
        private List<RoutePath> _hexPaths = new();
        public DungeonMapModel()
        {
            var dungeonFloor = FindDungeonFloor(CurrentStage.StageId.Value);
            _hexRoute = new HexRoute(dungeonFloor.floorSizeHorizontal, dungeonFloor.floorSizeHorizontal, MapCellInfos());
        }

        private Ariadne.FloorMapMasterData FindDungeonFloor(int stageId)
        {
            return DataSystem.FindDungeonFloor(stageId);
        }

        public List<MapCellInfo> MapCellInfos()
        {
            var list = new List<MapCellInfo>();
            var dungeonFloor = FindDungeonFloor(CurrentStage.StageId.Value);
            if (dungeonFloor != null)
            {
                foreach (var mapInfo in dungeonFloor.mapInfo)
                {
                    var hexField = new HexField
                    {
                        X = mapInfo.eventId % dungeonFloor.floorSizeVertical,
                        Y = mapInfo.eventId / dungeonFloor.floorSizeVertical
                    };
                    var traversDates = PartyInfo.GetDungeonTraverse(CurrentStage.StageId.Value);
                    string key = dungeonFloor.floorId.ToString() + "-" + hexField.X.ToString() + "-" + hexField.Y.ToString();
                    var info = new MapCellInfo
                    {
                        MapInfo = mapInfo,
                        HexField = hexField,
                        IsPathSelect = _hexPaths.Find(a => a.X == hexField.X && a.Y == hexField.Y) != null,
                        Opened = traversDates != null && traversDates.Contains(key),
                        IsPlayerPosition = PartyInfo.CurrentDeckInfo.ExistPlayerPosition(mapInfo.eventId),
                    };
                    list.Add(info);
                }
            }
            return list;
        }

        public void FindPath(MapCellInfo mapCellInfo)
        {
            var dungeonFloor = FindDungeonFloor(CurrentStage.StageId.Value);
            var traversDates = PartyInfo.GetDungeonTraverse(CurrentStage.StageId.Value);
            string key = dungeonFloor.floorId.ToString() + "-" + mapCellInfo.HexField.X.ToString() + "-" + mapCellInfo.HexField.Y.ToString();
            if (traversDates == null || !traversDates.Contains(key))
            {
                return;
            }
            var startHex = new HexField
            {
                X = CurrentDeckInfo.PositionX.Value,
                Y = CurrentDeckInfo.PositionY.Value
            };
            var goalHex = new HexField
            {
                X = mapCellInfo.MapInfo.eventId % dungeonFloor.floorSizeVertical,
                Y = mapCellInfo.MapInfo.eventId / dungeonFloor.floorSizeVertical
            };
            _hexRoute.FindRoute(MoveType.Normal, startHex, goalHex, false);
            _hexPaths = _hexRoute.Pathlist;
        }

        public void SetRoutePaths()
        {
            if (_hexPaths.Count > 0)
            {
                CurrentDeckInfo.SetRoutePaths(_hexPaths);
            }
        }

        public int ConstraintCount()
        {
            var dungeonFloor = FindDungeonFloor(CurrentStage.StageId.Value);
            return dungeonFloor.floorSizeHorizontal;
        }
    }
}