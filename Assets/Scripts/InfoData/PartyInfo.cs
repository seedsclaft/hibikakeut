using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    [System.Serializable]
    public class PartyInfo
    {
        public PartyInfo()
        {
            InitDeckInfos();
            EvaluationValue.SetValue(100);
            Chapter.SetValue(1);
            Period.SetValue(1);
        }

        // レジュームシーン
        public Scene ResumeScene = Scene.None;
        // ダンジョン途中座標データ
        private List<DungeonResumeInfo> _dungeonResumeInfos = new();
        public List<DungeonResumeInfo> DungeonResumeInfos => _dungeonResumeInfos;
        public void UpdateDungeonResumeInfo(int dungeonId, int x, int y, int direction)
        {
            var find = _dungeonResumeInfos.Find(a => a.DungeonId.Value == dungeonId);
            if (find != null)
            {
                find.PositionX.SetValue(x);
                find.PositionY.SetValue(y);
                find.Direction.SetValue(direction);
            } else
            {
                var resumeInfo = new DungeonResumeInfo();
                resumeInfo.DungeonId.SetValue(dungeonId);
                resumeInfo.PositionX.SetValue(x);
                resumeInfo.PositionY.SetValue(y);
                resumeInfo.Direction.SetValue(direction);
                _dungeonResumeInfos.Add(resumeInfo);
            }
        }

        // 所持アクターリスト
        [UnityEngine.SerializeField] private List<ActorInfo> _actorInfos = new();
        public List<ActorInfo> ActorInfos => _actorInfos;
        // 派遣アクターリスト
        [UnityEngine.SerializeField] private List<ActorInfo> _transferActorInfos = new();

        public List<ActorInfo> EditableActorInfos()
        {
            return _actorInfos.FindAll(a => !_transferActorInfos.Contains(a));
        }
        public void AddTransferActorInfos(ActorInfo actorInfo)
        {
            if (_transferActorInfos.Find(a => a.ActorId.Value == actorInfo.ActorId.Value) == null)
            {
                _transferActorInfos.Add(actorInfo);
                CurrentDeckInfo.TransferActorInfo(actorInfo.ActorId.Value);
            }
        }

        // 現在のステージ場所
        public StageData StageMaster => DataSystem.FindStage(StageId.Value);
        public ParameterInt StageId = new();
        public ParameterInt TurnCount = new();

        private List<int> _clearedStages = new();
        public List<int> ClearedStages => _clearedStages;
        public void ClearStage(int stageId)
        {
            if (!IsClaeredStage(stageId))
            {
                _clearedStages.Add(stageId);
            }
        }

        public bool IsClaeredStage(int stageId)
        {
            return _clearedStages.Contains(stageId);
        }

        private List<int> _alartedStages = new();
        public List<int> AlartedStages => _alartedStages;
        public void AlartStage(int stageId)
        {
            if (!IsAlartedStage(stageId))
            {
                _alartedStages.Add(stageId);
            }
        }

        public bool IsAlartedStage(int stageId)
        {
            return _alartedStages.Contains(stageId);
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

        // 所持金
        public ParameterInt Currency = new();

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
                if (item.Value.Value > 0 && DataSystem.Items.Find(a => a.Id == item.Key).ItemType == ItemType.RandumAddSkill)
                {
                    return true;
                }
            }
            return false;
        }

        // 使用できるアイテムを取得
        public List<ItemInfo> UseItemInfos()
        {
            var list = new List<ItemInfo>();
            foreach (var item in _items)
            {
                var itemData = DataSystem.Items.Find(a => a.Id == item.Key);
                if (itemData.ItemType == ItemType.UseItem && item.Value.Value > 0)
                {
                    list.Add(new ItemInfo(itemData.Id, item.Value.Value));
                }
            }
            return list;
        }

        // 所持解放
        private List<int> _buildingIds = new();
        public List<int> BuildingIds => _buildingIds;
        private void GainBuilding(int buildingId)
        {
            if (_buildingIds.Contains(buildingId))
            {
                return;
            }
            _buildingIds.Add(buildingId);
        }

        public List<SkillInfo> BuildingSkills()
        {
            var list = new List<SkillInfo>();
            foreach (var buildingId in _buildingIds)
            {
                var buildingsData = DataSystem.Buildings.Find(a => a.Id == buildingId);
                list.Add(new SkillInfo(buildingsData.SkillId));
            }
            return list;
        }

        public List<SkillInfo> AritifactSkills()
        {
            var list = new List<SkillInfo>();
            foreach (var item in _items)
            {
                var itemData = DataSystem.Items.Find(a => a.Id == item.Key);
                if (itemData.ItemType == ItemType.Artifact && item.Value.Value > 0)
                {
                    list.Add(new SkillInfo(itemData.Param1));
                }
            }
            return list;
        }

        // フェーズ
        public ParameterInt Chapter = new();

        // フェーズ終了までのピリオド
        public ParameterInt Period = new();

        // 転送できるか
        public ParameterBool EnableTransfer = new();

        // 評価値
        public ParameterInt EvaluationValue = new();

        // 出撃回数
        public ParameterInt DepartureCount = new();
        public ParameterInt DepartureBattleFieldCount = new();
        // 勝利回数
        public ParameterInt BattleVictoryCount = new();
        // Nu消費レベルアップ回数
        public ParameterInt TacticsLvupCount = new();
        // バトル評価値
        public ParameterInt BattleScore = new();
        // 与ダメージ
        public ParameterInt TotalDamage = new();

        // 編成コマンド回数
        public ParameterInt DeckEditCommandCount = new();
        // 献上コマンド回数
        public ParameterInt PresentCommandCount = new();
        // 救済コマンド回数
        public ParameterInt ReliefCommandCount = new();
        // 転送コマンド回数
        public ParameterInt TransferCommandCount = new();
        // 解放コマンド回数
        public ParameterInt ReleaseCommandCount = new();
        // 取引コマンド回数
        public ParameterInt TradeCommandCount = new();
        // 魔法編成回数
        public ParameterInt StatusSkillChangeCount = new();

        // 覚醒スキル使用回数
        public ParameterInt UseAwakeSkillCount = new();
        // 交代スキル使用回数
        public ParameterInt UseChangeLineCount = new();

        // 帰還できるかのフラグ
        public ParameterBool Cursed = new();


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
            var notClear = _achievements.FindAll(a => !a.Achieved.Value);
            if (notClear.Count == 0)
            {
                return null;
            }
            notClear.Sort((a, b) => a.Master.Rank - b.Master.Rank > 0 ? -1 : 1);
            return notClear[0];
        }

        public void SetAchievementRank(List<AchievementData> achievementDatas)
        {
            // 未受取を引き継ぐ
            var notClearAchivements = new List<AchievementInfo>();
            foreach (var achievement in _achievements)
            {
                if (!achievement.Presented.Value)
                {
                    notClearAchivements.Add(achievement);
                }
            }
            _achievements.Clear();
            foreach (var notClearAchivement in notClearAchivements)
            {
                _achievements.Add(notClearAchivement);
            }
            foreach (var achievementData in achievementDatas)
            {
                if (achievementData.Rank == MissionRank.Value)
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
                if (!checkMissionRank && achievementInfo.Master.ConditionType == AchievementConditionType.Complete)
                {
                    continue;
                }
                CheckAchievementCondition(achievementInfo);
            }
        }

        public bool IsRankUpBefore()
        {
            var find = _achievements.Find(a => a.Master.ConditionType == AchievementConditionType.Complete);
            if (find != null)
            {
                var mains = _achievements.FindAll(a => a.Master != null && a.Master.Category == AchievementCategory.Main);
                var achived = mains.FindAll(a => a.Achieved.Value).Count;
                return mains.Count-1 == achived;
            }
            return false;
        }

        private void CheckAchievementCondition(AchievementInfo achievementInfo)
        {
            switch(achievementInfo.Master.ConditionType)
            {
                case AchievementConditionType.Complete:
                    // 達成数
                    var mains = _achievements.FindAll(a => a.Master.Category == AchievementCategory.Main);
                    var achived = mains.FindAll(a => a.Achieved.Value).Count;
                    achievementInfo.SetCondition(achived,mains.Count-1);
                    break;
                case AchievementConditionType.DepartureCount:
                    // 出撃回数
                    achievementInfo.SetCondition(DepartureCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.DepartureBattleFieldCount:
                    // 出撃回数
                    achievementInfo.SetCondition(DepartureBattleFieldCount.Value, achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.BattleVictory:
                    // 勝利回数
                    achievementInfo.SetCondition(BattleVictoryCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.CharacterLevel:
                    // キャラLv　Param2がActorId,-1なら任意
                    var level = 0;
                    if (achievementInfo.Master.Param2 == -1)
                    {
                        level = ActorInfos.Max(a => a.Level);
                    } else
                    {
                        var levelChara = ActorInfos.Find(a => a.ActorId.Value == achievementInfo.Master.Param2);
                        level = levelChara != null ? levelChara.Level : 0;
                    }
                    achievementInfo.SetCondition(level,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.CharacterLevelNum:
                    var findAll = ActorInfos.FindAll(a => a.Level >= achievementInfo.Master.Param1);
                    achievementInfo.SetCondition(findAll.Count,achievementInfo.Master.Param2);
                    break;
                case AchievementConditionType.TacticsLvupCount:
                    // Nu消費レベルアップ回数
                    achievementInfo.SetCondition(TacticsLvupCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.BattleScore:
                    // バトル評価値
                    achievementInfo.SetCondition(BattleScore.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.TotalDamage:
                    // 与ダメージ
                    achievementInfo.SetCondition(TotalDamage.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.ClearStage:
                    // ステージクリア
                    var cleared = _clearedStages.Contains(achievementInfo.Master.Param1);
                    achievementInfo.SetCondition(cleared ? 1 : 0,1);
                    break;
                case AchievementConditionType.UseAwakeSkillCount:
                    // 覚醒スキル使用回数
                    achievementInfo.SetCondition(UseAwakeSkillCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.UseChangeLineCount:
                    // 交代スキル使用回数
                    achievementInfo.SetCondition(UseChangeLineCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.DeckEditCommandCount:
                    // 編成コマンド回数
                    achievementInfo.SetCondition(DeckEditCommandCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.PresentCommandCount:
                    // 献上コマンド回数
                    achievementInfo.SetCondition(PresentCommandCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.ReliefCommandCount:
                    // 救済コマンド回数
                    achievementInfo.SetCondition(ReliefCommandCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.TransferCommandCount:
                    // 転送コマンド回数
                    achievementInfo.SetCondition(TransferCommandCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.ReleaseCommandCount:
                    // 解放コマンド回数
                    achievementInfo.SetCondition(ReleaseCommandCount.Value,achievementInfo.Master.Param1);
                    break;
                case AchievementConditionType.StatusSkillChangeCount:
                    // 魔法編成回数
                    achievementInfo.SetCondition(StatusSkillChangeCount.Value,achievementInfo.Master.Param1);
                    break;
            }
        }

        public List<GetItemInfo> AchievementGetItemInfos()
        {
            var list = new List<GetItemInfo>();
            var achievements = AchievementInfos.FindAll(a => !a.Presented.Value && a.Achieved.Value);
            foreach (var achievement in achievements)
            {
                list.AddRange(achievement.GetItemInfos);
                achievement.Presented.SetValue(true);
            }
            return list;
        }

        // 所持アイテム情報
        private List<GetItemInfo> _getItemInfos = new();
        public void AddGetItemInfo(GetItemInfo getItemInfo)
        {
            _getItemInfos.Add(getItemInfo);
            switch (getItemInfo.GetItemType)
            {
                case GetItemType.Item:
                    GainItemNum(getItemInfo.Param1,getItemInfo.Param2);
                    break;
                case GetItemType.Building:
                    GainBuilding(getItemInfo.Param1);
                    break;
                case GetItemType.RankUp:
                    MissionRank.GainValue(1);
                    break;
                case GetItemType.ClearStage:
                    ClearStage(getItemInfo.Param1);
                    break;
                case GetItemType.Evaluate:
                    EvaluationValue.GainValue(getItemInfo.Param1);
                    break;
                case GetItemType.SkillMastary:
                    var target = _actorInfos.Find(a => a.ActorId.Value == getItemInfo.Param1);
                    target.GainSkillMastary(getItemInfo.Param2);
                    break;
                default:
                    CheckAddActor();
                    CheckLearningSkillId();
                    break;
            }
        }

        private List<int> _learningSkillIds = new();
        public List<int> LearningSkillIds => _learningSkillIds;
        private void CheckLearningSkillId()
        {
            var addSkillInfos = _getItemInfos.FindAll(a => a.GetFlag && a.GetItemType == GetItemType.Skill);
            foreach (var addSkillInfo in addSkillInfos)
            {
                if (!_learningSkillIds.Contains(addSkillInfo.Param1))
                {
                    // 新規魔法入手
                    _learningSkillIds.Add(addSkillInfo.Param1);
                }
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
                    if (_transferActorInfos.Find(a => a.ActorId.Value == actorData.Id) != null)
                    {
                        return;
                    }
                    var actorInfo = new ActorInfo(actorData);
                    actorInfo.BattleIndex.SetValue(_actorInfos.Count+1);
                    actorInfo.SetLevel(actorData.InitLv);
                    actorInfo.ChangeHp(actorInfo.MaxHp);
                    // 最初に加入したキャラは自動編成
                    if (_actorInfos.Count == 0)
                    {
                        var first = CurrentDeckInfo.ActorIdDict.Where(a => a.Value == -1).First();
                        if (first.Value == -1)
                        {
                            CurrentDeckInfo.ActorIdDict[first.Key] = actorInfo.ActorId.Value;
                        }
                    }
                    _actorInfos.Add(actorInfo);
                    // 整列
                    _actorInfos.Sort((a,b) => a.BattleIndex.Value - b.BattleIndex.Value > 0 ? 1 : -1);

                }
            }
        }

        public ParameterInt DeckId = new(1);
        private Dictionary<int,DeckInfo> _deckInfos = new();
        private void InitDeckInfos()
        {
            for (int i = 1;i <= 5;i++)
            {
                _deckInfos[i] = new DeckInfo();
            }
        }

        public DeckInfo CurrentDeckInfo => _deckInfos[DeckId.Value];

        public List<BattlerInfo> DeckEditBattlerInfos()
        {
            // 空きスロットも表示する
            var battlerInfos = new List<BattlerInfo>();
            for (int i = 1;i <= 6;i++)
            {
                var dictValue = CurrentDeckInfo.ActorIdDict[i];
                if (dictValue > -1)
                {
                    var actorInfo = _actorInfos.Find(a => a.ActorId.Value == dictValue);
                    battlerInfos.Add(new BattlerInfo(actorInfo,i));
                } else
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

        public void UseCurrencyHeal()
        {
            foreach (var actorId in CurrentDeckInfo.ActorIdDict)
            {
                var find = _actorInfos.Find(a => a.ActorId.Value == actorId.Value);
                find?.ChangeHp(find.CurrentHp.Value + 10);
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

        public void CursedParty(bool cursed)
        {
            Cursed.SetValue(cursed);
        }

        public int PartyEvaluate()
        {
            return _actorInfos.Sum(a => a.Evaluate());
        }
    }
}