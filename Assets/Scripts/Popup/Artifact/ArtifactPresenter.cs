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
            _model = new ArtifactListModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("CHARACTER_LIST");
            _view.SetArtifactList(MakeListData(_model.ArtifactSkills(), 0));
            _view.OpenAnimation();
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
            }
        }
    }
}