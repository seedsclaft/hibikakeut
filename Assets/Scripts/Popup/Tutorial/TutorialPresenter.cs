using System.Collections.Generic;
using System.Threading.Tasks;

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
            _view.SetEvent(async (type) => await UpdateCommand(type));
            Initialize(true);
            _busy = false;
        }

        private void Initialize(bool first)
        {
            _model = new TutorialModel();
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
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Tutorial)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case Tutorial.CommandType.Initialize:
                    Initialize(false);
                    break;
                case Tutorial.CommandType.Back:
                    CommandBack();
                    return;
            }
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
            _view.SetTutorialData(_model.TutorialData);
        }
    }
}