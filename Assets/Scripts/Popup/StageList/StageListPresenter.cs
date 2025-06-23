using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.StageList;

namespace Ryneus
{
    public class StageListPresenter : BasePresenter
    {
        StageListModel _model = null;
        StageListView _view = null;

        private bool _busy = true;
        public StageListPresenter(StageListView view)
        {
            _view = view;
            _model = new StageListModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            Func<StageInfo, bool> enable = (stageInfo) =>
            {
                // 出撃可能か
                return !_model.IsLimitedRank(stageInfo);
            };
            var stageInfos = _model.StageInfos();
            var index = stageInfos.FindIndex(a => a.StageId.Value == _model.CurrentDeckInfo.StageNo.Value);
            _view.SetStageList(MakeListDataFunc(stageInfos, index != -1 ? index : 0, enable));
            _view.OpenAnimation();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.StageList)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.DecideStage:
                    CommandDecideStage((StageInfo)viewEvent.Template);
                    break;
            }
        }

        private void CommandDecideStage(StageInfo stageInfo)
        {
            if (stageInfo == null)
            {
                return;
            }
            if (!_model.IsLimitedRank(stageInfo))
            {
                _busy = true;
                _view.SetBusy(true);
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(32030), (a) =>
                {
                    if (a == ConfirmCommandType.Yes)
                    {
                        _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
                        _model.PartyInfo.DepartureCount.GainValue(1);
                        CheckAchievements();
                        _model.MakeStageInfoDepature(stageInfo.StageId.Value);
                        _view.CommandSceneChange(Scene.Dungeon);
                    }
                    _busy = false;
                    _view.SetBusy(false);
                });
                _view.CommandCallConfirm(confirmInfo);
            } else
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(32040));
                _view.CommandCallCaution(cautionInfo);
            }
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}