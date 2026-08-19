using System.Collections.Generic;

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
                Id = (int)SideMenuCommandType.Option,
                TextId = 19101,
            };
            list.Add(optionCommand);
            var menuCommand = new SystemData.CommandData
            {
                Id = (int)SideMenuCommandType.Licence,
                TextId = 19107,
            };
            list.Add(menuCommand);
            var initCommand = new SystemData.CommandData
            {
                Id = (int)SideMenuCommandType.InitializeData,
                TextId = 19108,
            };
            list.Add(initCommand);
#if !UNITY_WEBGL
            var endCommand = new SystemData.CommandData
            {
                Id = (int)SideMenuCommandType.EndGame,
                TextId = 19109,
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
            PartyInfo.SetAchievementRank(DataSystem.Dates[DataType.Achievements].ToList<AchievementData>());
            TempInfo.LastStartTime.SetValue((int)TempInfo.LocalEpochTime());
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
            PartyInfo.SetAchievementRank(DataSystem.Dates[DataType.Achievements].ToList<AchievementData>());
            TempInfo.LastStartTime.SetValue((int)TempInfo.LocalEpochTime());
        }

        public AdvData GameStartEventAdv()
        {
            var events = GetAdvDates(EventTiming.GameStart, true);
            return events.Count > 0 ? events[0] : null;
        }
    }
}