using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ryneus.NameEntry;

namespace Ryneus
{
    public class NameEntryPresenter : BasePresenter
    {
        NameEntryModel _model = null;
        NameEntryView _view = null;

        private bool _busy = true;
        public NameEntryPresenter(NameEntryView view)
        {
            _view = view;
            SetView(_view);
            _model = new NameEntryModel();
            SetModel(_model);

            Initialize();
        }

        private async Task Initialize()
        {
            _view.SetHelpWindow();
            _view.SetEvent((type) => UpdateCommand(type));

            //var bgm = await _model.GetBgmData("MAINMENU");
            //SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            CommandStartEntry();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.NameEntry)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.StartEntry:
                    CommandStartEntry();
                    break;
                case CommandType.EntryEnd:
                    CommandEntryEnd((string)viewEvent.Template);
                    break;
            }
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
            _view.StartNameEntry();
        }

        private void CommandStartEntry()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(5000),(a) => UpdatePopup(a));
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
            _view.ShowNameEntry("");
        }

        private void CommandEntryEnd(string nameText)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            if (nameText == "")
            {
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(5002),(a) => UpdatePopup(a));
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
            } else
            {
                _model.SetPlayerName(nameText);
                _view.CallSystemCommand(Base.CommandType.DecidePlayerName,nameText);
                _view.CommandGotoSceneChange(Scene.Tactics);
            }
        }
    }
}