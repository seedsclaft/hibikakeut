using System.Collections.Generic;

namespace Ryneus
{
    public class TutorialPresenter : BasePresenter
    {
        private TutorialModel _model = null;
        private TutorialView _view = null;
        private bool _busy = false;
        public TutorialPresenter(TutorialView view)
        {
            _view = view;
            SetView(_view);
            _model = new TutorialModel();
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));

            CommandRefresh();
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            UnityEngine.Debug.Log(viewEvent.ViewCommandType.CommandType);
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case Tutorial.CommandType.Back:
                    CommandBack();
                    return;
                case Tutorial.CommandType.CallTutorialData:
                    CommandCallTutorialData((TutorialData)viewEvent.Template);
                    return;
            }
        }

        private void CommandCallTutorialData(TutorialData tutorialData)
        {
        }

        private void CommandBack()
        {
            if (_view.CheckToggle)
            {
                // チュートリアル省略
                OptionUtility.ChangeTutorialCheck(false);
            }
            _view.CommandBack();
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }

        private void CommandRefresh()
        {
        }
    }
}