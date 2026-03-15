using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Ryneus.Battle;

namespace Ryneus
{
    public partial class BattlePresenter : BasePresenter
    {
        BattleModel _model = null;
        BattleView _view = null;

        private bool _busy = true;
        private bool _skipBattle = false;
        private bool _beforeBattle = true;
#if UNITY_EDITOR
        private bool _debug = false;
        public void SetDebug(bool busy)
        {
            _debug = busy;
        }
        private bool _testBattle = false;
#endif
        private bool _triggerAfterChecked = false;
        /*
        private bool _triggerInterruptChecked = false;
        private bool _triggerUseBeforeChecked = false;
        private bool _triggerOpponentBeforeChecked = false;
        */
        private bool _slipDamageChecked = false;
        private bool _regenerateChecked = false;
        private bool _battleEnded = false;
        private CommandType _backCommandType = CommandType.None;
        public BattlePresenter(BattleView view)
        {
            _view = view;
            SetView(_view);
            _model = new BattleModel();
            SetModel(_model);

#if UNITY_EDITOR
            _view.gameObject.AddComponent<DebugBattleData>();
            var debugger = _view.gameObject.GetComponent<DebugBattleData>();
            debugger.SetDebugger(_model, this, _view);
            debugger.consoleInputField = GameSystem.DebugBattleData.consoleInputField;
#endif
            _view.SetHelpText("");
            Initialize();
        }

        private async void Initialize()
        {
            _view.SetBattleBusy(true);
            _model.CreateBattleData();

            await ViewInitialize();
            BattleChecker.Instance.SetModel(_model, _view);
            _view.CommandCallLoading();
            await _model.LoadEffects();
            _view.CommandCloseLoading();

            _view.CommandStartTransition(() =>
            {
                _view.CallSystemCommand(Base.CommandType.ClosePopup);
                StartBattle();
            });
        }

        public async Task ViewInitialize()
        {
            await _view.SetBackGround(_model.CurrentStage.Master.BackGround);

            _view.ClearCurrentSkillData();
            _view.CreateObject();
            _view.RefreshTurn(_model.TurnCount);
            _view.SetBattleAutoButton(_model.IsBattleAuto());
            _view.ChangeBackCommandActive(false);
            _view.SetActiveBattleAutoButton(false);
            _view.SetBattleSpeedButton(OptionUtility.CurrentBattleSpeedText());
            _view.SetBattleSkipButton(DataSystem.GetText(16010));
            _view.SetSkillLogButton(DataSystem.GetText(16020));
            _view.SetActors(MakeListData(_model.ViewBattlerActors()));
            _view.SetEnemies(MakeListData(_model.ViewBattlerEnemies()));
            _view.SetGridMembers(_model.Battlers);
            await _view.SetFieldMembers(_model.Battlers);
            _view.BattlerBattleClearSelect();

            _view.RefreshStatus();
            _view.EndActionSelect();
#if UNITY_EDITOR
            if (_view.TestMode && _view.TestBattleMode)
            {
                StartBattle();
                _model.MakeTestBattleAction();
                _testBattle = _model.testActionDates.Count > 0;
                return;
            }
#endif
        }

        private async void StartBattle()
        {
            _view.SetHelpInputInfo("BATTLE");
            _view.SetEvent((type) => UpdateCommand(type));
            _view.StartUIAnimation();
            _view.StartBattleStartAnim(_model.BattleStartText());
            _view.SetActiveBattleAutoButton(true);
            //_view.StartBattle(_model.BattlerEnemies().Count);
            await UniTask.WaitUntil(() => !_view.StartAnimIsBusy);
            //_view.SetBattleSkipActive(true);
            // バトル開始と並べ替え選択待ち
            _view.SetActiveBeforeBattles(true);
            _view.ShowEnemiesStatus();
            _busy = false;
        }

        private void CheckTutorialState(CommandType commandType = CommandType.None)
        {
            /*
            Func<TutorialData,bool> enable = (tutorialData) => 
            {
                var checkFlag = true;
                if (tutorialData.Param1 == 600)
                {
                    // 初めてボスバトル
                    checkFlag = _model.CurrentSelectRecord().SymbolType == SymbolType.Boss;
                }
                return checkFlag;
            };
            Func<TutorialData,bool> checkEnd = (tutorialData) => 
            {
                var checkFlag = true;
                if (tutorialData.Param3 == 610)
                {
                    checkFlag = false;
                }
                if (tutorialData.Param3 == 620)
                {
                    checkFlag = _model.TargetEnemy != null && _model.TargetEnemy.Index > 100;
                    if (checkFlag)
                    {
                        _busy = false;
                    }
                }
                return checkFlag;
            };
            var tutorialViewInfo = new TutorialViewInfo
            {
                SceneType = (int)Scene.Battle,
                CheckEndMethod = checkEnd,
                CheckMethod = enable,
                CheckTrueAction = () => 
                {
                    _busy = true;
                },
                EndEvent = () => 
                {
                    _busy = false;
                    CheckTutorialState(commandType);
                }
            };
            _view.CommandCheckTutorialState(tutorialViewInfo);
            */
        }

