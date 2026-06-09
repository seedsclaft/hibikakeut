using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class PartyInfo
    {
        public PartyInfo()
        {
            InitDeckInfos();
            //EvaluationValue.SetValue(100);
            Chapter.SetValue(1);
            Period.SetValue(1);
        }

        // レジュームシーン
        public Scene ResumeScene = Scene.None;
        // ダンジョン途中座標データ
        private List<DungeonResumeInfo> _dungeonResumeInfos = new();
        public List<DungeonResumeInfo> DungeonResumeInfos => _dungeonResumeInfos;
        public void UpdateDungeonResumeInfo(int stageNo, int dungeonId, int x, int y, int direction)
        {
            var find = _dungeonResumeInfos.Find(a => a.StageNo.Value == stageNo);
            if (find != null)
            {
                find.DungeonId.SetValue(dungeonId);
                find.PositionX.SetValue(x);
                find.PositionY.SetValue(y);
                find.Direction.SetValue(direction);
            }
            else
            {
                var resumeInfo = new DungeonResumeInfo();
                resumeInfo.StageNo.SetValue(stageNo);
                resumeInfo.DungeonId.SetValue(dungeonId);
                resumeInfo.PositionX.SetValue(x);
                resumeInfo.PositionY.SetValue(y);
                resumeInfo.Direction.SetValue(direction);
                _dungeonResumeInfos.Add(resumeInfo);
            }
        }

        // 所持アクターリスト
        [SerializeField] private List<ActorInfo> _actorInfos = new();
        public List<ActorInfo> ActorInfos => _actorInfos;
        // リーダーキャラId
        public ParameterInt LeaderActorId = new();
        // エンフェリアの加入順
        private List<int> _releifActorIndexes = new();
        public List<int> ReleifActorIndexes => _releifActorIndexes;
        public void AddReleifActorIndexes(int index)
        {
            if (index > 100)
            {
                return;
            }
            if (!_releifActorIndexes.Contains(index))
            {
                _releifActorIndexes.Add(index);
            }
        }
        public void RemoveReleifActorIndexes(int index)
        {
            if (_releifActorIndexes.Contains(index))
            {
                _releifActorIndexes.Remove(index);
            }
        }

        public List<ActorInfo> EditableActorInfos()
        {
            return _actorInfos.FindAll(a => !a.Transfer.Value);
        }

        public ActorInfo GetReleifActorInfo(int index)
        {
            if (_releifActorIndexes.Count < index)
            {
                return null;
            }
            var actorIndex = _releifActorIndexes[index - 1];
            return _actorInfos.Find(a => a.ActorId.Value == actorIndex);
        }

        public void AddTransferActorInfos(ActorInfo actorInfo)
        {
            if (_actorInfos.Find(a => a.ActorId.Value == actorInfo.ActorId.Value) != null)
            {
                CurrentDeckInfo.TransferActorInfo(actorInfo.ActorId.Value);
            }
        }

        public void EndTransfer()
        {
            _actorInfos.ForEach(a => a.Transfer.SetValue(false));
        }

        public List<ParameterInt> ClearedStages = new();
        public void ClearStage(int stageId)
        {
            if (IsClaeredStage(stageId))
            {
                return;
            }
            ClearedStages.Add(new ParameterInt(stageId));
        }

        public bool IsClaeredStage(int stageId)
        {
            return ClearedStages.Find(a => a.Value == stageId) != null;
        }

        private List<int> _alartedStages = new();
        public void AlartStage(int stageNo)
        {
            if (IsAlartedStage(stageNo))
            {
                return;
            }
            _alartedStages.Add(stageNo);
        }

        public bool IsAlartedStage(int stageNo)
        {
            return _alartedStages.Contains(stageNo);
        }

        // 開示マス情報
        private Dictionary<int, List<string>> _traverseDict = new();
        public void SetupDungeonTraverse(int dungeonId)
        {
            if (_traverseDict.ContainsKey(dungeonId))
            {
                return;
            }
            _traverseDict[dungeonId] = new();
        }

        public void AddDungeonTraverse(int dungeonId, Dictionary<string, bool> traverses)
        {
            SetupDungeonTraverse(dungeonId);
            foreach (var traverse in traverses)
            {
                if (!traverse.Value)
                {
                    continue;
                }
                if (_traverseDict[dungeonId].Find(a => a == traverse.Key) == null)
                {
                    _traverseDict[dungeonId].Add(traverse.Key);
                }
            }
        }

        public List<string> GetDungeonTraverse(int stageId)
        {
            return _traverseDict.ContainsKey(stageId) ? _traverseDict[stageId] : null;
        }

        public float DungeonCompletionRate()
        {
            var stageId = CurrentDeckInfo.StageNo.Value;
            if (stageId > 0 && _traverseDict.ContainsKey(stageId))
            {
                var dungeon = DataSystem.FindDungeonFloor(stageId);
                float all = dungeon.DungeonCompletion;
                float completions = _traverseDict[stageId].Count;
                if (all == 0)
                {
                    return 100;
                }
                var rate = 100 * completions / all;
                return Mathf.Min(rate, 100);
            }
            return 0;
        }

        // 所持金
        public ParameterInt Currency = new();

        // 回復可能加算数
        public ParameterInt RecoveryCount = new();
        public int HpHealValue()
        {
            return DataSystem.System.HpHealValue;
        }
        public int MoveHpHealValue()
        {
            var hpHeal = 0;
            foreach (var item in _items)
            {
                var itemData = DataSystem.FindItem(item.Key);
                if (itemData != null && itemData.ItemType == ItemType.Artifact)
                {
                    var skillData = DataSystem.FindSkill(itemData.Param1);
                    var trigger = skillData.TriggerDates.Find(a => a.TriggerType == TriggerType.DungeonMoveEnd);
                    if (trigger != null && trigger.Param1 == (CurrentDeckInfo.TurnCount.Value % trigger.Param2))
                    {
                        foreach (var featureData in skillData.FeatureDates)
                        {
                            if (featureData.FeatureType == FeatureType.HpHeal)
                            {
                                hpHeal += featureData.Param1;
                            }
                        }
                    }
                }
            }
            return hpHeal;
        }

        // 所持装備
        [SerializeField] private List<int> _equipmentIds = new();
        public List<int> EquipmentIds => _equipmentIds;

        // 所持アイテム
        private Dictionary<int, ParameterInt> _items = new();
        public Dictionary<int, ParameterInt> Items => _items;
        private void GainItemNum(int itemId, int num)
        {
            if (!_items.ContainsKey(itemId))
            {
                _items[itemId] = new();
            }
            _items[itemId].GainValue(num, 0, 9999);
        }

        public void ConsuneItemNum(int itemId, int num)
        {
            if (!_items.ContainsKey(itemId))
            {
                return;
            }
            _items[itemId].GainValue(num * -1, 0, 9999);
        }

        public bool IsOwnItem()
        {
            foreach (var item in _items)
            {
                var master = DataSystem.FindItem(item.Key);
                if (master != null && item.Value.Value > 0 && master.IsPresentItem())
                {
                    return true;
                }
            }
            return false;
        }

        public int OwnItemCount(int itemId)
        {
            return _items.ContainsKey(itemId) ? _items[itemId].Value : 0;
        }

        public List<ItemInfo> GetOwnItemInfos()
        {
            var list = new List<ItemInfo>();
            foreach (var item in _items)
            {
                var itemData = DataSystem.FindItem(item.Key);
                if (item.Value.Value > 0)
                {
                    list.Add(new ItemInfo(itemData.Id, item.Value.Value));
                }
            }
            return list;
        }

        public List<ItemInfo> GetOwnUseItemInfos(List<UseItemType> itemTypes)
        {
            var useItemInfos = UseItemInfos();
            useItemInfos.AddRange(DungeonUseItemInfos());
            return useItemInfos.FindAll(a => itemTypes.Contains((UseItemType)a.Master.Param1));
        }

        public List<ItemInfo> GetOwnItemInfos(ItemType itemType)
        {
            var list = new List<ItemInfo>();
            foreach (var item in _items)
            {
                var itemData = DataSystem.FindItem(item.Key);
                if (itemData != null && itemData.ItemType == itemType && item.Value.Value > 0)
                {
                    list.Add(new ItemInfo(itemData.Id, item.Value.Value));
                }
            }
            return list;
        }

        // 使用できるアイテムを取得
        public List<ItemInfo> UseItemInfos()
        {
            return GetOwnItemInfos(ItemType.UseItem);
        }

        // ダンジョンで使用できるアイテムを取得
        public List<ItemInfo> DungeonUseItemInfos()
        {
            return GetOwnItemInfos(ItemType.DungeonItem);
        }

        // アーティファクトアイテムを取得
        public List<ItemInfo> ArtifactItemInfos()
        {
            return GetOwnItemInfos(ItemType.Artifact);
        }

        public List<SkillInfo> AritifactSkills()
        {
            var list = new List<SkillInfo>();
            foreach (var artifactItemInfo in ArtifactItemInfos())
            {
                list.Add(new SkillInfo(artifactItemInfo.Master.Param1));
            }
            return list;
        }

        // フェーズ
        public ParameterInt Chapter = new();

        // フェーズ終了までのピリオド
        public ParameterInt Period = new();

        // 召喚できる回数
        public ParameterInt ReliefItemCount = new(1);
        // 行動実績
        public PartyStatInfo PartyStatInfo = new();

        // 評価値
        // public ParameterInt EvaluationValue = new();
        // 評価値マイナスの警告を受けたか
        public ParameterBool EvaluationCaution = new();


        // 報告リストのRank
        public ParameterInt MissionRank = new(1);

        private List<AchievementInfo> _achievements = new();
        public List<AchievementInfo> AchievementInfos => _achievements;

        // 次達成する目標
        public AchievementInfo NearAchievementInfo()
        {
            if (_achievements.Count == 0)
            {
                return null;
            }
            var list = new List<AchievementInfo>();
            foreach (var achievementInfo in _achievements)
            {
                if (achievementInfo.Master.Rank != MissionRank.Value)
                {
                    continue;
                }
                if (!achievementInfo.Achieved.Value)
                {
                    list.Add(achievementInfo);
                }
            }
            if (list.Count == 0)
            {
                return null;
            }
            //list.Sort((a, b) => a.Master.Rank - b.Master.Rank > 0 ? -1 : 1);
            list.Sort((a, b) => a.SortKey() - b.SortKey() > 0 ? 1 : -1);
            return list[0];
        }

        public void SetAchievementRank(List<AchievementData> achievementDatas)
        {
            // 全データ作成する
            foreach (var achievementData in achievementDatas)
            {
                var find = _achievements.Find(a => a.Id.Value == achievementData.Id);
                if (find != null)
                {
                }
                else
                {
                    var achievementInfo = new AchievementInfo(achievementData);
                    _achievements.Add(achievementInfo);
                }
            }
        }

        public void UpdateAchievementConditions(bool checkMissionRank = false)
        {
            foreach (var achievementInfo in _achievements)
            {
                if (achievementInfo.Master == null)
                {
                    continue;
                }
                if (achievementInfo.Master.Rank != MissionRank.Value)
                {
                    continue;
                }
                if (achievementInfo.Achieved.Value)
                {
                    continue;
                }
                if (!checkMissionRank && achievementInfo.Master.ConditionType == AchievementConditionType.Complete)
                {
                    continue;
                }
                CheckAchievementCondition(achievementInfo);
            }
        }

        public bool IsRankUpBefore()
        {
            var find = _achievements.Find(a => a.Master.ConditionType == AchievementConditionType.Complete && a.Master.Rank == MissionRank.Value);
            if (find != null)
            {
                var mains = _achievements.FindAll(a => a.Master != null && a.Master.Category == AchievementCategory.Main && a.Master.Rank == MissionRank.Value);
                var achived = mains.FindAll(a => a.Achieved.Value).Count;
                return mains.Count - 1 == achived;
            }
            return false;
        }

        private void CheckAchievementCondition(AchievementInfo achievementInfo)
        {
            switch (achievementInfo.Master.ConditionType)
            {
                case AchievementConditionType.Complete:
                    // 達成数
                    var mains = _achievements.FindAll(a => a.Master.Category == AchievementCategory.Main && a.Master.Rank == MissionRank.Value);
                    var achived = mains.FindAll(a => a.Achieved.Value).Count;
                    achievementInfo.SetCondition(achived, mains.Count - 1);
                    break;
                case AchievementConditionType.DepartureCount:
                    // 出撃回数
                    achievementInfo.SetCondition(PartyStatInfo.DepartureCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.DepartureBattleFieldCount:
                    // 出撃回数
                    achievementInfo.SetCondition(PartyStatInfo.DepartureBattleFieldCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.BattleVictory:
                    // 勝利回数
                    achievementInfo.SetCondition(PartyStatInfo.BattleVictoryCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.CharacterLevel:
                    // キャラLv　Param2がActorId,-1なら任意
                    var level = 0;
                    if (achievementInfo.Master.Param2 == -1)
                    {
                        level = ActorInfos.Max(a => a.Level);
                    }
                    else
                    {
                        var levelChara = ActorInfos.Find(a => a.ActorId.Value == achievementInfo.Master.Param2);
                        level = levelChara != null ? levelChara.Level : 0;
                    }
                    achievementInfo.SetCondition(level, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.CharacterLevelNum:
                    var findAll = ActorInfos.FindAll(a => a.Level >= achievementInfo.Master.Param1);
                    achievementInfo.SetCondition(findAll.Count, achievementInfo.Master.Param2);
                    break;
                case AchievementConditionType.TacticsLvupCount:
                    // Nu消費レベルアップ回数
                    achievementInfo.SetCondition(PartyStatInfo.TacticsLvupCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.BattleScore:
                    // バトル評価値
                    achievementInfo.SetCondition(PartyStatInfo.GainBattleScoreTotal.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.TotalDamage:
                    // 与ダメージ
                    achievementInfo.SetCondition(PartyStatInfo.TotalDamage.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.ClassChangeCount:
                    // クラスチェンジ数
                    var classChange = _actorInfos.FindAll(a => a.IsClassChenged.Value).Count;
                    achievementInfo.SetCondition(classChange, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.LearnSkillCount:
                    // 封印魔法解放数
                    var learnSkillCount = 0;
                    foreach (var actorInfo in _actorInfos)
                    {
                        learnSkillCount += actorInfo.LearnSkillIds.Count;
                    }
                    achievementInfo.SetCondition(learnSkillCount, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.ClearStage:
                    // ステージクリア
                    var cleared = ClearedStages.Find(a => a.Value == achievementInfo.Master.Param1) != null;
                    achievementInfo.SetCondition(cleared ? 1 : 0, 1);
                    break;
                case AchievementConditionType.UseAwakeSkillCount:
                    // 覚醒スキル使用回数
                    achievementInfo.SetCondition(PartyStatInfo.UseAwakeSkillCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.UseChangeLineCount:
                    // 交代スキル使用回数
                    achievementInfo.SetCondition(PartyStatInfo.UseChangeLineCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.DeckEditCommandCount:
                    // 編成コマンド回数
                    achievementInfo.SetCondition(PartyStatInfo.DeckEditCommandCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.PresentCommandCount:
                    // 献上コマンド回数
                    achievementInfo.SetCondition(PartyStatInfo.PresentCommandCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.ReliefCommandCount:
                    // 救済コマンド回数
                    achievementInfo.SetCondition(PartyStatInfo.ReliefCommandCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.TransferCommandCount:
                    // 転送コマンド回数
                    achievementInfo.SetCondition(PartyStatInfo.TransferCommandCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.TradeCommandCount:
                    // 取引コマンド回数
                    achievementInfo.SetCondition(PartyStatInfo.TradeCommandCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.StatusSkillChangeCount:
                    // 魔法編成回数
                    achievementInfo.SetCondition(PartyStatInfo.StatusSkillChangeCount.Value, achievementInfo.Master.Param1);
                    break;
            }
        }

        public void RemoveAchievedAchirvements()
        {
            _achievements = _achievements.FindAll(a => !a.Achieved.Value);
        }

        public List<GetItemInfo> AchievementGetItemInfos()
        {
            var list = new List<GetItemInfo>();
            var achievements = AchievementInfos.FindAll(a => !a.Presented.Value && a.Achieved.Value);
            foreach (var achievement in achievements)
            {
                foreach (var prizeSetsMaster in achievement.PrizeSetsMaster)
                {
                    list.Add(new GetItemInfo(prizeSetsMaster.GetItem));
                }
                achievement.Presented.SetValue(true);
            }
            return list;
        }

        // 所持アイテム情報
        private List<GetItemInfo> _getItemInfos = new();
        public void AddGetItemInfo(GetItemInfo getItemInfo)
        {
            if (getItemInfo == null)
            {
                return;
            }
            _getItemInfos.Add(getItemInfo);
            switch (getItemInfo.GetItemType)
            {
                case GetItemType.Item:
                    GainItemNum(getItemInfo.Param1, getItemInfo.Param2);
                    break;
                case GetItemType.Equipment:
                    if (!_equipmentIds.Contains(getItemInfo.Param1))
                    {
                        _equipmentIds.Add(getItemInfo.Param1);
                    }
                    break;
                case GetItemType.RankUp:
                    MissionRank.GainValue(1);
                    break;
                case GetItemType.ClearStage:
                    ClearStage(getItemInfo.Param1);
                    break;
                case GetItemType.Evaluate:
                    PartyStatInfo.BattleScore.GainValue(getItemInfo.Param1);
                    break;
                case GetItemType.SkillMastary:
                    var target = _actorInfos.Find(a => a.ActorId.Value == getItemInfo.Param1);
                    target.GainSkillMastary(getItemInfo.Param2);
                    // 強化スキルならステータス加算
                    var skillData = DataSystem.FindSkill(getItemInfo.Param2);
                    if (skillData != null)
                    {
                        foreach (var featureData in skillData.FeatureDates)
                        {
                            if (featureData.FeatureType != FeatureType.EquipmentStatusUp)
                            {
                                continue;
                            }
                            target.AddStatusUpper((StatusParamType)featureData.Param1, featureData.Param3);
                        }
                    }
                    break;
                case GetItemType.AddReliefCommandCount:
                    ReliefItemCount.GainValue(1, 0);
                    break;
                case GetItemType.AddRecoveryCount:
                    RecoveryCount.GainValue(1, 0);
                    break;
                default:
                    CheckAddActor();
                    CheckLearningSkillId();
                    break;
            }
        }

        public void RemoveGetItemInfo(GetItemInfo getItemInfo)
        {
            var remove = _getItemInfos.Find(a => a.GetItemType == getItemInfo.GetItemType && a.Param1 == getItemInfo.Param1);
            if (remove != null)
            {
                _getItemInfos.Remove(remove);
                if (remove.GetItemType == GetItemType.AddActor)
                {
                    var removeActor = _actorInfos.Find(a => a.ActorId.Value == remove.Param1);
                    if (removeActor != null)
                    {
                        _actorInfos.Remove(removeActor);
                        RemoveReleifActorIndexes(removeActor.ActorId.Value);
                        var removeIndex = -1;
                        foreach (var actorDict in CurrentDeckInfo.ActorIdDict)
                        {
                            if (actorDict.Value == remove.Param1)
                            {
                                removeIndex = actorDict.Key;
                            }
                        }
                        if (removeIndex > -1)
                        {
                            CurrentDeckInfo.ActorIdDict[removeIndex] = -1;
                        }
                    }
                }
            }
        }

        private List<int> _learningSkillIds = new();
        public List<int> LearningSkillIds => _learningSkillIds;
        private void CheckLearningSkillId()
        {
            var addSkillInfos = _getItemInfos.FindAll(a => a.GetFlag && a.GetItemType == GetItemType.Skill);
            foreach (var addSkillInfo in addSkillInfos)
            {
                AddLearningSkill(addSkillInfo.Param1);
            }
        }

        public void AddLearningSkill(int skillId)
        {
            if (!_learningSkillIds.Contains(skillId))
            {
                // 新規魔法入手
                _learningSkillIds.Add(skillId);
            }
        }

        private void CheckAddActor()
        {
            var addActorInfos = _getItemInfos.FindAll(a => a.GetFlag && a.GetItemType == GetItemType.AddActor);
            foreach (var addActorInfo in addActorInfos)
            {
                if (_actorInfos.Find(a => a.ActorId.Value == addActorInfo.Param1) == null)
                {
                    // 新規加入
                    var actorData = DataSystem.FindActor(addActorInfo.Param1);
                    if (_actorInfos.Find(a => a.ActorId.Value == actorData.Id) != null)
                    {
                        return;
                    }
                    var actorInfo = new ActorInfo(actorData);
                    actorInfo.BattleIndex.SetValue(_actorInfos.Count + 1);
                    actorInfo.SetLevel(actorData.InitLv);
                    if (addActorInfo.Param2 > 0)
                    {
                        actorInfo.SetLevel(addActorInfo.Param2);
                    }
                    actorInfo.ChangeHp(actorInfo.MaxHp);
                    // 最初に加入したキャラは自動編成
                    if (_actorInfos.Count == 0)
                    {
                        var first = CurrentDeckInfo.ActorIdDict.Where(a => a.Value == -1).First();
                        if (first.Value == -1)
                        {
                            CurrentDeckInfo.ActorIdDict[first.Key] = actorInfo.ActorId.Value;
                        }
                        LeaderActorId.SetValue(actorInfo.ActorId.Value);
                    }
                    _actorInfos.Add(actorInfo);
                    AddReleifActorIndexes(actorInfo.ActorId.Value);
                    // 整列
                    _actorInfos.Sort((a, b) => a.BattleIndex.Value - b.BattleIndex.Value > 0 ? 1 : -1);

                }
            }
        }

        // 取引ラインナップ
        private List<TradeItemInfo> _tradeItemInfos = new();
        public List<TradeItemInfo> TradeItemInfos => _tradeItemInfos;
        public void SetTardeItemInfos(List<TradeItemInfo> tradeItemInfos)
        {
            _tradeItemInfos = tradeItemInfos;
        }
        public void RemoveTradeItemInfos(List<TradeItemInfo> tradeItemInfos)
        {
            for (int i = _tradeItemInfos.Count - 1; i >= 0; i--)
            {
                if (tradeItemInfos.Contains(_tradeItemInfos[i]))
                {
                    _tradeItemInfos.RemoveAt(i);
                }
            }
        }
        public void ClearTradeItemInfos()
        {
            _tradeItemInfos.Clear();
        }

        public float TradeDownRate()
        {
            // 取引レートダウン
            var tradeDownRate = AritifactSkills().Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.TrafeRateDown) != null);
            if (tradeDownRate != null)
            {
                var downRate = tradeDownRate.FeatureDates.Find(a => a.FeatureType == FeatureType.TrafeRateDown);
                return 1 - (downRate.Param1 * 0.01f);
            }
            return 1;
        }

        public ParameterInt DeckId = new(1);
        private Dictionary<int, DeckInfo> _deckInfos = new();
        private void InitDeckInfos()
        {
            for (int i = 1; i <= 5; i++)
            {
                _deckInfos[i] = new DeckInfo();
            }
        }

        public DeckInfo CurrentDeckInfo => _deckInfos[DeckId.Value];

        public List<BattlerInfo> DeckEditBattlerInfos()
        {
            // 空きスロットも表示する
            var battlerInfos = new List<BattlerInfo>();
            for (int i = 1; i <= 6; i++)
            {
                var dictValue = CurrentDeckInfo.ActorIdDict[i];
                if (dictValue > -1)
                {
                    var actorInfo = _actorInfos.Find(a => a.ActorId.Value == dictValue);
                    battlerInfos.Add(new BattlerInfo(actorInfo, i));
                }
                else
                {
                    battlerInfos.Add(new BattlerInfo());
                }
            }
            return battlerInfos;
        }

        public List<ActorInfo> CurrentDeckActorInfos()
        {
            var actorInfos = new List<ActorInfo>();
            var idx = 1;
            foreach (var actorId in CurrentDeckInfo.ActorIdDict)
            {
                var find = _actorInfos.Find(a => a.ActorId.Value == actorId.Value);
                if (find != null)
                {
                    find.BattleIndex.SetValue(idx);
                    find.SetLineIndex(idx <= 3 ? LineType.Front : LineType.Back);
                    actorInfos.Add(find);
                }
                idx++;
            }
            return actorInfos;
        }

        public void UseRecoveryHeal()
        {
            foreach (var actorId in CurrentDeckInfo.ActorIdDict)
            {
                var find = _actorInfos.Find(a => a.ActorId.Value == actorId.Value);
                find?.ChangeHp(find.CurrentHp.Value + DataSystem.System.HpHealValue);
            }
        }

        public void UseItemHeal(int heal)
        {
            foreach (var actorId in CurrentDeckInfo.ActorIdDict)
            {
                var find = _actorInfos.Find(a => a.ActorId.Value == actorId.Value);
                find?.ChangeHp(find.CurrentHp.Value + heal);
            }
        }

        public void DamageFloor(int damage)
        {
            foreach (var actorId in CurrentDeckInfo.ActorIdDict)
            {
                var find = _actorInfos.Find(a => a.ActorId.Value == actorId.Value);
                find?.ChangeHp(find.CurrentHp.Value - damage);
            }
        }

        public void ClearSkillUseCount()
        {
            // Period回数制限の使用回数を初期化
            foreach (var actorInfo in _actorInfos)
            {
                actorInfo.ClearSkillUseCount();
            }
        }

        public int PartyEvaluate()
        {
            return _actorInfos.Sum(a => a.Evaluate());
        }

        public int EvaluationAddictValue()
        {
            // アーティファクト所持数分評価値を減らす
            var artifactNum = 0;
            foreach (var item in _items)
            {
                if ((item.Key > 2000 && item.Key < 3000) && item.Value.Value > 0)
                {
                    artifactNum++;
                }
            }
            return -4 * artifactNum;
        }
    }
}