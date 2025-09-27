using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.Interlude;

namespace Ryneus
{
    public class InterludePresenter : BasePresenter
    {
        InterludeModel _model = null;
        InterludeView _view = null;
        private bool _busy = true;

        private bool _startEvent = false;
        public InterludePresenter(InterludeView view)
        {
            _view = view;
            SetView(_view);
            _model = new InterludeModel();
            SetModel(_model);

            Initialize();
        }

        private async void Initialize()
        {
            _busy = true;

            _view.SetEvent((type) => UpdateCommand(type));
            _view.InitResultList(MakeListData(_model.ResultCommand()));
            _view.SetTitle();
            CommandRefresh();
            var route = _model.MakeEvaluateResults();
            _view.CallSystemCommand(Base.CommandType.SetRouteSelect,route);
            var bgm = await _model.GetBgmData("Interrude");
            SoundManager.Instance.PlayBgm(bgm, 1.0f, true);
            var displayActorInfos = _model.DisplayActorInfos();
            _view.StartTitleAnimation();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Interlude)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.EndAnimation:
                    CommandEndAnimation();
                    break;
                case CommandType.ResultClose:
                    CommandResultClose();
                    break;
            }
        }

        private void CommandEndAnimation()
        {
            // タイトル終了後
            if (!_startEvent)
            {
                var advInfo = new AdvCallInfo();
                advInfo.Label.SetValue(_model.GetAdvFile(_model.InterrudeEventId()));
                advInfo.SetCallEvent(() =>
                {
                    _view.ChangeUIActive(true);
                    _view.StartResultAnimation();
                });
                _view.ChangeUIActive(false);
                _view.CommandCallAdv(advInfo);
                _startEvent = true;
                return;
            }
            // 報酬授与後
            ShowResultList();
        }

        private void ShowResultList()
        {
            _view.ShowResultList(MakeListData(_model.ResultViewInfos),
                null,
                _model.ClearStageNum(),
                _model.PartyInfo.MissionRank.Value.ToString(),
                DataSystem.GetReplaceDecimalText(_model.PartyInfo.PartyEvaluate()),
                null,
                null
            );
        }

        private void CommandResultClose()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            // 報酬授与後
            _view.HideResultList();
            var advInfo = new AdvCallInfo();
            advInfo.Label.SetValue(_model.GetAdvFile(_model.AfterInterrudeEventId()));
            advInfo.SetCallEvent(() =>
            {
                _view.ChangeUIActive(true);
                _model.EndInterludePhase();
                _view.CommandGotoSceneChange(Scene.MainMenu);
            });
            _view.ChangeUIActive(false);
            _view.CommandCallAdv(advInfo);
        }

        private void CommandRefresh()
        {
        }

    }
}