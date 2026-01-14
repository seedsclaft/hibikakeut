using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    using System.Threading.Tasks;
    using FileList;
    public class FileListPresenter : BasePresenter
    {
        FileListModel _model = null;
        FileListView _view = null;

        private bool _busy = true;
        public FileListPresenter(FileListView view)
        {
            _view = view;

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize();
            _busy = false;
        }

        private void Initialize()
        {
            _model = new FileListModel();
            SetModel(_model);
            _view.SetFileList(MakeListData(_model.SaveFileInfos(), _model.SaveFileLastIndex()));
            _view.OpenAnimation();
        }

        private async Task UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.FileList)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.Initialize:
                    Initialize();
                    break;
                case CommandType.DecideFile:
                    await CommandDecideFile((SaveFileInfo)viewEvent.Template);
                    break;
                case CommandType.DeleteFile:
                    await CommandDeleteFile((SaveFileInfo)viewEvent.Template);
                    break;
            }
        }

        private async Task CommandDecideFile(SaveFileInfo saveFileInfo)
        {
            if (saveFileInfo == null)
            {
                return;
            }
            var success = await _model.DecideFile(saveFileInfo);
            var isLoad = _model.IsLoad;
            if (success)
            {
                if (isLoad)
                {
                    var resumeScene = _model.PartyInfo.ResumeScene;
                    if (resumeScene == Scene.None)
                    {
                        resumeScene = Scene.MainMenu;
                    }
                    _view.CommandGotoSceneChange(resumeScene);
                }
                _view.CommandEnd();
            }
        }

        private async Task CommandDeleteFile(SaveFileInfo saveFileInfo)
        {
            if (saveFileInfo == null)
            {
                return;
            }
            _busy = true;
            _view.SetBusy(true);
            CallConfirmView(DataSystem.GetText(31071), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _model.DeleteFile(saveFileInfo);
                    Initialize();
                }
                _busy = false;
                _view.SetBusy(false);
            });
        }
    }
}