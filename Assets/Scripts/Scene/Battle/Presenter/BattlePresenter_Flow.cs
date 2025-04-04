using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public partial class BattlePresenter : BasePresenter
    {
        /// <summary>
        /// Apが0以下の行動者を決める
        /// </summary>
        /// <returns></returns>
        public BattlerInfo CheckApCurrentBattler()
        {
            return _model.CheckApCurrentBattler();
        }

        /// <summary>
        /// Apを更新する
        /// </summary>
        /// <returns></returns>
        public async void UpdateApBattlerInfos()
        {
            //while (_model.CurrentBattler == null)
            //{
            //}
        }

        /// <summary>
        /// Ap更新で行動するキャラがいる
        /// </summary>
        private void CommandStartSelect()
        {
            var currentBattler = _model.CurrentBattler;
            if (currentBattler != null)
            {
                _view.SetBattleBusy(true);
                _model.UpdateApModify(currentBattler);
                _view.UpdateGridLayer();
                CheckFirstActionBattler();
                // 行動不可の場合は行動しない
                /*
                if (!_model.EnableCurrentBattler())
                {
                    MakeActionInfoTargetIndexes(_model.CurrentBattler,0);
                    return;
                }
                */
                _model.SetCurrentActionBattler(currentBattler);
                if (currentBattler.IsActor)
                {
                    // マニュアルなら魔法選択
                    ShowMagicList(currentBattler,true);
                    _view.SelectedCharacter(currentBattler);
                    _view.SetAnimationBusy(false);
                } else
                {
                    MakeActionInfoSkillTrigger();
                }
            }
        }

        private void ShowMagicList(BattlerInfo currentBattler,bool resetScrollRect)
        {
            var skillInfos = _model.SkillActionList(currentBattler);
            int selectIndex = 0;
            if (resetScrollRect)
            {
                selectIndex = skillInfos.FindIndex(a => a.Id.Value == currentBattler.LastSelectSkill.Value);
            }
            _view.ShowMagicList(MakeListData(skillInfos),resetScrollRect,selectIndex);
        }

        /// <summary>
        /// 手動で魔法を選択
        /// </summary>
        /// <param name="skillInfo"></param> <summary>
        private void CommandOnSelectSkill(SkillInfo skillInfo)
        {
            if (skillInfo == null)
            {
                return;
            }
            var currentBattler = _model.CurrentBattler;
            // 選択中のActionInfoを生成
            var actionInfo = _model.MakeActionInfo(currentBattler,skillInfo,false,false);
            // 選択中のActionInfoを上書き
            _model.SetSelectActionInfo(actionInfo);
            // 選択対象を決定
            var targetIndexes = _model.GetSkillTargetIndexList(skillInfo.Id.Value,currentBattler.Index.Value,false);
            actionInfo.SetCandidateTargetIndexList(targetIndexes);
            if (targetIndexes.Count > 0)
            {
                _model.SetTargetBattler(_model.GetBattlerInfo(targetIndexes[0]));
            }
            //_view.UpdateSelectCursor(targetIndexes);
            var selectTargetIndexes = _model.MakeAutoSelectIndex(_model.SelectActionInfo,_model.TargetBattler.Index.Value);
            _view.UpdateSelectCursor(selectTargetIndexes);
        }

        /// <summary>
        /// 対象を左右変更
        /// </summary>
        /// <param name="inputKeyType"></param>
        private void CommandOnSelectTarget(InputKeyType inputKeyType)
        {           
            var candidateTargetIndexes = _model.SelectActionInfo.CandidateTargetIndexList;
            if (candidateTargetIndexes.Count <= 1)
            {
                return;
            }
            var findIndex = candidateTargetIndexes.FindIndex(a => a == _model.TargetBattler.Index.Value);
            if (findIndex == -1)
            {
                return;
            }
            if (inputKeyType == InputKeyType.Right)
            {
                var nextIndex = candidateTargetIndexes.Count > (findIndex+1) ? (findIndex+1) : 0;
                _model.SetTargetBattler(_model.GetBattlerInfo(candidateTargetIndexes[nextIndex]));
                
            } else
            if (inputKeyType == InputKeyType.Left)
            {
                var nextIndex = (findIndex-1) < 0 ? candidateTargetIndexes.Count : findIndex-1;
                _model.SetTargetBattler(_model.GetBattlerInfo(candidateTargetIndexes[nextIndex]));
            }
            var selectTargetIndexes = _model.MakeAutoSelectIndex(_model.SelectActionInfo,_model.TargetBattler.Index.Value);
            _view.UpdateSelectCursor(selectTargetIndexes);
        }

        private void CommandDecideSkill()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            // ActionInfoを設定する
            var actionInfo = _model.SelectActionInfo;
            var targetIndexes = _model.MakeAutoSelectIndex(actionInfo,_model.TargetBattler.Index.Value);
            _model.SetActiveActionInfo(actionInfo);
            MakeResultInfoStartAction(actionInfo,targetIndexes);

            _view.EndActionSelect();
        }

        /// <summary>
        /// 対象選択をキャンセル
        /// </summary>
        private void CommandOnCancelEnemy()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.EndActionSelect();
            _model.SetSelectActionInfo(null);
            ShowMagicList(_model.CurrentBattler,false);
        }

        /// <summary>
        /// 対象に選択カーソルを表示する
        /// </summary>
        /// <param name="battlerInfo"></param>
        private void CommandTargetSelectCursor(BattlerInfo battlerInfo)
        {
            // battlerInfoを選択した時の範囲対象を表示
            if (battlerInfo != null)
            {
                var targetIndexes = _model.MakeAutoSelectIndex(_model.SelectActionInfo,battlerInfo.Index.Value);
                _view.UpdateSelectCursor(targetIndexes);
            }
        }

        /// <summary>
        /// 手動で対象を決定
        /// </summary>
        /// <param name="battlerInfo"></param>
        private void CommandOnSelectEnemy(BattlerInfo battlerInfo)
        {
            // 対象を決定
            if (battlerInfo != null)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                // ActionInfoを設定する
                var actionInfo = _model.SelectActionInfo;
                var targetIndexes = _model.MakeAutoSelectIndex(actionInfo,battlerInfo.Index.Value);
                _model.SetActiveActionInfo(actionInfo);
                MakeResultInfoStartAction(actionInfo,targetIndexes);

                _view.EndActionSelect();
            }
        }


        /// <summary>
        /// 行動者を登録する
        /// </summary>
        /// <returns></returns>
        public async void CheckFirstActionBattler()
        {
            if (_model.FirstActionBattler == null)
            {
                var currentBattler = _model.CurrentBattler;
                _model.SetFirstActionBattler(currentBattler);
                // 解除判定は行動開始の最初のみ
                var removed =_model.UpdateNextSelfTurn(currentBattler);
                foreach (var removedState in removed)
                {
                    _view.StartStatePopup(removedState.TargetIndex.Value,DamageType.State,"-" + removedState.Master.Name);
                }
                // Passive解除
                await RemovePassiveInfos();
                // 行動開始前トリガー
                _model.CheckTriggerPassiveInfos(BattleUtility.BeforeTriggerTimings(),null,null);
            }
        }

        /// <summary>
        /// 作戦に基づいてActionInfoを決定する
        /// </summary>
        private void MakeActionInfoSkillTrigger()
        {
            var currentBattler = _model.CurrentBattler;
            if (currentBattler != null)
            {
                int autoSkillId;
                int targetIndex;
                (autoSkillId,targetIndex) = _model.MakeAutoSkillTriggerSkillId(currentBattler);
                if (autoSkillId == -1)
                {
                    // 何もしない
                    autoSkillId = 20010;
                }
                MakeActionInfoTargetIndexes(currentBattler,autoSkillId,targetIndex);
            }
        }

        private void MakeActionInfoTargetIndexes(BattlerInfo battlerInfo,int skillId,int oneTargetIndex = -1)
        {
            // 対象を自動決定
            var (actionInfo,targetIndexes) = _model.GetActionInfoTargetIndexes(battlerInfo,skillId,oneTargetIndex);
            MakeResultInfoStartAction(actionInfo,targetIndexes);
        }

        /// <summary>
        /// 行動結果を生成する
        /// </summary>
        /// <param name="indexList"></param>
        public async void MakeResultInfoStartAction(ActionInfo actionInfo,List<int> indexList)
        {
            _view.SetHelpText("");
            _view.ChangeBackCommandActive(false);

            _model.SetActionInfoParameter(actionInfo);
            await MakeActionResultInfo(actionInfo,indexList);
            _model.SetActiveActionInfo(actionInfo);
            StartActionInfo(actionInfo);
        }

        /// <summary>
        /// 行動結果を生成する
        /// </summary>
        /// <param name="indexList"></param>
        private async UniTask MakeActionResultInfo(ActionInfo actionInfo,List<int> indexList)
        {
            if (actionInfo != null)
            {
                _view.BattlerBattleClearSelect();

                // 自分,味方,相手の行動前パッシブ
                /*
                CheckBeforeActionInfo(actionInfo);
                */

                // 開始行動のアクションの結果を生成
                _model.MakeActionResultInfo(actionInfo,indexList);

                /*
                var current = _model.CurrentActionInfo;
                // かばう専用割り込み判定
                CheckPrimaryInterruptActionInfoTriggerTimings();

                // かばうが成立する場合
                if (current != _model.CurrentActionInfo)
                {
                    // 先にかばう結果を設定する
                    var beforeActionInfos = _model.BeforeActionInfo(current);
                    foreach (var beforeActionInfo in beforeActionInfos)
                    {
                        await ExecActionResultInfos(beforeActionInfo.ActionResults);
                    }
                    beforeActionInfos.Reverse();
                    
                    foreach (var beforeActionInfo in beforeActionInfos)
                    {
                        _model.RemoveActionInfo(beforeActionInfo);
                    }
                    // 強制的に再生成
                    _model.MakeActionResultInfo(current,indexList,false,true);
                }

                // 行動決定後の割り込みスキル判定
                CheckInterruptActionInfoTriggerTimings(current);
                */
            }
        }

        /// <summary>
        /// 行動前パッシブを確認
        /// </summary>
        /// <param name="indexList"></param>
        private void CheckBeforeActionInfo(ActionInfo actionInfo)
        {
            if (actionInfo != null)
            {
                _view.BattlerBattleClearSelect();

                // 自分,味方,相手の行動前パッシブを確認
                _model.CheckTriggerPassiveInfos(BattleUtility.BeforeActionInfoTriggerTimings(),actionInfo,actionInfo.ActionResults);
            }
        }
        
        /// <summary>
        /// かばう割り込みトリガー確認
        /// </summary>
        private void CheckPrimaryInterruptActionInfoTriggerTimings()
        {
            var actionInfo = _model.ReceiveActionInfo;
            _model.CheckTriggerActiveInfos(TriggerTiming.PrimaryInterrupt,actionInfo,actionInfo.ActionResults,true);
            _model.CheckTriggerPassiveInfos(new List<TriggerTiming>(){TriggerTiming.PrimaryInterrupt},actionInfo,actionInfo.ActionResults);
        }

        /// <summary>
        /// 行動割り込みトリガー確認
        /// </summary>
        private void CheckInterruptActionInfoTriggerTimings(ActionInfo actionInfo)
        {    
            _model.CheckTriggerActiveInfos(TriggerTiming.Interrupt,actionInfo,actionInfo.ActionResults,true);
            _model.CheckTriggerPassiveInfos(new List<TriggerTiming>(){TriggerTiming.Interrupt},actionInfo,actionInfo.ActionResults);
            _model.CheckTriggerPassiveInfos(new List<TriggerTiming>(){TriggerTiming.Use},actionInfo,actionInfo.ActionResults);
        }

        private void StartActionInfo(ActionInfo actionInfo)
        {
            // 行動変化対応のため再取得
            _view.EndActionSelect();
            _view.HideBattleThumb();
            //LogOutput.Log(actionInfo.Master.Id + "行動");
            if (actionInfo != null)
            {
                // 待機か戦闘不能なら何もしない
                if (actionInfo.IsWait() || !_model.CurrentActionBattler.IsAlive())
                {
                    StartWaitCommand(actionInfo);
                } else
                {
                    StartActionInfoAnimation(actionInfo);
                }
            }
        }

        /// <summary>
        /// ActionInfoのアニメーションが終了した後処理
        /// </summary>
        /// <param name="actionInfo"></param>
        private void CommandEndAnimation()
        {
            var actionInfo = _model.ActiveActionInfo;
            if (actionInfo != null)
            {
                // ダメージなどを適用
                _model.ExecCurrentAction(actionInfo,true);

                // Hp変化での行動・パッシブを確認
                _model.CheckTriggerActiveInfos(TriggerTiming.HpDamaged,actionInfo,actionInfo.ActionResults,true);
                _model.CheckTriggerPassiveInfos(BattleUtility.HpDamagedTriggerTimings(),actionInfo,actionInfo.ActionResults);
            
                StartDeathAnimation(actionInfo.ActionResults);
                StartAliveAnimation(actionInfo.ActionResults);
                // 繰り返しがある場合
                if (actionInfo.RepeatTime.Value > 0)
                {
                    RepeatActionInfo(actionInfo);
                    return;
                }
            }
            _view.ClearCurrentSkillData();

            // スリップダメージ
            var slipDamageActionResult = SlipDamageActionResult(actionInfo);
            if (slipDamageActionResult != null && slipDamageActionResult.Count > 0)
            {
                StartAnimationSlipDamage(slipDamageActionResult);
                return;
            }

            // リジェネ回復
            var regenerationActionResult = RegenerationActionResult(actionInfo);
            if (regenerationActionResult != null && regenerationActionResult.Count > 0)
            {
                StartAnimationRegenerate(regenerationActionResult);
                return;
            }

            EndTurn();
        }

        /// <summary>
        /// 連続行動するActionInfo
        /// </summary>
        /// <param name="actionInfo"></param>
        private async void RepeatActionInfo(ActionInfo actionInfo)
        {
            _model.ResetTargetIndexList(actionInfo);
            await MakeActionResultInfo(actionInfo,actionInfo.CandidateTargetIndexList);
            // 再取得
            if (actionInfo == _model.ActiveActionInfo)
            {
                actionInfo = _model.ActiveActionInfo;
                //LogOutput.Log(actionInfo.Master.Id + "再行動");
                RepeatAnimationSkill(actionInfo);
            } else
            {
                // 割り込みでアクションが変わった場合
                StartActionInfo(actionInfo);
            }
        }

        private List<ActionResultInfo> SlipDamageActionResult(ActionInfo actionInfo)
        {
            // スリップダメージ
            bool isTriggeredSkill = actionInfo.TriggeredSkill;
            if (_triggerAfterChecked == false && _slipDamageChecked == false && isTriggeredSkill == false)
            {
                if (_model.FirstActionBattler != null && actionInfo.SubjectIndex.Value == _model.FirstActionBattler.Index.Value)
                {
                    _slipDamageChecked = true;
                    var slipResult = _model.CheckSlipDamage();
                    if (slipResult.Count > 0)
                    {
                        return slipResult;
                    }
                }
            }
            return null;
        }

        private List<ActionResultInfo> RegenerationActionResult(ActionInfo actionInfo)
        {
            bool isTriggeredSkill = actionInfo.TriggeredSkill;
            // regenerate
            if (_triggerAfterChecked == false && _regenerateChecked == false && isTriggeredSkill == false)
            {
                if (_model.FirstActionBattler != null && actionInfo.SubjectIndex.Value == _model.FirstActionBattler.Index.Value)
                {
                    _regenerateChecked = true;
                    if (_model.FirstActionBattler.IsAlive())
                    {
                        var regenerateResult = _model.CheckRegenerate(actionInfo);
                        if (regenerateResult.Count > 0)
                        {
                            return regenerateResult;
                        }
                    }
                }
            }
            return null;
        }
        
        private async void EndTurn()
        {
            var actionInfo = _model.ActiveActionInfo;
            // ターン終了
            _view.RefreshStatus();
            // PlusSkill
            _model.CheckPlusSkill(actionInfo);
            // Passive付与
            _model.CheckTriggerPassiveInfos(BattleUtility.AfterTriggerTimings(),actionInfo,actionInfo.ActionResults);
            
            //PassiveInfoAction(BattleUtility.AfterTriggerTimings());
            // Passive解除
            await RemovePassiveInfos();

            // 行動者のActionInfoか
            bool isTriggeredSkill = actionInfo.TriggeredSkill;
            // TriggerAfterがある
            var result = _model.CheckTriggerActiveInfos(TriggerTiming.After,actionInfo,actionInfo.ActionResults,true);
            var result2 = _model.CheckTriggerActiveInfos(TriggerTiming.AfterAndStartBattle,actionInfo,actionInfo.ActionResults,true);
            result.AddRange(result2);

            var checkNoResetAp = _model.CheckNoResetAp(actionInfo);
            if (checkNoResetAp == false && result.Count == 0 && _triggerAfterChecked == false && isTriggeredSkill == false)
            {
                // 行動者のターンを進める
                var removed =_model.UpdateTurn();
                foreach (var removedState in removed)
                {
                    _view.StartStatePopup(removedState.TargetIndex.Value,DamageType.State,"-" + removedState.Master.Name);
                }
                // Passive付与
                _model.CheckTriggerPassiveInfos(BattleUtility.AfterTriggerTimings(),null,null);
                    
                // Passive解除
                await RemovePassiveInfos();

                // 10010行動後にAP+
                var gainAp = _model.CheckActionAfterGainAp(actionInfo);
                if (gainAp > 0)
                {
                    if (_skipBattle == false)
                    {
                        _view.StartHeal(_model.FirstActionBattler.Index.Value,DamageType.MpHeal,gainAp); 
                        await UniTask.DelayFrame(_model.WaitFrameTime(16));           
                    }
                    _model.ActionAfterGainAp(gainAp);
                    _view.RefreshStatus();
                }

            }
            var reaction = _model.CheckReaction(actionInfo);
            _model.TurnEnd(actionInfo);
            if (reaction)
            {
                _view.SetBattleBusy(false);
                return;
            }
            if (isTriggeredSkill == false)
            {
                _triggerAfterChecked = true;
            }

            // 勝敗判定
            if (IsBattleEnd() && result.Count == 0)
            {
                BattleEnd();
                return;
            }
            if (result.Count > 0)
            {
                _battleEnded = false;
            }

            // 敵の蘇生を反映
            var aliveEnemies = _model.PreservedAliveEnemies();
            foreach (var aliveEnemy in aliveEnemies)
            {
                _view.StartAliveAnimation(aliveEnemy.Index.Value);                
            }
            // Hp0以上の戦闘不能を回復
            var notDeadMembers = _model.NotDeadMembers();
            foreach (var notDeadMember in notDeadMembers)
            {
                _view.StartAliveAnimation(notDeadMember.Index.Value);                
            }
            // 戦闘不能に聖棺がいたら他の対象に移す
            var changeHolyCoffinStates = _model.EndHolyCoffinState();
            foreach (var addState in changeHolyCoffinStates)
            {
                _view.StartStatePopup(addState.TargetIndex.Value,DamageType.State,"+" + addState.Master.Name);
            }
            // 透明が外れるケースを適用
            var removeShadowStates = _model.EndRemoveShadowState();
            foreach (var removeShadowState in removeShadowStates)
            {
                _view.StartStatePopup(removeShadowState.TargetIndex.Value,DamageType.State,"-" + removeShadowState.Master.Name);
            };
            // 戦闘不能の拘束ステートを解除する
            var removeChainStates = _model.EndRemoveState();
            foreach (var removeChainState in removeChainStates)
            {
                _view.StartStatePopup(removeChainState.TargetIndex.Value,DamageType.State,"-" + removeChainState.Master.Name);
            };

            // 待機できなくなった場合は待機状態をはずす
            _model.RemoveOneMemberWaitBattlers();
            //_view.UpdateGridLayer();
            _view.RefreshStatus();

            // 誘発行動があれば続ける
            var receiveActionInfo = _model.ReceiveActionInfo;
            if (receiveActionInfo != null)
            {
                _battleEnded = false;
                MakeResultInfoStartAction(receiveActionInfo,receiveActionInfo.CandidateTargetIndexList);
                return;
            }
            var linkage = _model.CheckLinkageBattlerInfo();
            if (linkage)
            {
                _view.SetActors(MakeListData(_model.BattlerActors()));
                _view.UpdateGridLayer();
            }

            // 行動を全て終了する
            _model.SeekTurnCount();
            _view.RefreshTurn(_model.TurnCount);
            _view.ShowStateOverlay();
            _triggerAfterChecked = false;
            _slipDamageChecked = false;
            _regenerateChecked = false;
            // ウェイトがいたら復帰する
            _model.AssignWaitBattler();
            _model.SetFirstActionBattler(null);
            _view.SetBattleBusy(false);
        }
    }
}