        private async Task UpdateCommand(ViewEvent viewEvent)
        {
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Battle)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.ChangeBattleAuto:
                    CommandChangeBattleAuto();
                    break;
                case CommandType.UpdateBattleAuto:
                    CommandUpdateBattleAuto();
                    break;
                case CommandType.ChangeBattleSpeed:
                    CommandChangeBattleSpeed((int)viewEvent.Template);
                    break;
                case CommandType.SkipBattle:
                    CommandSkipBattle();
                    break;
                case CommandType.ForceVictory:
                    CommandForceVictory();
                    break;
                case CommandType.StopApCount:
                    CommandStopApCount((bool)viewEvent.Template);
                    break;
            }
            if (_busy)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.DecideBattle:
                    await CommandDecideBattle();
                    break;
                case CommandType.Formation:
                    CommandFormation();
                    break;
                case CommandType.EndFormation:
                    CommandEndFormation();
                    break;
                case CommandType.SelectCharacter:
                    CommandSelectCharacter((int)viewEvent.Template);
                    break;
                case CommandType.UpdateAp:
                    // ターンスキップあり
                    if (GameSystem.OptionData.BattleTurnSkip)
                    {
                        CommandTurnSkip();
                        break;
                    }
                    CommandUpdateAp();
                    break;
                case CommandType.OnDecideSkill:
                    CommandDecideSkill();
                    break;
                case CommandType.OnSelectSkill:
                    CommandOnSelectSkill((SkillInfo)viewEvent.Template);
                    break;
                case CommandType.OnSelectTarget:
                    CommandOnSelectTarget((InputKeyType)viewEvent.Template);
                    break;
                case CommandType.OnSelectTargetCursor:
                    CommandOnSelectTargetCursor((BattlerInfo)viewEvent.Template);
                    break;
                case CommandType.OnDecideEnemy:
                    CommandOnDecideEnemy((BattlerInfo)viewEvent.Template);
                    break;
                case CommandType.OnCancelEnemy:
                    CommandOnCancelEnemy();
                    break;
                case CommandType.OnDecideActor:
                    CommandOnDecideActor((BattlerInfo)viewEvent.Template);
                    break;
                case CommandType.OnCancelActor:
                    CommandOnCancelActor();
                    break;
                case CommandType.AttributeType:
                    //RefreshSkillInfos();
                    break;
                case CommandType.StartSelect:
                    CommandStartSelect();
                    break;
                case CommandType.Back:
                    CommandBack();
                    break;
                case CommandType.Escape:
                    CommandEscape();
                    break;
                case CommandType.EnemyDetail:
                    CommandEnemyDetail((BattlerInfo)viewEvent.Template);
                    break;
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
                case CommandType.SkillLog:
                    CommandSkillLog();
                    break;
            }
            //CheckTutorialState(viewEvent.commandType);
        }

        private async Task CommandDecideBattle()
        {
            if (!_beforeBattle)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _beforeBattle = false;
            _model.CreateBattleRecords();
            _view.SetActiveBeforeBattles(false);
            _view.HideEnemiesStatus();
            _view.EndFormation();
            _view.UpdateStartActivate();

            _view.UpdateSelectCursor(new List<int>() { });
            CommandStartBattleAction();
            _view.SetStartActors();
            //_view.StartBattleStartAnim("Battle Start!");
            await UniTask.WaitUntil(() => !_view.AllBusy);
            _view.SetBattleBusy(false);
            _view.SetIdle();
        }

        private void CommandFormation()
        {
            if (!_beforeBattle)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.StartFormation();
        }

        private void CommandSelectCharacter(int selectIndex)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            if (_model.SelectIndex.Value > -1)
            {
                var adjust = _model.SwapSelectIndex(selectIndex);
                if (adjust)
                {
                    CommandCautionInfo(DataSystem.GetText(43010));
                }
                _view.SetActors(MakeListData(_model.ViewBattlerActors(), selectIndex));
                _view.StartFormation();
                _view.UpdateSelectCursor(new List<int>() { });
                return;
            }
            _model.SelectIndex.SetValue(selectIndex);
            _view.UpdateSelectCursor(new List<int>() { _model.SelectedCharacterIndex() });
        }

        private void CommandEndFormation()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.UpdateSelectCursor(new List<int>() { });
            _view.CancelFormation();
            _model.SelectIndex.SetValue(-1);
        }

        private void CommandBack()
        {
            if (_backCommandType != CommandType.None)
            {
                //UpdateCommand(_backCommandType);
            }
        }

        private void CommandEscape()
        {
        }

        private void CommandEnemyDetail(BattlerInfo battlerInfo)
        {
            if (battlerInfo == null)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            CommandEnemyInfo(new List<BattlerInfo>() { battlerInfo }, true, () =>
            {
                _busy = false;
            });
        }

        /// <summary>
        /// バトル開始時のパッシブを付与
        /// </summary>
        private void CommandStartBattleAction()
        {
            _view.UpdateGridLayer();
            _model.CheckTriggerPassiveInfos(BattleUtility.StartTriggerTimings(), null, null);
            // 開始誘発を発動
            var receiveActionInfo = _model.ReceiveActionInfo;
            if (receiveActionInfo != null)
            {
                _view.SetBattleBusy(true);
                _model.SetActiveActionInfo(receiveActionInfo);
                StartActionInfo(receiveActionInfo);
                //MakeResultInfoStartAction(receiveActionInfo,receiveActionInfo.CandidateTargetIndexList);
            }
        }

        private void CommandUpdateAp()
        {
            var currentBattler = CheckApCurrentBattler();
            if (currentBattler != null)
            {
                CommandStartSelect();
                return;
            }
            CheckUpdateAp();
            if (GameSystem.OptionData.BattleSpeed == 2)
            {
                currentBattler = CheckApCurrentBattler();
                if (currentBattler != null)
                {
                    CommandStartSelect();
                    return;
                }
                CheckUpdateAp();
            }
            if (GameSystem.OptionData.BattleSpeed > 2)
            {
                currentBattler = CheckApCurrentBattler();
                if (currentBattler != null)
                {
                    CommandStartSelect();
                    return;
                }
                CheckUpdateAp();
            }
        }

        private async void CheckUpdateAp()
        {
            if (IsBattleEnd())
            {
                BattleEnd();
                return;
            }
            var removeStateList = _model.UpdateAp();
            if (removeStateList.Count > 0)
            {
                _view.ClearDamagePopup();
                foreach (var removeState in removeStateList)
                {
                    _view.StartStatePopup(removeState.TargetIndex.Value, DamageType.State, "-" + removeState.Master.Name);
                }
                // Passive解除
                await RemovePassiveInfos();
            }
            _view.UpdateGridLayer();
        }

        private void CommandTurnSkip()
        {
            while (CheckApCurrentBattler() == null)
            {
                CheckUpdateAp();
            }
            CommandStartSelect();
        }

        private void PlayDamageSound(DamageType damageType)
        {
            if (damageType == DamageType.HpDamage)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Damage);
            }
            else
            if (damageType == DamageType.HpCritical)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Critical);
            }
        }

        private void StartDeathAnimation(List<ActionResultInfo> actionResultInfos)
        {
            var deathBattlerIndexes = _model.DeathBattlerIndex(actionResultInfos);
            foreach (var deathBattlerIndex in deathBattlerIndexes)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Defeat);
                _view.StartDeathAnimation(deathBattlerIndex);
            }
        }

        private void StartAliveAnimation(List<ActionResultInfo> actionResultInfos)
        {
            var aliveBattlerIndexes = _model.AliveBattlerIndex(actionResultInfos);
            foreach (var aliveBattlerIndex in aliveBattlerIndexes)
            {
                _view.StartAliveAnimation(aliveBattlerIndex);
            }
        }

        /*
        private void RefreshSkillInfos()
        {
            var skillInfos = _model.SkillActionList();
            _view.RefreshMagicList(GetListData(skillInfos),_model.SelectSkillIndex(skillInfos));
            //SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        }
        */

        private bool IsBattleEnd()
        {
            return _model.CheckVictory() || _model.CheckDefeat() || _model.CheckIsOver();
        }

        private async void BattleEnd()
        {
            if (_battleEnded)
            {
                return;
            }
            var strategySceneInfo = new StrategySceneInfo
            {
                BattlerInfos = _model.Battlers,
                InBattle = true
            };
            if (_model.CheckDefeat())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(16110));
                await UniTask.DelayFrame((int)(150f / GameSystem.OptionData.BattleSpeed));
                // 敗北可のバトルの場合
                if (_model.IsEnableDefeat())
                {
                    var dungeonSceneInfo = new DungeonSceneInfo
                    {
                        BattleEnd = true
                    };
                    _view.CommandSceneChange(Scene.Dungeon, dungeonSceneInfo);
                    return;
                }
                _view.CallSystemCommand(Base.CommandType.MapClear);
                _view.CommandGotoSceneChange(Scene.Title);
                return;
                /*
                _view.StartBattleStartAnim(DataSystem.GetText(16110));
                strategySceneInfo.GetItemInfos = new List<GetItemInfo>();
                _model.MakeBattleScore(false, strategySceneInfo);
                strategySceneInfo.BattleScore.TurnCount = -1;
                strategySceneInfo.BattleResultVictory = false;
                strategySceneInfo.ReturnScene = Scene.MainMenu;
                CheckAchievements();
                //_model.CurrentStage.GainLoseCount();
                */
            }
            else
            if (_model.CheckVictory())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(16100));
                _view.BattleVictory(_model.BattlerActors()[0].Index.Value);
                strategySceneInfo.GetItemInfos = _model.MakeBattlerResult();
                _model.MakeBattleScore(true, strategySceneInfo);
                strategySceneInfo.BattleScore.TurnCount = _model.TurnCount;
                strategySceneInfo.BattleResultVictory = true;
                _model.AddEnemyInfoSkillId();
                _model.PartyInfo.PartyStatInfo.BattleVictoryCount.GainValue(1);
                CheckAchievements();
            }/* else
            if (_model.CheckIsOver())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(16100));
                _view.BattleVictory(_model.BattlerActors()[0].Index.Value);
                strategySceneInfo.GetItemInfos = _model.MakeBattlerResult();
                strategySceneInfo.BattleTurn = _model.TurnCount;
                strategySceneInfo.BattleResultScore = _model.MakeBattleScore(true,strategySceneInfo);
                strategySceneInfo.BattleResultVictory = true;
                _model.AddEnemyInfoSkillId();
            }
            */

            _model.EndBattle();
            _battleEnded = true;
            _view.HideStateOverlay();
            if (_skipBattle)
            {
                _view.CommandCallLoading();
            }
            await UniTask.DelayFrame((int)(150f / GameSystem.OptionData.BattleSpeed));
            //_view.SetBattleBusy(false);
            /*
            if (SoundManager.Instance.CrossFadeMode)
            {
                SoundManager.Instance.ChangeCrossFade();
            } else
            {
                PlayTacticsBgm();
            }
            */

            BattleChecker.Instance.SetModel(null, null);
            _view.CommandCloseLoading();
            //_view.CommandChangeViewToTransition(null);
            _view.CommandGotoSceneChange(Scene.Strategy, strategySceneInfo);
        }

        private void CommandSkillLog()
        {
            /*
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var SkillLogViewInfo = new SkillLogViewInfo(_model.SkillLogs,() => 
            {
                _busy = false;
            });

            _view.CallSystemCommand(Base.CommandType.CallSkillLogView,SkillLogViewInfo);
            */
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            CommandCallSideMenu(MakeListData(_model.SideMenu(), 0), () =>
            {
                _busy = false;
            });
        }

        private void CommandForceVictory()
        {
            _model.ForceVictory();
        }

        private void CommandStopApCount(bool isStop)
        {
            _model.StopApCount(isStop);
        }

        private void CommandChangeBattleAuto()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.ChangeBattleAuto();
            CommandUpdateBattleAuto();
        }

        private void CommandUpdateBattleAuto()
        {
            _view.ChangeBattleAuto(_model.IsBattleAuto());
            if (!_view.AnimationBusy.Value && _view.BattleSeekBusy.Value && _model.IsBattleAuto())
            {
                _model.ClearActionInfo();
                _view.BattlerBattleClearSelect();
                _view.HideSkillActionList();
                _view.HideBattleThumb();
                MakeActionInfoSkillTrigger();
            }
        }

        private void CommandChangeBattleSpeed(int plus)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            OptionUtility.ChangeBattleSpeed(plus);
            _view.SetBattleSpeedButton(OptionUtility.CurrentBattleSpeedText());
        }

        private void CommandSkipBattle()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _skipBattle = true;
            _view.CommandCallLoading();
        }
    }
}