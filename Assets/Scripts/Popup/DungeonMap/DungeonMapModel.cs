using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class DungeonMapModel : BaseModel
    {
        public DungeonMapModel()
        {
        }

        public List<MapCellInfo> MapCellInfos()
        {
            var list = new List<MapCellInfo>();
            var dungeonFloor = DataSystem.FindDungeonFloor(CurrentStage.StageId.Value);
            if (dungeonFloor != null)
            {
                foreach (var mapInfo in dungeonFloor.mapInfo)
                {
                    var info = new MapCellInfo
                    {
                        MapInfo = mapInfo
                    };
                    list.Add(info);
                }
            }
            return list;
        }
    }
}