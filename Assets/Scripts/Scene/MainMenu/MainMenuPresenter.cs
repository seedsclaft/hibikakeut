using System;
using System.Collections.Generic;
using Ryneus.MainMenu;

namespace Ryneus
{
    public class MainMenuPresenter : BasePresenter
    {
        MainMenuModel _model = null;
        MainMenuView _view = null;

        private bool _busy = true;
        public MainMenuPresenter(MainMenuView view)
        {
            _view = view;
            SetView(_view);
            _model = new MainMenuModel();
            SetModel(_model);

            Initialize();
        }

        private void Initialize()
        {
            _view.SetHelpWindow();
            _view.SetEvent((type) => UpdateCommand(type));

            if (_model.IsEnding())
            {
                // エンディング再生
                var advInfo = _model.GetAdvCallInfo(101);
                advInfo.SetCallEvent(() =>
                {
                    _busy = false;
                    _view.ChangeUIActive(true);
                    _view.CommandGotoSceneChange(Scene.Result);
                });
                _view.CommandCallAdv(advInfo);
                _view.ChangeUIActive(false);
                return;
            }

            _view.SetCharaLayer(_model.MainMenuActorInfos());
            if (_model.SceneParam != null && _model.SceneParam.PeriodAnimation)
            {
                _model.SavePlayerStageData(Scene.MainMenu);
                _model.SaveAutoFile();
                SoundManager.Instance.FadeOutBgm();
                var chapter = _model.PartyInfo.Chapter.Value;
                var period = Math.Min(_model.PartyInfo.Period.Value, DataSystem.System.PeriodTurns);
                _view.MainMenuStartAnim(chapter, period, DataSystem.System.PeriodTurns, (DataSystem.System.PeriodTurns * 6) - (((_model.PartyInfo.Chapter.Value - 1) * DataSystem.System.PeriodTurns) + period));
                _view.SetActiveCommandList(false);
                _view.UpdateBattleFieldNotice(_model.HasBattleField());
                CommandRefresh();
                _busy = true;
            }
            else
            {
                _view.ClearMainMenuStart();
                CommandEndPeriodAnimation();
            }
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                switch (viewEvent.ViewCommandType.CommandType)
                {
                    case CommandType.EndPeriodAnimation:
                        CommandEndPeriodAnimation();
                        break;
                }
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.MainMenu)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.MainMenuCommand:
                    CommandMainMenuCommand((SystemData.CommandData)viewEvent.Template);
                    break;
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
                case CommandType.PartyInfo:
                    CommandPartyInfo();
                    break;
                case CommandType.SaveCommand:
                    CommandSaveCommand();
                    break;
                case CommandType.Aritifact:
                    CommandAritifact();
                    break;
            }
        }

        private async void CommandEndPeriodAnimation()
        {
            _view.UpdateBattleFieldNotice(_model.HasBattleField());

            await _model.PlayMainStageBgmData();

            //_model.SaveAutoFile();
            // 幕間に移動
            if (_model.InterludePhase())
            {
                _busy = true;
                // 転送を解除
                if (_model.PartyInfo.ActorInfos.Find(a => a.Transfer.Value) != null)
                {
                    _model.EndTransfer();
                    ReturnTransfer();
                    return;
                }
                SendInterlude();
                return;
            }
            // 信仰度がマイナス1回目
            if (_model.PartyInfo.EvaluationValue.Value < 0 && !_model.PartyInfo.EvaluationCaution.Value)
            {
                _busy = true;
                _model.PartyInfo.EvaluationValue.SetValue(1);
                _model.PartyInfo.EvaluationCaution.SetValue(true);
                // イベントを再生
                CallAdvEvent(20010, 0, () =>
                {
                    _busy = false;
                    _view.CommandGotoSceneChange(Scene.MainMenu);
                });
                return;
            }
            // 信仰度がマイナス2回目
            if (_model.PartyInfo.EvaluationValue.Value < 0 && _model.PartyInfo.EvaluationCaution.Value)
            {
                _busy = true;
                // イベントを再生
                CallAdvEvent(20020, 0, () =>
                {
                    _busy = false;
                    _view.CommandGotoSceneChange(Scene.Title);
                });
                return;
            }
            // メインメニューイベント再生
            var findEvent = _model.MainmenuEvent();
            if (findEvent != null)
            {
                _model.AddEventReadFlag(findEvent.EventKey);
                // イベントを再生
                CallAdvEvent(findEvent.Id, 0, () =>
                {
                    _busy = false;
                    _view.CallSystemCommand(Base.CommandType.SceneShowUI);
                    CommandEndPeriodAnimation();
                });
                return;
            }
            _view.SetActiveCommandList(true);
            _view.OpenAnimation();
            CommandRefresh();
            CheckTutorialState(null);
            _busy = false;
        }

        private void CommandMainMenuCommand(SystemData.CommandData commandData)
        {
            switch (commandData.Key)
            {
                case "Departure":
                    CommandDepature();
                    break;
                case "DeckEdit":
                    CommandDeckEdit();
                    break;
                case "Mission":
                    CommandAchievement();
                    break;
                case "Present":
                    CommandPresent();
                    break;
                case "Relief":
                    CommandRelief();
                    break;
                case "Transfer":
                    CommandTransfer();
                    break;
                case "Trade":
                    CommandTrade();
                    break;
                case "Status":
                    UpdateCommandSelecting(false);
                    var actorInfos = _model.PartyInfo.ActorInfos;
                    CommandActorStatusInfo(actorInfos, false, actorInfos[0].ActorId.Value, () =>
                    {
                        UpdateCommandSelecting(true);
                        CommandRefresh();
                    });
                    break;
            }
        }

        private void ReturnTransfer()
        {
            CallConfirmNoChoiceView(DataSystem.GetText(11040), (a) =>
            {
                SendInterlude();
            });
        }

        private void SendInterlude()
        {
            CallConfirmNoChoiceView(DataSystem.GetText(11020), (a) =>
            {
                _view.CommandGotoSceneChange(Scene.Interlude);
            });
        }

        private void ClosePopupView()
        {
            _busy = false;
            UpdateCommandSelecting(true);
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }

        private void CommandDepature()
        {
            // 未編成のキャラがいる
            if (_model.CheckBeforeDepature())
            {
                CallConfirmView(DataSystem.GetText(11030), (a) =>
                {
                    if (a == ConfirmCommandType.Yes)
                    {
                        CommandDepature();
                    }
                });
                return;
            }
            // 出撃できるステージがない
            if (_model.CheckDepatureDungeon())
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(32050));
                return;
            }
            _busy = true;
            UpdateCommandSelecting(false);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CallPopupView(PopupType.StageList, () =>
            {
                ClosePopupView();
            });
        }

        private void CommandDeckEdit()
        {
            _busy = true;
            UpdateCommandSelecting(false);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CallPopupView(PopupType.DeckEdit, () =>
            {
                ClosePopupView();
                CommandRefresh();
                _view.SetCharaLayer(_model.MainMenuActorInfos());
            });
        }

        private void CommandAchievement()
        {
            var rankup = CheckAchievements(true, CheckNewStage);
            if (rankup)
            {
                return;
            }
            /*
            if (getItemInfos.Count > 0)
            {
                var strategySceneInfo = new StrategySceneInfo
                {
                    ActorInfos = _model.PartyInfo.CurrentDeckActorInfos(),
                    InBattle = false
                };
                strategySceneInfo.GetItemInfos = getItemInfos;
                _view.CommandSceneChange(Scene.Strategy,strategySceneInfo);
                return;
            }
            */
            ShowAchievementList();
        }

        private void CheckNewStage()
        {
            // ステージが解放されたら表示
            var find = _model.StageInfos().Find(a => !a.Alarted.Value);
            if (find != null)
            {
                _model.PartyInfo.AlartStage(find.Master.StageNo);
                _busy = true;
                //SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(32100), (a) =>
                {
                    var find = _model.StageInfos().Find(a => !a.Alarted.Value);
                    if (find != null)
                    {
                        CheckNewStage();
                        return;
                    }
                    // 招聘コマンド回数が増えたらアナウンス
                    if (false && _model.PartyInfo.ReliefItemCount.Value > 0)
                    {
                        var reliefConfirmInfo = new ConfirmInfo(DataSystem.GetText(11014), (a) =>
                        {
                            _busy = false;
                            CommandAchievement();
                        });
                        reliefConfirmInfo.SetIsNoChoice(true);
                        _view.CommandCallConfirm(reliefConfirmInfo);
                    }
                    else
                    {
                        _busy = false;
                        CommandAchievement();
                    }
                }, ConfirmType.NewStageAlert);
                confirmInfo.SetStageInfo(find);
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
                return;
            }
            CommandAchievement();
        }

        private void ShowAchievementList()
        {
            _busy = true;
            UpdateCommandSelecting(false);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CallPopupView(PopupType.Achievement, () =>
            {
                ClosePopupView();
                CommandRefresh();
            });
        }

        private void CommandPresent()
        {
            _busy = true;
            UpdateCommandSelecting(false);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CallPopupView(PopupType.ItemList, () =>
            {
                ClosePopupView();
                CommandRefresh();
            });
        }

        private void CommandTransfer()
        {
            if (_model.PartyInfo.Chapter.Value <= 3)
            {
                return;
            }
            _busy = true;
            UpdateCommandSelecting(false);

            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CallPopupView(PopupType.Transfer, () =>
            {
                ClosePopupView();
                CommandRefresh();
                _view.SetCharaLayer(_model.MainMenuActorInfos());
            });
        }

        private void CommandTrade()
        {
            _busy = true;
            UpdateCommandSelecting(false);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CallPopupView(PopupType.Trade, () =>
            {
                ClosePopupView();
                CommandRefresh();
            });
        }

        private void CommandRelief()
        {
            var enableCount = _model.PartyInfo.ReliefItemCount.Value;
            if (enableCount <= 0)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(11011));
                return;
            }
            _busy = true;
            UpdateCommandSelecting(false);
            var countText = DataSystem.GetReplaceText(11010, enableCount.ToString());
            CallConfirmView(countText, (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    StartRelief();
                }
                else
                {
                    _busy = false;
                    UpdateCommandSelecting(true);
                    CommandRefresh();
                }
            });
        }

        private async void StartRelief()
        {
            _model.PartyInfo.ReliefItemCount.GainValue(-1);
            _model.PartyInfo.PartyStatInfo.ReliefCommandCount.GainValue(1);
            _view.CallSystemCommand(Base.CommandType.SceneHideUI);
            await _model.PlayReliefBgmData();
            // 結果を表示する
            var releifActorInfos = _model.ReleifActoInfos();
            _view.StartReliefAnimation(async () =>
            {
                await _model.PlayReliefBgmData2();
                var strategySceneInfo = new StrategySceneInfo
                {
                    ActorInfos = releifActorInfos,
                    InBattle = false
                };
                strategySceneInfo.GetItemInfos = _model.ReleifGetItemInfos(releifActorInfos);
                _view.CommandSceneChange(Scene.Strategy, strategySceneInfo);
                /*
                _view.CallSystemCommand(Base.CommandType.SceneShowUI);
                List<ActorInfo> actorInfos = _model.AddSelectActorInfos();
                CommandAddActorStatusInfo(actorInfos, async () =>
                {
                    await _model.PlayMainStageBgmData();
                    CheckAchievements();
                });
                */
            }, _model.PartyInfo.ActorInfos[0], releifActorInfos);
        }

        private void CommandPartyInfo()
        {
            _busy = true;
            UpdateCommandSelecting(false);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var actorInfos = _model.CurrentDeckActorInfos();
            CommandActorStatusInfo(actorInfos, false, actorInfos[0].ActorId.Value, () =>
            {
                _busy = false;
                UpdateCommandSelecting(true);
                CommandRefresh();
            });
        }

        private void CommandSaveCommand()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            UpdateCommandSelecting(false);
            var sceneParam = new FileListSceneInfo
            {
                IsLoad = false
            };
            CallPopupView(PopupType.FileList, () =>
            {
                ClosePopupView();
                CommandRefresh();
            }, sceneParam);
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            UpdateCommandSelecting(false);
            Func<SystemData.CommandData, bool> batch = (sideMenu) =>
            {
                // 仲間強化関連の課題がある
                if (sideMenu.Key == "Status")
                {
                    return _model.IsStatusBatch();
                }
                return false;
            };
            CommandCallSideMenu(MakeListData(_model.SideMenu(), null, null, batch, 0), () =>
            {
                _busy = false;
                UpdateCommandSelecting(true);
                CommandRefresh();
            });
        }

        private void CommandAritifact()
        {
            if (_model.PartyInfo.AritifactSkills().Count == 0)
            {
                return;
            }
            _busy = true;
            UpdateCommandSelecting(false);
            CallPopupView(PopupType.ArtifactList, () =>
            {
                ClosePopupView();
            });
        }

        private void UpdateCommandSelecting(bool isSelecting)
        {
            _view.SetActiveCommandList(isSelecting);
            _view.SetActiveParticleObject(isSelecting);
        }

        private void CommandRefresh()
        {
            _view.UpdateCommandList(_model.MainMenuCommand());
            _view.CommandRefresh();
            _view.UpdateSidemenuBatch(_model.IsSideManuBatch());
        }

        private void CheckTutorialState(object commandType = null)
        {
            Func<TutorialData, bool> enable = (tutorialData) =>
            {
                var checkFlag = false;
                if (tutorialData.Param1 == 0 || tutorialData.Param1 == 100)
                {
                    // 出撃選択
                    checkFlag = true;
                }
                return checkFlag;
            };
            Action<TutorialData> checkTrue = (tutorialData) =>
            {
                _view.SetActiveCommandList(false);
                _busy = true;
                CallPopupView(PopupType.Tutorial, () =>
                {
                    _busy = false;
                    CommandEndPeriodAnimation();
                }, tutorialData);
            };
            Func<TutorialData, bool> checkEnd = (tutorialData) =>
            {
                return true;
            };
            var tutorialViewInfo = new TutorialViewInfo
            {
                SceneType = (int)Scene.MainMenu,
                CheckEndMethod = checkEnd,
                CheckMethod = enable,
                CheckTrueAction = checkTrue,
                EndEvent = () =>
                {
                    _busy = false;
                    _view.SetActiveCommandList(true);
                    CheckTutorialState(commandType);
                }
            };
            _view.CommandCheckTutorialState(tutorialViewInfo);
        }
    }
}