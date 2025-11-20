using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public class MainMenuModel : BaseModel
    {
        private MainMenuSceneInfo _sceneParam;
        public MainMenuSceneInfo SceneParam => _sceneParam;
        public MainMenuModel()
        {
            _sceneParam = (MainMenuSceneInfo)GameSystem.SceneStackManager.LastSceneParam;
        }

        public async UniTask PlayMainStageBgmData()
        {
            var key = "Mainmenu";
            if (StageInfos().Find(a => a.Master.Category == StageCategory.BattleField) != null)
            {
                key = "Mainmenu2";
            }
            var bgmData = DataSystem.BGM.Find(a => a.Key == key);
            var bgm = await ResourceSystem.LoadBGMAsset(key);
            SoundManager.Instance.PlayBgm(bgm, bgmData.Volume, true);
        }

        public async UniTask PlayReliefBgmData()
        {
            var key = "Relief1";
            var bgmData = DataSystem.BGM.Find(a => a.Key == key);
            var bgm = await ResourceSystem.LoadBGMAsset(key);
            SoundManager.Instance.PlayBgm(bgm, bgmData.Volume, true);
        }

        public async UniTask PlayReliefBgmData2()
        {
            var key = "Relief2";
            var bgmData = DataSystem.BGM.Find(a => a.Key == key);
            var bgm = await ResourceSystem.LoadBGMAsset(key);
            SoundManager.Instance.ChangeCrossFade(bgm, bgmData.Volume, true);
        }

        public bool InterludePhase()
        {
            return PartyInfo.Period.Value > DataSystem.System.PeriodTurns;
        }

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

        public bool CheckBeforeDepature()
        {
            var editNum = 0;
            foreach (var actorIdDict in CurrentDeckInfo.ActorIdDict)
            {
                if (actorIdDict.Value > -1)
                {
                    editNum++;
                }
            }
            if (editNum < 6)
            {
                if (editNum < PartyInfo.EditableActorInfos().Count)
                {
                    return true;
                }
            }
            return false;
        }

        public AdvData MainmenuEvent()
        {
            var eventKeys = CurrentGameInfo.ReadEventKeys;
            var events = DataSystem.Adventures.FindAll(a => a.Timing == EventTiming.BeforeMainMenu && a.Param1 == PartyInfo.Chapter.Value && a.Param2 == PartyInfo.Period.Value && !eventKeys.Contains(a.EventKey));
            return events.Count > 0 ? events[0] : null;
        }

        public bool CheckDepatureDungeon()
        {
            return StageInfos().Count == 0;
        }

        public List<ActorInfo> MainMenuActorInfos()
        {
            return PartyInfo.CurrentDeckActorInfos().FindAll(a => a.Master.Rank != 10);
        }

        public List<ActorInfo> ReleifActoInfos()
        {
            // 初回はRank20
            // 2回目以降はRank30
            var limitRank = 20;
            if (PartyInfo.ActorInfos.Count > 2)
            {
                limitRank = 30;
            }
            // 未加入の仲間
            var actorDates = DataSystem.Actors.Where(a => PartyInfo.ActorInfos.Find(b => a.Value.Id == b.ActorId.Value) == null).ToList();
            var actorInfos = new List<ActorInfo>();
            foreach (var actorDate in actorDates)
            {
                if (actorDate.Value.Rank < limitRank)
                {
                    continue;
                }
                actorInfos.Add(new ActorInfo(actorDate.Value));
            }
            // 3名までに絞る
            if (actorInfos.Count > 3)
            {
                var minusCount = actorInfos.Count - 3;
                var removedIds = new List<int>();
                while (minusCount != 0)
                {
                    var rand = UnityEngine.Random.Range(0, actorInfos.Count);
                    if (!removedIds.Contains(actorInfos[rand].ActorId.Value))
                    {
                        removedIds.Add(actorInfos[rand].ActorId.Value);
                        minusCount--;
                    }
                }
                for (int i = actorInfos.Count - 1; i >= 0; i--)
                {
                    if (removedIds.Contains(actorInfos[i].ActorId.Value))
                    {
                        actorInfos.Remove(actorInfos[i]);
                    }
                }
            }
            return actorInfos;
        }

        public List<GetItemInfo> ReleifGetItemInfos(List<ActorInfo> actorInfos)
        {
            var getItemInfos = new List<GetItemInfo>();
            foreach (var actorInfo in actorInfos)
            {
                var getItemInfo = MakeGetItemInfo(GetItemType.SelectAddActor, actorInfo.ActorId.Value, 0);
                getItemInfos.Add(getItemInfo);
            }
            return getItemInfos;
        }

        public List<ListData> MainMenuCommand()
        {
            var selectIndex = _sceneParam != null ? _sceneParam.CommandIndex : 0;
            return ListData.MakeListData(DataSystem.TacticsCommand, (a) =>
            {
                if (a.Key == "Transfer")
                {
                    return PartyInfo.Chapter.Value > 3;
                }
                return true;
            }, null, (a) =>
            {
                switch (a.Key)
                {
                    case "Departure":
                        return StageInfos().Find(a => PartyInfo.GetDungeonTraverse(a.StageId.Value) == null) != null || IsDepatureBatch();
                    case "DeckEdit":
                        return CheckBeforeDepature();
                    case "Mission":
                        return PartyInfo.IsRankUpBefore();
                    case "Relief":
                        return PartyInfo.ReliefItemCount.Value > 0;
                    case "Present":
                        return PartyInfo.IsOwnItem();
                }
                return false;
            }, selectIndex);
        }

        public bool HasBattleField()
        {
            return StageInfos().Find(a => a.Master.Category == StageCategory.BattleField) != null;
        }

        public bool IsSideManuBatch()
        {
            var achievement = PartyInfo.NearAchievementInfo();
            return !achievement.Achieved.Value && (achievement.Master.ConditionType == AchievementConditionType.TacticsLvupCount || achievement.Master.ConditionType == AchievementConditionType.StatusSkillChangeCount);
        }

        public bool IsStatusBatch()
        {
            return PartyInfo.AchievementInfos.Find(a => !a.Achieved.Value && (a.Master.ConditionType == AchievementConditionType.TacticsLvupCount || a.Master.ConditionType == AchievementConditionType.StatusSkillChangeCount)) != null;
        }

        private bool IsDepatureBatch()
        {
            var achievement = PartyInfo.NearAchievementInfo();
            return !achievement.Achieved.Value && achievement.Master.ConditionType == AchievementConditionType.ClearStage && StageInfos().Find(a => a.Master.Id == achievement.Master.Param1) != null;
        }

        public void EndTransfer()
        {
            PartyInfo.EndTransfer();
        }

        public List<SystemData.CommandData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var status = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(19006),
                Key = "Status"
            };
            list.Add(status);
            var artifact = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(37000),
                Key = "Artifact"
            };
            list.Add(artifact);
            var option = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(19101),
                Key = "Option"
            };
            list.Add(option);
            var menuCommand = new SystemData.CommandData
            {
                Id = 3,
                Name = DataSystem.GetText(19102),
                Key = "Help"
            };
            list.Add(menuCommand);
            var dictionaryCommand = new SystemData.CommandData
            {
                Id = 11,
                Name = DataSystem.GetText(19103),
                Key = "Dictionary"
            };
            list.Add(dictionaryCommand);
            var saveCommand = new SystemData.CommandData
            {
                Id = 4,
                Name = DataSystem.GetText(19104),
                Key = "Save"
            };
            list.Add(saveCommand);
            var titleCommand = new SystemData.CommandData
            {
                Id = 5,
                Name = DataSystem.GetText(19106),
                Key = "Title"
            };
            list.Add(titleCommand);
            return list;
        }
    }

    public class MainMenuSceneInfo
    {
        public int CommandIndex;
        public bool PeriodAnimation = false;
    }
}