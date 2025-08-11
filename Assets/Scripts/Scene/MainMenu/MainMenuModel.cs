using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

        public bool InterludePhase()
        {
            // 6ピリオドでチャプター切り替え
            if (PartyInfo.Period.Value > DataSystem.System.PeriodTurns)
            {
                //PartyInfo.Period.SetValue(1);
                //PartyInfo.Chapter.GainValue(1);
                //PartyInfo.ThisPeriodReliefCount.SetValue(0);
                return true;
            }
            return false;
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

        public bool CheckDepatureDungeon()
        {
            return StageInfos().Count == 0;
        }

        public List<ListData> MainMenuCommand()
        {
            var selectIndex = _sceneParam != null ? _sceneParam.CommandIndex : 0;
            return ListData.MakeListData(DataSystem.TacticsCommand,(a) =>
            {
                if (a.Key == "Transfer")
                {
                    return PartyInfo.Chapter.Value > 3;
                }
                return true;
            },null,(a) =>
            {
                switch (a.Key)
                {
                    case "Departure":
                        return StageInfos().Find(a => PartyInfo.GetDungeonTraverse(a.StageId.Value) == null) != null;
                    case "DeckEdit":
                        return CheckBeforeDepature();
                    case "Mission":
                        return PartyInfo.IsRankUpBefore();
                    case "Relief":
                        return PartyInfo.ReliefCommandCount.Value < PartyInfo.Chapter.Value;
                    case "Present":
                        return PartyInfo.IsOwnItem();
                }
                return false;
            },selectIndex);
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

        public List<SystemData.CommandData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var artifact = new SystemData.CommandData
            {
                Id = 1,
                Name = "アーティファクト",
                Key = "Artifact"
            };
            list.Add(artifact);
            var status = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(19006),
                Key = "Status"
            };
            list.Add(status);
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
    }
}