using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

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
            if (_skipBattle || actionInfo.Master.IsDisplayStartBattle())
            {
                CommandEndAnimation();
                return;
            }
            StartAnimation(actionInfo);
        }

        private async void StartAnimation(ActionInfo actionInfo)
        {
            if (actionInfo.FirstAttack() && actionInfo.Master.AnimationId > 0)
            {
                if (actionInfo.Master.SkillType == SkillType.Unique)
                {
                    var isActor = _model.GetBattlerInfo(actionInfo.SubjectIndex).IsActorView;
                    await StartAnimationMessiah(actionInfo,isActor);
                } else
                if (actionInfo.Master.SkillType == SkillType.Awaken)
                {
                    await StartAnimationAwaken(actionInfo);
                }
            }
            StartAnimationSkill(actionInfo);
        }
        
        /// <summary>
        /// 覚醒アニメーション再生してからアニメーション再生
        /// </summary>
        private async UniTask<bool> StartAnimationMessiah(ActionInfo actionInfo,bool isActor)
        {
            var subject = _model.GetBattlerInfo(actionInfo.SubjectIndex);
            Sprite sprite;
            if (isActor)
            {
                var actorId = subject.ActorInfo != null ? subject.ActorInfo.ActorId : subject.EnemyData.Id - 1000;
                sprite = _model.AwakenSprite(actorId);
            } else
            {
                sprite = _model.AwakenEnemySprite(subject.EnemyData.Id);
            }
            await _view.StartAnimationMessiah(subject,sprite);
            return true;
        }

        /// <summary>
        /// カットインアニメーション再生してからアニメーション再生
        /// </summary>
        private async UniTask<bool> StartAnimationAwaken(ActionInfo actionInfo)
        {
            await _view.StartAnimationDemigod(_model.GetBattlerInfo(actionInfo.SubjectIndex),actionInfo.Master);
            return true;
        }

        private async void StartAnimationSkill(ActionInfo actionInfo)
        {
            _view.ChangeSideMenuButtonActive(false);
            _view.SetBattlerThumbAlpha(true);
            //_view.ShowEnemyStateOverlay();
            _view.HideStateOverlay();
            _view.SetAnimationBusy(true);
            if (actionInfo.ActionResults.Count == 0)
            {
                CommandEndAnimation();
                return;
            }

            await SelfAnimation(actionInfo);

            await ShowCutinBattleThumb(actionInfo);
            
            if (actionInfo.Master.IsDisplayBattleSkill())
            {
                _view.SetCurrentSkillData(actionInfo.SkillInfo,_model.GetBattlerInfo(actionInfo.SubjectIndex));
            }
            
            StartAliveAnimation(actionInfo.ActionResults);
            var animationData = BattleUtility.AnimationData(actionInfo.Master.AnimationId);
            if (animationData != null && animationData.AnimationPath != "" && GameSystem.ConfigData.BattleAnimationSkip == false)
            {
                var targetIndexList = actionInfo.ResultTargetIndexes();
                PlayAnimation(animationData,actionInfo.Master.AnimationType,targetIndexList,false);
                await UniTask.DelayFrame((int)(animationData.DamageTiming / GameSystem.ConfigData.BattleSpeed));
                foreach (var actionResultInfo in actionInfo.ActionResults)
                {
                    PopupActionResult(actionResultInfo,actionResultInfo.TargetIndex,true,true);
                }
                var waitFrame = _model.WaitFrameTime(48);
                if (!actionInfo.LastAttack() && waitFrame > 1)
                {
                    waitFrame = 36;
                }
                await UniTask.DelayFrame(waitFrame);
            } else
            {
                foreach (var actionResultInfo in actionInfo.ActionResults)
                {
                    PopupActionResult(actionResultInfo,actionResultInfo.TargetIndex,true,true);
                }
                var waitFrame = _model.WaitFrameTime(30);
                if (!actionInfo.LastAttack() && waitFrame > 1)
                {
                    waitFrame = 16;
                }
                await UniTask.DelayFrame(waitFrame);
            }
            CommandEndAnimation();
        }
        
        private async UniTask<bool> SelfAnimation(ActionInfo actionInfo)
        {
            var selfAnimation = ResourceSystem.LoadResourceEffect("MAGICALxSPIRAL/WHead1");
            _view.StartAnimationBeforeSkill(actionInfo.SubjectIndex,selfAnimation);
            await UniTask.DelayFrame(_model.WaitFrameTime(30));
            return true;
        }

        private async UniTask<bool> ShowCutinBattleThumb(ActionInfo actionInfo)
        {
            if (actionInfo.TriggeredSkill && actionInfo.Master.SkillType != SkillType.Unique && actionInfo.Master.SkillType != SkillType.Awaken)
            {
                if (actionInfo.Master.IsDisplayBattleSkill() && _model.GetBattlerInfo(actionInfo.SubjectIndex).IsActor)
                {
                    _view.ShowCutinBattleThumb(_model.GetBattlerInfo(actionInfo.SubjectIndex));
                    await UniTask.DelayFrame(_model.WaitFrameTime(16));
                }
            }
            return true;
        }

        private async void RepeatAnimationSkill(ActionInfo actionInfo)
        {           
            if (actionInfo.ActionResults.Count == 0 || !_model.CurrentActionBattler.IsAlive())
            {
                CommandEndAnimation();
                return;
            }
            if (actionInfo.FirstAttack())
            {
                //StartAnimation(actionInfo);
            }
            
            if (actionInfo.Master.IsDisplayBattleSkill())
            {
                _view.SetCurrentSkillData(actionInfo.SkillInfo,_model.GetBattlerInfo(actionInfo.SubjectIndex));
            }

            StartAliveAnimation(actionInfo.ActionResults);
            foreach (var actionResultInfo in actionInfo.ActionResults)
            {
                PopupActionResult(actionResultInfo,actionResultInfo.TargetIndex,true,true);
            }
            await UniTask.DelayFrame(_model.WaitFrameTime(16));
            CommandEndAnimation();
        }

        private async void StartAnimationSlipDamage(List<ActionResultInfo> slipDamageResults)
        {
            var actionInfo = _model.ActiveActionInfo;
            await ExecActionResultInfos(slipDamageResults);
            if (_skipBattle == false)
            {
                _view.StartAnimationSlipDamage(ActionResultInfo.ConvertIndexes(slipDamageResults));
            }
            StartDeathAnimation(slipDamageResults);
            //_model.CheckTriggerPassiveInfos(BattleUtility.HpDamagedTriggerTimings(),null,slipDamageResults);

            // regenerate
            bool isTriggeredSkill = actionInfo.TriggeredSkill;
            if (_triggerAfterChecked == false && _regenerateChecked == false && isTriggeredSkill == false)
            {
                if (_model.FirstActionBattler != null && actionInfo.SubjectIndex == _model.FirstActionBattler.Index)
                {
                    _regenerateChecked = true;
                    if (_model.FirstActionBattler.IsAlive())
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
            await ExecActionResultInfos(regenerateActionResults);
            if (_skipBattle == false)
            {
                _view.StartAnimationRegenerate(ActionResultInfo.ConvertIndexes(regenerateActionResults));
            }
            EndTurn();
        }
    }
}
