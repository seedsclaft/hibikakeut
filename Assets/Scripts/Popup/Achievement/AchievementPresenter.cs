using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.Achievement;

namespace Ryneus
{
    public class AchievementPresenter : BasePresenter
    {
        AchievementModel _model = null;
        AchievementView _view = null;

        private bool _busy = true;
        public AchievementPresenter(AchievementView view)
        {
            _view = view;

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize(true);
        }

        private void Initialize(bool first)
        {
            _model = new AchievementModel();
            SetModel(_model);
            _view.OpenAnimation(first ? InitializeAfter : null);
            if (!first)
            {
                InitializeAfter();
            }
        }

        private void InitializeAfter()
        {
            _view.SetAchievement(MakeListData(_model.AchivementDates(), 0));
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Achievement)
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

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}