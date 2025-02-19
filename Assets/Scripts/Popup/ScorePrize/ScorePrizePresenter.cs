using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class ScorePrizePresenter : BasePresenter
    {
        ScorePrizeModel _model = null;
        ScorePrizeView _view = null;

        private bool _busy = true;
        public ScorePrizePresenter(ScorePrizeView view)
        {
            _view = view;
            _model = new ScorePrizeModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("CHARACTER_LIST");
            //_view.SetScorePrize(GetListData(_model.ScorePrize()));
            _view.OpenAnimation();
        }

        private void UpdateCommand(ScorePrizeViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
        }
    }
}