using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerSelfState : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData, BattlerInfo battlerInfo, CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.SelfActionInfo:
                    isTrigger = CheckSelfActionInfo(triggerData, battlerInfo, checkTriggerInfo).Count > 0;
                    break;
                case TriggerType.MastarySkill:
                    isTrigger = CheckMastarySkill(triggerData, battlerInfo, checkTriggerInfo).Count > 0;
                    break;
            }
            return isTrigger;
        }


        public int CheckTargetIndex(SkillData.TriggerData triggerData, BattlerInfo battlerInfo, CheckTriggerInfo checkTriggerInfo, int targetBattlerIndex)
        {
            return -1;
        }

        public void AddTargetIndexList(List<int> targetIndexList, List<int> targetIndexes, BattlerInfo targetBattler, SkillData.TriggerData triggerData, SkillData skillData, CheckTriggerInfo checkTriggerInfo)
        {


        }

        public void AddTriggerTargetList(List<int> targetIndexList, SkillData.TriggerData triggerData, BattlerInfo battlerInfo, CheckTriggerInfo checkTriggerInfo)
        {
            switch (triggerData.TriggerType)
            {
                case TriggerType.SelfActionInfo:
                    targetIndexList.AddRange(CheckSelfActionInfo(triggerData, battlerInfo, checkTriggerInfo));
                    break;
                case TriggerType.MastarySkill:
                    targetIndexList.AddRange(CheckMastarySkill(triggerData, battlerInfo, checkTriggerInfo));
                    break;
            }
        }

        private List<int> CheckSelfActionInfo(SkillData.TriggerData triggerData, BattlerInfo battlerInfo, CheckTriggerInfo checkTriggerInfo)
        {
            var list = new List<int>();
            var actionInfo = checkTriggerInfo.ActionInfo;
            if (!battlerInfo.IsAlive())
            {
                return list;
            }
            if (actionInfo == null)
            {
                return list;
            }
            
            if (actionInfo.SubjectIndex.Value == battlerInfo.Index.Value)
            {
                list.Add(actionInfo.SubjectIndex.Value);
            }
            return list;
        }

        private List<int> CheckMastarySkill(SkillData.TriggerData triggerData, BattlerInfo battlerInfo, CheckTriggerInfo checkTriggerInfo)
        {
            var list = new List<int>();
            var actionInfo = checkTriggerInfo.ActionInfo;
            if (!battlerInfo.IsAlive())
            {
                return list;
            }
            if (actionInfo == null || battlerInfo.ActorInfo == null)
            {
                return list;
            }
            
            if (battlerInfo.ActorInfo.MastarySkillIds.Contains(actionInfo.Master.Id))
            {
                list.Add(actionInfo.SubjectIndex.Value);
            }
            return list;
        }
    }
}
