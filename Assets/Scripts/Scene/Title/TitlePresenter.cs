using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    using Title;
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
            var bgmData = DataSystem.BGM.Find(a => a.Key == "Title");
            var bgm = await _model.GetBgmData("Title");
            SoundManager.Instance.PlayBgm(bgm,bgmData.Volume,true);
            if (!SaveSystem.ExistsLoadPlayerFile())
            {
                SaveSystem.SavePlayerInfo();
            } else
            {
                var loadSuccess = SaveSystem.LoadPlayerInfo();
                if (loadSuccess == false)
                {
                    var confirmInfo = new ConfirmInfo(DataSystem.GetText(13330),(a) => UpdatePopup(a));
                    confirmInfo.SetIsNoChoice(true);
                    _view.CommandCallConfirm(confirmInfo);
                    return;
                }
                // プレイヤーネームを設定しなおし
                _view.CallSystemCommand(Base.CommandType.DecidePlayerName,GameSystem.CurrentData.PlayerInfo.PlayerName.Value);
            }
            _busy = false;
            _view.SetTitleCommand(_model.TitleCommand());
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
                    CommandSelectTitle();
                    break;
            }
        }

        private void CommandSelectTitle()
        {
            var titleCommand = _view.TitleCommand;
            switch (titleCommand?.Key)
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
            }
        }

        private void CommandNewGame()
        {
            _busy = true;
            _model.InitializeNewGame();
            SoundManager.Instance.PlayStaticSe(SEType.PlayStart);
            _view.WaitFrame(2,() => 
            {
                _view.CommandGotoSceneChange(Scene.Tactics);
            });
        }

        private void CommandContinue()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            
            var sceneParam = new FileListSceneInfo
            {
                IsLoad = true
            };
            var popupInfo = new PopupInfo()
            {
                PopupType = PopupType.FileList,
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                },
                template = sceneParam
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandOption()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.CommandCallOption(() => 
            {
                _busy = false;
                CommandRefresh();
            });
        }

        private void CommandRefresh()
        {
            _view.SetHelpInputInfo("TITLE");
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            CommandCallSideMenu(MakeListData(_model.SideMenu()),() => 
            {            
                CommandRefresh();
                _busy = false;
            });
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGotoSceneChange(Scene.Title);
        }
    }
}