using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.Dungeon;
using TMPro;

namespace Ryneus
{
    public class DungeonView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private BattleBattlerList partyUnitList = null;
        [SerializeField] private StageInfoComponent stageInfoComponent = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Dungeon);
            InitializePartyUnitList();
            SideMenuButton.OnClickAddListener(() =>
            {
                CallSideMenu();
            });
            CommandRefresh();
            _ = new DungeonPresenter(this);
            GameSystem.DungeonViewManager.SetMoveEndEvent((a) => CallViewEvent(CommandType.MoveEnd,a));
        }

        private void InitializePartyUnitList()
        {
            partyUnitList.Initialize();
            AddViewActives(partyUnitList);
        }

        public void SetPartyUnitList(List<ListData> listDatas)
        {
            partyUnitList.SetData(listDatas);
        }

        private void CallSideMenu()
        {
            CallViewEvent(CommandType.SelectSideMenu);
        }

        public void CommandRefresh()
        {
            stageInfoComponent.UpdateCurrent();
        }

        public void SetHelpWindow()
        {
            HelpWindow.SetHelpText("");
            HelpWindow.SetInputInfo("");
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (keyTypes.Contains(InputKeyType.Option1))
            {
                CallSideMenu();
            }
        }
    }

    namespace Dungeon
    {
        public enum CommandType
        {
            None = 0,
            MoveEnd,
            SelectSideMenu
        }
    }
}