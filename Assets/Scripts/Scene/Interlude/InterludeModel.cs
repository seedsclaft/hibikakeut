using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class InterludeModel : BaseModel
    {
        private List<GetItemResultViewInfo> _resultInfos = new();
        public List<GetItemResultViewInfo> ResultViewInfos => _resultInfos;
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
            var evaluatePrizes = DataSystem.Dates[DataType.EvaluatePrizes].FindAll<EvaluatePrizeData>(a => a.Chapter == PartyInfo.Chapter.Value);
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
                var prizeSets = DataSystem.Dates[DataType.PrizeSets].FindAll<PrizeSetData>(a => a.Id == evaluatePrize.PriseSetId);
                foreach (var prizeSet in prizeSets)
                {
                    var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                    getItemInfos.Add(getItemInfo);
                }
            }

            // バトルスコアをNuに変換
            var battleScoreItemInfo = MakeGetItemInfo(GetItemType.BattleSocreCurrency, BattleScorePoint());
            getItemInfos.Add(battleScoreItemInfo);

            _resultInfos.Clear();
            _resultInfos = MakeGetItemResultViewInfos(getItemInfos);

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

            // 評価値を決定
            if (score >= 5)
            {
                return 3;
            }
            else
            if (score >= 3)
            {
                return 2;
            }
            else
            if (score >= 1)
            {
                return 1;
            }
            return 0;
        }

        private bool CheckAchieved(EvaluatePrizeData evaluatePrize)
        {
            switch (evaluatePrize.ConditionType)
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

        public string ClearStageNum()
        {
            var clearStageNum = 0;
            foreach (var stageData in DataSystem.Dates[DataType.Stages].ToList<StageData>())
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

        public int BattleScorePoint()
        {
            var battleScore = PartyInfo.PartyStatInfo.BattleScore.Value;
            // 0-50
            return battleScore / 10;
        }

        public bool EndInterludePhase()
        {
            if (PartyInfo.Period.Value > DataSystem.System.PeriodTurns)
            {
                PartyInfo.Period.SetValue(1);
                PartyInfo.Chapter.GainValue(1);
                PartyInfo.PartyStatInfo.BattleScore.SetValue(0);
                return true;
            }
            return false;
        }
    }
}