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
            _model = new AchievementModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("CHARACTER_LIST");
            _view.SetAchievement(MakeListData(_model.AchivementDates(), 0));
            _view.OpenAnimation();
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
            }
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}