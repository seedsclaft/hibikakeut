using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ryneus.Title;
using UnityEngine.Video;

namespace Ryneus
{
    public class TitleView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private TextMeshProUGUI versionText = null;
        [SerializeField] private BaseList titleCommandList = null;
        [SerializeField] private InputInfoComponent sideMenuInput = null;
        [SerializeField] private VideoPlayer videoPlayer = null;
        [SerializeField] private VideoPlayer webglVideoPlayer = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Title);
            InitializeTitleCommand();
            SideMenuButton.OnClickAddListener(() =>
            {
                CallSideMenu();
            });
            sideMenuInput.UpdateGuideIcon(InputKeyType.SideRight1);
#if UNITY_WEBGL
            webglVideoPlayer.gameObject.SetActive(true);
#else
            videoPlayer.gameObject.SetActive(true);
#endif
            _ = new TitlePresenter(this);
        }

        public new void SetBusy(bool busy)
        {
            SetActivate(busy ? null : titleCommandList);
        }

        private void InitializeTitleCommand()
        {
            titleCommandList.Initialize();
            titleCommandList.SetInputHandler(InputKeyType.Decide, OnClickTitle);
            titleCommandList.SetInputHandler(InputKeyType.SideRight1, CallSideMenu);
            AddViewActives(titleCommandList);
        }

        public void SetTitleCommand(List<ListData> titleCommand)
        {
            titleCommandList.SetData(titleCommand);
            SetActivate(titleCommandList);
        }

        public void SetVersion(string text)
        {
            UIComponent.SetText(versionText, text);
        }

        private void OnClickTitle()
        {
            CallViewEvent(CommandType.SelectTitle, titleCommandList.ListItemData<SystemData.CommandData>());
        }

        private void CallSideMenu()
        {
            CallViewEvent(CommandType.SelectSideMenu);
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (InputSystem.GetInputDate(InputKeyType.SideLeft1).IsDownTrigger())
            {
                CommandSceneChange(Scene.Demo);
            }
        }
    }

    namespace Title
    {
        public enum CommandType
        {
            None = 0,
            SelectTitle,
            SelectSideMenu,
        }
    }
}