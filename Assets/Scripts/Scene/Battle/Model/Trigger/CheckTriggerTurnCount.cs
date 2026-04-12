using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerTurnCount : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData, BattlerInfo battlerInfo, CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.TurnNumUnder:
                    return battlerInfo.TurnCount.Value < triggerData.Param1;
                case TriggerType.TurnNum:
                    return CheckTurnNum(triggerData, battlerInfo).Count > 0;
                case TriggerType.TurnNumPer:
                    return CheckTurnNumPer(triggerData, battlerInfo).Count > 0;
                case TriggerType.ActionCountPer:
                    var turns = checkTriggerInfo.Turns;
                    if (triggerData.Param1 > 0)
                    {
                        return (turns % triggerData.Param1) - triggerData.Param2 == 0;
                    }
                    else
                    {
                        return turns - triggerData.Param2 == 0;
                    }
                case TriggerType.SelfTargetOnly:
                    return battlerInfo.IsAlive();
                case TriggerType.SelfTargetNotOnly:
                    /*
                        if (checkTriggerInfo.ActionInfo != null)
                        {
                            if (checkTriggerInfo.ActionResultInfos != null)
                            {
                                return checkTriggerInfo.ActionResultInfos.Find(a => a.TargetIndex != battlerInfo.Index) != null;
                            }
                        }
                        return false;
                        */

                    return checkTriggerInfo.Friends.Count > 1;
                case TriggerType.ActionInfoTurnNumPer:
                    if (checkTriggerInfo.ActionInfo != null)
                    {
                        var actionBattlerInfo = checkTriggerInfo.GetBattlerInfo(checkTriggerInfo.ActionInfo.SubjectIndex.Value);
                        if (triggerData.Param1 == 0)
                        {
                            if (actionBattlerInfo != null && actionBattlerInfo.TurnCount.Value - triggerData.Param2 == 0)
                            {
                                isTrigger = true;
                            }
                        }
                        else
                        {
                            if ((actionBattlerInfo.TurnCount.Value % triggerData.Param1) - triggerData.Param2 == 0)
                            {
                                isTrigger = true;
                            }
                        }
                    }
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
            var IsFriend = checkTriggerInfo.IsFriend(targetBattler);
            var targetIndex = targetBattler.Index;


            switch (triggerData.TriggerType)
            {
                case TriggerType.TurnNum:
                    targetIndexList.AddRange(CheckTurnNum(triggerData, targetBattler));
                    break;
                case TriggerType.TurnNumPer:
                    targetIndexList.AddRange(CheckTurnNumPer(triggerData, targetBattler));
                    break;
                case TriggerType.SelfTargetOnly:
                    if (checkTriggerInfo.BattlerInfo.Index == targetIndex)
                    {
                        targetIndexList.Add(targetIndex.Value);
                    }
                    break;
                case TriggerType.SelfTargetNotOnly:
                    if (checkTriggerInfo.BattlerInfo.Index != targetIndex)
                    {
                        targetIndexList.Add(targetIndex.Value);
                    }
                    break;
            }
        }

        public void AddTriggerTargetList(List<int> targetIndexList, SkillData.TriggerData triggerData, BattlerInfo battlerInfo, CheckTriggerInfo checkTriggerInfo)
        {

        }

        
        private List<int> CheckTurnNum(SkillData.TriggerData triggerData, BattlerInfo battlerInfo)
        {
            var list = new List<int>();
            if (battlerInfo.TurnCount.Value == triggerData.Param1)
            {
                list.Add(battlerInfo.Index.Value);
            }
            return list;
        }

        private List<int> CheckTurnNumPer(SkillData.TriggerData triggerData, BattlerInfo battlerInfo)
        {
            var list = new List<int>();
            if (triggerData.Param1 == 0)
            {
                if (battlerInfo.TurnCount.Value - triggerData.Param2 == 0)
                {
                    list.Add(battlerInfo.Index.Value);
                }
            }
            else
            {
                if ((battlerInfo.TurnCount.Value % triggerData.Param1) - triggerData.Param2 == 0)
                {
                    list.Add(battlerInfo.Index.Value);
                }
            }
            return list;
        }
    }
}
