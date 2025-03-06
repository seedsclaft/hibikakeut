using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    using FileList;
    public class FileListPresenter :BasePresenter
    {
        FileListModel _model = null;
        FileListView _view = null;

        private bool _busy = true;
        public FileListPresenter(FileListView view)
        {
            _view = view;
            _model = new FileListModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
            _busy = false;
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetFileList(MakeListData(_model.SaveFileInfos(),_model.SaveFileLastIndex()));
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
                case CommandType.DecideFile:
                    CommandDecideFile((SaveFileInfo)viewEvent.template);
                    break;
            }
        }

        private void CommandDecideFile(SaveFileInfo saveFileInfo)
        {
            var success = _model.DecideFile(saveFileInfo);
            var isLoad = _model.IsLoad;
            if (success)
            {
                _view.CommandEnd();
                if (isLoad)
                {
                    _view.CommandGotoSceneChange(Scene.Tactics);
                }
            }
        }
    }
}