using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ryneus.Confirm;

namespace Ryneus
{
    public class ConfirmView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private BaseList commandList = null;
        [SerializeField] private TextMeshProUGUI titleText = null;
        [SerializeField] private BaseList skillInfoList = null;
        [SerializeField] private ConfirmAnimation confirmAnimation = null;
        [SerializeField] private GameObject cautionArtifact = null;
        [SerializeField] private StageInfoComponent stageInfoComponent = null;
        [SerializeField] private BaseListComponent baseListComponent = null;

        private System.Action<ConfirmCommandType> _confirmEvent = null;
        private ConfirmInfo _confirmInfo = null;

        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Confirm);
            InitializeCommandList();
            if (skillInfoList != null)
            {
                skillInfoList.Initialize();
            }
            SetBaseAnimation(confirmAnimation);
            _ = new ConfirmPresenter(this);
            //SetHelpInputInfo("CONFIRM");
        }

        private void InitializeCommandList()
        {
            commandList.Initialize();
            commandList.SetInputHandler(InputKeyType.Decide, () => CallConfirmCommand());
            commandList.SetInputHandler(InputKeyType.Cancel, () =>
            {
                BackEvent();
            });
            AddViewActives(commandList);
        }

        public void SetSelectIndex(int selectIndex)
        {
            commandList.UpdateSelectIndex(selectIndex);
        }

        public void SetConfirmCommand(List<ListData> menuCommands)
        {
            commandList.SetData(menuCommands);
        }

        public void OpenAnimation()
        {
            confirmAnimation.OpenAnimation(UiRoot.transform, null);
        }

        public void SetTitle(string title)
        {
            UIComponent.SetText(titleText, title);
        }

        public void SetSkillInfo(List<ListData> skillInfos)
        {
            if (skillInfos == null || skillInfoList == null)
            {
                return;
            }
            skillInfoList.SetData(skillInfos);
        }

        public void SetStageInfo(StageInfo stageInfo)
        {
            if (stageInfo == null || stageInfoComponent == null)
            {
                return;
            }
            stageInfoComponent.UpdateInfo(stageInfo);
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
                CallViewEvent(CommandType.DisableIds, disableIds);
            }
        }

        public void SetConfirmEvent(System.Action<ConfirmCommandType> commandData)
        {
            _confirmEvent = commandData;
        }

        public void SetViewInfo(ConfirmInfo confirmInfo)
        {
            _confirmInfo = confirmInfo;
            SetIsNoChoice(confirmInfo.IsNoChoice.Value);
            SetTitle(confirmInfo.Title.Value);
            SetSkillInfo(confirmInfo.SkillInfos());
            SetStageInfo(confirmInfo.StageInfo);
            SetConfirmEvent(confirmInfo.ReturnEvent);
            SetDisableIds(confirmInfo.DisableIds);
            if (cautionArtifact != null)
            {
                cautionArtifact.SetActive(confirmInfo.IsArtifact.Value);
            }
            if (confirmInfo.ItemInfos().Count > 0)
            {
                baseListComponent.SetListData(confirmInfo.ItemInfos()[0], 0);
                baseListComponent.UpdateViewItem();
            }
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
                }
                else
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
                BackEvent();
                _confirmEvent(commandType);
            }
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {

        }

        public new void MouseCancelHandler()
        {
            if (_confirmInfo.IsNoChoice.Value)
            {
                CallConfirmCommand();
            }
            else
            {
                CallConfirmCommand();
            }
        }

        public void CallCancelEvent()
        {
            //SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _confirmEvent(ConfirmCommandType.No);
        }
    }

    namespace Confirm
    {
        public enum CommandType
        {
            None = 0,
            Initialize,
            IsChoice = 100,
            IsNoChoice = 101,
            DisableIds = 102,
        }
    }
}
