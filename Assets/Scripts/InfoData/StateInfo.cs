
namespace Ryneus
{
    [System.Serializable]
    public class StateInfo 
    {
        public StateData Master => DataSystem.States.Find(a => a.StateType == _stateType);
        private StateType _stateType = 0;
        public StateType StateType => _stateType;
        public ParameterInt Turns = new();
        public ParameterInt BaseTurns = new();
        public ParameterInt Effect = new();
        public ParameterInt BattlerId = new();
        public ParameterInt TargetIndex = new();
        public ParameterInt SkillId = new();
        private RemovalTiming _removeTiming = 0;
        public RemovalTiming RemovalTiming => _removeTiming;
        public void SetRemoveTiming(RemovalTiming removalTiming)
        {
            _removeTiming = removalTiming;
        }

        public bool IsStartPassive()
        {
            var skillData = DataSystem.FindSkill(SkillId.Value);
            if (skillData != null)
            {
                return skillData.FeatureDates.Find(a => a.FeatureType == FeatureType.AddState) != null && skillData.SkillType == SkillType.Passive;
            }
            return false;
        }

        public StateInfo(StateType stateType,int turns,int effect,int battlerId,int targetIndex,int skillId)
        {
            _stateType = stateType;
            Turns.SetValue(turns);
            BaseTurns.SetValue(turns);
            Effect.SetValue(effect);
            BattlerId.SetValue(battlerId);
            TargetIndex.SetValue(targetIndex);
            SkillId.SetValue(skillId);
            _removeTiming = Master.RemovalTiming;
        }

        public bool CheckOverWriteState(StateInfo stateInfo,int overLapCount = 0)
        {
            // 重複可能カウント判定
            if (stateInfo.Master.OverLap != 0)
            {
                // 重複不可
                return stateInfo.Master.OverLap <= overLapCount;
            }
            if (stateInfo.StateType == StateType.Death)
            {
                return stateInfo.StateType == _stateType;
            }
            if (stateInfo.Master.OverWrite)
            {
                return (stateInfo.StateType == _stateType) && (stateInfo.SkillId.Value == SkillId.Value);
            }
            return stateInfo.StateType == _stateType;
        }

        public bool UpdateTurn()
        {
            Turns.GainValue(-1);
            if (Turns.Value <= 0)
            {
                return true;
            }
            return false;
        }

        public void ResetTurns()
        {
            Turns.SetValue(BaseTurns.Value);
        }

        public bool CheckSameStateType(StateInfo otherStateInfo)
        {
            return _stateType == otherStateInfo.StateType && SkillId.Value == otherStateInfo.SkillId.Value && TargetIndex.Value == otherStateInfo.TargetIndex.Value;
        }

        public bool IsBuff()
        {
            return Master.Removal || !Master.Abnormal;
        }

        public bool IsDeBuff()
        {
            return !Master.Removal;
        }
    }
}