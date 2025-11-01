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

        private void UpdateCommand(ViewEvent viewEvent)
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
                    CommandDecideFile((SaveFileInfo)viewEvent.Template);
                    break;
            }
        }

        private async Task CommandDecideFile(SaveFileInfo saveFileInfo)
        {
            var success = await _model.DecideFile(saveFileInfo);
            var isLoad = _model.IsLoad;
            if (success)
            {
                if (isLoad)
                {
                    _view.CommandGotoSceneChange(_model.PartyInfo.ResumeScene);
                }
                _view.CommandEnd();
            }
        }
    }
}