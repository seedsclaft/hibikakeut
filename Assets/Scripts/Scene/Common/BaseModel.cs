using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Ryneus
{
    public partial class BaseModel
    {
        public SaveInfo CurrentData => GameSystem.CurrentData;
        public SaveGameInfo CurrentGameInfo => GameSystem.GameInfo;
        public TempInfo TempInfo => GameSystem.TempData;
        public StageInfo CurrentStage => CurrentGameInfo.StageInfo;

        public PartyInfo PartyInfo => CurrentGameInfo.PartyInfo;

        public int Currency => 0;
        //public float TotalScore => PartyInfo.TotalScore(CurrentStage.WorldType);

        //public int RemainTurns => CurrentStage.Master.StageSymbols.Max(a => a.Seek) - CurrentStage.Seek + 1;

        public CancellationTokenSource _cancellationTokenSource;
        public void InitSaveInfo()
        {
            GameSystem.CurrentData = new SaveInfo();
        }

        public void InitSaveStageInfo()
        {
            var saveGameInfo = new SaveGameInfo();
            saveGameInfo.Initialize();
            GameSystem.GameInfo = saveGameInfo;
        }

        public void InitConfigInfo()
        {
            GameSystem.ConfigData = new SaveConfigInfo();
        }

        public List<ActorInfo> Actors()
        {
            return PartyInfo.GetActorInfos();
        }

        public void LostActors(List<ActorInfo> lostMembers)
        {
        }

        public List<ActorInfo> StageMembers()
        {
            return PartyInfo.GetActorInfos();
        }

        public List<ActorInfo> BattleMembers()
        {
            var members = StageMembers().FindAll(a => a.BattleIndex.Value >= 0);
            members.Sort((a,b) => a.BattleIndex.Value > b.BattleIndex.Value ? 1 : -1);
            return members;
        }

        public List<ActorInfo> EditMembers()
        {
            var members = StageMembers().FindAll(a => a.BattleIndex.Value >= 0);
            // 最大6人で空いた枠に空データを入れる
            for (int i = 1;i <= 6;i++)
            {
                if (members.Find(a => a.BattleIndex.Value == i) == null)
                {
                    var temp = new ActorInfo(null);
                    temp.BattleIndex.SetValue(i);
                    members.Add(temp);
                }
            }
            members.Sort((a,b) => a.BattleIndex.Value > b.BattleIndex.Value ? 1 : -1);
            return members;
        }
        
        public void SaveTempBattleMembers()
        {
            TempInfo.CashBattleActors(BattleMembers());
        }
        
        public List<ActorInfo> PartyMembers()
        {
            return PartyInfo.GetActorInfos();
        }

        public List<SkillInfo> SortSkillInfos(List<SkillInfo> skillInfos)
        {
            var sortList1 = new List<SkillInfo>();
            var sortList2 = new List<SkillInfo>();
            var sortList3 = new List<SkillInfo>();
            skillInfos.Sort((a,b) => {return a.Master.Id > b.Master.Id ? 1 : -1;});
            foreach (var skillInfo in skillInfos)
            {
                if (skillInfo.LearningState == LearningState.Learned && skillInfo.Master.SkillType == SkillType.Active || skillInfo.IsBattleSpecialSkill())
                {
                    sortList1.Add(skillInfo);
                } else
                if (skillInfo.LearningState == LearningState.Learned && skillInfo.Master.SkillType == SkillType.Passive)
                {
                    sortList2.Add(skillInfo);
                } else
                {
                    sortList3.Add(skillInfo);
                }
            }
            skillInfos.Clear();
            skillInfos.AddRange(sortList1);
            skillInfos.AddRange(sortList2);
            sortList3.Sort((a,b) => {return a.LearningLv.Value > b.LearningLv.Value ? 1 : -1;});
            skillInfos.AddRange(sortList3);
            return skillInfos;
        }

        public List<SkillInfo> ChangeAbleSkills(ActorInfo actorInfo)
        {
            var changeAbleSkills = actorInfo.ChangeAbleSkills();
            foreach (var learnSkillId in PartyInfo.LearningSkillIds)
            {
                if (actorInfo.EquipmentSkillIds.Find(a => a.Value == learnSkillId) != null)
                {
                    continue;
                }
                var skillInfo = new SkillInfo(learnSkillId);
                skillInfo.SetLearningState(LearningState.Learned);
                skillInfo.SetEnable(true);
                changeAbleSkills.Add(skillInfo);
            }
            foreach (var changeAbleSkill in SortSkillInfos(changeAbleSkills))
            {
                if (changeAbleSkill.Master != null && !changeAbleSkill.IsBattleSpecialSkill())
                {
                    var cost = TacticsUtility.LearningMagicCost(actorInfo,changeAbleSkill.Attribute,PartyInfo.ActorInfos,changeAbleSkill.Master.Rank);
                    changeAbleSkill.LearningCost.SetValue(cost);
                    if (changeAbleSkill.Enable)
                    {
                        changeAbleSkill.SetEnable(cost <= actorInfo.CurrentMp.Value);
                    }
                }
            }
            return changeAbleSkills;
        }

        public List<SkillInfo> EquipSkills(ActorInfo actorInfo)
        {
            var equipSkills = new List<SkillInfo>();
            var equipSkillIds = actorInfo.EquipmentSkillIds;
            foreach (var equipSkillId in equipSkillIds)
            {
                if (equipSkillId.Value < 1000)
                {
                    continue;
                }
                var skillInfo = new SkillInfo(equipSkillId.Value);
                skillInfo.SetLearningState(LearningState.Learned);
                skillInfo.SetEnable(true);
                equipSkills.Add(skillInfo);
            }            
            if (equipSkills.Count < 8)
            {
                var count = 8 - equipSkills.Count;
                for (int i = 0;i < count;i++)
                {
                    var skillInfo = new SkillInfo(0);
                    skillInfo.SetEnable(true);
                    equipSkills.Add(skillInfo);
                }
            }
            return equipSkills;
        }
        

        public BGMData TacticsBgmData()
        {
            if (CurrentStage != null && PartyInfo != null)
            {
                if (CurrentStage.EndSeek == PartyInfo.Seek.Value)
                {
                    return null;
                }
                int bgmId;
                if (PartyInfo.StartStage.Value == false)
                {
                    bgmId = CurrentStage.Master.MenuBGMId;
                } else
                {
                    bgmId = CurrentStage.Master.BGMId;
                }
                return DataSystem.Data.GetBGM(bgmId);
            }
            return null;
        }

        public string PlayerName()
        {
            return CurrentData.PlayerInfo?.PlayerName.Value;
        }

        public string PlayerId()
        {
            return CurrentData.PlayerInfo?.UserId.ToString();
        }

        public List<StageEventData> StageEventDates => CurrentStage.Master.StageEvents;

        public List<StageEventData> StageEvents(EventTiming eventTiming)
        {
            var eventKeys = CurrentGameInfo.ReadEventKeys;
            return StageEventDates.FindAll(a => a.Timing == eventTiming && a.Turns == PartyInfo.Seek.Value && !eventKeys.Contains(a.EventKey));
        }
        

        public void AddEventsReadFlag(List<StageEventData> stageEventDates)
        {
            foreach (var eventData in stageEventDates)
            {
                AddEventReadFlag(eventData);
            }
        }

        public void AddEventReadFlag(StageEventData stageEventDates)
        {
            if (!stageEventDates.ReadFlag)
            {
                return;
            }
            CurrentGameInfo.AddEventReadFlag(stageEventDates.EventKey);
        }

        public async UniTask<List<AudioClip>> GetBgmData(string bgmKey)
        {
            return await ResourceSystem.LoadBGMAsset(bgmKey);
        }

        public List<SystemData.CommandData> BaseConfirmCommand(int yesTextId,int noTextId = 0)
        {
            var menuCommandDates = new List<SystemData.CommandData>();
            var yesCommand = new SystemData.CommandData
            {
                Key = "Yes",
                Name = DataSystem.GetText(yesTextId),
                Id = 0
            };
            if (noTextId != 0)
            {
                var noCommand = new SystemData.CommandData
                {
                    Key = "No",
                    Name = DataSystem.GetText(noTextId),
                    Id = 1
                };
                menuCommandDates.Add(noCommand);
            }
            menuCommandDates.Add(yesCommand);
            return menuCommandDates;
        }

        public List<SkillInfo> BasicSkillInfos(GetItemInfo getItemInfo)
        {
            var skillInfos = new List<SkillInfo>();
            if (getItemInfo.IsSkill())
            {
                var skillInfo = new SkillInfo(getItemInfo.Param1);
                skillInfo.SetEnable(true);
                skillInfos.Add(skillInfo);
            }
            if (getItemInfo.IsAttributeSkill())
            {
                var skillDates = DataSystem.Skills.Where(a => a.Value.Rank == (RankType)getItemInfo.ResultParam && a.Value.Attribute == (AttributeType)((int)getItemInfo.GetItemType - 10));
                foreach (var skillData in skillDates)
                {
                    var skillInfo = new SkillInfo(skillData.Key);
                    skillInfo.SetEnable(true);
                    skillInfos.Add(skillInfo);
                }
            }
            return skillInfos;
        }

        public List<SkillInfo> BasicSkillGetItemInfos(List<GetItemInfo> getItemInfos)
        {
            var skillInfos = new List<SkillInfo>();
            foreach (var getItemInfo in getItemInfos)
            {
                if (getItemInfo.IsSkill())
                {
                    var skillInfo = new SkillInfo(getItemInfo.Param1);
                    skillInfo.SetEnable(true);
                    skillInfos.Add(skillInfo);
                }
                if (getItemInfo.IsAttributeSkill())
                {
                    var skillDates = DataSystem.Skills.Where(a => a.Value.Rank == (RankType)getItemInfo.ResultParam && a.Value.Attribute == (AttributeType)((int)getItemInfo.GetItemType - 10));
                    foreach (var skillData in skillDates)
                    {
                        var skillInfo = new SkillInfo(skillData.Key);
                        skillInfo.SetEnable(true);
                        skillInfos.Add(skillInfo);
                    }
                }
            }
            return skillInfos;
        }


        public string SelectAddActorConfirmText(string actorName)
        {
            int textId = 14180;
            return DataSystem.GetReplaceText(textId,actorName);
        }

        /// <summary>
        /// 加入歴あるキャラも含めたステータスメンバー
        /// </summary>
        public List<ActorInfo> PastActorInfos()
        {
            var stageMembers = StageMembers();
            foreach (var actorInfo in PartyInfo.ActorInfos)
            {
                if (!stageMembers.Contains(actorInfo))
                {
                    stageMembers.Add(actorInfo);
                }
            }
            stageMembers.Sort((a,b) => a.Level - b.Level > 0 ? -1 : 1);
            return stageMembers;
        }

        public string GetAdvFile(int id)
        {
            var adventureFile = DataSystem.Adventures.Find(a => a.Id == id);
            if (adventureFile.PrizeSetId > 0)
            {
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == adventureFile.PrizeSetId);
                foreach (var prizeSet in prizeSets)
                {
                    var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                    AddGetItemInfo(getItemInfo);
                }
            }
            return adventureFile.AdvName;
        }

        public void AddGetItemInfo(GetItemInfo getItemInfo)
        {
            getItemInfo.SetGetFlag(true);
            switch (getItemInfo.GetItemType)
            {
                case GetItemType.Currency:
                    PartyInfo.Currency.GainValue(getItemInfo.Param1,0);
                    break;
                default:
                    PartyInfo.AddGetItemInfo(getItemInfo);
                    break;
            }
        }

        public void ClearGame()
        {
            CurrentData.PlayerInfo.GainClearCount();
        }




        public void StartOpeningStage()
        {
            InitSaveStageInfo();
            SavePlayerStageData(true);
        }

        public async UniTask LoadBattleResources(List<BattlerInfo> battlers)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var filePaths = BattleUtility.AnimationResourcePaths(battlers);
            int count = filePaths.Count;
            foreach (var filePath in filePaths)
            {
                await Resources.LoadAsync<Sprite>( filePath );
                count -= 1;
            }
            try {
                await UniTask.WaitUntil( () => count == 0 ,PlayerLoopTiming.Update,_cancellationTokenSource.Token);
            } catch (OperationCanceledException e)
            {
                Debug.Log(e);
            }
        }

        public void SetResumeStage(bool resumeStage)
        {
            //CurrentSaveData.SetResumeStage(resumeStage);
        }
        
        public void SavePlayerData()
        {
            SaveSystem.SavePlayerInfo(GameSystem.CurrentData);
        }

        public void SavePlayerStageData(bool isResumeStage)
        {
            TempInfo.ClearRankingInfo();
            SetResumeStage(isResumeStage);
            SaveSystem.SaveStageInfo(GameSystem.GameInfo);
            SavePlayerData();
        }

    #if UNITY_ANDROID
        public List<RankingActorData> RankingActorDates()
        {
            var list = new List<RankingActorData>();
            foreach (var actorInfo in StageMembers())
            {
                var skillIds = new List<int>();
                foreach (var skill in actorInfo.Skills)
                {
                    skillIds.Add(skill.Id);
                }
                var rankingActorData = new RankingActorData()
                {
                    ActorId = actorInfo.ActorId,
                    Level = actorInfo.Level,
                    Hp = actorInfo.CurrentParameter(StatusParamType.Hp),
                    Mp = actorInfo.CurrentParameter(StatusParamType.Mp),
                    Atk = actorInfo.CurrentParameter(StatusParamType.Atk),
                    Def = actorInfo.CurrentParameter(StatusParamType.Def),
                    Spd = actorInfo.CurrentParameter(StatusParamType.Spd),
                    SkillIds = skillIds,
                    DemigodParam = actorInfo.DemigodParam,
                    Lost = actorInfo.Lost
                };
                list.Add(rankingActorData);
            }
            return list;
        }
    #endif

        public async void CurrentRankingData(Action<string> endEvent)
        {
            var userId = CurrentData.PlayerInfo.UserId.ToString();
            var rankingText = "";
#if UNITY_WEBGL || UNITY_ANDROID && !UNITY_EDITOR
            FirebaseController.Instance.CurrentRankingData(userId);
            await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);
            var currentScore = FirebaseController.CurrentScore;
            var evaluate = TotalScore;

            // 更新あり
            if (evaluate > currentScore)
            {
                var playerScore = (int)(evaluate * 100);
                FirebaseController.Instance.WriteRankingData(
                    CurrentStage.Id,
                    userId,
                    playerScore,
                    CurrentData.PlayerInfo.PlayerName,
                    StageMembers()
                );
                await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);

                FirebaseController.Instance.ReadRankingData();
                await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);
                var results = FirebaseController.RankingInfos;
                var rank = 1;
                var include = false;
                foreach (var result in results)
                {
                    if (result.Score == playerScore)
                    {
                        include = true;
                    }
                    if (result.Score > playerScore)
                    {
                        rank++;
                    }
                }

                if (include == true)
                {
                    // 〇位
                    rankingText = DataSystem.GetReplaceText(23030, rank.ToString());
                } else
                {
                    // 圏外
                    rankingText = DataSystem.GetText(23031);
                }
            } else
            {          
                // 記録更新なし  
                rankingText = DataSystem.GetText(23032);
            }
