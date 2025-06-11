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
        [SerializeField] private Ariadne.DungeonViewManager dungeonViewManager = null;
        [SerializeField] private Ariadne.MoveController moveController = null;
        public Ariadne.MoveController MoveController => moveController;
        [SerializeField] private BattleBattlerList partyUnitList = null;
        [SerializeField] private StageInfoComponent stageInfoComponent = null;
        [SerializeField] private PartyInfoComponent partyInfoComponent = null;
        [SerializeField] private OnOffButton healButton = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Dungeon);
            InitializePartyUnitList();
            if (healButton != null)
            {
                healButton.OnClickAddListener(() =>
                {
                    CallViewEvent(CommandType.Heal);
                });
            }
            SideMenuButton.OnClickAddListener(() =>
            {
                CallSideMenu();
            });
            CommandRefresh();
            _ = new DungeonPresenter(this);
        }

        public void SetupDungeon()
        {
            dungeonViewManager.Initialize();
            dungeonViewManager.SetMoveController(moveController);
            dungeonViewManager.SetMoveEndEvent(() => CallViewEvent(CommandType.MoveEnd));
        }

        private void InitializePartyUnitList()
        {
            partyUnitList.Initialize();
            AddViewActives(partyUnitList);
        }

        public void SetPartyUnitList(List<ListData> listDatas)
        {
            partyUnitList.SetData(listDatas);
            SetActivate(null);
        }

        private void CallSideMenu()
        {
            CallViewEvent(CommandType.SelectSideMenu);
        }

        public void CommandRefresh()
        {
            stageInfoComponent.UpdateCurrent();
            partyInfoComponent.UpdateCurrentInfo();
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
            moveController.UpdateKey(keyTypes);
        }

        public void SetActiveStageInfo(bool isActive)
        {
            if (stageInfoComponent == null)
            {
                return;
            }
            stageInfoComponent.gameObject.SetActive(isActive);
        }
    }

    namespace Dungeon
    {
        public enum CommandType
        {
            None = 0,
            MoveEnd,
            Heal,
            SelectSideMenu
        }
    }
}