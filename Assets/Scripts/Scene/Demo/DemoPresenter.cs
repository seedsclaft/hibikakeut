namespace Ryneus
{
    public class DemoPresenter : BasePresenter
    {
        DemoView _view = null;
        DemoModel _model = null;

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
            var bgmData = DataSystem.GetBGMByKey("Mainmenu");
            var bgm = await _model.GetBgmData("Mainmenu");
            SoundManager.Instance.PlayBgm(bgm, bgmData.Volume, true);
            _view.StartAnimation();
        }
    }
}
