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

        private async void Initialize()
        {
            _view.SetHelpWindow();
            _view.SetEvent((type) => UpdateCommand(type));

            if (_model.IsEnding())
            {
                // エンディング再生
                var advInfo = new AdvCallInfo();
                advInfo.Label.SetValue(_model.GetAdvFile(101));
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

            _view.SetCharaLayer(_model.PartyInfo.CurrentDeckActorInfos());
            CommandRefresh();
            _view.UpdateBattleFieldNotice(_model.HasBattleField());

            var bgm = await _model.GetMainStageBgmData();
            SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            // 幕間に移動
            if (_model.InterludePhase())
            {
                _busy = true;
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(11020),(a) =>
                {
                    _view.CommandGotoSceneChange(Scene.Interlude);
                });
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
                return;
            }
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
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
                case CommandType.Aritifact:
                    CommandAritifact();
                    break;
            }
        }

        private void CommandMainMenuCommand(SystemData.CommandData commandData)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
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
                case "Release":
                    CommandRelease();
                    break;
                case "Trade":
                    CommandTrade();
                    break;
                case "Status":
                    UpdateCommandSelecting(false);
                    var actorInfos = _model.PartyInfo.ActorInfos;
                    CommandStatusInfo(actorInfos,false,true,true,false,actorInfos[0].ActorId.Value,() => 
                    {
                        UpdateCommandSelecting(true);
                        CommandRefresh();
                    },false,true);
                    break;
            }
        }

        private void CommandDepature()
        {
            // 未編成のキャラがいる
            if (_model.CheckBeforeDepature())
            {
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(11030),(a) =>
                {
                    if (a == ConfirmCommandType.Yes)
                    {
                        CommandDepature();
                    }
                });
                confirmInfo.SetBackEvent(() => {});
                _view.CommandCallConfirm(confirmInfo);
                return;
            }
            // 出撃できるステージがない
            if (_model.CheckDepatureDungeon())
            {
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(32050));
                _view.CommandCallCaution(cautionInfo);
                return;
            }
            _busy = true;
            UpdateCommandSelecting(false);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.StageList,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    UpdateCommandSelecting(true);
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView, popupInfo);
        }

        private void CommandDeckEdit()
        {
            _busy = true;
            UpdateCommandSelecting(false);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.DeckEdit,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    UpdateCommandSelecting(true);
                    CommandRefresh();
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    _view.SetCharaLayer(_model.PartyInfo.CurrentDeckActorInfos());
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandAchievement()
        {
            var rankup = CheckAchievements(true,() => CheckNewStage());
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
                _model.PartyInfo.AlartStage(find.StageId.Value);
                _busy = true;
                _view.SetBusy(true);
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(32100), (a) =>
                {
                    _busy = false;
                    _view.SetBusy(false);
                    CommandAchievement();
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
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Achievement,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    UpdateCommandSelecting(true);
                    CommandRefresh();
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandPresent()
        {
            _busy = true;
            UpdateCommandSelecting(false);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.ItemList,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    UpdateCommandSelecting(true);
                    CommandRefresh();
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandTransfer()
        {
            if (_model.PartyInfo.MissionRank.Value <= 3)
            {
                return;
            }
            _busy = true;
            UpdateCommandSelecting(false);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Transfer,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    UpdateCommandSelecting(true);
                    CommandRefresh();
                    _view.SetCharaLayer(_model.PartyInfo.CurrentDeckActorInfos());
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandRelease()
        {
            _busy = true;
            UpdateCommandSelecting(false);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.ReleaseList,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    UpdateCommandSelecting(true);
                    CommandRefresh();
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView, popupInfo);
        }

        private void CommandTrade()
        {
            _busy = true;
            UpdateCommandSelecting(false);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Trade,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    UpdateCommandSelecting(true);
                    CommandRefresh();
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView, popupInfo);
        }

        private void CommandRelief()
        {
            var enableCount = _model.PartyInfo.MissionRank.Value - _model.PartyInfo.ReliefCommandCount.Value;
            if (enableCount <= 0)
            {
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(11011));
                _view.CommandCallCaution(cautionInfo);
                return;
            }
            _busy = true;
            UpdateCommandSelecting(false);
            var countText = DataSystem.GetReplaceText(11010, enableCount.ToString());
            var confirmInfo = new ConfirmInfo(countText, (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _model.PartyInfo.ReliefCommandCount.GainValue(1);
                    _model.PartyNextPeriod(true);
                    List<ActorInfo> actorInfos =_model.AddSelectActorInfos();
                    CommandAddActorStatusInfo(actorInfos,() =>
                    {
                        CheckAchievements();
                    });
                } else
                {
                    _busy = false;
                    UpdateCommandSelecting(true);
                    CommandRefresh();
                }
            });
            confirmInfo.SetBackEvent(() => {});
            _view.CommandCallConfirm(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
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
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.ArtifactList,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    UpdateCommandSelecting(true);
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
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
    }
}