using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        // 現アクションより前の割り込み
        public ActionInfo InterruptActionInfo => _battleFlowInfo.InterruptActionInfo;
        // 誘発した行動
        public ActionInfo ReceiveActionInfo => _battleFlowInfo.ReceiveActionInfo;
        /// <summary>
        /// 誘発したアクションを登録する
        /// </summary>
        /// <param name="actionInfo"></param>
        /// <param name="indexList"></param>
        /// <param name="IsInterrupt"></param>
        public void AddReceiveActionInfo(ActionInfo actionInfo, List<int> indexList, bool IsInterrupt)
        {
            SetActionInfoParameter(actionInfo, false);
            MakeActionResultInfo(actionInfo, indexList);
            AddActionInfo(actionInfo, IsInterrupt);
            AddTurnActionInfos(actionInfo, IsInterrupt);
        }

        public void AddActionInfo(ActionInfo actionInfo, bool IsInterrupt)
        {
            _battleFlowInfo.AddActionInfo(actionInfo, IsInterrupt);
        }

        private void PopActionInfo(ActionInfo actionInfo)
        {
            _battleFlowInfo.PopActionInfo(actionInfo);
        }

        /// <summary>
        /// 行動を初期化
        /// </summary>
        public void ClearActionInfo()
        {
            _battleFlowInfo.ClearActionInfo();
        }

        // 行動を生成
        public ActionInfo MakeActionInfo(BattlerInfo subject, SkillInfo skillInfo, bool IsInterrupt, bool IsTrigger)
        {
            var skillData = skillInfo.Master;
            var targetIndexList = GetSkillTargetIndexList(skillInfo.Id.Value, subject.Index.Value, true);
            if (subject.IsState(StateType.Substitute))
            {
                int substituteId = subject.GetStateInfo(StateType.Substitute).BattlerId.Value;
                if (targetIndexList.Contains(substituteId))
                {
                    targetIndexList.Clear();
                    targetIndexList.Add(substituteId);
                }
                else
                {
                    var tempIndexList = GetSkillTargetIndexList(skillInfo.Id.Value, subject.Index.Value, false);
                    if (tempIndexList.Contains(substituteId))
                    {
                        targetIndexList.Clear();
                        targetIndexList.Add(substituteId);
                    }
                }
            }
            int lastTargetIndex = -1;
            if (subject.IsActor)
            {
                lastTargetIndex = subject.LastTargetIndex();
                if (skillData.TargetType == TargetType.Opponent)
                {
                    var targetBattler = _troop.AliveBattlerInfos.Find(a => a.Index.Value == lastTargetIndex && targetIndexList.Contains(lastTargetIndex));
                    if (targetBattler == null && _troop.BattlerInfos.Count > 0)
                    {
                        var containsOpponent = _troop.AliveBattlerInfos.Find(a => targetIndexList.Contains(a.Index.Value));
                        if (containsOpponent != null)
                        {
                            lastTargetIndex = containsOpponent.Index.Value;
                        }
                    }
                }
                else
                {
                    lastTargetIndex = subject.Index.Value;
                    if (targetIndexList.Count > 0)
                    {
                        lastTargetIndex = targetIndexList[0];
                    }
                }
            }
            var actionInfo = new ActionInfo(skillInfo, _actionIndex, subject.Index.Value, lastTargetIndex, targetIndexList);
            _actionIndex++;
            actionInfo.SetRangeType(CalcRangeType(actionInfo.Master, subject));
            var actionScopeType = CalcScopeType(subject, actionInfo);
            actionInfo.SetScopeType(actionScopeType);
            if (IsTrigger)
            {
                actionInfo.SetTriggerSkill(true);
            }
            AddTurnActionInfos(actionInfo, IsInterrupt);
            return actionInfo;
        }

        public void HitWeakPoint(int targetIndex, int skillId)
        {
            var target = GetBattlerInfo(targetIndex);
            if (!target.IsActor)
            {
                var kindType = (KindType)DataSystem.FindSkill(skillId).Attribute;
                CurrentData.PlayerInfo.AddEnemyWeakPointDict(target.EnemyData.Id, kindType);
                target.SetWeakPoint(kindType);
            }
        }
    }
}
