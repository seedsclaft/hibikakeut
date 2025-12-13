using UnityEngine;

namespace Ryneus
{
    public class DemoPresenter : BasePresenter
    {
        DemoView _view = null;
        DemoModel _model = null;
        private bool _busy = true;

        public DemoPresenter(DemoView view)
        {
            _view = view;
            SetView(_view);
            _model = new DemoModel();
            SetModel(_model);
            Initialize();
        }

        private async void Initialize()
        {
            _busy = true;

            var bgmData = DataSystem.BGM.Find(a => a.Key == "Mainmenu");
            var bgm = await _model.GetBgmData("Mainmenu");
            SoundManager.Instance.PlayBgm(bgm, bgmData.Volume, true);
            _view.StartAnimation();
            _busy = false;
        }
    }
}
