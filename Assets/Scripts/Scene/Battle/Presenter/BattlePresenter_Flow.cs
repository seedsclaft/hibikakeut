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
        public void UpdateApBattlerInfos()
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
            var currentBattler = _model.CheckApCurrentBattler();
            if (currentBattler == null)
            {
                return;
            }
            _view.SetBattleBusy(true);
            _model.UpdateApModify(currentBattler);
            _view.UpdateGridLayer();
            CheckFirstActionBattler();

            if (currentBattler.IsActor && !_model.IsBattleAuto())
            {
                // マニュアルなら魔法選択
                ShowMagicList(currentBattler, true);
                _view.SelectedCharacter(currentBattler);
                _view.AnimationBusy.SetValue(false);
            }
            else
            {
                MakeActionInfoSkillTrigger();
            }
        }

        private void ShowMagicList(BattlerInfo currentBattler, bool resetScrollRect)
        {
            var skillInfos = _model.SkillActionList(currentBattler);
            int selectIndex = 0;
            if (resetScrollRect)
            {
                selectIndex = skillInfos.FindIndex(a => a.Id.Value == currentBattler.LastSelectSkill.Value);
            }
            _view.ShowMagicList(MakeListData(skillInfos), resetScrollRect, selectIndex);
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
            var currentBattler = _model.CurrentSelectBattler;
            // 選択中のActionInfoを生成
            var actionInfo = _model.MakeActionInfo(currentBattler, skillInfo, false, false);
            // 選択中のActionInfoを上書き
            _model.SetSelectActionInfo(actionInfo);
            // 選択対象を決定
            var targetIndexes = _model.GetSkillTargetIndexList(skillInfo.Id.Value, currentBattler.Index.Value, false);
            actionInfo.SetCandidateTargetIndexList(targetIndexes);
            _view.SetBattlerActiveStatus(targetIndexes);
            if (targetIndexes.Count > 0)
            {
                var targetIndex = targetIndexes[0];
                if (targetIndexes.Contains(currentBattler.LastTargetIndex()))
                {
                    targetIndex = currentBattler.LastTargetIndex();
                }
                _model.SetSelectTargetBattler(_model.GetBattlerInfo(targetIndex));
            }
            _view.UpdateSelectCursor(targetIndexes);
        }

        /// <summary>
        /// 対象を左右変更
        /// </summary>
        /// <param name="inputKeyType"></param>
        private void CommandOnSelectTarget(InputKeyType inputKeyType)
        {
            if (_beforeBattle)
            {
                return;
            }
            var candidateTargetIndexes = _model.SelectActionInfo.CandidateTargetIndexList;
            if (candidateTargetIndexes.Count <= 1)
            {
                SelectSkillSelectTarget();
                return;
            }
            var findIndex = candidateTargetIndexes.FindIndex(a => a == _model.SelectTargetBattler.Index.Value);
            if (findIndex == -1)
            {
                SelectSkillSelectTarget();
                return;
            }
            var targetBattlerInfo = _model.SelectTargetBattler;
            var friends = _model.GetBattlerInfoByIndex(targetBattlerInfo.IsActor, false);
            friends = friends.FindAll(a => candidateTargetIndexes.Contains(a.Index.Value));
            var opponents = _model.GetBattlerInfoByIndex(!targetBattlerInfo.IsActor, false);
            opponents = opponents.FindAll(a => candidateTargetIndexes.Contains(a.Index.Value));

            var nextIndex = 0;
            switch (inputKeyType)
            {
                case InputKeyType.Right:
                    findIndex = friends.FindIndex(a => a.Index.Value == targetBattlerInfo.Index.Value);
                    nextIndex = friends.Count > (findIndex + 1) ? (findIndex + 1) : 0;
                    targetBattlerInfo = friends[nextIndex];
                    break;
                case InputKeyType.Left:
                    findIndex = friends.FindIndex(a => a.Index.Value == targetBattlerInfo.Index.Value);
                    nextIndex = (findIndex - 1) < 0 ? friends.Count - 1 : findIndex - 1;
                    targetBattlerInfo = friends[nextIndex];
                    break;
                case InputKeyType.Up:
                    if (targetBattlerInfo.IsActor && opponents.Count > 0)
                    {
                        targetBattlerInfo = opponents[0];
                    }
                    break;
                case InputKeyType.Down:
                    if (!targetBattlerInfo.IsActor && opponents.Count > 0)
                    {
                        targetBattlerInfo = opponents[0];
                    }
                    break;
            }
            if (targetBattlerInfo == null)
            {
                return;
            }
            _model.SetSelectTargetBattler(targetBattlerInfo);
            SelectSkillSelectTarget();
        }

        private void CommandOnSelectTargetCursor(BattlerInfo battlerInfo)
        {
            if (_beforeBattle)
            {
                return;
            }
            var candidateTargetIndexes = _model.SelectActionInfo.CandidateTargetIndexList;
            if (candidateTargetIndexes.Count <= 1)
            {
                SelectSkillSelectTarget();
                return;
            }
            var findIndex = candidateTargetIndexes.FindIndex(a => a == battlerInfo.Index.Value);
            if (findIndex == -1)
            {
                SelectSkillSelectTarget();
                return;
            }
            var targetBattlerInfo = battlerInfo;
            _model.SetSelectTargetBattler(targetBattlerInfo);
            SelectSkillSelectTarget();
        }

        private void CommandDecideSkill()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            SelectSkillSelectTarget(true);
            // ActionInfoを設定する
            /*
            _model.SetActiveActionInfo(actionInfo);
            MakeResultInfoStartAction(actionInfo,targetIndexes);

            _view.EndActionSelect();
            */
        }

        private void SelectSkillSelectTarget(bool autoTarget = false)
        {
            // 対象選択を行う
            var actionInfo = _model.SelectActionInfo;
            var subject = _model.GetBattlerInfo(actionInfo.SubjectIndex.Value);
            var targetIndexes = _model.MakeAutoSelectIndex(actionInfo, _model.SelectTargetBattler.Index.Value);
            if (targetIndexes.Count > 0)
            {
                if (targetIndexes[0] < 100)
                {
                    _view.SelectActorList(targetIndexes);
                }
                else
                {
                    if (autoTarget && actionInfo.CandidateTargetIndexList.Contains(subject.LastTargetIndex()))
                    {
                        targetIndexes[0] = subject.LastTargetIndex();
                    }
                    _view.SelectEnemyList(targetIndexes);
                }
            }
            _view.UpdateSelectCursor(actionInfo.CandidateTargetIndexList);
            _view.SetCurrentSkillData(actionInfo.SkillInfo, subject);
        }

        private void CommandOnDecideEnemy(BattlerInfo battlerInfo)
        {
            // 対象選択として有効か
            var actionInfo = _model.SelectActionInfo;
            var targetIndexes = _model.MakeAutoSelectIndex(actionInfo, battlerInfo.Index.Value);
            if (targetIndexes.FindIndex(a => a == battlerInfo.Index.Value) > -1)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                //_model.SetActiveActionInfo(actionInfo);
                _model.SetFirstActionInfo(actionInfo);
                MakeResultInfoStartAction(actionInfo, targetIndexes);
                _view.EndActionSelect();
            }
        }

        /// <summary>
        /// 対象選択をキャンセル
        /// </summary>
        private void CommandOnCancelEnemy()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.EndActionSelect();
            _model.SetSelectActionInfo(null);
            _view.SelectedCharacter(_model.CurrentSelectBattler);
            ShowMagicList(_model.CurrentSelectBattler, false);
            _view.ClearCurrentSkillData();
        }

        private void CommandOnDecideActor(BattlerInfo battlerInfo)
        {
            // 対象選択として有効か
            var actionInfo = _model.SelectActionInfo;
            var targetIndexes = _model.MakeAutoSelectIndex(actionInfo, _model.SelectTargetBattler.Index.Value);
            if (targetIndexes.FindIndex(a => a == battlerInfo.Index.Value) > -1)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                //_model.SetActiveActionInfo(actionInfo);
                _model.SetFirstActionInfo(actionInfo);
                MakeResultInfoStartAction(actionInfo, targetIndexes);
                _view.EndActionSelect();
            }
        }

        /// <summary>
        /// 対象選択をキャンセル
        /// </summary>
        private void CommandOnCancelActor()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.EndActionSelect();
            _model.SetSelectActionInfo(null);
            _view.SelectedCharacter(_model.CurrentSelectBattler);
            ShowMagicList(_model.CurrentSelectBattler, false);
            _view.ClearCurrentSkillData();
        }

        /// <summary>
        /// 行動者を登録する
        /// </summary>
        /// <returns></returns>
        public async void CheckFirstActionBattler()
        {
            if (_model.FirstActionBattler != null)
            {
                return;
            }
            var currentBattler = _model.CurrentSelectBattler;
            _model.SetFirstActionBattler(currentBattler);
            // 解除判定は行動開始の最初のみ
            var removeStateInfos = _model.UpdateNextSelfTurn(currentBattler);
            await RemoveStateInfoPopup(removeStateInfos);
            // 行動開始前トリガー
            _model.CheckTriggerPassiveInfos(BattleUtility.BeforeTriggerTimings(), null, null);

        }

        /// <summary>
        /// 作戦に基づいてActionInfoを決定する
        /// </summary>
        private void MakeActionInfoSkillTrigger()
        {
            var currentBattler = _model.CurrentSelectBattler;
            if (currentBattler != null)
            {
                int autoSkillId;
                int targetIndex;
                (autoSkillId, targetIndex) = _model.MakeAutoSkillTriggerSkillId(currentBattler);
                if (autoSkillId == -1)
                {
                    // 何もしない
                    autoSkillId = 20010;
                }
                MakeActionInfoTargetIndexes(currentBattler, autoSkillId, targetIndex);
            }
        }

        private void MakeActionInfoTargetIndexes(BattlerInfo battlerInfo, int skillId, int oneTargetIndex = -1)
        {
            // 対象を自動決定
            var (actionInfo, targetIndexes) = _model.GetActionInfoTargetIndexes(battlerInfo, skillId, oneTargetIndex);
            _model.SetFirstActionInfo(actionInfo);
            MakeResultInfoStartAction(actionInfo, targetIndexes);
        }

        /// <summary>
        /// 行動結果を生成する
        /// </summary>
        /// <param name="indexList"></param>
        public void MakeResultInfoStartAction(ActionInfo actionInfo, List<int> indexList)
        {
            _view.SetHelpText("");
            _view.ChangeBackCommandActive(false);

            _model.SetActionInfoParameter(actionInfo, true);
            MakeActionResultInfo(actionInfo, indexList);
            // 割り込み判定
            var interruptAction = _model.InterruptActionInfo;
            if (interruptAction != null)
            {
                _model.SetActiveActionInfo(interruptAction);
                StartActionInfo(interruptAction);
                return;
            }
            _model.SetActiveActionInfo(actionInfo);
            StartActionInfo(actionInfo);
        }

        /// <summary>
        /// 行動結果を生成する
        /// </summary>
        /// <param name="indexList"></param>
        private void MakeActionResultInfo(ActionInfo actionInfo, List<int> indexList)
        {
            if (actionInfo != null)
            {
                _view.BattlerBattleClearSelect();

                // 自分,味方,相手の行動前パッシブ
                CheckBeforeActionInfo(actionInfo);

                // 開始行動アクションの結果を生成
                _model.MakeActionResultInfo(actionInfo, indexList);

                // 自分,味方,相手の対象決定後パッシブ
                // CheckAfterActionInfo(actionInfo);

                // 行動決定後の割り込みスキル判定
                CheckInterruptActionInfoTriggerTimings(actionInfo);
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
                _model.CheckTriggerPassiveInfos(BattleUtility.BeforeActionInfoTriggerTimings(), actionInfo, actionInfo.ActionResults);
            }
        }

        /// <summary>
        /// 行動前パッシブを確認
        /// </summary>
        /// <param name="indexList"></param>
        private void CheckAfterActionInfo(ActionInfo actionInfo)
        {
            if (actionInfo != null)
            {
                _view.BattlerBattleClearSelect();

                // 自分,味方,相手の行動前パッシブを確認
                _model.CheckTriggerPassiveInfos(BattleUtility.AfterActionInfoTriggerTimings(), actionInfo, actionInfo.ActionResults);
            }
        }

        /// <summary>
        /// 行動割り込みトリガー確認
        /// </summary>
        private void CheckInterruptActionInfoTriggerTimings(ActionInfo actionInfo)
        {
            _model.CheckTriggerActiveInfos(TriggerTiming.Interrupt, actionInfo, actionInfo.ActionResults, true);
            _model.CheckTriggerPassiveInfos(new List<TriggerTiming>() { TriggerTiming.Interrupt }, actionInfo, actionInfo.ActionResults);
            _model.CheckTriggerPassiveInfos(new List<TriggerTiming>() { TriggerTiming.Use }, actionInfo, actionInfo.ActionResults);
        }

        private void StartActionInfo(ActionInfo actionInfo)
        {
            _view.EndActionSelect();
            _view.HideBattleThumb();
            if (actionInfo != null)
            {
                var battlerInfo = _model.GetBattlerInfo(actionInfo.SubjectIndex.Value);
                // 待機か戦闘不能なら何もしない
                if (actionInfo.IsWait() || (battlerInfo != null && !battlerInfo.IsAlive() && actionInfo.SubjectIndex.Value == battlerInfo.Index.Value))
                {
                    StartWaitCommand(actionInfo);
                }
                else
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
                _model.ExecCurrentAction(actionInfo, true);
                CheckAchievements();

                // Hp変化での行動・パッシブを確認
                _model.CheckTriggerActiveInfos(TriggerTiming.HpDamaged, actionInfo, actionInfo.ActionResults, true);
                _model.CheckTriggerPassiveInfos(BattleUtility.HpDamagedTriggerTimings(), actionInfo, actionInfo.ActionResults);

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
        private void RepeatActionInfo(ActionInfo actionInfo)
        {
            _model.ResetTargetIndexList(actionInfo);
            MakeActionResultInfo(actionInfo, actionInfo.CandidateTargetIndexList);
            // 再取得
            if (actionInfo == _model.ActiveActionInfo)
            {
                actionInfo = _model.ActiveActionInfo;
                //LogOutput.Log(actionInfo.Master.Id + "再行動");
                RepeatAnimationSkill(actionInfo);
            }
            else
            {
                // 割り込みでアクションが変わった場合
                StartActionInfo(actionInfo);
            }
        }

        private List<ActionResultInfo> SlipDamageActionResult(ActionInfo actionInfo)
        {
            // スリップダメージ
            bool isTriggeredSkill = actionInfo.TriggeredSkill;
            var battlerInfo = _model.FirstActionBattler;
            if (!_triggerAfterChecked && !_slipDamageChecked && !isTriggeredSkill)
            {
                if (battlerInfo != null && actionInfo.SubjectIndex.Value == battlerInfo.Index.Value)
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
            var battlerInfo = _model.FirstActionBattler;
            // regenerate
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
            _model.CheckTriggerPassiveInfos(BattleUtility.AfterTriggerTimings(), actionInfo, actionInfo.ActionResults);

            //PassiveInfoAction(BattleUtility.AfterTriggerTimings());
            // Passive解除
            await RemovePassiveInfos();

            // 行動者のActionInfoか
            bool isTriggeredSkill = actionInfo.TriggeredSkill;
            // TriggerAfterがある
            var result = _model.CheckTriggerActiveInfos(TriggerTiming.After, actionInfo, actionInfo.ActionResults, true);

            var checkNoResetAp = _model.CheckNoResetAp(actionInfo);
            if (!checkNoResetAp && result.Count == 0 && !_triggerAfterChecked && !isTriggeredSkill)
            {
                // 行動者のターンを進める
                var removeStateInfos = _model.UpdateTurn();
                await RemoveStateInfoPopup(removeStateInfos);
                // Passive付与
                _model.CheckTriggerPassiveInfos(BattleUtility.AfterTriggerTimings(), null, null);

                // Passive解除
                //await RemovePassiveInfos();

                // 行動後にAP+
                var gainAp = _model.CheckActionAfterGainAp(actionInfo);
                if (gainAp > 0)
                {
                    if (!_skipBattle)
                    {
                        _view.StartHeal(_model.FirstActionBattler.Index.Value, DamageType.MpHeal, gainAp);
                        await UniTask.DelayFrame(_model.WaitFrameTime(16));
                    }
                    _model.ActionAfterGainAp(gainAp);
                    _view.RefreshStatus();
                }

            }
            var reaction = _model.CheckReaction(actionInfo);
            _model.TurnEnd(actionInfo);
            _model.ChangeBattlerInfosLineType();
            // 行動後に交代
            var change = _model.CheckActionAfterChange(actionInfo);
            if (change)
            {
                _view.UpdateFieldMembers(_model.Battlers);
                _view.UpdateGridLayer();
            }
            _view.UpdateActors(MakeListData(_model.ViewBattlerActors()));
            _view.SetEnemies(MakeListData(_model.ViewBattlerEnemies()));
            _view.SetIdle();
            if (reaction)
            {
                _view.SetBattleBusy(false);
                return;
            }
            if (!isTriggeredSkill)
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
                _view.StartStatePopup(addState.TargetIndex.Value, DamageType.State, "+" + addState.Master.Name);
            }
            // 透明が外れるケースを適用
            var removeShadowStates = _model.EndRemoveShadowState();
            foreach (var removeShadowState in removeShadowStates)
            {
                _view.StartStatePopup(removeShadowState.TargetIndex.Value, DamageType.State, "-" + removeShadowState.Master.Name);
            };
            // 戦闘不能の拘束ステートを解除する
            var removeChainStates = _model.EndRemoveState();
            foreach (var removeChainState in removeChainStates)
            {
                _view.StartStatePopup(removeChainState.TargetIndex.Value, DamageType.State, "-" + removeChainState.Master.Name);
            };

            // 待機できなくなった場合は待機状態をはずす
            _model.RemoveOneMemberWaitBattlers();
            //_view.UpdateGridLayer();
            _view.RefreshStatus();

            // 現アクションがまだならそのまま続ける
            var mainActionInfo = _model.FirstActionInfo;
            if (mainActionInfo != null)
            {
                _battleEnded = false;
                MakeResultInfoStartAction(mainActionInfo, mainActionInfo.CandidateTargetIndexList);
                return;
            }
            // 誘発行動があれば続ける
            var receiveActionInfo = _model.ReceiveActionInfo;
            if (receiveActionInfo != null)
            {
                _battleEnded = false;
                MakeResultInfoStartAction(receiveActionInfo, receiveActionInfo.CandidateTargetIndexList);
                return;
            }
            var linkage = _model.CheckLinkageBattlerInfo();
            if (linkage)
            {
                _view.UpdateActors(MakeListData(_model.ViewBattlerActors()));
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
            _view.HideEnemiesStatus();
        }
    }
}
