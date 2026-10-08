using System;
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
            RemoveInputHandler(gameObject);
            SetViewCommandSceneType(ViewCommandSceneType.Confirm);
            InitializeCommandList();
            if (skillInfoList != null)
            {
                skillInfoList.Initialize();
            }
            SetBaseAnimation(confirmAnimation);
            _ = new ConfirmPresenter(this);
        }

        public void SetViewInfo(ConfirmInfo confirmInfo)
        {
            _confirmInfo = confirmInfo;
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

        public void OpenAnimation(Action initializeAfter)
        {
            confirmAnimation.OpenAnimation(UiRoot.transform, initializeAfter);
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

        public void SetIsNoChoice(bool isNoChoice, List<int> textIds)
        {
            var commandType = isNoChoice ? CommandType.IsNoChoice : CommandType.IsChoice;
            CallViewEvent(commandType, textIds);
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

        public void UpdateViewInfo()
        {
            SetIsNoChoice(_confirmInfo.IsNoChoice.Value, _confirmInfo.CommandTextIds);
            SetTitle(_confirmInfo.Title.Value);
            SetSkillInfo(_confirmInfo.SkillInfos());
            SetStageInfo(_confirmInfo.StageInfo);
            SetConfirmEvent(_confirmInfo.ReturnEvent);
            SetDisableIds(_confirmInfo.DisableIds);
            UIComponent.SetActive(cautionArtifact, _confirmInfo.IsArtifact.Value);
            if (_confirmInfo.ItemInfos().Count > 0)
            {
                baseListComponent.SetListData(_confirmInfo.ItemInfos()[0], 0);
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
                    _confirmInfo.NoCancelSound.SetValue(true);
                }
                else
                {
                    //SoundManager.Instance.PlayStaticSe(SEType.Cancel);
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
            if (!_confirmInfo.NoCancelSound.Value)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            }
            BackEvent?.Invoke();
        }

        public void CallCloseEvent()
        {
            if (!_confirmInfo.NoCancelSound.Value)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            }
            _confirmEvent(ConfirmCommandType.Close);
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
