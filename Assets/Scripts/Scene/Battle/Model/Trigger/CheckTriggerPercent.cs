using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class CheckTriggerPercent : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.AttackState:
                if (battlerInfo.IsAlive() && checkTriggerInfo.ActionInfo != null && checkTriggerInfo.ActionInfo.SubjectIndex.Value == battlerInfo.Index.Value && checkTriggerInfo.ActionInfo.ActionResults.Find(a => a.HpDamage.Value > 0) != null)
                {
                    if (triggerData.Param1 > Random.Range(0,100))
                    {
                        isTrigger = true;
                    }
                }
                break;
                case TriggerType.Percent:
                var rand = Random.Range(0,100);
                if (rand <= triggerData.Param1)
                {
                    isTrigger = true;
                }
                break;
                case TriggerType.AttackStateNoFreeze:
                if (battlerInfo.IsAlive() && checkTriggerInfo.ActionInfo != null && checkTriggerInfo.ActionInfo.SubjectIndex.Value == battlerInfo.Index.Value && checkTriggerInfo.ActionInfo.ActionResults.Find(a => a.HpDamage.Value > 0) != null)
                {
                    if (checkTriggerInfo.ActionInfo.ActionResults.Count > 0)
                    {
                        if (checkTriggerInfo.ActionInfo.ActionResults.Find(a => checkTriggerInfo.GetBattlerInfo(a.TargetIndex.Value)?.GetStateInfo(StateType.Freeze) == null) != null)
                        {
                            isTrigger = true;
                        }
                    }
                }
                break;
            }
            return isTrigger;
        }

        public int CheckTargetIndex(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo,int targetBattlerIndex)
        {
            return -1;
        }

        public void AddTargetIndexList(List<int> targetIndexList,List<int> targetIndexes,BattlerInfo targetBattler,SkillData.TriggerData triggerData,SkillData skillData,CheckTriggerInfo checkTriggerInfo)
        {
            
        }
        
        public void AddTriggerTargetList(List<int> targetIndexList,SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {

        }
    }
}
