using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.LevelUp;

namespace Ryneus
{
    public class LevelUpPresenter : BasePresenter
    {
        LevelUpModel _model = null;
        LevelUpView _view = null;

        private bool _busy = true;
        public LevelUpPresenter(LevelUpView view)
        {
            _view = view;

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize();
        }

        private void Initialize()
        {
            _model = new LevelUpModel();
            SetModel(_model);
            _busy = false;
            CommandLevelUpNext();
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.LevelUp)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.Initialize:
                    Initialize();
                    break;
                case CommandType.EndAnimation:
                    CommandEndAnimation();
                    break;
                case CommandType.LevelUpNext:
                    CommandLevelUpNext();
                    break;
            }
        }

        private void CommandLevelUpNext()
        {
            // LevelUp
            if (_model.LevelUpDates().Count > 0)
            {
                _view.OpenAnimation(_model.TitleText());
                return;
            }
            // 魔法獲得
            var learnSkill = _model.LearnSkillInfos();
            if (learnSkill != null)
            {
                _view.OpenAnimation(_model.LearnSkillText());
                return;
            }
            _view.PopupClose();
        }

        private void CommandEndAnimation()
        {
            // LevelUp
            if (_model.LevelUpDates().Count > 0)
            {
                _view.UpdateLevelUp(
                    _model.LevelUpActorInfo(),
                    MakeListData(_model.LevelUpDates()));
                _model.ClearLevelUpDates();
            }
            // 魔法獲得
            var learnSkill = _model.LearnSkillInfos();
            if (learnSkill != null)
            {
                _view.UpdateLearnSkillText(_model.LearnSkillText());
                _view.UpdateLearnSkill(_model.LevelUpActorInfo(), learnSkill);
                _model.ClearSkillInfo();
            }
            _view.UpdateEvaluate(_model.SceneParam.From.Value, _model.SceneParam.To.Value);
        }



        private void CommandRefresh()
        {
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}