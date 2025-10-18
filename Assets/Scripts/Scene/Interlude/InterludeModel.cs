using System.Collections.Generic;

namespace Ryneus
{
    public class InterludeModel : BaseModel
    {
        private List<StrategyResultViewInfo> _resultInfos = new();
        public List<StrategyResultViewInfo> ResultViewInfos => _resultInfos;
        private List<SkillInfo> _selectLearnSkills = new();
        public List<SkillInfo> SelectLearnSkills => _selectLearnSkills;

        public InterludeModel()
        {
        }

        public int InterrudeEventId()
        {
            return PartyInfo.Chapter.Value * 10;
        }

        public int AfterInterrudeEventId()
        {
            return InterrudeEventId() + 1;
        }

        public int MakeEvaluateResults()
        {
            var evaluatePrizes = DataSystem.EvaluatePrizes.FindAll(a => a.Chapter == PartyInfo.Chapter.Value);
            var evaluatePrizeDates = new List<EvaluatePrizeData>();
            var evaluateDicts = new Dictionary<int, List<EvaluatePrizeData>>();
            foreach (var evaluatePrizeData in evaluatePrizes)
            {
                if (!evaluateDicts.ContainsKey(evaluatePrizeData.Category))
                {
                    evaluateDicts[evaluatePrizeData.Category] = new();
                }
                evaluateDicts[evaluatePrizeData.Category].Add(evaluatePrizeData);
            }
            var score = 0;
            foreach (var evaluateDict in evaluateDicts)
            {
                var findIndex = evaluateDict.Value.FindIndex(a => CheckAchieved(a));
                score += evaluateDict.Value.Count - findIndex;
                if (findIndex > -1)
                {
                    evaluatePrizeDates.Add(evaluateDict.Value[findIndex]);
                }
            }

            var getItemInfos = new List<GetItemInfo>();
            foreach (var evaluatePrize in evaluatePrizeDates)
            {
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == evaluatePrize.PriseSetId);
                foreach (var prizeSet in prizeSets)
                {
                    var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                    getItemInfos.Add(getItemInfo);
                }
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

            // アイテム入手
            var itemGetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Item);
            foreach (var itemGetItemInfo in itemGetItemInfos)
            {
                AddGetItemInfo(itemGetItemInfo);
            }

            // RankUp
            var rankUpgetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.RankUp);
            foreach (var rankUpgetItemInfo in rankUpgetItemInfos)
            {
                AddGetItemInfo(rankUpgetItemInfo);
            }

            // 評価値加算
            var evaluateGetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Evaluate);
            foreach (var evaluateGetItemInfo in evaluateGetItemInfos)
            {
                AddGetItemInfo(evaluateGetItemInfo);
            }

            // 招聘コマンド回数増加
            var addReliefCommandCountGetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.AddReliefCommandCount);
            foreach (var addReliefCommandCountGetItemInfo in addReliefCommandCountGetItemInfos)
            {
                AddGetItemInfo(addReliefCommandCountGetItemInfo);
            }

            // ステージクリア
            var claerStageGetItemInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.ClearStage);
            foreach (var claerStageGetItemInfo in claerStageGetItemInfos)
            {
                AddGetItemInfo(claerStageGetItemInfo);
            }

            // 獲得エナジー、魔法情報を生成
            _resultInfos.Clear();
            if (gainCurrency > 0)
            {
                var resultInfo = new StrategyResultViewInfo();
                resultInfo.SetTitle("+" + gainCurrency.ToString() + DataSystem.GetText(1000) + "を入手！");
                _resultInfos.Add(resultInfo);
            }
            foreach (var itemGetItemInfo in itemGetItemInfos)
            {
                var resultInfo = new StrategyResultViewInfo();
                var itemData = DataSystem.Items.Find(a => a.Id == itemGetItemInfo.Param1);
                resultInfo.SetTitle(itemData.Name + " x" + itemGetItemInfo.Param2);
                _resultInfos.Add(resultInfo);
            }
            foreach (var skillGetItemInfo in skillGetItemInfos)
            {
                var resultInfo = new StrategyResultViewInfo();
                var skillData = DataSystem.FindSkill(skillGetItemInfo.Param1);
                resultInfo.SetSkillId(skillData.Id);
                resultInfo.SetTitle(DataSystem.GetReplaceText(20100, skillData.Name));
                _resultInfos.Add(resultInfo);
            }
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
                    case GetItemType.Ending:
                        getItemInfo.SetGetFlag(true);
                        break;
                    case GetItemType.AddReliefCommandCount:
                        var addReliefCommandCount = new StrategyResultViewInfo();
                        addReliefCommandCount.SetTitle(DataSystem.GetText(20420));
                        _resultInfos.Add(addReliefCommandCount);
                        break;
                }
            }
            // 評価値を決定
            if (score >= 5)
            {
                return 3;
            } else
            if (score >= 3)
            {
                return 2;
            } else
            if (score >= 1)
            {
                return 1;
            }
            return 0;
        }

        private bool CheckAchieved(EvaluatePrizeData evaluatePrize)
        {
            switch(evaluatePrize.ConditionType)
            {
                case AchievementConditionType.MissionRank:
                    if (PartyInfo.MissionRank.Value >= evaluatePrize.Param1)
                    {
                        return true;
                    }
                    break;
                case AchievementConditionType.ClearStageNum:
                    if (PartyInfo.ClearedStages.Count >= evaluatePrize.Param1)
                    {
                        return true;
                    }
                    break;
                case AchievementConditionType.PartyEvaluate:
                    if (PartyInfo.PartyEvaluate() >= evaluatePrize.Param1)
                    {
                        return true;
                    }
                    break;
                case AchievementConditionType.ClearStage:
                    // ステージクリア
                    return PartyInfo.ClearedStages.Find(a => a.Value == evaluatePrize.Param1) != null;
                default:
                    return true;
            }
            return false;
        }

        public List<ActorInfo> DisplayActorInfos()
        {
            return PartyInfo.ActorInfos;
        }

        public string ClearStageNum()
        {
            var clearStageNum = 0;
            foreach (var stageData in DataSystem.Stages)
            {
                if (!stageData.Selectable)
                {
                    continue;
                }
                var cleared = PartyInfo.IsClaeredStage(stageData.StageNo);
                if (cleared)
                {
                    clearStageNum++;
                }
            }
            return DataSystem.GetReplaceText(20430, clearStageNum.ToString());
        }

        public List<SystemData.CommandData> ResultCommand()
        {
            return BaseConfirmCommand(3040);
        }

        public bool EndInterludePhase()
        {
            // 6ピリオドでチャプター切り替え
            if (PartyInfo.Period.Value > DataSystem.System.PeriodTurns)
            {
                PartyInfo.Period.SetValue(1);
                PartyInfo.Chapter.GainValue(1);
                return true;
            }
            return false;
        }
    }
}