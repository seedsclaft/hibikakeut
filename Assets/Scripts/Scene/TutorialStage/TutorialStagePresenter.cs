using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    using TutorialStage;
    public class TutorialStagePresenter :BasePresenter
    {
        TutorialStageModel _model = null;
        TutorialStageView _view = null;

        private bool _busy = true;
        public TutorialStagePresenter(TutorialStageView view)
        {
            _view = view;
            _model = new TutorialStageModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
            _busy = false;
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetTutorialStage(MakeListData(_model.TutorialStageInfos()));
            _view.OpenAnimation();
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.TutorialStage)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.DecideStage:
                    CommandDecideStage((StageInfo)viewEvent.template);
                    break;
            }
        }

        private void CommandDecideStage(StageInfo stageInfo)
        {
            if (stageInfo == null)
            {
                return;
            }
            _view.DeactivateStageList();
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo("開始しますか？",(a) => 
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _model.StartTutorial(stageInfo.StageId.Value);
                    _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
                    _view.CommandGotoSceneChange(Scene.Tactics);
                } else
                {
                    _view.ActivateStageList();
                }
            });
            _view.CommandCallConfirm(confirmInfo);
        }
    }
}