using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    using Confirm;
    public class ConfirmPresenter : BasePresenter
    {
        private ConfirmView _view = null;
        private ConfirmModel _model = null;
        private bool _busy = true;
        public ConfirmPresenter(ConfirmView view)
        {
            _view = view;

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize();
            _busy = false;
        }

        private void Initialize()
        {
            _model = new ConfirmModel();
            SetModel(_model);
            _view.OpenAnimation();
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Confirm)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.Initialize:
                    Initialize();
                    break;
                case CommandType.IsNoChoice:
                    CommandIsNoChoice();
                    break;
                case CommandType.IsChoice:
                    CommandIsChoice();
                    break;
                case CommandType.DisableIds:
                    CommandDisableIds((List<int>)viewEvent.Template);
                    break;
            }
        }

        private void CommandIsChoice()
        {
            _view.SetConfirmCommand(MakeListData(_model.ConfirmCommand(), 0));
        }

        private void CommandIsNoChoice()
        {
            _view.SetConfirmCommand(MakeListData(_model.NoChoiceConfirmCommand(), 0));
            _view.SetSelectIndex(0);
        }

        private void CommandDisableIds(List<int> disableIds)
        {
            _view.CommandDisableIds(disableIds);
        }
    }

    public class ConfirmInfo
    {
        public ParameterString Title = new();
        private System.Action<ConfirmCommandType> _callEvent = null;
        public System.Action<ConfirmCommandType> CallEvent => _callEvent;
        public ParameterBool IsNoChoice = new();
        private List<SkillInfo> _skillInfos = null;
        public List<ListData> SkillInfos()
        {
            var list = new List<ListData>();
            if (_skillInfos != null)
            {
                return ListData.MakeListData(_skillInfos);
            }
            return list;
        }
        private StageInfo _stageInfo = null;
        public StageInfo StageInfo => _stageInfo;
        public void SetStageInfo(StageInfo stageInfo)
        {
            _stageInfo = stageInfo;
        }
        private List<ItemInfo> _itemInfos = null;
        public List<ListData> ItemInfos()
        {
            var list = new List<ListData>();
            if (_itemInfos != null)
            {
                return ListData.MakeListData(_itemInfos);
            }
            return list;
        }
        private List<int> _disableIds = new();
        public List<int> DisableIds => _disableIds;
        private List<int> _commandTextIds = new();
        public List<int> CommandTextIds => _commandTextIds;
        private ConfirmType _confirmType;
        public ConfirmType ConfirmType => _confirmType;
        public ParameterBool IsArtifact = new();
        private System.Action _backEvent = null;
        public System.Action BackEvent => _backEvent;
        public void SetBackEvent(System.Action backEvent)
        {
            _backEvent = backEvent;
        }

        public ConfirmInfo(string title, System.Action<ConfirmCommandType> callEvent, ConfirmType confirmType = ConfirmType.Confirm)
        {
            _confirmType = confirmType;
            Title.SetValue(title);
            _callEvent = callEvent;
        }

        public void SetIsNoChoice(bool isNoChoice)
        {
            IsNoChoice.SetValue(isNoChoice);
        }

        public void SetDisableIds(List<int> ids)
        {
            _disableIds = ids;
        }

        public void SetCommandTextIds(List<int> ids)
        {
            _commandTextIds = ids;
        }

        public void SetSkillInfo(List<SkillInfo> skillInfos)
        {
            _skillInfos = skillInfos;
        }

        public void SetItemInfo(List<ItemInfo> itemInfos)
        {
            _itemInfos = itemInfos;
        }
    }
}