using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class LoadingPresenter
    {
        LoadingModel _model = null;
        LoadingView _view = null;

        private bool _busy = true;
        public LoadingPresenter(LoadingView view)
        {
            _view = view;
            _model = new LoadingModel();

            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand());
            CommandRefresh();
            _busy = false;
        }

        private void UpdateCommand()
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
        }

        private void CommandRefresh()
        {
            _model.RefreshTips();
            var sprite = _model.TipsImage();
            _view.SetTips(sprite,_model.TipsText());
        }
    }
}