using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class StrategyModel : BaseModel
    {
        private StrategySceneInfo _sceneParam;
        public StrategySceneInfo SceneParam => _sceneParam;
        private Scene _returnScene = Scene.None;
        public Scene ReturnScene => _returnScene;
        private bool _battleResultVictory = false;
        public bool BattleResultVictory => _battleResultVictory;

        private bool _inBattleResult = false;
        public bool InBattleResult => _inBattleResult;

        private List<ActorInfo> _displayActorInfos = new();
        public List<ActorInfo> DisplayActorInfos => _displayActorInfos;
        private List<StrategyActorLevelUpInfo> _displayLevelUpInfos = new();
        public List<StrategyActorLevelUpInfo> DisplayLevelUpInfos => _displayLevelUpInfos;
        private List<HexUnitInfo> _lostUnitInfos = new();

        public StrategyModel()
        {
            _sceneParam = (StrategySceneInfo)GameSystem.SceneStackManager.LastSceneParam;
            _inBattleResult = _sceneParam.InBattle;
            _battleResultVictory = _sceneParam.BattleResultVictory;
            _returnScene = _sceneParam.ReturnScene;
            if (_inBattleResult)
            {
                var actors = _sceneParam.BattlerInfos.FindAll(a => a.ActorInfo != null);
                foreach (var actor in actors)
                {
                    _displayActorInfos.Add(PartyInfo.ActorInfos.Find(a => a.ActorId == actor.ActorInfo.ActorId));
                }
            } else
            {
                _displayActorInfos = _sceneParam.ActorInfos;
            }
            MakeResult();
        }

        public void ClearSceneParam()
        {
            _sceneParam = null;
        }

        public bool StageEnd()
        {
            return false;
        }

        private List<StrategyResultViewInfo> _resultInfos = new();
        public List<StrategyResultViewInfo> ResultViewInfos => _resultInfos;

        private List<SkillInfo> _selectLearnSkills = new();
        public List<SkillInfo> SelectLearnSkills => _selectLearnSkills;

        private List<ActorInfo> _levelUpActorInfos = new();
        public List<ActorInfo> LevelUpActorInfos => _levelUpActorInfos;
        private bool _beforeLevelUpAnimation = false;
        public bool BeforeLevelUpAnimation => _beforeLevelUpAnimation;
        public void SetBeforeLevelUpAnimation(bool beforeLevelUpAnimation) => _beforeLevelUpAnimation = beforeLevelUpAnimation;


        private List<LearnSkillInfo> _learnSkillInfo = new();
        public List<LearnSkillInfo> LearnSkillInfo => _learnSkillInfo;
        public List<ListData> LevelUpActorStatus()
        {
            var strategyStrengthInfos = new List<StrategyStrengthInfo>
            {
                new StrategyStrengthInfo()
                {
                    ActorInfo = _levelUpActorInfos[0],
                    StatusParamType = StatusParamType.Hp,
                },
                new StrategyStrengthInfo()
                {
                    ActorInfo = _levelUpActorInfos[0],
                    StatusParamType = StatusParamType.Atk,
                },
                new StrategyStrengthInfo()
                {
                    ActorInfo = _levelUpActorInfos[0],
                    StatusParamType = StatusParamType.Def,
                },
                new StrategyStrengthInfo()
                {
                    ActorInfo = _levelUpActorInfos[0],
                    StatusParamType = StatusParamType.Spd,
                },
                new StrategyStrengthInfo()
                {
                    ActorInfo = _levelUpActorInfos[0],
                    StatusParamType = StatusParamType.Cost,
                }
            };
            var list = new List<ListData>();
            foreach (var strategyStrengthInfo in strategyStrengthInfos)
            {
                var listData = new ListData(strategyStrengthInfo);
                list.Add(listData);
            }
            return list;
        }

        public string BackGround()
        {
            if (CurrentStage != null && GameSystem.SceneStackManager.Current == Scene.Dungeon)
            {
                return CurrentStage.Master.BackGround;
            }
            return null;
        }

        public void MakeResult()
        {
            var getItemInfos = _sceneParam.GetItemInfos;

            var lvUpList = new List<ActorInfo>();
            // Expを付与する
            var expGetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Exp);
            foreach (var expGetItemInfo in expGetItemInfos)
            {
                var target = PartyInfo.ActorInfos.Find(a => a.ActorId.Value == expGetItemInfo.Param1);
                if (target != null)
                {
                    var beforeLv = target.Level;
                    var from = target.Evaluate();
                    var beforeRate = target.Exp.Value % 100 * 0.01f;
                    // 新規魔法取得があるか
                    var skills = target.LearningSkills(1);
                    target.Exp.GainValue(expGetItemInfo.Param2);
                    var afterLv = target.Level;
                    var afterRate = target.Exp.Value % 100 * 0.01f;
                    _displayLevelUpInfos.Add(new StrategyActorLevelUpInfo()
                    {
                        IsLevelUp = beforeLv != target.Level,
                        AfterRate = afterRate,
                        BeforeRate = beforeRate,
                        PlusLv = target.Level - beforeLv,
                        PlusExp = expGetItemInfo.Param2
                    });
                    if (beforeLv < afterLv)
                    {
                        var to = target.Evaluate();
                        if (skills.Count > 0)
                        {
                            foreach (var skill in skills)
                            {
                                var learnSkillInfo = new LearnSkillInfo(from, to, skill);
                                _learnSkillInfo.Add(learnSkillInfo);
                            }
                        } else
                        {
                            _learnSkillInfo.Add(null);
                        }
                        if (!lvUpList.Contains(target))
                        {
                            lvUpList.Add(target);
                        }
                    }
                    target.Exp.GainValue(expGetItemInfo.Param2 * -1);
                }
            }
            _levelUpActorInfos = lvUpList;
            if (lvUpList.Count > 0)
            {
                _beforeLevelUpAnimation = true;
            }

            // エナジー獲得
            var gainCurrency = 0;
            var currencyGetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Currency);
            foreach (var currencyGetItemInfo in currencyGetItemInfos)
            {
                AddGetItemInfo(currencyGetItemInfo);
                gainCurrency += currencyGetItemInfo.Param1;
            }

            // 魔法入手
            var skillGetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Skill);
            foreach (var skillGetItemInfo in skillGetItemInfos)
            {
                AddPlayerInfoSkillId(skillGetItemInfo.Param1);
                AddGetItemInfo(skillGetItemInfo);
            }

            // 獲得エナジー、魔法情報を生成
            _resultInfos.Clear();
            if (gainCurrency > 0)
            {
                var resultInfo = new StrategyResultViewInfo();
                resultInfo.SetTitle("+" + DataSystem.GetReplaceText(20100, gainCurrency.ToString() + DataSystem.GetText(1000)));
                _resultInfos.Add(resultInfo);
            }

            foreach (var skillGetItemInfo in skillGetItemInfos)
            {
                var resultInfo = new StrategyResultViewInfo();
                var skillData = DataSystem.FindSkill(skillGetItemInfo.Param1);
                resultInfo.SetSkillId(skillData.Id);
                resultInfo.SetTitle(DataSystem.GetReplaceText(20100,skillData.Name));
                _resultInfos.Add(resultInfo);
            }

            //
            var skillExpGetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.SkillMastary);
            foreach (var skillExpGetItemInfo in skillExpGetItemInfos)
            {
                var resultInfo = new StrategyResultViewInfo();
                var target = PartyInfo.ActorInfos.Find(a => a.ActorId.Value == skillExpGetItemInfo.Param1);
                var skillData = DataSystem.FindSkill(skillExpGetItemInfo.Param2);
                if (skillData.Id > 1000 && skillData.Rank > RankType.ActiveRank1 && !target.MastarySkillIds.Contains(skillExpGetItemInfo.Param2))
                {
                    resultInfo.SetTitle(DataSystem.GetReplaceText(20110, target.Master.Name) + DataSystem.GetReplaceText(20111, skillData.Name));
                    _resultInfos.Add(resultInfo);
                }

            }

            var evaluateGetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Evaluate);
            foreach (var evaluateGetItemInfo in evaluateGetItemInfos)
            {
                var resultInfo = new StrategyResultViewInfo();
                resultInfo.SetTitle(DataSystem.GetText(3210) + " +" + evaluateGetItemInfo.Param1);
                _resultInfos.Add(resultInfo);
            }

            foreach (var getItemInfo in getItemInfos)
            {
                var resultInfo = new StrategyResultViewInfo();
                switch (getItemInfo.GetItemType)
                {
                    case GetItemType.Regeneration:
                    case GetItemType.Demigod:
                    case GetItemType.StatusUp:
                        break;
                    case GetItemType.AddActor:
                        getItemInfo.SetResultParam(getItemInfo.Param1);
                        AddGetItemInfo(getItemInfo);
                        AddPlayerInfoActorSkillId(getItemInfo.Param1);
                        // キャラ加入
                        var actorData = DataSystem.FindActor(getItemInfo.Param1);
                        resultInfo.SetTitle(DataSystem.GetReplaceText(20200,actorData.Name));
                        _resultInfos.Add(resultInfo);
                        break;
                    case GetItemType.SelectAddActor:
                        AddGetItemInfo(getItemInfo);
                        AddPlayerInfoActorSkillId(getItemInfo.ResultParam);
                        // キャラ加入
                        var actorData2 = DataSystem.FindActor(getItemInfo.ResultParam);
                        resultInfo.SetTitle(DataSystem.GetReplaceText(20200,actorData2.Name));
                        _resultInfos.Add(resultInfo);
                        break;
                    case GetItemType.SelectRelic:
                        if (getItemInfo.Param1 > 1000)
                        {
                            var skillInfo = new SkillInfo(getItemInfo.Param1);
                            skillInfo.SetEnable(true);
                            _selectLearnSkills.Add(skillInfo);
                        }
                        break;
                    case GetItemType.Evaluate:
                        AddGetItemInfo(getItemInfo);
                        break;
                    case GetItemType.ClearStage:
                    case GetItemType.Ending:
                    case GetItemType.SkillMastary:
                        AddGetItemInfo(getItemInfo);
                        break;
                    case GetItemType.Item:
                        var itemResultInfo = new StrategyResultViewInfo();
                        var itemData = DataSystem.Items.Find(a => a.Id == getItemInfo.Param1);
                        itemResultInfo.SetTitle(itemData.Name + " x" + getItemInfo.Param2);
                        _resultInfos.Add(itemResultInfo);
                        AddGetItemInfo(getItemInfo);
                        break;
                }
            }
        }

        public void MakeSelectLearnSkill(int skillId)
        {
            var getItemInfos = _sceneParam.GetItemInfos;
            var selectRelicInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.SelectRelic);
            // 魔法取得
            var selectRelic = _selectLearnSkills.Find(a => a.Id.Value == skillId);
            foreach (var selectRelicInfo in selectRelicInfos)
            {
                selectRelicInfo.SetGetFlag(false);
                var remove =_resultInfos.Find(a => a.SkillId == selectRelicInfo.Param1);
                _resultInfos.Remove(remove);
            }
            var learnGetItemInfo = getItemInfos.Find(a => a.GetItemType == GetItemType.SelectRelic && skillId == a.Param1);

            var getItemInfo = MakeGetItemInfo(GetItemType.Skill,skillId);
            AddPlayerInfoSkillId(skillId);
            AddGetItemInfo(getItemInfo);
            var resultInfo = new StrategyResultViewInfo();
            resultInfo.SetSkillId(skillId);
            resultInfo.SetTitle(DataSystem.FindSkill(skillId).Name);
            _resultInfos.Add(resultInfo);
            _selectLearnSkills.Clear();
        }

        public void RemoveLevelUpData()
        {
            _levelUpActorInfos.RemoveAt(0);
            _learnSkillInfo.RemoveAt(0);
        }

        public void ClearExpDict()
        {
            var getItemInfos = _sceneParam.GetItemInfos;

            // Expを付与する
            var expGetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Exp);
            foreach (var expGetItemInfo in expGetItemInfos)
            {
                expGetItemInfo.SetGetFlag(true);
                var target = PartyInfo.ActorInfos.Find(a => a.ActorId.Value == expGetItemInfo.Param1);
                if (target != null)
                {
                    target.Exp.GainValue(expGetItemInfo.Param2);
                }
            }
            _displayLevelUpInfos.Clear();
            //_displayExpDict.Clear();
        }

        public string TitleText()
        {
            if (_inBattleResult)
            {
                return DataSystem.GetText(20010);
            }
            return DataSystem.GetText(20040);
        }

        public List<SystemData.CommandData> ResultCommand()
        {
            return BaseConfirmCommand(3040);
            /*
            if (_inBattleResult && !_battleResultVictory)
            {
                return BaseConfirmCommand(3040,3054); // 再戦
            }
            return BaseConfirmCommand(3040,19040);
            */
        }

        public bool IsBonusTactics(int actorId)
        {
            return false;
        }

        public void EndStrategy()
        {
            SavePlayerStageData(true, _returnScene);
        }
    }

    public class StrategySceneInfo
    {
        public List<GetItemInfo> GetItemInfos;
        public List<ActorInfo> ActorInfos;
        public List<BattlerInfo> BattlerInfos;
        public bool InBattle;
        public BattleScore BattleScore;
        public bool BattleResultVictory;
        public Scene ReturnScene;
        public MainMenuSceneInfo ReturnMainMenuSceneParam;
    }
}