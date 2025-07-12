using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class ReleaseListModel : BaseModel
    {
        public ReleaseListModel()
        {
        }

        public List<BuildingInfo> BuildingInfos()
        {
            var list = new List<BuildingInfo>();
            var dates = DataSystem.Buildings.FindAll(a => a.Chapter <= PartyInfo.Chapter.Value);
            foreach (var data in dates)
            {
                if (data.NeedBuildingId > 0)
                {
                    if (!PartyInfo.BuildingIds.Contains(data.NeedBuildingId))
                    {
                        continue;
                    }
                }
                var info = new BuildingInfo();
                info.Id.SetValue(data.Id);
                list.Add(info);
            }
            return list;
        }

    }
}