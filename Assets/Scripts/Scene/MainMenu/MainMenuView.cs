using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.MainMenu;

namespace Ryneus
{
    public class MainMenuView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private PartyInfoComponent component;
        [SerializeField] private BaseList commandList;
        [SerializeField] private TacticsCharaLayer tacticsCharaLayer;
        [SerializeField] private AlcanaInfoComponent alcanaInfoComponent;
        [SerializeField] private Button alcanaInfoButton;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.MainMenu);
            InitializeCommandList();
            SideMenuButton.OnClickAddListener(() =>
            {
                CallSideMenu();
            });
            if (alcanaInfoButton != null)
            {
                alcanaInfoButton.onClick.AddListener(() => CallViewEvent(CommandType.Aritifact));
            }
            SetInputHandler(gameObject);
            CommandRefresh();
            _ = new MainMenuPresenter(this);
        }

        private void InitializeCommandList()
        {
            commandList.Initialize();
            commandList.SetInputHandler(InputKeyType.Decide,() => CallMainMenuCommand());
            commandList.SetInputHandler(InputKeyType.Option2,() => CallSideMenu());
            AddViewActives(commandList);
        }

        public void SetCommandList(List<ListData> listDatas)
        {
            commandList.SetData(listDatas);
        }

        private void CallMainMenuCommand()
        {
            var command = commandList.ListItemData<SystemData.CommandData>();
            if (command != null)
            {
                CallViewEvent(CommandType.MainMenuCommand,command);
            }
        }

        public void SetCharaLayer(List<ActorInfo> actorInfos)
        {
            tacticsCharaLayer.SetData(actorInfos,() => {});
        }

        private void CallSideMenu()
        {
            CallViewEvent(CommandType.SelectSideMenu);
        }

        public void SetInitHelpText()
        {
            HelpWindow.SetHelpText(DataSystem.GetText(11040));
            HelpWindow.SetInputInfo("MAINMENU");
        }

        public void SetHelpWindow()
        {
            SetInitHelpText();
        }

        public void CommandRefresh()
        {
            component.UpdateCurrentInfo();
            alcanaInfoComponent.UpdateCurrentInfo();
        }

        public void SetActiveCommandList(bool isActive)
        {
            if (isActive)
            {
                SetActivate(commandList);
            } else
            {
                SetActivate(null);
            }
        }

        public void InputHandler(List<InputKeyType> keyTypes,bool pressed)
        {
        }
    }

    namespace MainMenu
    {
        public enum CommandType
        {
            None = 0,
            MainMenuCommand,
            SelectSideMenu,
            Aritifact
        }
    }
}