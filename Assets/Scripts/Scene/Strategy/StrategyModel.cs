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

        public StrategyModel()
        {
            _sceneParam = (StrategySceneInfo)GameSystem.SceneStackManager.LastSceneParam;
            _inBattleResult = _sceneParam.InBattle;
            _battleResultVictory = _sceneParam.BattleResultVictory;
            MakeResult();
        }
        
        public void ClearSceneParam()
        {
            _sceneParam = null;
        }

        public bool StageEnd()
        {
            if (CurrentStage != null && PartyInfo != null)
            {
                return CurrentStage.EndSeek == PartyInfo.Seek.Value && _inBattleResult;
            }
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

        public List<ActorInfo> TacticsActors()
        {
            if (_sceneParam != null)
            {
                return _sceneParam.ActorInfos;
            }
            return null;
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
                var target = _sceneParam.ActorInfos.Find(a => a.ActorId.Value == expGetItemInfo.Param1);
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
                currencyGetItemInfo.SetGetFlag(true);
                var gain = currencyGetItemInfo.Param1;
                PartyInfo.Currency.GainValue(gain,0);
                gainCurrency += gain;
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
                resultInfo.SetTitle("+" + gainCurrency.ToString() + DataSystem.GetText(1000));
                _resultInfos.Add(resultInfo);
            }
            foreach (var skillGetItemInfo in skillGetItemInfos)
            {
                var resultInfo = new StrategyResultViewInfo();
                var skillData = DataSystem.FindSkill(skillGetItemInfo.Param1);
                resultInfo.SetSkillId(skillData.Id);
                resultInfo.SetTitle(skillData.Name);
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
                        getItemInfo.SetGetFlag(true);
                        getItemInfo.SetResultParam(getItemInfo.Param1);
                        AddPlayerInfoActorSkillId(getItemInfo.Param1);
                        // キャラ加入
                        var actorData = DataSystem.FindActor(getItemInfo.Param1);
                        resultInfo.SetTitle(DataSystem.GetReplaceText(20200,actorData.Name));
                        _resultInfos.Add(resultInfo);
                        break;
                    case GetItemType.SelectAddActor:
                        getItemInfo.SetGetFlag(true);
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

        public List<ActorInfo> BattleResultActors()
        {
            return _sceneParam.ActorInfos;
        }

        public void ClearBattleData(List<ActorInfo> actorInfos)
        {
            foreach (var actorInfo in actorInfos)
            {
                if (actorInfo.BattleIndex.Value >= 0)
                {
                    //actorInfo.SetBattleIndex(-1);
                }
            }
        }

        public List<ActorInfo> LostMembers()
        {
            return BattleResultActors().FindAll(a => a.BattleIndex.Value >= 0 && a.CurrentHp.Value == 0);
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
            CurrentStage.SeekIndex.SetValue(0);
        }

        public void SeekStage()
        {
            SeekNext();
            SavePlayerStageData(true);
        }



        public void ReturnTempBattleMembers()
        {
            foreach (var tempActorInfo in TempInfo.TempActorInfos)
            {
                //tempActorInfo.SetBattleIndex(-1);
                //PartyInfo.UpdateActorInfo(tempActorInfo);
            }
            //TempInfo.ClearBattleActors();
        }
    }

    public class StrategySceneInfo
    {
        public int BattleTurn;
        public List<GetItemInfo> GetItemInfos;
        public List<ActorInfo> ActorInfos;
        public bool InBattle;
        public int BattleResultScore;
        public int BattleRemainHpPercent;
        public int BattleMaxDamage;
        public int BattleDefeatedCount;
        public bool BattleResultVictory;
    }
}