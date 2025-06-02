using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace Ryneus
{
    public class MainMenuModel : BaseModel
    {

        public bool IsEnding()
        {
            return false;//PartyInfo.HasEndingGetItem();
        }

        public List<ActorInfo> AddSelectActorInfos()
        {
            // 未加入の仲間
            var actorDates = DataSystem.Actors.Where(a => PartyInfo.ActorInfos.Find(b => a.Value.Id == b.ActorId.Value) == null).ToList();
            var actorInfos = new List<ActorInfo>();
            foreach (var actorDate in actorDates)
            {
                actorInfos.Add(new ActorInfo(actorDate.Value));
            }
            return actorInfos;
        }

        public List<ListData> MainMenuCommand()
        {
            return ListData.MakeListData(DataSystem.TacticsCommand,(a) =>
            {
                return true;
            },0);
        }

        public List<SystemData.CommandData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            /*
            var status = new SystemData.CommandData
            {
                Id = 1,
                Name = "メンバー確認",
                Key = "Status"
            };
            list.Add(status);
            */
            var option = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(13410),
                Key = "Option"
            };
            list.Add(option);
            var menuCommand = new SystemData.CommandData
            {
                Id = 3,
                Name = DataSystem.GetText(19700),
                Key = "Help"
            };
            list.Add(menuCommand);
            var dictionaryCommand = new SystemData.CommandData
            {
                Id = 11,
                Name = DataSystem.GetText(19730),
                Key = "Dictionary"
            };
            list.Add(dictionaryCommand);
            var saveCommand = new SystemData.CommandData
            {
                Id = 4,
                Name = DataSystem.GetText(19710),
                Key = "Save"
            };
            list.Add(saveCommand);
            var titleCommand = new SystemData.CommandData
            {
                Id = 5,
                Name = DataSystem.GetText(19720),
                Key = "Title"
            };
            list.Add(titleCommand);
            return list;
        }
    }
}