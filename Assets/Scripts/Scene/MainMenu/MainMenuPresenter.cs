using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.MainMenu;

namespace Ryneus
{
    public class MainMenuPresenter : BasePresenter
    {
        MainMenuModel _model = null;
        MainMenuView _view = null;

        private bool _busy = true;
        private CommandType _backCommand = CommandType.None;
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
            _view.SetCommandList(_model.MainMenuCommand());

            var bgm = await _model.GetBgmData("Mainmenu");
            SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            // 幕間に移動
            if (_model.InterludePhase())
            {
                _busy = true;
                var confirmInfo = new ConfirmInfo("Periodが終了しヘイムダルから定期報告が入りました。",(a) =>
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
                case "Status":
                    var actorInfos = _model.PartyInfo.ActorInfos;
                    CommandStatusInfo(actorInfos,false,true,true,false,actorInfos[0].ActorId.Value,() => 
                    {
                        _view.CommandRefresh();
                    },false,true);
                    break;
            }
        }

        private void CommandDepature()
        {
            _busy = true;
            _view.SetActiveCommandList(false);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.StageList,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    _view.SetActiveCommandList(true);
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandDeckEdit()
        {
            _busy = true;
            _view.SetActiveCommandList(false);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.DeckEdit,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    _view.SetActiveCommandList(true);
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandAchievement()
        {
            var rankup = CheckAchievements(true,() => CommandAchievement());
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

        private void ShowAchievementList()
        {
            _busy = true;
            _view.SetActiveCommandList(false);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Achievement,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    _view.SetActiveCommandList(true);
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandPresent()
        {
            _busy = true;
            _view.SetActiveCommandList(false);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.ItemList,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    _view.SetActiveCommandList(true);
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandRelief()
        {
            if (_model.PartyInfo.ThisPeriodReliefCount.Value > 0)
            {
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle("今は召喚に応じるエインフェリアはいないようだ…");
                _view.CommandCallCaution(cautionInfo);
                return;
            }
            var confirmInfo = new ConfirmInfo("エインフェリアを召喚しますか？",(a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _model.PartyInfo.ReliefCommandCount.GainValue(1);
                    _model.PartyNextPeriod();
                    _model.PartyInfo.ThisPeriodReliefCount.GainValue(1);
                    List<ActorInfo> actorInfos =_model.AddSelectActorInfos();
                    CommandAddActorStatusInfo(actorInfos,() =>
                    {
                        CheckAchievements();
                    });
                }
            });
            confirmInfo.SetBackEvent(() => {});
            _view.CommandCallConfirm(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            CommandCallSideMenu(MakeListData(_model.SideMenu()), () =>
            {
                _busy = false;
            });
        }

        private void CommandAritifact()
        {
            if (_model.PartyInfo.AritifactSkills().Count == 0)
            {
                return;
            }
            _busy = true;
            _view.SetActiveCommandList(false);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.ArtifactList,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    _view.SetActiveCommandList(true);
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }
    }
}