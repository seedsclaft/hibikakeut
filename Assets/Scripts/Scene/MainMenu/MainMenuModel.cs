using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Ryneus
{
    public class MainMenuModel : BaseModel
    {
        public void CheckAchievementConditions()
        {
            foreach (var achievementInfo in PartyInfo.AchievementInfos)
            {
                CheckAchievementCondition(achievementInfo);
            }
        }

        private void CheckAchievementCondition(AchievementInfo achievementInfo)
        {
            switch(achievementInfo.Master.ConditionType)
            {
                case AchievementConditionType.DepartureCount:
                    // 出撃回数
                    achievementInfo.SetCondition(PartyInfo.DepartureCount.Value,achievementInfo.Master.Param1);
                    break;
            }
        }

        public List<GetItemInfo> AchievementGetItemInfos()
        {
            var list = new List<GetItemInfo>();
            var achievements = PartyInfo.AchievementInfos.FindAll(a => !a.Presented.Value && a.Achieved.Value);
            foreach (var achievement in achievements)
            {
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == achievement.Master.PriseSetId);
                if (prizeSets != null)
                {
                    foreach (var prizeSet in prizeSets)
                    {
                        var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                        list.Add(getItemInfo);
                    }
                }
                achievement.Presented.SetValue(true);
            }
            return list;
        }

        public bool IsEnding()
        {
            return false;//PartyInfo.HasEndingGetItem();
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
            var status = new SystemData.CommandData
            {
                Id = 1,
                Name = "メンバー確認",
                Key = "Status"
            };
            list.Add(status);
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