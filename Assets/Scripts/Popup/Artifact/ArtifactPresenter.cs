using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.ArtifactList;

namespace Ryneus
{
    public class ArtifactListPresenter : BasePresenter
    {
        ArtifactListModel _model = null;
        ArtifactListView _view = null;

        private bool _busy = true;
        public ArtifactListPresenter(ArtifactListView view)
        {
            _view = view;

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize(true);
        }

        private void Initialize(bool first)
        {
            _model = new ArtifactListModel();
            SetModel(_model);
            _view.OpenAnimation(first ? InitializeAfter : null);
            if (!first)
            {
                InitializeAfter();
            }
        }
        
        private void InitializeAfter()
        {
            _view.SetArtifactList(MakeListData(_model.ArtifactSkills(), 0));
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.ArtifactList)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.Initialize:
                    Initialize(false);
                    break;
            }
        }
    }
}