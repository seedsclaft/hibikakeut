using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                case AchievementConditionType.Complete:
                    // 達成数
                    var achived = PartyInfo.AchievementInfos.FindAll(a => a.Achieved.Value).Count;
                    achievementInfo.SetCondition(achived,PartyInfo.AchievementInfos.Count-1);
                    if (achievementInfo.Achieved.Value)
                    {
                        PartyInfo.MissionRank.GainValue(1);
                    }
                    break;
                case AchievementConditionType.DepartureCount:
                    // 出撃回数
                    achievementInfo.SetCondition(PartyInfo.DepartureCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.BattleVictory:
                    // 勝利回数
                    achievementInfo.SetCondition(PartyInfo.BattleVictoryCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.CharacterLevel:
                    // キャラLv　Param2がActorId,-1なら任意
                    var level = 0;
                    if (achievementInfo.Master.Param2 == -1)
                    {
                        level = PartyInfo.ActorInfos.Max(a => a.Level);
                    } else
                    {
                        var levelChara = PartyInfo.ActorInfos.Find(a => a.ActorId.Value == achievementInfo.Master.Param2);
                        level = levelChara != null ? levelChara.Level : 0;
                    }
                    achievementInfo.SetCondition(level,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.TacticsLvupCount:
                    // Nu消費レベルアップ回数
                    achievementInfo.SetCondition(PartyInfo.TacticsLvupCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.BattleScore:
                    // バトル評価値
                    achievementInfo.SetCondition(PartyInfo.BattleScore.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.TotalDamage:
                    // 与ダメージ
                    achievementInfo.SetCondition(PartyInfo.TotalDamage.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.DeckEditCommandCount:
                    // 編成コマンド回数
                    achievementInfo.SetCondition(PartyInfo.DeckEditCommandCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.PresentCommandCount:
                    // 献上コマンド回数
                    achievementInfo.SetCondition(PartyInfo.PresentCommandCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.ReliefCommandCount:
                    // 救済コマンド回数
                    achievementInfo.SetCondition(PartyInfo.ReliefCommandCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.StatusSkillChangeCount:
                    // 魔法編成回数
                    achievementInfo.SetCondition(PartyInfo.StatusSkillChangeCount.Value,achievementInfo.Master.Param1);
                    break;
                    
            }
        }

        public List<GetItemInfo> AchievementGetItemInfos()
        {
            var list = new List<GetItemInfo>();
            var achievements = PartyInfo.AchievementInfos.FindAll(a => !a.Presented.Value && a.Achieved.Value);
            foreach (var achievement in achievements)
            {
                list.AddRange(achievement.GetItemInfos);
                achievement.Presented.SetValue(true);
            }
            return list;
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