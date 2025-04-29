using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class StrategyModel : BaseModel
    {
        private StrategySceneInfo _sceneParam;
        public StrategySceneInfo SceneParam => _sceneParam;
        private bool _battleResultVictory = false;
        public bool BattleResultVictory => _battleResultVictory;

        private bool _inBattleResult = false;
        public bool InBattleResult => _inBattleResult;

        private List<ActorInfo> _displayActorInfos = new();
        public List<ActorInfo> DisplayActorInfos => _displayActorInfos;
        private List<HexUnitInfo> _lostUnitInfos = new();

        public StrategyModel()
        {
            _sceneParam = (StrategySceneInfo)GameSystem.SceneStackManager.LastSceneParam;
            _inBattleResult = _sceneParam.InBattle;
            _battleResultVictory = _sceneParam.BattleResultVictory;
            if (_inBattleResult)
            {
                var actors = _sceneParam.BattlerInfos.FindAll(a => a.ActorInfo != null);
                foreach (var actor in actors)
                {
                    _displayActorInfos.Add(PartyInfo.ActorInfos.Find(a => a.ActorId == actor.ActorInfo.ActorId));
                }
                // バトル結果をUnitと同期
                foreach (var battlerInfo in _sceneParam.BattlerInfos)
                {
                    if (battlerInfo.ActorInfo != null)
                    {
                        var team = CurrentStage.TeamInfos.Find(a => a.TeamId.Value == (int)TeamIdType.Home);
                        team.UpdateUnitStatus(battlerInfo);
                    } else
                    if (battlerInfo.EnemyData != null)
                    {
                        var team = CurrentStage.TeamInfos.Find(a => a.TeamId.Value == (int)TeamIdType.Away);
                        team.UpdateUnitStatus(battlerInfo);
                    }
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
            var list = new List<ListData>();
            var listData = new ListData(_levelUpActorInfos[0]);
            list.Add(listData);
            list.Add(listData);
            list.Add(listData);
            list.Add(listData);
            list.Add(listData);
            return list;
        }

        public void MakeSelectRelicData()
        {
            /*
            var record = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentSelectRecord()));
            
            var getItemInfos = SceneParam.GetItemInfos;
            var selectRelicInfo = getItemInfos.Find(a => a.GetItemType == GetItemType.SelectRelic);
            if (selectRelicInfo != null)
            {
                var relicInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Skill);
                _relicData = new List<SkillInfo>();
                foreach (var relicInfo in relicInfos)
                {
                    var skillInfo = new SkillInfo(relicInfo.Param1);
                    skillInfo.SetEnable(true);
                    _relicData.Add(skillInfo);
                }
            }
            */
        }

        public void MakeResult()
        {
            var getItemInfos = _sceneParam.GetItemInfos;

            var lvUpList = new List<ActorInfo>();
            // Expを付与する,結果非表示
            var expGetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Exp);
            foreach (var expGetItemInfo in expGetItemInfos)
            {
                expGetItemInfo.SetGetFlag(true);
                var target = PartyInfo.ActorInfos.Find(a => a.ActorId.Value == expGetItemInfo.Param1);
                if (target != null)
                {
                    var beforeLv = target.Level;
                    var from = target.Evaluate();
                    target.Exp.GainValue(expGetItemInfo.Param2);
                    if (beforeLv != target.Level)
                    {
                        // 新規魔法取得があるか
                        var skills = target.LearningSkills(target.Level - beforeLv);
                        var to = target.Evaluate();
                        if (skills.Count > 0)
                        {
                            foreach (var skill in skills)
                            {
                                var learnSkillInfo = new LearnSkillInfo(from,to,skill);
                                _learnSkillInfo.Add(learnSkillInfo);
                            }
                        } else
                        {
                            _learnSkillInfo.Add(null);
                        }
                        lvUpList.Add(target);
                    }
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
                resultInfo.SetTitle("+" + gainCurrency.ToString() + DataSystem.GetText(1000) + "を入手！");
                _resultInfos.Add(resultInfo);
            }
            foreach (var skillGetItemInfo in skillGetItemInfos)
            {
                var resultInfo = new StrategyResultViewInfo();
                var skillData = DataSystem.FindSkill(skillGetItemInfo.Param1);
                resultInfo.SetSkillId(skillData.Id);
                resultInfo.SetTitle(skillData.Name + "を入手！");
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
                    case GetItemType.Ending:
                        getItemInfo.SetGetFlag(true);
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

        public string BattleResultTurn()
        {
            if (!_inBattleResult)
            {
                return null;
            }
            var turn = _sceneParam.BattleTurn;
            if (turn > 0)
            {
                return turn.ToString() + "ターン";
            }
            return null;
        }

        public string BattleResultScore()
        {
            if (!_inBattleResult)
            {
                return null;
            }
            var recordScore = _sceneParam.BattleResultScore;
            if (recordScore >= 0)
            {
                return "+" + (recordScore*0.01f).ToString("F2") + "%";
            }
            return null;
        }

        public string BattleResultRemainHpPercent()
        {
            if (!_inBattleResult)
            {
                return null;
            }
            var remainHpPercent = _sceneParam.BattleRemainHpPercent;
            if (remainHpPercent > 0)
            {
                return remainHpPercent.ToString() + "%";
            }
            return null;
        }

        public string BattleResultMaxDamage()
        {
            if (!_inBattleResult)
            {
                return null;
            }
            var maxDamage = _sceneParam.BattleMaxDamage;
            if (maxDamage > 0)
            {
                return maxDamage.ToString();
            }
            return null;
        }

        public string BattleResultDefeatedCount()
        {
            if (!_inBattleResult)
            {
                return null;
            }
            if (!_battleResultVictory)
            {
                return null;
            }
            var defeatedCount = _sceneParam.BattleDefeatedCount;
            if (defeatedCount >= 0)
            {
                return defeatedCount.ToString();
            }
            return null;
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
            if (_inBattleResult && _battleResultVictory == false)
            {
                return BaseConfirmCommand(3040,3054); // 再戦
            }
            return BaseConfirmCommand(3040,19040);
        }

        public bool IsBonusTactics(int actorId)
        {
            return false;
        }

        public void EndStrategy()
        {
            SavePlayerStageData(true);
        }
    }

    public class StrategySceneInfo
    {
        public int BattleTurn;
        public List<GetItemInfo> GetItemInfos;
        public List<ActorInfo> ActorInfos;
        public List<BattlerInfo> BattlerInfos;
        public bool InBattle;
        public int BattleResultScore;
        public int BattleRemainHpPercent;
        public int BattleMaxDamage;
        public int BattleDefeatedCount;
        public bool BattleResultVictory;
    }
}