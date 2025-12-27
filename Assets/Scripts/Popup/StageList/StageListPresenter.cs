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
            Initialize();
        }

        private void Initialize()
        {
            _model = new StageListModel();
            SetModel(_model);
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

            _view.SetStageList(MakeListData(stageInfos,enable,null,batch,index != -1 ? index : 0));
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
                case CommandType.Initialize:
                    Initialize();
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
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(32040));
                _view.CommandCallCaution(cautionInfo);
                return;
            }
            _busy = true;
            _view.SetBusy(true);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(32030), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    CheckResumeStage(stageInfo.Master.StageNo, stageInfo.StageId.Value);
                }
                _busy = false;
                _view.SetBusy(false);
            }, ConfirmType.StageConfirm);
            confirmInfo.SetStageInfo(stageInfo);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CheckResumeStage(int stageNo, int stageId)
        {
            var resume = _model.GetDungeonResumeInfo(stageNo);
            StartStage(resume != null ? resume.DungeonId.Value : stageId, resume != null);
        }

        private void StartStage(int stageId, bool resumeStart)
        {
            _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
            _view.CallSystemCommand(Base.CommandType.CallLoading);
            _model.MakeStageInfoDepature(stageId, resumeStart);
            CheckAchievements();
            _view.CommandSceneChange(Scene.Dungeon);
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}