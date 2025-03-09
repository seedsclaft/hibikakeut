using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    using Title;
    public class TitleView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private TextMeshProUGUI versionText = null;
        [SerializeField] private BaseList titleCommandList = null;
        public SystemData.CommandData TitleCommand => titleCommandList.ListItemData<SystemData.CommandData>();
        
        public override void Initialize() 
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Title);
            InitializeTitleCommand();
            SideMenuButton.OnClickAddListener(() => 
            {
                CallSideMenu();
            });
            new TitlePresenter(this);
        }

        private void InitializeTitleCommand()
        {
            titleCommandList.Initialize();
            SetInputHandler(titleCommandList.gameObject);
            titleCommandList.SetInputHandler(InputKeyType.Decide,OnClickTitle);
        }        
        
        public void SetTitleCommand(List<ListData> titleCommand)
        {
            titleCommandList.SetData(titleCommand);
            titleCommandList.Activate();
        }

        public void SetVersion(string text)
        {
            versionText.SetText(text);
        }

        private void OnClickTitle()
        {
            CallViewEvent(CommandType.SelectTitle);
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