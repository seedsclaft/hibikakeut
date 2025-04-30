using System.Collections.Generic;

namespace Ryneus
{
    public class TacticsStatusPresenter : BasePresenter
    {
        private TacticsStatusModel _model = null;
        private TacticsStatusView _view = null;
        private bool _busy = false;
        public TacticsStatusPresenter(TacticsStatusView view)
        {
            _view = view;
            SetView(_view);
            _model = new TacticsStatusModel();
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
        }
    }
}