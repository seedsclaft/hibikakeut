using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerAwaken : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            if (!battlerInfo.IsAlive())
            {
                return isTrigger;
            }
            switch (triggerData.TriggerType)
            {
                case TriggerType.IsNotAwaken:
                    return !battlerInfo.IsAwaken;
                case TriggerType.IsAwaken:
                    return battlerInfo.IsAwaken;
                case TriggerType.FriendIsNotAwaken:
                    if (checkTriggerInfo.Friends.Find(a => !a.IsAwaken) != null)
                    {
                        isTrigger = true;
                    }
                    if (triggerData.Param2 == 1)
                    {
                        isTrigger = checkTriggerInfo.Friends.Count > 0;
                    }
                    break;
                case TriggerType.FriendIsAwaken:
                    if (checkTriggerInfo.Friends.Find(a => a.IsAwaken) != null)
                    {
                        isTrigger = true;
                    }
                    if (triggerData.Param2 == 1)
                    {
                        isTrigger = checkTriggerInfo.Friends.Count > 0;
                    }
                    break;
                case TriggerType.OpponentIsNotAwaken:
                    if (checkTriggerInfo.Opponents.Find(a => !a.IsAwaken) != null)
                    {
                        isTrigger = true;
                    }
                    if (triggerData.Param2 == 1)
                    {
                        isTrigger = checkTriggerInfo.Friends.Count > 0;
                    }
                    break;
                case TriggerType.OpponentIsAwaken:
                    if (checkTriggerInfo.Opponents.Find(a => a.IsAwaken) != null)
                    {
                        isTrigger = true;
                    }
                    if (triggerData.Param2 == 1)
                    {
                        isTrigger = checkTriggerInfo.Friends.Count > 0;
                    }
                    break;
                case TriggerType.AwakenCountOver:
                    var count = 0;
                    foreach (var opponent in checkTriggerInfo.AliveBattlerInfos(false))
                    {
                        var over = opponent.Skills.FindAll(a => a.Master.SkillType == SkillType.Awaken && a.UseCount.Value > 0);
                        count += over.Count;
                    }
                    foreach (var opponent in checkTriggerInfo.AliveBattlerInfos(true))
                    {
                        var over = opponent.Skills.FindAll(a => a.Master.SkillType == SkillType.Awaken && a.UseCount.Value > 0);
                        count += over.Count;
                    }
                    isTrigger = count >= triggerData.Param2;
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
            var IsFriend = checkTriggerInfo.IsFriend(targetBattler);
            var targetIndex = targetBattler.Index;
            
            switch (triggerData.TriggerType)
            {
                case TriggerType.IsNotAwaken:
                    if (checkTriggerInfo.BattlerInfo.Index == targetIndex && !targetBattler.IsAwaken)
                    {
                        targetIndexList.Add(targetIndex.Value);
                    }
                    break;
                case TriggerType.IsAwaken:
                    if (checkTriggerInfo.BattlerInfo.Index == targetIndex && targetBattler.IsAwaken)
                    {
                        targetIndexList.Add(targetIndex.Value);
                    }
                    break;
                case TriggerType.FriendIsNotAwaken:
                    if (IsFriend && !targetBattler.IsAwaken)
                    {
                        targetIndexList.Add(targetIndex.Value);
                    }
                    break;
                case TriggerType.FriendIsAwaken:
                    if (IsFriend && targetBattler.IsAwaken)
                    {
                        targetIndexList.Add(targetIndex.Value);
                    }
                    break;
                case TriggerType.OpponentIsNotAwaken:
                    if (!IsFriend && !targetBattler.IsAwaken)
                    {
                        targetIndexList.Add(targetIndex.Value);
                    }
                    break;
                case TriggerType.OpponentIsAwaken:
                    if (!IsFriend && targetBattler.IsAwaken)
                    {
                        targetIndexList.Add(targetIndex.Value);
                    }
                    break;
                case TriggerType.AwakenCountOver:
                    targetIndexList.Add(targetIndex.Value);
                    break;
            }
        }

        public void AddTriggerTargetList(List<int> targetIndexList,SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            switch (triggerData.TriggerType)
            {
                case TriggerType.FriendIsAwaken:
                    targetIndexList.AddRange(CheckFriendIsAwaken(triggerData,battlerInfo,checkTriggerInfo));
                    break;
            }
        }

        private List<int> CheckFriendIsAwaken(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var list = new List<int>();
            if (battlerInfo.IsAlive())
            {
                foreach (var targetBattler in checkTriggerInfo.Friends)
                {
                    if (targetBattler.IsAwaken)
                    {
                        list.Add(targetBattler.Index.Value);
                    }
                }
            }
            return list;
        }
    }
}
