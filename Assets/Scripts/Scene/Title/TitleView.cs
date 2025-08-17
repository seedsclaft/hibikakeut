using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ryneus.Title;

namespace Ryneus
{
    public class TitleView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private TextMeshProUGUI versionText = null;
        [SerializeField] private BaseList titleCommandList = null;
        [SerializeField] private InputInfoComponent sideMenuInput = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Title);
            InitializeTitleCommand();
            SideMenuButton.OnClickAddListener(() =>
            {
                CallSideMenu();
            });
            sideMenuInput.UpdateGuideIcon(InputKeyType.Option2);
            _ = new TitlePresenter(this);
        }

        private void InitializeTitleCommand()
        {
            titleCommandList.Initialize();
            SetInputHandler(titleCommandList.gameObject);
            titleCommandList.SetInputHandler(InputKeyType.Decide, OnClickTitle);
            titleCommandList.SetInputHandler(InputKeyType.Option2, CallSideMenu);
        }

        public void SetTitleCommand(List<ListData> titleCommand)
        {
            titleCommandList.SetData(titleCommand);
            ActivateTitleCommand();
        }

        public void ActivateTitleCommand()
        {
            titleCommandList.Activate();
        }

        public void DeactivateTitleCommand()
        {
            titleCommandList.Deactivate();
        }

        public void SetVersion(string text)
        {
            versionText.SetText(text);
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