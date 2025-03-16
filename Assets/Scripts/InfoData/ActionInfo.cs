using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    [System.Serializable]
    public class ActionInfo 
    {
        private int _index;
        public ParameterInt SubjectIndex = new();

        private int _lastTargetIndex = 0;
        public int LastTargetIndex => _lastTargetIndex;
        public SkillData Master => DataSystem.FindSkill(_skillInfo.Id.Value);
        private SkillInfo _skillInfo = null;
        public SkillInfo SkillInfo => _skillInfo;
        
        private RangeType _rangeType = RangeType.None;
        public RangeType RangeType => _rangeType;
        private TargetType _targetType = TargetType.None;
        public TargetType TargetType => _targetType;
        private ScopeType _scopeType = ScopeType.None;
        public ScopeType ScopeType => _scopeType;

        private List<ActionResultInfo> _actionResults = new ();
        public List<ActionResultInfo> ActionResults => _actionResults;

        public ParameterInt MpCost = new();
        public ParameterInt HpCost = new();
        public ParameterInt BaseRepeatTime = new();
        public ParameterInt RepeatTime = new();
        public void SetRepeatTime(int repeatTime)
        {
            if (_actionedRepeatTimes.Count > 0 && !_actionedRepeatTimes.Contains(RepeatTime.Value))
            {
                // 実際に行動した回数分減らす
                RepeatTime.SetValue(repeatTime - _actionedRepeatTimes.Count);
                return;
            }
            RepeatTime.SetValue(repeatTime);
        }

        public void SeekRepeatTime()
        {
            RepeatTime.GainValue(-1);
        }

        private bool _isSettingParameter = false;
        public bool IsSettingParameter => _isSettingParameter;
        public void SetIsSettingParameter(bool isSettingParameter) => _isSettingParameter = isSettingParameter;

        public bool FirstAttack()
        {
            return (BaseRepeatTime.Value-1) == RepeatTime.Value;
        }

        public bool LastAttack()
        {
            return RepeatTime.Value == 1;
        }

        private List<int> _actionedRepeatTimes = new ();
        public void AddActionedRepeatTimes(int repeatTime)
        {
            _actionedRepeatTimes.Add(repeatTime);
        }
        
        // 選択可能な対象情報
        private List<int> _candidateTargetIndexList;
        public List<int> CandidateTargetIndexList => _candidateTargetIndexList;
        public void SetCandidateTargetIndexList(List<int> candidateTargetIndexList)
        {
            _candidateTargetIndexList = candidateTargetIndexList;
        }

        private bool _triggeredSkill = false;
        public bool TriggeredSkill => _triggeredSkill;

        public ActionInfo(SkillInfo skillInfo,int index,int subjectIndex,int lastTargetIndex,List<int> targetIndexList)
        {
            _index = index;
            _skillInfo = skillInfo;
            _scopeType = Master.Scope;
            _rangeType = Master.Range;
            _targetType = Master.TargetType;
            SubjectIndex.SetValue(subjectIndex);
            _lastTargetIndex = lastTargetIndex;
            _candidateTargetIndexList = targetIndexList;
        }

        public void SetRangeType(RangeType rangeType)
        {
            _rangeType = rangeType;
        }

        public void SetScopeType(ScopeType scopeType)
        {
            _scopeType = scopeType;
        }

        public void SetActionResult(List<ActionResultInfo> actionResult)
        {
            _actionResults = actionResult;
        }

        public List<ActionInfo> CheckPlusSkill()
        {
            // 行動後スキル
            var featureDates = SkillInfo.FeatureDates;
            var PlusSkill = featureDates.FindAll(a => a.FeatureType == FeatureType.PlusSkill);
            
            var actionInfos = new List<ActionInfo>();
            for (var i = 0;i < PlusSkill.Count;i++)
            {
                var skillInfo = new SkillInfo(PlusSkill[i].Param1);
                var actionInfo = new ActionInfo(skillInfo,_index,SubjectIndex.Value,-1,null);
                actionInfo.SetTriggerSkill(true);
                actionInfos.Add(actionInfo);
            }
            return actionInfos;
        }

        public List<SkillInfo> CheckPlusSkillTrigger()
        {
            var featureDates = SkillInfo.FeatureDates;
            var skillInfos = new List<SkillInfo>();
            var PlusSkillTrigger = featureDates.FindAll(a => a.FeatureType == FeatureType.PlusSkillTrigger);
            for (var i = 0;i < PlusSkillTrigger.Count;i++)
            {
                var skillInfo = new SkillInfo(PlusSkillTrigger[i].Param1);
                skillInfos.Add(skillInfo);
            }
            return skillInfos;
        }

        public void SetTriggerSkill(bool triggeredSkill)
        {
            _triggeredSkill = triggeredSkill;
        }

        public bool IsWait()
        {
            return SkillInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.AddState && (StateType)a.Param1 == StateType.Wait) != null;
        }

        public List<int> ResultTargetIndexes()
        {
            var targetIndexList = new List<int>();
            if (_actionResults == null)
            {
                return targetIndexList;
            }
            foreach (var actionResult in _actionResults)
            {
                if (actionResult.NoAnimation.Value == false)
                {
                    targetIndexList.Add(actionResult.TargetIndex.Value);
                }
            }
            return targetIndexList;
        }
    }
}