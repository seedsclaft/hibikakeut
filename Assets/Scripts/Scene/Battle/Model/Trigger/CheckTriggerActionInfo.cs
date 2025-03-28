using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerActionInfo : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var actionInfo = checkTriggerInfo.ActionInfo;
            var actionResultInfos = checkTriggerInfo.ActionResultInfos;
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.TargetHpRateUnder:
                    isTrigger = CheckTargetHpRateUnder(triggerData,battlerInfo,checkTriggerInfo).Count > 0;
                    break;
                case TriggerType.TargetAbnormal:
                    isTrigger = CheckTargetAbnormal(triggerData,battlerInfo,checkTriggerInfo).Count > 0;
                    break;
                case TriggerType.TargetBuff:
                    isTrigger = CheckTargetBuff(triggerData,battlerInfo,checkTriggerInfo).Count > 0;
                    break;
                case TriggerType.TargetDeath:
                    if (battlerInfo.IsAlive() && actionResultInfos != null && actionResultInfos.Count > 0)
                    {
                        foreach (var actionResultInfo in actionResultInfos)
                        {
                            if (actionResultInfo.TargetIndex != actionResultInfo.SubjectIndex)
                            {
                                var targetBattlerInfo = checkTriggerInfo.GetBattlerInfo(actionResultInfo.TargetIndex.Value);
                                if (targetBattlerInfo != null && battlerInfo.IsActor == targetBattlerInfo.IsActor && actionResultInfo.DeadIndexList.Contains(targetBattlerInfo.Index.Value))
                                {
                                    isTrigger = true;
                                }               
                            }
                        }
                    }
                    break;
                case TriggerType.OneAttackOverDamage:
                    if (battlerInfo.IsAlive() && battlerInfo.Examine.MaxDamage.Value >= triggerData.Param1)
                    {
                        isTrigger = true;
                    }
                    break;
                case TriggerType.FriendAttackedAction:
                    isTrigger = CheckFriendAttackedAction(triggerData,battlerInfo,checkTriggerInfo).Count > 0;
                    break;
                case TriggerType.SelfAbnormalAction:
                    if (battlerInfo.IsAlive())
                    {
                        if (actionInfo != null && actionInfo.ActionResults != null && actionInfo.Master.IsAddAbnormalFeature())
                        {
                            // 自分から自分には含まない
                            var results = actionInfo.ActionResults.FindAll(a => a.AddedStates.Find(b => b.Master.Abnormal) != null && a.TargetIndex.Value == battlerInfo.Index.Value && a.SubjectIndex.Value != battlerInfo.Index.Value);
                            if (results.Count > 0)
                            {
                                isTrigger = true;
                            }
                        }
                    }
                    break;
                case TriggerType.FriendAbnormalAction:
                    isTrigger = CheckFriendAbnormalAction(triggerData,battlerInfo,checkTriggerInfo).Count > 0;
                    break;
                case TriggerType.SelfPassiveAction:
                    if (battlerInfo.IsAlive())
                    {
                        if (actionInfo != null && actionInfo.Master.SkillType == SkillType.Passive && actionInfo.ActionResults != null && actionInfo.TriggeredSkill)
                        {
                            // 自分で発動したパッシブは除く
                            var results = actionInfo.ActionResults.FindAll(a => a.TargetIndex.Value == battlerInfo.Index.Value && a.SubjectIndex.Value != battlerInfo.Index.Value);
                            if (results.Count > 0)
                            {
                                isTrigger = true;
                            }
                        }
                    }
                    break;
                case TriggerType.FriendAction:
                    if (battlerInfo.IsAlive())
                    {
                        if (actionInfo != null && actionInfo.ActionResults != null && actionInfo.Master.IsHpDamageFeature())
                        {
                            var subject = checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex.Value);
                            if (subject != null && battlerInfo.IsActor == subject.IsActor && battlerInfo.Index.Value != actionInfo.SubjectIndex.Value)
                            {
                                var success = actionInfo.ActionResults.FindAll(a => !a.Missed).Count > 0;
                                if (success)
                                {
                                    isTrigger = true;
                                }
                            }
                        }
                    }
                    break;
                case TriggerType.FriendAttackAction:
                    if (battlerInfo.IsAlive())
                    {
                        if (actionInfo != null && actionInfo.TriggeredSkill == false && actionInfo.ActionResults != null && actionInfo.Master.IsHpDamageFeature())
                        {
                            var subject = checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex.Value);
                            if (subject != null && battlerInfo.IsActor == subject.IsActor && battlerInfo.Index.Value != actionInfo.SubjectIndex.Value)
                            {
                                var results = actionInfo.ActionResults.FindAll(a => a.HpDamage.Value > 0);
                                if (results.Count > 0)
                                {
                                    isTrigger = true;
                                }
                            }
                        }
                    }
                    break;
                case TriggerType.OpponentHealAction:
                    if (battlerInfo.IsAlive())
                    {
                        if (actionInfo != null && actionInfo.ActionResults != null && actionInfo.Master.IsHpHealFeature())
                        {
                            var subject = checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex.Value);
                            if (subject != null && battlerInfo.IsActor != subject.IsActor && battlerInfo.Index.Value != actionInfo.SubjectIndex.Value)
                            {
                                var results = actionInfo.ActionResults.FindAll(a => a.HpHeal.Value > 0);
                                if (results.Count > 0)
                                {
                                    isTrigger = true;
                                }
                            }
                        }
                    }
                    break;
                case TriggerType.OpponentDamageShieldAction:
                    if (battlerInfo.IsAlive())
                    {
                        if (actionInfo != null && actionInfo.ActionResults != null)
                        {
                            var subject = checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex.Value);
                            if (subject != null && battlerInfo.IsActor == subject.IsActor && battlerInfo.Index.Value != actionInfo.SubjectIndex.Value)
                            {
                                foreach (var actionResultInfo in actionInfo.ActionResults)
                                {
                                    foreach (var execStateInfo in actionResultInfo.ExecStateInfos)
                                    {
                                        if (execStateInfo.Key == actionResultInfo.TargetIndex.Value)
                                        {
                                            if (execStateInfo.Value.Find(a => a.StateType == StateType.NoDamage) != null)
                                            {
                                                isTrigger = true;
                                            }
                                        }
                                    }
                                }
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
            switch (triggerData.TriggerType)
            {
                case TriggerType.TargetHpRateUnder:
                    targetIndexList.AddRange(CheckTargetHpRateUnder(triggerData,battlerInfo,checkTriggerInfo));
                    break;
                case TriggerType.TargetAbnormal:
                    targetIndexList.AddRange(CheckTargetAbnormal(triggerData,battlerInfo,checkTriggerInfo));
                    break;
                case TriggerType.TargetBuff:
                    targetIndexList.AddRange(CheckTargetBuff(triggerData,battlerInfo,checkTriggerInfo));
                    break;
                case TriggerType.FriendAttackedAction:
                    var selectIndex = CheckFriendAttackedAction(triggerData,battlerInfo,checkTriggerInfo);
                    targetIndexList.AddRange(selectIndex);
                    break;
            }
        }

        private List<int> CheckTargetHpRateUnder(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var list = new List<int>();
            var actionInfo = checkTriggerInfo.ActionInfo;
            var actionResultInfos = checkTriggerInfo.ActionResultInfos;
            if (!battlerInfo.IsAlive() && triggerData.Param2 == 0)
            {
                return list;
            }
            if (actionResultInfos == null || actionResultInfos.Count == 0)
            {
                return list;
            }
            var results = actionResultInfos.FindAll(a => a.HpDamage.Value > 0 && a.TargetIndex != a.SubjectIndex);
            foreach (var result in results)
            {
                var targetBattlerInfo = checkTriggerInfo.GetBattlerInfo(result.TargetIndex.Value);
                if (targetBattlerInfo != null && battlerInfo.IsActor == targetBattlerInfo.IsActor)
                {
                    var targetHp = 0f;
                    if (targetBattlerInfo.Hp.Value != 0)
                    {
                        targetHp = targetBattlerInfo.Hp.Value / (float)targetBattlerInfo.MaxHp;
                    }
                    if (targetHp <= triggerData.Param1 * 0.01f)
                    {
                        list.Add(targetBattlerInfo.Index.Value);
                    }
                }
            }
            return list;
        }

        private List<int> CheckTargetAbnormal(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var list = new List<int>();
            var actionInfo = checkTriggerInfo.ActionInfo;
            var actionResultInfos = checkTriggerInfo.ActionResultInfos;
            if (!battlerInfo.IsAlive())
            {
                return list;
            }
            if (actionResultInfos == null || actionResultInfos.Count == 0)
            {
                return list;
            }
            var results = actionResultInfos.FindAll(a => a.TargetIndex != a.SubjectIndex);
            foreach (var result in results)
            {
                var targetBattlerInfo = checkTriggerInfo.GetBattlerInfo(result.TargetIndex.Value);
                if (targetBattlerInfo != null && battlerInfo.IsActor == targetBattlerInfo.IsActor)
                {
                    var abnormalStates = result.AddedStates.FindAll(a => a.Master.Abnormal);
                    var abnormalTarget = abnormalStates.Find(a => a.TargetIndex.Value == targetBattlerInfo.Index.Value) != null;
                    if (abnormalTarget)
                    {
                        list.Add(targetBattlerInfo.Index.Value);
                    }               
                }
            }
            return list;
        }

        private List<int> CheckTargetBuff(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var list = new List<int>();
            var actionInfo = checkTriggerInfo.ActionInfo;
            var actionResultInfos = checkTriggerInfo.ActionResultInfos;
            if (!battlerInfo.IsAlive())
            {
                return list;
            }
            if (actionResultInfos == null || actionResultInfos.Count == 0)
            {
                return list;
            }
            var results = actionResultInfos.FindAll(a => a.TargetIndex != a.SubjectIndex);
            foreach (var result in results)
            {
                var targetBattlerInfo = checkTriggerInfo.GetBattlerInfo(result.TargetIndex.Value);
                if (targetBattlerInfo != null && battlerInfo.IsActor != targetBattlerInfo.IsActor)
                {
                    var buffStates = result.AddedStates.FindAll(a => a.Master.Buff);
                    var buffTarget = buffStates.Find(a => a.TargetIndex.Value == targetBattlerInfo.Index.Value) != null;
                    if (buffTarget)
                    {
                        list.Add(targetBattlerInfo.Index.Value);
                    }               
                }
            }
            return list;
        }

        private List<int> CheckFriendAttackedAction(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var list = new List<int>();
            var actionInfo = checkTriggerInfo.ActionInfo;
            var actionResultInfos = checkTriggerInfo.ActionResultInfos;
            if (!battlerInfo.IsAlive())
            {
                return list;
            }
            if (actionInfo == null || actionInfo.ActionResults == null)
            {
                return list;
            }
            if (!actionInfo.Master.IsHpDamageFeature())
            {
                return list;
            }
            var results = actionInfo.ActionResults.FindAll(a => checkTriggerInfo.Friends.Find(b => b.Index.Value == a.TargetIndex.Value) != null);
            foreach (var result in results)
            {
                var targetBattlerInfo = checkTriggerInfo.GetBattlerInfo(result.TargetIndex.Value);
                if (targetBattlerInfo != null && !targetBattlerInfo.IsActor == battlerInfo.IsActor)
                {
                    continue;
                }
                // Param2で対象を指定
                if ((ScopeType)triggerData.Param2 == ScopeType.Self)
                {
                    if (result.TargetIndex.Value == battlerInfo.Index.Value)
                    {
                        list.Add(result.TargetIndex.Value);
                    }
                } else
                if ((ScopeType)triggerData.Param2 == ScopeType.WithoutSelfAll)
                {
                    if (result.TargetIndex.Value != battlerInfo.Index.Value)
                    {
                        list.Add(result.TargetIndex.Value);
                    }
                } else
                {
                    list.Add(result.TargetIndex.Value);
                }
            }
            return list;
        }

        private List<int> CheckFriendAbnormalAction(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var list = new List<int>();
            var actionInfo = checkTriggerInfo.ActionInfo;
            var actionResultInfos = checkTriggerInfo.ActionResultInfos;
            if (!battlerInfo.IsAlive())
            {
                return list;
            }
            if (actionInfo == null || actionInfo.ActionResults == null)
            {
                return list;
            }
            if (!actionInfo.Master.IsAddAbnormalFeature())
            {
                return list;
            }
            var results = actionInfo.ActionResults.FindAll(a => checkTriggerInfo.Friends.Find(b => b.Index.Value == a.TargetIndex.Value) != null);
            foreach (var result in results)
            {
                var targetBattlerInfo = checkTriggerInfo.GetBattlerInfo(result.TargetIndex.Value);
                if (targetBattlerInfo != null && !targetBattlerInfo.IsActor == battlerInfo.IsActor)
                {
                    continue;
                }
                // Param2で対象を指定
                if ((ScopeType)triggerData.Param2 == ScopeType.Self)
                {
                    if (result.TargetIndex.Value == battlerInfo.Index.Value)
                    {
                        list.Add(result.TargetIndex.Value);
                    }
                } else
                if ((ScopeType)triggerData.Param2 == ScopeType.WithoutSelfAll)
                {
                    if (result.TargetIndex.Value != battlerInfo.Index.Value)
                    {
                        list.Add(result.TargetIndex.Value);
                    }
                } else
                {
                    list.Add(result.TargetIndex.Value);
                }
            }
            return list;
        }
    }
}