#endif
            endEvent(rankingText);
        }

        public string SavePopupTitle()
        {
            return DataSystem.GetText(19500);
        }

        public string FailedSavePopupTitle()
        {
            var baseText = DataSystem.GetText(11082);
            return baseText;
        }

        public bool NeedAdsSave()
        {
            var needAds = false;
#if UNITY_ANDROID
            needAds = (CurrentStage.SavedCount + 1) >= CurrentStage.Master.SaveLimit;
#endif
            return needAds;
        }

        public void GainSaveCount()
        {
        }

        public bool EnableContinue()
        {
            return false;
        }

        public bool EnableUserContinue()
        {
            var enable = true;
            return enable;
        }


        public bool NeedAdsContinue()
        {
            var needAds = false;
#if UNITY_ANDROID
            needAds = (CurrentStage.ContinueCount + 1) >= CurrentStage.Master.ContinueLimit;
#endif
            return needAds;
        }

        public void GainContinueCount()
        {
        }

        public List<int> SaveAdsCommandTextIds()
        {
            return new List<int>(){3053,3051};
        }

        public int PartyEvaluate()
        {
            var evaluate = 0;
            foreach (var actorInfo in BattleMembers())
            {
                evaluate += actorInfo.Evaluate();
            }
            return evaluate;
        }

        public string CurrentStageKey()
        {
            var stageKey = new System.Text.StringBuilder();
            if (PartyInfo != null)
            {
                stageKey.Append(string.Format(PartyInfo.StageId.Value.ToString("00")));
                stageKey.Append(string.Format(PartyInfo.Seek.Value.ToString("00")));
                stageKey.Append(string.Format(PartyInfo.SeekIndex.Value.ToString("00")));
            }
            return stageKey.ToString();
        }

        public void ActorLevelUp(ActorInfo actorInfo)
        {
            var cost = ActorLevelUpCost(actorInfo);
            // 新規魔法取得があるか
            var skills = actorInfo.LearningSkills(1);
            var levelUpInfo = actorInfo.LevelUp(cost,PartyInfo.StageId.Value,PartyInfo.Seek.Value,-1);
            foreach (var skill in skills)
            {
                actorInfo.AddSkillTriggerSkill(skill.Id.Value);
            }
        }

        public int ActorLevelUpCost(ActorInfo actorInfo)
        {
            return TacticsUtility.TrainCost(actorInfo);
        }
        
        public bool EnableActorLevelUp(ActorInfo actorInfo)
        {
            return Currency >= ActorLevelUpCost(actorInfo);
        }

        public bool ActorLevelLinked(ActorInfo actorInfo)
        {
            return false;
        }

        public void ActorLearnMagic(ActorInfo actorInfo,int skillId)
        {
            var skillInfo = new SkillInfo(skillId);
            var learningCost = TacticsUtility.LearningMagicCost(actorInfo,skillInfo.Attribute,StageMembers(),skillInfo.Master.Rank);
            actorInfo.AddSkillTriggerSkill(skillId);
        }

        public void AddPlayerInfoActorSkillId(int actorId)
        {
            foreach (var skillInfo in Actors().Find(a => a.ActorId.Value == actorId).ChangeAbleSkills())
            {
                AddPlayerInfoSkillId(skillInfo.Id.Value);
            }
        }

        public void AddPlayerInfoSkillId(int skillId)
        {
            CurrentData.PlayerInfo.AddSkillId(skillId);
        }

        public List<TutorialData> SceneTutorialDates(int scene)
        {
            return DataSystem.TutorialDates.FindAll(a => (int)a.SceneType == scene && !CurrentData.PlayerInfo.ReadTutorials.Contains(a.Id));
        }

        public List<TutorialData> SceneTutorialDates(PopupType popupType)
        {
            return DataSystem.TutorialDates.FindAll(a => (int)a.SceneType == ((int)popupType + 100) && !CurrentData.PlayerInfo.ReadTutorials.Contains(a.Id));
        }
        
        public List<TutorialData> SceneTutorialDates(StatusType statusType)
        {
            return DataSystem.TutorialDates.FindAll(a => (int)a.SceneType == ((int)statusType + 200) && !CurrentData.PlayerInfo.ReadTutorials.Contains(a.Id));
        }

        public void ReadTutorialData(TutorialData tutorialData)
        {
            CurrentData.PlayerInfo.AddReadTutorials(tutorialData.Id);
        }
    }
}