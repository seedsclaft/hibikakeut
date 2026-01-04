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

        public List<SkillInfo> SortSkillInfos(List<SkillInfo> skillInfos)
        {
            skillInfos.Sort((a, b) => { return a.Master.Id > b.Master.Id ? 1 : -1; });
            skillInfos.Sort((a, b) => { return a.SortKeyId() > b.SortKeyId() ? 1 : -1; });
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
                skillInfo.PeriodUseCount.SetValue(actorInfo.GetSkillUseCount(equipSkillId.Value));
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

        public async UniTask<List<AudioClip>> GetBgmData(string bgmKey)
        {
            return await ResourceSystem.LoadBGMAsset(bgmKey);
        }

        public List<SystemData.CommandData> ConfirmCommand()
        {
            return BaseConfirmCommand(3050, 3051);
        }

        public List<SystemData.CommandData> NoChoiceConfirmCommand()
        {
            return new List<SystemData.CommandData>() { BaseConfirmCommand(3052, 0)[0] };
        }

        public List<SkillInfo> SkillActionList(ActorInfo actorInfo)
        {
            return new List<SkillInfo>();
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

        public GetItemInfo MakeGetItemInfo(GetItemType getItemType, int param1, int param2 = 0)
        {
            var getItemData = new GetItemData
            {
                Type = getItemType,
                Param1 = param1,
                Param2 = param2,
            };
            return new GetItemInfo(getItemData);
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

        public void RemoveGetItemInfo(GetItemInfo getItemInfo)
        {
            PartyInfo.RemoveGetItemInfo(getItemInfo);
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
            actorInfo.AddSkillTriggerSkill(skillId);
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
                                    categoryItems = categoryItems.FindAll(a => a.Param1 == featureData.Param2);
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
    }
}