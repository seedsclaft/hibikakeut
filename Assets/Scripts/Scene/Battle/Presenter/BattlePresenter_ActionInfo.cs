using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace Ryneus
{
    public partial class BattlePresenter : BasePresenter
    {
        // ActionInfoのアニメーション開始～スリップダメージ～リジェネ回復～Animation終了までを管理
        // この間はActionInfoは変わらない

        /// <summary>
        /// ActionInfoのアニメーション開始
        /// </summary>
        /// <param name="actionInfo"></param>
        public void StartActionInfoAnimation(ActionInfo actionInfo)
        {
            if (_skipBattle/* || actionInfo.Master.IsDisplayStartBattle()*/)
            {
                CommandEndAnimation();
                return;
            }
            StartAnimation(actionInfo);
        }

        private void StartWaitCommand(ActionInfo actionInfo)
        {
            _model.WaitCommand(actionInfo);
            CommandEndAnimation();
        }

        private async void StartAnimation(ActionInfo actionInfo)
        {
            if (actionInfo.Master.AnimationId > 0 && actionInfo.Master.Id > 1000)
            {
                if (actionInfo.Master.SkillType == SkillType.Unique)
                {
                    _ = await StartAnimationMessiah(actionInfo);
                }
                else
                if (actionInfo.Master.SkillType == SkillType.Awaken)
                {
                    _ = await StartAnimationAwaken(actionInfo);
                }
            }
            StartAnimationSkill(actionInfo);
        }

        /// <summary>
        /// 覚醒アニメーション再生してからアニメーション再生
        /// </summary>
        private async UniTask<bool> StartAnimationMessiah(ActionInfo actionInfo)
        {
            var subject = _model.GetBattlerInfo(actionInfo.SubjectIndex.Value);
            await _view.StartAnimationMessiah(subject);
            return true;
        }

        /// <summary>
        /// カットインアニメーション再生してからアニメーション再生
        /// </summary>
        private async UniTask<bool> StartAnimationAwaken(ActionInfo actionInfo)
        {
            await _view.StartAnimationDemigod(_model.GetBattlerInfo(actionInfo.SubjectIndex.Value), actionInfo.Master);
            return true;
        }

        private async void StartAnimationSkill(ActionInfo actionInfo)
        {
            _view.ChangeSideMenuButtonActive(false);
            _view.SetBattlerActiveStatus(actionInfo.ResultTargetIndexes());
            //_view.ShowEnemyStateOverlay();
            _view.HideStateOverlay();
            _view.AnimationBusy.SetValue(true);
            if (actionInfo.ActionResults.Count == 0)
            {
                CommandEndAnimation();
                return;
            }

            var subject = _model.GetBattlerInfo(actionInfo.SubjectIndex.Value);
            if (actionInfo.FirstAttack() && !actionInfo.SkillInfo.ActionAfterChange() && !actionInfo.SkillInfo.ActionWaitCommand())
            {
                _view.HideGridLayer();
                await SelfAnimation(actionInfo);
                await _view.SetStartActorMagic(subject.Index.Value, subject.IsActor, actionInfo.Master.Attribute);
            }

            _ = await ShowCutinBattleThumb(actionInfo);

            if (!GameSystem.OptionData.BattlePassiveAnimationSkip)
            {
                _view.SetCurrentSkillData(actionInfo.SkillInfo, subject);
            }

            StartAliveAnimation(actionInfo.ActionResults);
            var animationData = BattleUtility.AnimationData(actionInfo.Master.AnimationId);
            if (animationData != null && animationData.AnimationPath != "" && !GameSystem.OptionData.BattleAnimationSkip && actionInfo.Master.IsDisplayBattleSkill())
            {
                var targetIndexList = actionInfo.ResultTargetIndexes();
                PlayAnimation(animationData, actionInfo.Master.AnimationType, targetIndexList, false);
                await UniTask.DelayFrame((int)(animationData.DamageTiming / GameSystem.OptionData.BattleSpeed));
                foreach (var actionResultInfo in actionInfo.ActionResults)
                {
                    PopupActionResult(actionResultInfo, actionResultInfo.TargetIndex.Value, true, true);
                }
                var waitFrame = _model.WaitFrameTime(60);
                if (!actionInfo.LastAttack() && waitFrame > 1)
                {
                    waitFrame = 30;
                }
                await UniTask.DelayFrame(waitFrame);
            }
            else
            {
                foreach (var actionResultInfo in actionInfo.ActionResults)
                {
                    PopupActionResult(actionResultInfo, actionResultInfo.TargetIndex.Value, true, true);
                }
                if (GameSystem.OptionData.BattlePassiveAnimationSkip && actionInfo.Master.IsBattlePassiveSkill())
                {
                    CommandEndAnimation();
                    return;
                }
                var waitFrame = _model.WaitFrameTime(48);
                if (!actionInfo.LastAttack() && waitFrame > 1)
                {
                    waitFrame = 24;
                }
                await UniTask.DelayFrame(waitFrame);
            }
            CommandEndAnimation();
        }

        private void PlayAnimation(AnimationData animationData, AnimationType animationType, List<int> targetIndexList, bool isCurse = false)
        {
            var animation = _model.EffectAssets[animationData.AnimationPath];
            _view.ClearDamagePopup();
            // 全体エフェクト
            if (animationType == AnimationType.All)
            {
                _view.StartAnimationAll(animation, animationData.Position, animationData.Scale, animationData.Speed);
                return;
            }
            // 個別エフェクト
            foreach (var targetIndex in targetIndexList)
            {
                var oneAnimation = isCurse ? _model.EffectAssets["NA_Effekseer/NA_curse_001"] : animation;
                _view.StartAnimation(targetIndex, oneAnimation, animationData.Position, animationData.Scale, animationData.Speed, targetIndex == targetIndexList[0]);
            }
        }

        /// <summary>
        /// 行動結果のポップアップ表示
        /// </summary>
        private void PopupActionResult(ActionResultInfo actionResultInfo, int targetIndex, bool needDamageBlink = true, bool needPopupDelay = true)
        {
            if (actionResultInfo.TargetIndex.Value != targetIndex)
            {
                return;
            }
            if (actionResultInfo.Missed)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Miss);
                _view.StartStatePopup(targetIndex, DamageType.State, "Miss!");
            }
            if (actionResultInfo.HpDamage.Value > 0)
            {
                _model.GainAttackedCount(actionResultInfo.TargetIndex.Value);
                _model.GainMaxDamage(actionResultInfo.TargetIndex.Value, actionResultInfo.HpDamage.Value);
                if (actionResultInfo.Critical)
                {
                    _model.GainBeCriticalCount(actionResultInfo.TargetIndex.Value);
                }
                var damageType = actionResultInfo.Critical || actionResultInfo.WeakPoint ? DamageType.HpCritical : DamageType.HpDamage;
                _view.StartDamage(targetIndex, damageType, actionResultInfo.HpDamage.Value, needPopupDelay);
                _view.SetDamageAnimation(targetIndex, actionResultInfo.TargetIndex.Value < 100);
                if (needDamageBlink)
                {
                    _view.StartBlink(targetIndex);
                    PlayDamageSound(damageType);
                }
            }
            if (actionResultInfo.WeakPoint)
            {
                _model.HitWeakPoint(actionResultInfo.TargetIndex.Value, actionResultInfo.SkillId.Value);
            }
            if (actionResultInfo.HpHeal.Value > 0)
            {
                if (!actionResultInfo.DeadIndexList.Contains(targetIndex))
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Heal);
                    _view.StartHeal(targetIndex, DamageType.HpHeal, actionResultInfo.HpHeal.Value, needPopupDelay);
                }
            }
            if (actionResultInfo.CtDamage.Value > 0 || actionResultInfo.PassiveCtDamage.Value > 0)
            {
                _view.StartDamage(targetIndex, DamageType.MpDamage, actionResultInfo.CtDamage.Value + actionResultInfo.PassiveCtDamage.Value);
            }
            if (actionResultInfo.CtHeal.Value > 0)
            {
                _view.StartHeal(targetIndex, DamageType.MpHeal, actionResultInfo.CtHeal.Value);
            }
            if (actionResultInfo.ApHeal.Value > 0)
            {
                _view.StartStatePopup(targetIndex, DamageType.State, DataSystem.GetReplaceText(16200, actionResultInfo.ApHeal.Value.ToString()));
            }
            if (actionResultInfo.ApDamage.Value > 0)
            {
                _view.StartStatePopup(targetIndex, DamageType.State, DataSystem.GetReplaceText(16210, actionResultInfo.ApDamage.Value.ToString()));
            }
            if (actionResultInfo.ReDamage.Value > 0 || actionResultInfo.CurseDamage.Value > 0)
            {
                var reDamage = 0;
                //if (!actionResultInfo.DeadIndexList.Contains(targetIndex) && _model.GetBattlerInfo(targetIndex).IsAlive())
                //{
                    reDamage += actionResultInfo.ReDamage.Value;
                //}
                reDamage += actionResultInfo.CurseDamage.Value;
                if (reDamage > 0)
                {
                    var damageType = actionResultInfo.Critical || actionResultInfo.WeakPoint ? DamageType.HpCritical : DamageType.HpDamage;
                    PlayDamageSound(damageType);
                    _view.StartDamage(actionResultInfo.SubjectIndex.Value, damageType, reDamage);
                    _view.StartBlink(actionResultInfo.SubjectIndex.Value);
                }
            }
            if (actionResultInfo.ReHeal.Value > 0)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Heal);
                _view.StartHeal(actionResultInfo.SubjectIndex.Value, DamageType.HpHeal, actionResultInfo.ReHeal.Value);
            }
            foreach (var addedState in actionResultInfo.AddedStates)
            {
                if (addedState.IsBuff())
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Buff);
                }
                else
                if (addedState.IsDeBuff())
                {
                    SoundManager.Instance.PlayStaticSe(SEType.DeBuff);
                }
                _view.StartStatePopup(addedState.TargetIndex.Value, DamageType.State, "+" + addedState.Master.Name, addedState.IsBuff(), addedState.IsDeBuff());
            }
            foreach (var removedState in actionResultInfo.RemovedStates)
            {
                _view.StartStatePopup(removedState.TargetIndex.Value, DamageType.State, "-" + removedState.Master.Name, removedState.IsBuff(), removedState.IsDeBuff());
            }
            foreach (var displayState in actionResultInfo.DisplayStates)
            {
                _view.StartStatePopup(displayState.TargetIndex.Value, DamageType.State, displayState.Master.Name, displayState.IsBuff(), displayState.IsDeBuff());
            }
            foreach (var displayUpperState in actionResultInfo.DisplayUpperStates)
            {
                _view.StartStatePopup(displayUpperState.TargetIndex.Value, DamageType.State, displayUpperState.Master.Name + DataSystem.GetText(16230), displayUpperState.IsBuff(), displayUpperState.IsDeBuff());
            }
            if (actionResultInfo.StartDash)
            {
                //先制攻撃
                _view.StartStatePopup(targetIndex, DamageType.State, DataSystem.GetText(16220));
            }
        }

        private async Task SelfAnimation(ActionInfo actionInfo)
        {
            var selfAnimation = _model.EffectAssets["MAGICALxSPIRAL/WHead1"];
            await _view.StartAnimationBeforeSkill(actionInfo.SubjectIndex.Value, selfAnimation);
        }

        private async UniTask<bool> ShowCutinBattleThumb(ActionInfo actionInfo)
        {
            if (!GameSystem.OptionData.BattlePassiveAnimationSkip && actionInfo.TriggeredSkill && !actionInfo.Master.IsBattleSpecialSkill())
            {
                var subject = _model.GetBattlerInfo(actionInfo.SubjectIndex.Value);
                if (actionInfo.Master.IsDisplayBattleSkill() && subject.IsActor)
                {
                    _view.ShowCutinBattleThumb(subject);
                    await UniTask.DelayFrame(_model.WaitFrameTime(30));
                }
            }
            return true;
        }

        private async void RepeatAnimationSkill(ActionInfo actionInfo)
        {
            if (actionInfo.ActionResults.Count == 0 || !_model.GetBattlerInfo(actionInfo.SubjectIndex.Value).IsAlive())
            {
                CommandEndAnimation();
                return;
            }

            if (actionInfo.Master.IsDisplayBattleSkill())
            {
                _view.SetCurrentSkillData(actionInfo.SkillInfo, _model.GetBattlerInfo(actionInfo.SubjectIndex.Value));
            }

            StartAliveAnimation(actionInfo.ActionResults);
            foreach (var actionResultInfo in actionInfo.ActionResults)
            {
                PopupActionResult(actionResultInfo, actionResultInfo.TargetIndex.Value, true, true);
            }
            await UniTask.DelayFrame(_model.WaitFrameTime(30));
            CommandEndAnimation();
        }

        private async UniTask RemovePassiveInfos()
        {
            var removePassiveResults = _model.CheckRemovePassiveInfos();
            _ = await ExecActionResultInfos(removePassiveResults, true);
        }

        public async UniTask<bool> ExecActionResultInfos(List<ActionResultInfo> resultInfos, bool removePassive = false)
        {
            _model.AdjustActionResultInfo(resultInfos);
            if (!_skipBattle)
            {
                foreach (var resultInfo in resultInfos)
                {
                    var skillData = DataSystem.FindSkill(resultInfo.SkillId.Value);
                    if (skillData != null && !GameSystem.OptionData.BattleAnimationSkip)
                    {
                        var animationData = BattleUtility.AnimationData(skillData.AnimationId);
                        // パッシブが消えるアニメーションは固定
                        if (removePassive)
                        {
                            animationData = BattleUtility.AnimationData(61);
                        }
                        if (animationData != null && animationData.AnimationPath != "")
                        {
                            PlayAnimation(animationData, skillData.AnimationType, new List<int>(){resultInfo.TargetIndex.Value});
                            await UniTask.DelayFrame(_model.WaitFrameTime(animationData.DamageTiming));
                        }
                    }
                    // ダメージ表現をしない
                    PopupActionResult(resultInfo, resultInfo.TargetIndex.Value, true, false);
                    await UniTask.DelayFrame(_model.WaitFrameTime(30));
                }
            }
            _model.ExecActionResultInfos(resultInfos, true);
            if (resultInfos.Count > 0)
            {
                _view.RefreshStatus();
            }
            return true;
        }

        private async void StartAnimationSlipDamage(List<ActionResultInfo> slipDamageResults)
        {
            var actionInfo = _model.ActiveActionInfo;
            if (!_skipBattle)
            {
                await _view.StartAnimationSlipDamage(ActionResultInfo.ConvertIndexes(slipDamageResults), _model.EffectAssets["NA_Effekseer/NA_Fire_001"]);
            }
            _ = await ExecActionResultInfos(slipDamageResults);
            StartDeathAnimation(slipDamageResults);
            //_model.CheckTriggerPassiveInfos(BattleUtility.HpDamagedTriggerTimings(),null,slipDamageResults);

            // regenerate
            bool isTriggeredSkill = actionInfo.TriggeredSkill;
            var battlerInfo = _model.FirstActionBattler;
            if (!_triggerAfterChecked && !_regenerateChecked && !isTriggeredSkill)
            {
                if (battlerInfo != null && actionInfo.SubjectIndex.Value == battlerInfo.Index.Value)
                {
                    _regenerateChecked = true;
                    if (battlerInfo.IsAlive())
                    {
                        var regenerateResult = _model.CheckRegenerate(actionInfo);
                        if (regenerateResult.Count > 0)
                        {
                            StartAnimationRegenerate(regenerateResult);
                            return;
                        }
                    }
                }
            }
            EndTurn();
        }

        private async void StartAnimationRegenerate(List<ActionResultInfo> regenerateActionResults)
        {
            _ = await ExecActionResultInfos(regenerateActionResults);
            if (!_skipBattle)
            {
                await _view.StartAnimationRegenerate(ActionResultInfo.ConvertIndexes(regenerateActionResults), _model.EffectAssets["tktk01/Cure1"]);
            }
            EndTurn();
        }
    }
}
