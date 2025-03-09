using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Confirm;

namespace Ryneus
{
    public class ConfirmView : BaseView,IInputHandlerEvent
    {
        [SerializeField] private BaseList commandList = null;
        [SerializeField] private TextMeshProUGUI titleText = null;
        [SerializeField] private BaseList skillInfoList = null;
        [SerializeField] private ConfirmAnimation confirmAnimation = null;
        private System.Action<ConfirmCommandType> _confirmEvent = null;
        private ConfirmInfo _confirmInfo = null;

        public override void Initialize() 
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Confirm);
            InitializeCommandList();
            skillInfoList.Initialize();
            SetBaseAnimation(confirmAnimation);
            new ConfirmPresenter(this);
            SetHelpInputInfo("CONFIRM");
        }

        private void InitializeCommandList()
        {
            commandList.Initialize();
            commandList.SetInputHandler(InputKeyType.Decide,() => CallConfirmCommand());
            SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
        }

        public void SetSelectIndex(int selectIndex)
        {
            commandList.Refresh(selectIndex);
        }

        public void SetConfirmCommand(List<ListData> menuCommands)
        {
            commandList.SetData(menuCommands);
        }

        public void OpenAnimation()
        {
            confirmAnimation.OpenAnimation(UiRoot.transform,null);
        }
        
        public void SetTitle(string title)
        {
            titleText?.SetText(title);
        }

        public void SetSkillInfo(List<ListData> skillInfos)
        {
            if (skillInfos == null) return;
            skillInfoList.SetData(skillInfos);
        }

        public void SetIsNoChoice(bool isNoChoice)
        {
            var commandType = isNoChoice ? CommandType.IsNoChoice : CommandType.IsChoice;
            CallViewEvent(commandType);
        }

        public void SetDisableIds(List<int> disableIds)
        {
            if (disableIds.Count > 0)
            {
                CallViewEvent(CommandType.DisableIds,disableIds);
            }
        }

        public void SetConfirmEvent(System.Action<ConfirmCommandType> commandData)
        {
            _confirmEvent = commandData;
        }

        public void SetViewInfo(ConfirmInfo confirmInfo)
        {
            _confirmInfo = confirmInfo;
            SetIsNoChoice(confirmInfo.IsNoChoice);
            SetSelectIndex(confirmInfo.SelectIndex);
            SetTitle(confirmInfo.Title);
            SetSkillInfo(confirmInfo.SkillInfos());
            SetConfirmEvent(confirmInfo.CallEvent);
            SetDisableIds(confirmInfo.DisableIds);
        }

        public void CommandDisableIds(List<int> disableIds)
        {
            commandList.SetDisableIds(disableIds);
        }

        private void CallConfirmCommand()
        {
            var data = (SystemData.CommandData)commandList.ListData.Data;
            if (data != null)
            {
                var commandType = data.Key == "Yes" ? ConfirmCommandType.Yes : ConfirmCommandType.No;
                if (data.Key == "Yes")
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                } else
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
                BackEvent();
                _confirmEvent(commandType);
            }
        }

        public void InputHandler(List<InputKeyType> keyTypes,bool pressed)
        {

        }

        public new void MouseCancelHandler()
        {
            if (_confirmInfo.IsNoChoice)
            {
                CallConfirmCommand();
            } else
            {
                CallConfirmCommand();
            }
        }
    }
}

namespace Confirm
{
    public enum CommandType
    {
        None = 0,
        IsChoice = 100,
        IsNoChoice = 101,
        DisableIds = 102,
    }
}