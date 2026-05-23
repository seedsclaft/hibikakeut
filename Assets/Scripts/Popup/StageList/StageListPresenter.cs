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

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize(true);
        }

        private void Initialize(bool first)
        {
            _model = new StageListModel();
            SetModel(_model);
            _view.OpenAnimation(first ? InitializeAfter : null);
            if (!first)
            {
                InitializeAfter();
            }
        }

        private void InitializeAfter()
        {
            Func<StageInfo, bool> enable = (stageInfo) =>
            {
                // 出撃可能か
                return !_model.IsLimitedRank(stageInfo);
            };
            Func<StageInfo, bool> batch = (stageInfo) =>
            {
                // 未出撃か
                return _model.PartyInfo.GetDungeonTraverse(stageInfo.StageId.Value) == null;
            };
            var stageInfos = _model.StageInfos();
            var index = stageInfos.FindIndex(a => a.StageId.Value == _model.CurrentDeckInfo.StageNo.Value);

            _view.SetStageList(MakeListData(stageInfos,enable, null, batch, index != -1 ? index : 0));
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
                case CommandType.Initialize:
                    Initialize(false);
                    break;
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
            if (_model.IsLimitedRank(stageInfo))
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(32040));
                return;
            }
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CallConfirmStageDetailView(DataSystem.GetText(32030), stageInfo, (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    CheckResumeStage(stageInfo.Master.StageNo, stageInfo.StageId.Value);
                }
                _busy = false;
            });
        }

        private void CheckResumeStage(int stageNo, int stageId)
        {
            var resume = _model.GetDungeonResumeInfo(stageNo);
            StartStage(resume != null ? resume.DungeonId.Value : stageId, resume != null);
        }

        private void StartStage(int stageId, bool resumeStart)
        {
            _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
            _view.CommandCallLoading();
            _model.MakeStageInfoDepature(stageId, resumeStart);
            CheckAchievements();
            _view.CommandSceneChange(Scene.Dungeon);
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}