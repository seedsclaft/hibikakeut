using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.Traits;

namespace Ryneus
{
    public class TraitsPresenter : BasePresenter
    {
        TraitsModel _model = null;
        TraitsView _view = null;

        private bool _busy = true;
        public TraitsPresenter(TraitsView view)
        {
            _view = view;

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize(true);
        }

        private void Initialize(bool first)
        {
            _model = new TraitsModel();
            SetModel(_model);
             _view.OpenAnimation(first ? InitializeAfter : null);
            if (!first)
            {
                InitializeAfter();
            }
        }

        private void InitializeAfter()
        {
            _view.SetTraitsList(MakeListData(_model.TraitsDates(), 0));
        }

        private void CommandEndOpenAnimation()
        {
            CheckTutorialState();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Traits)
            {
                return;
            }
            if (_busy || _view.AnimationBusy)
            {
                switch (viewEvent.ViewCommandType.CommandType)
                {
                    case CommandType.EndOpenAnimation:
                        CommandEndOpenAnimation();
                        break;
                }
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.Initialize:
                    Initialize(false);
                    break;
            }
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}