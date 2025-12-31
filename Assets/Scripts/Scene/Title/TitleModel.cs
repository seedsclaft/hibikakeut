using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ryneus
{
    public class TitleModel : BaseModel
    {
        public List<ListData> TitleCommand()
        {
            var selectIndex = ExistsLoadFile() ? 1 : 0;
            return ListData.MakeListData(DataSystem.TitleCommand, (a) =>
            {
                switch (a.Key)
                {
                    case "CONTINUE":
                        return ExistsLoadFile();
                }
                return true;
            }, selectIndex);
        }

        public bool ExistsLoadFile()
        {
            return SaveSystem.ExistsStageFile(1);
        }

        public string VersionText()
        {
            return GameSystem.Version;
        }

        public List<SystemData.CommandData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var optionCommand = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(19101),
                Key = "Option"
            };
            list.Add(optionCommand);
            var menuCommand = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(19107),
                Key = "License"
            };
            list.Add(menuCommand);
            /*
            var deleteStage = new SystemData.CommandData
            {
                Id = 3,
                Name = DataSystem.GetText(13420),
                Key = "DeleteStage"
            };
            list.Add(deleteStage);
            */
            var initCommand = new SystemData.CommandData
            {
                Id = 4,
                Name = DataSystem.GetText(19108),
                Key = "InitializeData"
            };
            list.Add(initCommand);
#if !UNITY_WEBGL
            var endCommand = new SystemData.CommandData
            {
                Id = 5,
                Name = DataSystem.GetText(19109),
                Key = "EndGame"
            };
            list.Add(endCommand);
#endif
            return list;
        }

        public void InitializeNewGame()
        {
            var stageId = DataSystem.System.StartStageId;
            InitSaveStageInfo();
            MakeStageInfo(stageId, true);
            // ステージ開始時アイテム入手
            var stageEventDates = StageEvents(EventTiming.GameStart);
            foreach (var stageEventDate in stageEventDates)
            {
                switch (stageEventDate.Type)
                {
                    case StageEventType.AddActor:
                        var getItemData = new GetItemData
                        {
                            Type = GetItemType.AddActor,
                            Param1 = stageEventDate.Param
                        };
                        var getItemInfo = new GetItemInfo(getItemData);
                        AddGetItemInfo(getItemInfo);
                        break;
                }
            }
            var stageData = DataSystem.FindStage(stageId);
            var floor = DataSystem.FindDungeonFloor(stageId);
            CurrentDeckInfo.SetPosition(stageId, floor.entrancePos.x, floor.entrancePos.y, (int)floor.enteringDir);
            CurrentDeckInfo.StageNo.SetValue(stageData.StageNo);
            PartyInfo.SetAchievementRank(DataSystem.Achievements);
        }

        public void InitializeNewGameSkipOpening()
        {
            var stageId = 10;
            InitSaveStageInfo();
            MakeStageInfo(stageId, true);
            // ステージ開始時アイテム入手
            var stageEventDates = StageEvents(EventTiming.GameStart);
            foreach (var stageEventDate in stageEventDates)
            {
                switch (stageEventDate.Type)
                {
                    case StageEventType.AddActor:
                        var getItemData = new GetItemData
                        {
                            Type = GetItemType.AddActor,
                            Param1 = stageEventDate.Param
                        };
                        var getItemInfo = new GetItemInfo(getItemData);
                        AddGetItemInfo(getItemInfo);
                        break;
                }
            }
            var stageData = DataSystem.FindStage(stageId);
            var floor = DataSystem.FindDungeonFloor(stageId);
            CurrentDeckInfo.SetPosition(stageId, floor.entrancePos.x, floor.entrancePos.y, (int)floor.enteringDir);
            CurrentDeckInfo.StageNo.SetValue(stageData.StageNo);
            PartyInfo.SetAchievementRank(DataSystem.Achievements);
        }

        public async Task LoadFile()
        {
            _ = await SaveSystem.LoadStageInfo(0);
            //TempInfo.SetPlayingTime(saveFileInfo.PlayTime);
        }

        public AdvData GameStartEventAdv()
        {
            var events = GetAdvDates(EventTiming.GameStart, true);
            return events.Count > 0 ? events[0] : null;
        }
    }
}