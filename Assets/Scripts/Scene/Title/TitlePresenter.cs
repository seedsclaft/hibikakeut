using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.Title;
using System.Threading.Tasks;

namespace Ryneus
{
    public class TitlePresenter : BasePresenter
    {
        TitleModel _model = null;
        TitleView _view = null;
        private bool _busy = true;
        public TitlePresenter(TitleView view)
        {
            _view = view;
            SetView(_view);
            _model = new TitleModel();
            SetModel(_model);

            Initialize();
        }

        private async void Initialize()
        {
            _busy = true;
            OptionUtility.ApplyOptionData();

            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetVersion(_model.VersionText());
            CommandRefresh();
            if (!SaveSystem.ExistsLoadPlayerFile())
            {
                SaveSystem.SavePlayerInfo();
                await SaveSystem.LoadPlayerInfo();
            }
            else
            {
                var loadSuccess = await SaveSystem.LoadPlayerInfo();
                /*
                if (!loadSuccess)
                {
                    CallConfirmNoChoiceView(DataSystem.GetText(13330), (a) => UpdatePopup(a));
                    return;
                }
                */
                // プレイヤーネームを設定しなおし
                _view.CallSystemCommand(Base.CommandType.DecidePlayerName, GameSystem.CurrentData.PlayerInfo.PlayerName.Value);
            }
            _view.SetTitleCommand(_model.TitleCommand());
            var bgmData = DataSystem.GetBGMByKey("Title");
            var bgm = await _model.GetBgmData("Title");
            SoundManager.Instance.PlayBgm(bgm, bgmData.Volume, true);
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Title)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
                case CommandType.SelectTitle:
                    CommandSelectTitle((SystemData.CommandData)viewEvent.Template);
                    break;
            }
        }

        private void CommandSelectTitle(SystemData.CommandData titleCommand)
        {
            switch (titleCommand.Key)
            {
                case "NEWGAME":
                    CommandNewGame();
                    return;
                case "CONTINUE":
                    CommandContinue();
                    return;
                case "OPTION":
                    CommandOption();
                    return;
                case "TUTORIAL":
                    CommandTutorial();
                    return;
            }
        }

        private void CommandNewGame()
        {
            _busy = true;
            CallConfirmView("オープニングスキップ?", (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _model.InitializeNewGameSkipOpening();
                    _view.CommandGotoSceneChange(Scene.Dungeon);
                }
                else
                {
                    _model.InitializeNewGame();
                    SoundManager.Instance.PlayStaticSe(SEType.PlayStart);
                    _view.WaitFrame(2, () =>
                    {
                        _busy = false;
                        var gameStartEvent = _model.GameStartEventAdv();
                        if (gameStartEvent != null)
                        {
                            // イベントを再生
                            CallAdvEvent(gameStartEvent, 0, () =>
                            {
                                _view.CommandGotoSceneChange(Scene.Dungeon);
                            });
                            return;
                        }
                        _view.CommandGotoSceneChange(Scene.Dungeon);
                        //_view.CommandGotoSceneChange(Scene.Tactics);
                        //_view.CommandGotoSceneChange(Scene.NameEntry);
                    });
                }
            });
        }

        private void CommandContinue()
        {
            if (!_model.ExistsLoadFile())
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            _view.SetBusy(true);
            var sceneParam = new FileListSceneInfo
            {
                IsLoad = true
            };
            CallPopupView(PopupType.FileList, () =>
            {
                _view.SetBusy(false);
                _busy = false;
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            }, sceneParam);
        }

        private void CommandOption()
        {
            _busy = true;
            _view.SetBusy(true);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CallPopupView(PopupType.Option, () =>
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _model.UpdateOptionData();
                _view.SetBusy(false);
                _busy = false;
                CommandRefresh();
            });
        }

        private void CommandTutorial()
        {
        }

        private void CommandRefresh()
        {
            _view.SetHelpInputInfo("TITLE");
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            _view.SetBusy(true);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandCallSideMenu(MakeListData(_model.SideMenu(), 0), () =>
            {
                _busy = false;
                _view.SetBusy(false);
                CommandRefresh();
            });
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGotoSceneChange(Scene.Title);
        }
    }
}