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
            _model = new ConfirmModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.OpenAnimation();
            _busy = false;
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
                case CommandType.IsNoChoice:
                    CommandIsNoChoice();
                    break;
                case CommandType.IsChoice:
                    CommandIsChoice();
                    break;
                case CommandType.DisableIds:
                    CommandDisableIds((List<int>)viewEvent.template);
                    break;
            }
        }

        private void CommandIsChoice()
        {
            _view.SetConfirmCommand(_model.ConfirmCommand());
        }

        private void CommandIsNoChoice()
        {
            _view.SetConfirmCommand(_model.NoChoiceConfirmCommand());
        }

        private void CommandDisableIds(List<int> disableIds)
        {
            _view.CommandDisableIds(disableIds);
        }
    }

    public class ConfirmInfo
    {
        private string _title = "";
        public string Title => _title;
        private System.Action<ConfirmCommandType> _callEvent = null;
        public System.Action<ConfirmCommandType> CallEvent => _callEvent;
        private bool _isNoChoice = false;
        public bool IsNoChoice => _isNoChoice;
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
        private int _selectIndex = 0;
        public int SelectIndex => _selectIndex;
        private List<int> _disableIds = new ();
        public List<int> DisableIds => _disableIds;
        private List<int> _commandTextIds = new ();
        public List<int> CommandTextIds => _commandTextIds;
        private ConfirmType _confirmType;
        public ConfirmType ConfirmType => _confirmType;
        private System.Action _backEvent = null;
        public System.Action BackEvent => _backEvent;
        public void SetBackEvent(System.Action backEvent)
        {
            _backEvent = backEvent;
        }

        public ConfirmInfo(string title,System.Action<ConfirmCommandType> callEvent,ConfirmType confirmType = ConfirmType.Confirm)
        {
            _confirmType = confirmType;
            _title = title;
            _callEvent = callEvent;
        }

        public void SetIsNoChoice(bool isNoChoice)
        {
            _isNoChoice = isNoChoice;
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

        public void SetSelectIndex(int selectIndex)
        {
            _selectIndex = selectIndex;
        }
    }
}