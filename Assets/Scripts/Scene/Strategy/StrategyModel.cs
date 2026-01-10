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
        private List<LevelUpViewInfo> _levelUpViewInfos = new();
        public List<LevelUpViewInfo> LevelUpViewInfos => _levelUpViewInfos;

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

        public bool ReleifScene()
        {
            return _sceneParam.GetItemInfos.Find(a => a.GetItemType == GetItemType.SelectAddActor) != null;
        }

        public List<ActorInfo> AddSelectActorInfos()
        {
            // 未加入の仲間
            var actorGetItemInfos = _sceneParam.GetItemInfos.FindAll(a => a.GetItemType == GetItemType.SelectAddActor);
            var actorInfos = new List<ActorInfo>();
            foreach (var actorGetItemInfo in actorGetItemInfos)
            {
                actorInfos.Add(new ActorInfo(DataSystem.Actors[actorGetItemInfo.Param1]));
            }
            return actorInfos;
        }

        private List<GetItemResultViewInfo> _resultInfos = new();
        public List<GetItemResultViewInfo> ResultViewInfos => _resultInfos;

        private List<SkillInfo> _selectLearnSkills = new();
        public List<SkillInfo> SelectLearnSkills => _selectLearnSkills;
/*
        private List<ActorInfo> _levelUpActorInfos = new();
        public List<ActorInfo> LevelUpActorInfos => _levelUpActorInfos;

        private List<LearnSkillInfo> _learnSkillInfo = new();
        public List<LearnSkillInfo> LearnSkillInfo => _learnSkillInfo;
        /*
        public List<ListData> LevelUpActorStatus()
        {
            var strategyStrengthInfos = StrategyStrengthInfo.BasicStrategyStrengthInfos(_levelUpActorInfos[0]);
            var list = new List<ListData>();
            foreach (var strategyStrengthInfo in strategyStrengthInfos)
            {
                var listData = new ListData(strategyStrengthInfo);
                list.Add(listData);
            }
            return list;
        }
        */

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
            _resultInfos.Clear();
            _resultInfos = MakeGetItemResultViewInfos(getItemInfos);

            // Expを付与する
            var expGetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Exp);
            _levelUpViewInfos = new List<LevelUpViewInfo>();
            foreach (var expGetItemInfo in expGetItemInfos)
            {
                var target = PartyInfo.ActorInfos.Find(a => a.ActorId.Value == expGetItemInfo.Param1);
                if (target == null)
                {
                    continue;
                }
                var beforeLv = target.Level;
                var from = target.Evaluate();
                var beforeRate = target.Exp.Value % 100 * 0.01f;
                var levelUpInfoView = MakeLevelUpViewInfo(target, expGetItemInfo.Param2);
                // 新規魔法取得があるか
                var afterLv = target.Level;
                var afterRate = target.Exp.Value % 100 * 0.01f;
                _displayLevelUpInfos.Add(new StrategyActorLevelUpInfo()
                {
                    IsLevelUp = levelUpInfoView.StrategyStrengthInfos.Count > 0,
                    AfterRate = afterRate,
                    BeforeRate = beforeRate,
                    PlusLv = target.Level - beforeLv,
                    PlusExp = expGetItemInfo.Param2
                });
                if (levelUpInfoView.StrategyStrengthInfos.Count > 0 || levelUpInfoView.SkillInfo != null)
                {
                    _levelUpViewInfos.Add(levelUpInfoView);
                }
                target.Exp.GainValue(expGetItemInfo.Param2 * -1);
            }

            MakeGetItemResults(getItemInfos);
            foreach (var getItemInfo in getItemInfos)
            {
                switch (getItemInfo.GetItemType)
                {
                    case GetItemType.SelectRelic:
                        if (getItemInfo.Param1 > 1000)
                        {
                            var skillInfo = new SkillInfo(getItemInfo.Param1);
                            skillInfo.SetEnable(true);
                            _selectLearnSkills.Add(skillInfo);
                        }
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
                var remove = _resultInfos.Find(a => a.SkillId == selectRelicInfo.Param1);
                _resultInfos.Remove(remove);
            }
            var learnGetItemInfo = getItemInfos.Find(a => a.GetItemType == GetItemType.SelectRelic && skillId == a.Param1);

            var getItemInfo = MakeGetItemInfo(GetItemType.Skill,skillId);
            AddPlayerInfoSkillId(skillId);
            AddGetItemInfo(getItemInfo);
            var resultInfo = new GetItemResultViewInfo();
            resultInfo.SetSkillId(skillId);
            resultInfo.Title.SetValue(DataSystem.FindSkill(skillId).Name);
            _resultInfos.Add(resultInfo);
            _selectLearnSkills.Clear();
        }

        public List<ActorInfo> LevelUpActorInfos()
        {
            var actorInfos = new List<ActorInfo>();
            foreach (var levelUpViewInfos in _levelUpViewInfos)
            {
                actorInfos.Add(levelUpViewInfos.ActorInfo);
            }
            return actorInfos;
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
                    var beforeHp = target.MaxHp;
                    target.Exp.GainValue(expGetItemInfo.Param2);
                    var afterHp = target.MaxHp;
                    target.ChangeHp(target.CurrentHp.Value + afterHp - beforeHp);
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
            //SavePlayerStageData(_returnScene);
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