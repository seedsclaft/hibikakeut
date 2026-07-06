using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ryneus
{
    public class GuidePresenter : BasePresenter
    {
        GuideModel _model = null;
        GuideView _view = null;

        private bool _busy = true;
        public GuidePresenter(GuideView view)
        {
            _view = view;
            SetView(_view);
            _view.SetEvent(async (type) => await UpdateCommand(type));
            Initialize(true);
            _busy = false;
        }

        private void Initialize(bool first)
        {
            _model = new GuideModel();
            SetModel(_model);
            _view.OpenAnimation(first ? InitializeAfter : null);
            if (!first)
            {
                InitializeAfter();
            }
        }

        private void InitializeAfter()
        {
            CommandRefresh();
        }

        private async Task UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Guide)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case Guide.CommandType.PageLeft:
                    CommandPageLeft();
                    break;
                case Guide.CommandType.PageRight:
                    CommandPageRight();
                    break;
                case Guide.CommandType.CallHelp:
                    CommandCallHelp();
                    break;
            }
        }

        private void CommandPageLeft()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.PageLeft();
            CommandRefresh();
        }

        private void CommandPageRight()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.PageRight();
            CommandRefresh();
        }

        private void CommandCallHelp()
        {
            var callHelpId = _model.CallHelpId();
            _view.CallSystemCommand(Base.CommandType.CallHelpView,DataSystem.HelpText(callHelpId));
        }

        private void CommandRefresh()
        {
            _view.SetGuideImage(_model.GuideSprite());
            _view.SetHelpText(_model.GuideTextList());
            _view.SetLeftRight(_model.NeedLeftPage(),_model.NeedRightPage());
        }
    }
}