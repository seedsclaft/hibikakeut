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
        public DeckInfo CurrentDeckInfo => CurrentGameInfo.PartyInfo.CurrentDeckInfo;

        public int Currency => PartyInfo != null ? PartyInfo.Currency.Value : 0;
        //public float TotalScore => PartyInfo.TotalScore(CurrentStage.WorldType);

        //public int RemainTurns => CurrentStage.Master.StageSymbols.Max(a => a.Seek) - CurrentStage.Seek + 1;

        public CancellationTokenSource _cancellationTokenSource;
        public void InitSaveInfo()
        {
            GameSystem.CurrentData = new SaveInfo();
        }

        public void SaveAutoFile()
        {
            var saveFileInfo = CurrentData.AutoSave();
            if (saveFileInfo == null)
            {
                saveFileInfo = new SaveFileInfo
                {
                    SaveNo = 0
                };
            }
            saveFileInfo.StageNo = CurrentStage.StageId.Value;
            saveFileInfo.SaveTimeLong = DateTime.Now.ToFileTime();
            saveFileInfo.SaveTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            saveFileInfo.PlayTime = (int)TempInfo.PlayingTime;
            if (CurrentGameInfo.PartyInfo.ActorInfos != null && CurrentGameInfo.PartyInfo.ActorInfos.Count > 0)
            {
                saveFileInfo.ActorId = CurrentGameInfo.PartyInfo.LeaderActorId.Value;
            }
            saveFileInfo.Chapter = PartyInfo.Chapter.Value;
            saveFileInfo.Period = PartyInfo.Period.Value;
            saveFileInfo.Rank = PartyInfo.MissionRank.Value;
            CurrentData.PushSaveFile(saveFileInfo);
            SavePlayerData();
            SavePlayerStageData(true, GameSystem.SceneStackManager.Current);
            SaveSystem.SaveStageInfo(GameSystem.GameInfo, saveFileInfo.SaveNo);
        }

        public void InitSaveStageInfo()
        {
            var saveGameInfo = new SaveGameInfo();
            saveGameInfo.Initialize();
            GameSystem.GameInfo = saveGameInfo;
            PartyInfoChecker.Instance.UpdateInfo();
        }

        public void InitOptionInfo()
        {
            GameSystem.OptionData = new SaveOptionInfo();
        }

        public List<ActorInfo> StageMembers()
        {
            return PartyInfo.ActorInfos;
        }

        public List<ActorInfo> BattleMembers()
        {
            var members = StageMembers().FindAll(a => a.BattleIndex.Value >= 0);
            members.Sort((a, b) => a.BattleIndex.Value > b.BattleIndex.Value ? 1 : -1);
            return members;
        }

        public void SaveTempBattleMembers()
        {
            TempInfo.CashBattleActors(BattleMembers());
        }

        public List<SkillInfo> SortSkillInfos(List<SkillInfo> skillInfos)
        {
            var sortList1 = new List<SkillInfo>();
            var sortList2 = new List<SkillInfo>();
            var sortList3 = new List<SkillInfo>();
            skillInfos.Sort((a, b) => { return a.Master.Id > b.Master.Id ? 1 : -1; });
            foreach (var skillInfo in skillInfos)
            {
                if (skillInfo.LearningState == LearningState.Learned && skillInfo.Master.SkillType == SkillType.Active || skillInfo.IsBattleSpecialSkill())
                {
                    sortList1.Add(skillInfo);
                }
                else
                if (skillInfo.LearningState == LearningState.Learned && skillInfo.Master.SkillType == SkillType.Passive)
                {
                    sortList2.Add(skillInfo);
                }
                else
                {
                    sortList3.Add(skillInfo);
                }
            }
            skillInfos.Clear();
            skillInfos.AddRange(sortList1);
            skillInfos.AddRange(sortList2);
            sortList3.Sort((a, b) => { return a.LearningLv.Value > b.LearningLv.Value ? 1 : -1; });
            skillInfos.AddRange(sortList3);
            return skillInfos;
        }

        public List<SkillInfo> ChangeAbleSkills(ActorInfo actorInfo, int minusSp = 0)
        {
            var changeAbleSkills = actorInfo.ChangeAbleSkills();
            foreach (var learnSkillId in PartyInfo.LearningSkillIds)
            {
                if (actorInfo.EquipmentSkillIds.Find(a => a.Value == learnSkillId) != null)
                {
                    continue;
                }
                if (changeAbleSkills.Find(a => a.Id.Value == learnSkillId && a.LearningState == LearningState.Learned) != null)
                {
                    continue;
                }
                var skillInfo = new SkillInfo(learnSkillId);
                skillInfo.SetLearningState(LearningState.Learned);
                skillInfo.SetEnable(true);
                if (actorInfo.IsLearnedSkill(learnSkillId))
                {
                    skillInfo.PrimitiveLearned.SetValue(true);
                }
                skillInfo.ExpRate.SetValue(actorInfo.MastarySkillRate(learnSkillId));
                changeAbleSkills.Add(skillInfo);
            }
            foreach (var changeAbleSkill in SortSkillInfos(changeAbleSkills))
            {
                if (changeAbleSkill.Master != null && !changeAbleSkill.IsBattleSpecialSkill())
                {
                    var cost = actorInfo.EquipSkillCost(changeAbleSkill.Master.Id, PartyInfo.ActorInfos, null);
                    changeAbleSkill.LearningCost.SetValue(cost);
                    if (changeAbleSkill.Enable)
                    {
                        changeAbleSkill.SetEnable((cost - minusSp) <= actorInfo.CurrentCost.Value);
                    }
                    changeAbleSkill.ExpRate.SetValue(actorInfo.MastarySkillRate(changeAbleSkill.Master.Id));
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
                if (actorInfo.IsLearnedSkill(equipSkillId.Value))
                {
                    skillInfo.PrimitiveLearned.SetValue(true);
                }
                skillInfo.ExpRate.SetValue(actorInfo.MastarySkillRate(equipSkillId.Value));
                if (!skillInfo.IsBattleSpecialSkill())
                {
                    var cost = actorInfo.EquipSkillCost(skillInfo.Master.Id, PartyInfo.ActorInfos, null);
                    skillInfo.LearningCost.SetValue(cost);
                }
                equipSkills.Add(skillInfo);
            }
            if (equipSkills.Count < actorInfo.EquipSlotCount())
            {
                var count = actorInfo.EquipSlotCount() - equipSkills.Count;
                for (int i = 0; i < count; i++)
                {
                    var skillInfo = new SkillInfo(0);
                    skillInfo.SetEnable(true);
                    equipSkills.Add(skillInfo);
                }
            }
            return equipSkills;
        }

        public SoundData DungeonBgmData()
        {
            if (CurrentStage != null && PartyInfo != null)
            {
                int bgmId;
                bgmId = CurrentStage.Master.BGMId;
                return DataSystem.BGM.Find(a => a.Id == bgmId);
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
            return StageEventDates.FindAll(a => a.Timing == eventTiming && !eventKeys.Contains(a.EventKey));
        }

        public List<StageEventData> StageEvents(EventTiming eventTiming, int positionX, int positionY)
        {
            var eventKeys = CurrentGameInfo.ReadEventKeys;
            return StageEventDates.FindAll(a => a.Timing == eventTiming && a.PositionX == positionX && a.PositionY == positionY && !eventKeys.Contains(a.EventKey));
        }

        public List<StageEventData> EndStageEvents()
        {
            var eventKeys = CurrentGameInfo.ReadEventKeys;
            return StageEventDates.FindAll(a => eventKeys.Contains(a.EventKey));
        }

        public List<StageEventData> NotEndStageEvents()
        {
            var eventKeys = CurrentGameInfo.ReadEventKeys;
            return StageEventDates.FindAll(a => !eventKeys.Contains(a.EventKey));
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

        public void AddEventReadFlagForce(StageEventData stageEventDates)
        {
            CurrentGameInfo.AddEventReadFlag(stageEventDates.EventKey);
        }

        public async UniTask<List<AudioClip>> GetBgmData(string bgmKey)
        {
            return await ResourceSystem.LoadBGMAsset(bgmKey);
        }

        public List<SystemData.CommandData> BaseConfirmCommand(int yesTextId, int noTextId = 0)
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
            return DataSystem.GetReplaceText(textId, actorName);
        }

        public List<ActorInfo> CurrentDeckActorInfos()
        {
            var actorInfos = new List<ActorInfo>();
            if (CurrentDeckInfo != null)
            {
                actorInfos = PartyInfo.CurrentDeckActorInfos();
            }
            return actorInfos;
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
            stageMembers.Sort((a, b) => a.Level - b.Level > 0 ? -1 : 1);
            return stageMembers;
        }

        public string GetAdvFile(int id)
        {
            var adventureFile = DataSystem.Adventures.Find(a => a.Id == id);
            if (adventureFile == null)
            {
                return "";
            }
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
                    PartyInfo.Currency.GainValue(getItemInfo.Param1, 0);
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

        public bool IsActiveDungeon()
        {
            if (CurrentStage != null)
            {
                // stageLvが0はダンジョン以外の扱い
                return CurrentStage.Master.StageLv > 0;
            }
            return false;
        }

        public async UniTask LoadBattleResources(List<BattlerInfo> battlers)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var filePaths = BattleUtility.AnimationResourcePaths(battlers);
            int count = filePaths.Count;
            foreach (var filePath in filePaths)
            {
                await Resources.LoadAsync<Sprite>(filePath);
                count -= 1;
            }
            try
            {
                await UniTask.WaitUntil(() => count == 0, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException e)
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

        public void SavePlayerStageData(bool isResumeStage, Scene resumeScene)
        {
            SaveDungeonPlayerData();
            TempInfo.ClearRankingInfo();
            SetResumeStage(isResumeStage);
            PartyInfo.ResumeScene = resumeScene;
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
            return new List<int>() { 3053, 3051 };
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
                stageKey.Append(string.Format(PartyInfo.CurrentDeckInfo.StageNo.Value.ToString("00")));
            }
            return stageKey.ToString();
        }

        public void ActorLevelUp(ActorInfo actorInfo)
        {
            //var cost = ActorLevelUpCost(actorInfo);
            //PartyInfo.Currency.GainValue(-cost);
            // 新規魔法取得があるか
            var skills = actorInfo.LearningSkills(1);
            var levelUpInfo = actorInfo.LevelUp(0, PartyInfo.CurrentDeckInfo.StageNo.Value);
            foreach (var skill in skills)
            {
                actorInfo.AddSkillTriggerSkill(skill.Id.Value);
            }
        }

        public int ActorLevelUpCost(ActorInfo actorInfo)
        {
            return TacticsUtility.TrainCost(actorInfo);
        }

        public int ActorLevelUpAfterExp(ActorInfo actorInfo)
        {
            return ActorGetExpCurrency(actorInfo) + actorInfo.BeforeExp;
        }

        public int ActorGetExpCurrency(ActorInfo actorInfo)
        {
            return TacticsUtility.GetExpCurrency(actorInfo, 1);
        }

        public bool EnableActorLevelUp(ActorInfo actorInfo)
        {
            return Currency >= ActorLevelUpCost(actorInfo);
        }

        public bool ActorLevelLinked(ActorInfo actorInfo)
        {
            return false;
        }

        public void ActorLearnMagic(ActorInfo actorInfo, int skillId)
        {
            var skillInfo = new SkillInfo(skillId);
            actorInfo.AddSkillTriggerSkill(skillId);
        }

        public void AddPlayerInfoActorSkillId(int actorId)
        {
            foreach (var skillInfo in StageMembers().Find(a => a.ActorId.Value == actorId).ChangeAbleSkills())
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

        public void SaveDungeonPlayerData()
        {
            if (CurrentDeckInfo == null)
            {
                return;
            }
            if (GameSystem.SceneStackManager.Current != Scene.Dungeon)
            {
                return;
            }
            var playerDungeonId = Ariadne.PlayerPosition.Instance.currentDungeonId;
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            var playerDirection = Ariadne.PlayerPosition.Instance.direction;
            CurrentDeckInfo.SetPosition(playerDungeonId, playerPosition.x, playerPosition.y, (int)playerDirection);
            AddDungeonTraverse();
        }

        public void AddDungeonTraverse()
        {
            var playerDungeonId = Ariadne.PlayerPosition.Instance.currentDungeonId;
            // 開示マス情報を更新
            var traverses = Ariadne.TraverseManager.Instance.GetDungeonTraverseData(playerDungeonId);
            if (traverses != null)
            {
                PartyInfo.AddDungeonTraverse(playerDungeonId, traverses.traverseDict);
            }
        }

        private void UpdateAchievementConditions(bool checkMissionRank = false)
        {
            PartyInfo.UpdateAchievementConditions(checkMissionRank);
        }

        private List<GetItemInfo> AchievementGetItemInfos()
        {
            return PartyInfo.AchievementGetItemInfos();
        }

        public List<GetItemInfo> CheckAchievements(bool checkMissionRank = false)
        {
            // 達成状況更新
            UpdateAchievementConditions(checkMissionRank);

            // 達成報酬あれば遷移　なければリスト表示
            var getItemInfos = AchievementGetItemInfos();
            foreach (var getItemInfo in getItemInfos)
            {
                AddGetItemInfo(getItemInfo);
            }
            return getItemInfos;
        }

        public void ReturnDungeon()
        {
            // 全回復
            foreach (var actorInfo in PartyInfo.CurrentDeckActorInfos())
            {
                actorInfo.ChangeHp(actorInfo.MaxHp);
            }
            PartyNextPeriod();
            CurrentDeckInfo.GetDungeonBgmTimeStamp().SetValue(0);
            SaveDungeonPlayerData();
        }

        public void PartyNextPeriod(bool force = false)
        {
            if (!IsActiveDungeon() && !force)
            {
                return;
            }
            SaveAutoFile();
            // NotSeekPeriod効果判定
            var notSeekPeriod = PartyInfo.AritifactSkills().Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.NotSeekPeriod) != null);
            if (notSeekPeriod == null)
            {
                PartyInfo.Period.GainValue(1);
            } else
            {
                var artifact = PartyInfo.GetOwnItemInfos(ItemType.Artifact).Find(a => a.Master.Param1 == notSeekPeriod.Id.Value);
                PartyInfo.ConsuneItemNum(artifact.Master.Id, 1);
            }
            PartyInfo.ClearTradeItemInfos();
            if (PartyInfo.Chapter.Value >= 2)
            {
                PartyInfo.EvaluationValue.GainValue(PartyInfo.EvaluationAddictValue(), 0);
            }
            PartyInfo.ClearSkillUseCount();
        }

        public SkillInfo CheckNotSeekPeriod()
        {
            var notSeekPeriod = PartyInfo.AritifactSkills().Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.NotSeekPeriod) != null);
            return notSeekPeriod;
        }

        public List<GetItemInfo> PeriodGetItemInfos()
        {
            // ピリオド超える度にアイテム入手
            var getItemInfos = new List<GetItemInfo>();
            getItemInfos.AddRange(CheckItemGetSkill());
            return getItemInfos;
        }

        private List<GetItemInfo> CheckItemGetSkill()
        {
            var getItemInfos = new List<GetItemInfo>();
            foreach (var item in PartyInfo.Items)
            {
                var itemData = DataSystem.Items.Find(a => a.Id == item.Key);
                if (itemData != null && itemData.ItemType == ItemType.Artifact && item.Value.Value > 0)
                {
                    var skillData = DataSystem.FindSkill(itemData.Param1);
                    if (skillData.TriggerDates.Find(a => a.TriggerType == TriggerType.NextPeriod) != null)
                    {
                        foreach (var featureData in skillData.FeatureDates)
                        {
                            if (featureData.FeatureType == FeatureType.GetItem)
                            {
                                // Rank = Param1;
                                // ランダムでRankを入手
                                var categoryItems = DataSystem.Items.FindAll(a => (int)a.ItemType == featureData.Param1);
                                if (featureData.Param2 != -1)
                                {
                                    categoryItems = categoryItems.FindAll(a => a.Id == featureData.Param2);
                                }
                                var rand = UnityEngine.Random.Range(0, categoryItems.Count);
                                // 報酬設定
                                var getItemInfo = MakeGetItemInfo(GetItemType.Item, categoryItems[rand].Id, 1);
                                getItemInfos.Add(getItemInfo);
                            }
                        }
                    }
                }
            }
            foreach (var getItemInfo in getItemInfos)
            {
                //AddGetItemInfo(getItemInfo);
            }
            return getItemInfos;
        }

        public GetItemInfo MakeItemGetItemInfo(ItemData itemData)
        {
            switch (itemData.ItemType)
            {
                case ItemType.RandumAddSkill:
                    // ランダムでparam2属性のparam1Rankを入手
                    var candidateSkills = DataSystem.Skills.Where(a => SkillData.ConvertRankCost(a.Value.Rank) == itemData.Param1 && a.Value.Rank != RankType.PassiveEnhanceRank1 && a.Value.IsRandumAddSkill() && !PartyInfo.LearningSkillIds.Contains(a.Key)).ToList();
                    if (itemData.Param2 != -1)
                    {
                        candidateSkills = candidateSkills.Where(a => (int)a.Value.Attribute == itemData.Param2).ToList();
                    }
                    if (candidateSkills.Count == 0)
                    {
                        return MakeGetItemInfo(GetItemType.Currency, 4);
                    }
                    var rand = UnityEngine.Random.Range(0, candidateSkills.Count);
                    // 報酬設定
                    return MakeGetItemInfo(GetItemType.Skill, candidateSkills[rand].Value.Id);
                case ItemType.RandumAddItem:
                    // ランダムでparam1が同じアイテム
                    var candidateItems = DataSystem.Items.Where(a => a.ItemType == ItemType.UseItem && (int)a.Param1 == itemData.Param1).ToList();
                    var rand2 = UnityEngine.Random.Range(0, candidateItems.Count);
                    // 報酬設定
                    return MakeGetItemInfo(GetItemType.Item, candidateItems[rand2].Id, 1);
                case ItemType.Artifact:
                    return MakeGetItemInfo(GetItemType.Evaluate, 5);
                case ItemType.Currency:
                    return MakeGetItemInfo(GetItemType.Currency, itemData.Param1);
            }
            return null;
        }

        public string DungeonPrefabName()
        {
            if (CurrentStage != null)
            {
                return CurrentStage.Master.Id.ToString("D4");
            }
            return "";
        }

        public string DungeonSkyboxName()
        {
            if (CurrentStage != null)
            {
                return CurrentStage.Master.SkyboxName;
            }
            return "";
        }
    }
}