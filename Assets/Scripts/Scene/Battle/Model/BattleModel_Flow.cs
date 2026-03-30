using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        private BattleFlowInfo _battleFlowInfo = new();
        public BattlerInfo CurrentSelectBattler => _battleFlowInfo.CurrentSelectBattler;
        public void SetSelectBattlerInfo(BattlerInfo battlerInfo) => _battleFlowInfo.CurrentSelectBattler = battlerInfo;

        public ActionInfo SelectActionInfo => _battleFlowInfo.SelectActionInfo;
        public void SetSelectActionInfo(ActionInfo actionInfo) => _battleFlowInfo.SelectActionInfo = actionInfo;

        public BattlerInfo SelectTargetBattler => _battleFlowInfo.SelectTargetBattler;
        public void SetSelectTargetBattler(BattlerInfo battlerInfo) => _battleFlowInfo.SelectTargetBattler = battlerInfo;


        // ターンの最初の行動開始者
        public BattlerInfo FirstActionBattler => _battleFlowInfo.FirstActionBattler;
        public void SetFirstActionBattler(BattlerInfo firstActionBattler) => _battleFlowInfo.FirstActionBattler = firstActionBattler;

        // ターンの最初の行動開始者のアクション
        public ActionInfo FirstActionInfo => _battleFlowInfo.FirstActionInfo;
        public void SetFirstActionInfo(ActionInfo actionInfo) => _battleFlowInfo.FirstActionInfo = actionInfo;


        // 現在の行動
        public ActionInfo ActiveActionInfo => _battleFlowInfo.ActiveActionInfo;
        public void SetActiveActionInfo(ActionInfo actionInfo) => _battleFlowInfo.ActiveActionInfo = actionInfo;

        public BattlerInfo CheckApCurrentBattler()
        {
            var battlerInfos = FieldBattlerInfos().FindAll(a => a.IsAlive());
            battlerInfos.Sort((a, b) => (int)a.Ap.Value - (int)b.Ap.Value);
            SetSelectBattlerInfo(battlerInfos.Find(a => a.Ap.Value <= 0));
            return _battleFlowInfo.CurrentSelectBattler;
        }

        /// <summary>
        /// battlerInfoが魔法をoneTargetIndexに使用した時の対象を取得
        /// </summary>
        /// <param name="battlerInfo"></param>
        /// <param name="skillId"></param>
        /// <param name="oneTargetIndex"></param>
        /// <returns></returns>
        public (ActionInfo, List<int>) GetActionInfoTargetIndexes(BattlerInfo battlerInfo, int skillId, int oneTargetIndex = -1)
        {
            var skillInfo = battlerInfo.Skills.Find(a => a.Id.Value == skillId);
            if (skillInfo == null)
            {
                skillInfo = new SkillInfo(skillId);
            }
            var actionInfo = MakeActionInfo(battlerInfo, skillInfo, false, false);
            //AddActionInfo(actionInfo,false);
            // 対象を自動決定
            return (actionInfo, MakeAutoSelectIndex(actionInfo, oneTargetIndex));
        }

        /// <summary>
        /// ActionInfoの要素を決定する
        /// </summary>
        /// <param name="actionInfo"></param>
        public void SetActionInfoParameter(ActionInfo actionInfo, bool startAction)
        {
            if (actionInfo.IsSetParameter.Value)
            {
                return;
            }
            var subject = GetBattlerInfo(actionInfo.SubjectIndex.Value);
            int mpCost = CalcMpCost(subject, actionInfo.Master.MpCost);
            actionInfo.MpCost.SetValue(mpCost);
            int hpCost = CalcHpCost(actionInfo);
            actionInfo.HpCost.SetValue(hpCost);

            //var isPrism = PrismRepeatTime(subject,actionInfo) > 0;
            var repeatTime = CalcRepeatTime(subject, actionInfo);
            //repeatTime += PrismRepeatTime(subject,actionInfo);
            actionInfo.SetRepeatTime(repeatTime);
            actionInfo.BaseRepeatTime.SetValue(repeatTime);
            actionInfo.StartAction.SetValue(startAction);
            actionInfo.IsSetParameter.SetValue(true);
        }
    }
}
