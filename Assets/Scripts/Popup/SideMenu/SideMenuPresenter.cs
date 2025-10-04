using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.SideMenu;

namespace Ryneus
{
    public class SideMenuPresenter : BasePresenter
    {
        SideMenuModel _model = null;
        SideMenuView _view = null;

        private bool _busy = true;
        public SideMenuPresenter(SideMenuView view)
        {
            _view = view;
            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize();
        }

        private void Initialize()
        {
            _model = new SideMenuModel();
            SetModel(_model);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            ClosePopup();
            CommandRefresh();
            _view.SetSideMenuViewInfo(_model.SceneParam);
            _view.OpenAnimation();
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.SideMenu)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.Initialize:
                    Initialize();
                    break;
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu((SystemData.CommandData)viewEvent.Template);
                    break;
            }
        }

        private void CommandSelectSideMenu(SystemData.CommandData commandData)
        {
            if (commandData == null)
            {
                return;
            }
            switch (commandData.Key)
            {
                case "Status":
                    CommandStatus();
                    break;
                case "Return":
                    CommandReturn();
                    break;
                case "Artifact":
                    CommandAritifact();
                    break;
                case "Option":
                    CommandOption();
                    break;
                case "Retire":
                    CommandDropout();
                    break;
                case "Help":
                    CommandRule();
                    break;
                case "Save":
                    CommandSave();
                    break;
                case "License":
                    CommandCredit();
                    break;
                case "InitializeData":
                    CommandInitializeData();
                    break;
                case "DeleteStage":
                    CommandDeleteStage();
                    break;
                case "Title":
                    CommandTitle();
                    break;
                case "EndGame":
                    CommandEndGame();
                    break;
                case "Dictionary":
                    CommandDictionary();
                    break;
            }
        }

        private void CommandStatus()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var actorInfos = _model.CurrentDeckActorInfos();
            CommandStatusInfo(actorInfos,false,true,true,false,actorInfos[0].ActorId.Value,() => 
            {
                ClosePopup();
            });
        }

        private void CommandReturn()
        {
            // 帰還できない
            if (_model.PartyInfo.Cursed.Value)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(10131));
                _view.CommandCallCaution(cautionInfo);
                ClosePopup();
                return;
            }
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var textId = _model.CurrentStage.Master.OnlyOnce ? 10133 : 10130;
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(textId), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    var checkNotSeekPeriod = _model.CheckNotSeekPeriod();
                    if (checkNotSeekPeriod != null)
                    {
                        var cautionInfo = new CautionInfo();
                        cautionInfo.SetTitle(DataSystem.GetReplaceText(10200, checkNotSeekPeriod.Master.Name));
                        _view.CommandCallCaution(cautionInfo);
                    }
                    _model.ReturnDungeon();
                    ClosePopup();
                    _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
                    _view.CallSystemCommand(Base.CommandType.MapClear);
                    var periodItemInfos = _model.PeriodGetItemInfos();
                    if (periodItemInfos.Count > 0)
                    {
                        var strategySceneInfo = new StrategySceneInfo
                        {
                            ActorInfos = _model.PartyInfo.CurrentDeckActorInfos(),
                            InBattle = false,
                            GetItemInfos = periodItemInfos,
                            ReturnScene = Scene.MainMenu,
                        };
                        _view.CommandSceneChange(Scene.Strategy, strategySceneInfo);
                    } else
                    {
                        _view.CommandSceneChange(Scene.MainMenu);
                    }
                }
            });
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandAritifact()
        {
            if (_model.PartyInfo.AritifactSkills().Count == 0)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(37020));
                ClosePopup();
                _view.CommandCallCaution(cautionInfo);
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.ArtifactList,
                template = null,
                EndEvent = () =>
                {
                    ClosePopup();
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandOption()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.CommandCallOption(() =>
            {
                ClosePopup();
                //_view.CommandGameSystem(Base.CommandType.ClosePopup);
            });
        }

        private void CommandDropout()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(1100), (a) => UpdatePopupDropout(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupDropout(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                //_model.SavePlayerStageData(false);
                //_view.CallSystemCommand(Base.CommandType.CloseStatus);
                //_view.CommandGotoSceneChange(Scene.MainMenu);
            }
            else
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            }
            ClosePopup();
            //_view.ActivateCommandList();
            _view.CallSystemCommand(Base.CommandType.CloseConfirm);
        }


        private void CommandRule()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Ruling,
                EndEvent = () =>
                {
                    ClosePopup();
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
        }

        public void CommandSave()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var sceneParam = new FileListSceneInfo
            {
                IsLoad = false
            };
            var popupInfo = new PopupInfo()
            {
                PopupType = PopupType.FileList,
                EndEvent = () =>
                {
                    ClosePopup();
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                },
                template = sceneParam
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandCredit()
        {
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Credit,
                EndEvent = () =>
                {
                    ClosePopup();
                }
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandInitializeData()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(13300), (a) => UpdatePopupDeletePlayerData(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandDeleteStage()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(13301), (a) => UpdatePopupDeleteStageData(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandTitle()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(13320), (a) => UpdatePopupTitle((ConfirmCommandType)a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupDeletePlayerData(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _view.CallSystemCommand(Base.CommandType.ClosePopup);
                _model.DeletePlayerData();
                _view.CallSystemCommand(Base.CommandType.CloseConfirm);
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(13310), (a) =>

                {
                    SoundManager.Instance.StopBgm();
                    _view.CallSystemCommand(Base.CommandType.CloseStatus);
                    _view.CommandGotoSceneChange(Scene.Boot);
                });
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
            }
            else
            {

                ClosePopup();
            }
        }

        private void UpdatePopupDeleteStageData(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _view.CallSystemCommand(Base.CommandType.ClosePopup);
                _model.DeleteStageData();
                _view.CallSystemCommand(Base.CommandType.CloseConfirm);
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(13310), (a) =>

                {
                    SoundManager.Instance.StopBgm();
                    _view.CallSystemCommand(Base.CommandType.CloseStatus);
                    _view.CommandGotoSceneChange(Scene.Boot);
                });
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
            }
            else
            {

                ClosePopup();
            }
        }

        private void UpdatePopupTitle(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
                _view.CallSystemCommand(Base.CommandType.MapClear);
                _view.CommandGotoSceneChange(Scene.Title);
            }
            ClosePopup();
        }

        private void CommandEndGame()
        {
            _busy = true;
#if !UNITY_EDITOR
            Application.Quit();
#endif
        }

        private void CommandDictionary()
        {
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Dictionary,
                EndEvent = () =>
                {
                    ClosePopup();
                }
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void ClosePopup()
        {
            _busy = false;
            _view.ActivateSideMenu();
            CommandRefresh();
        }

        private void CommandRefresh()
        {
            _view.SetHelpInputInfo("SIDEMENU");
        }
    }
}