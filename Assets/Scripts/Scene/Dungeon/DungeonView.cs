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
        [SerializeField] private Button formationButton = null;
        [SerializeField] private InputInfoComponent formationInpurKey = null;
        [SerializeField] private Button healButton = null;
        [SerializeField] private InputInfoComponent healInpurKey = null;
        //[SerializeField] private OnOffButton healButton = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Dungeon);
            InitializePartyUnitList();
            if (healButton != null)
            {
                healButton.onClick.AddListener(() =>
                {
                    CallViewEvent(CommandType.Heal);
                });
            }
            if (healInpurKey != null)
            {
                healInpurKey.UpdateGuideIcon(9);
            }
            if (formationButton != null)
            {
                formationButton.onClick.AddListener(() =>
                {
                    CallViewEvent(CommandType.Formation);
                });
            }
            if (formationInpurKey != null)
            {
                formationInpurKey.UpdateGuideIcon(8);
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
            partyUnitList.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(CommandType.SelectCharacter,partyUnitList.Index));
            partyUnitList.SetInputHandler(InputKeyType.Cancel,() => CallViewEvent(CommandType.EndFormation));
            AddViewActives(partyUnitList);
        }

        public void SetPartyUnitList(List<ListData> listDatas)
        {
            partyUnitList.SetData(listDatas);
            SetActivate(null);
        }

        public void UpdatePartyUnitList(List<ListData> listDatas)
        {
            var lastIndex = partyUnitList.Index;
            partyUnitList.SetData(listDatas);
            partyUnitList.UpdateSelectIndex(lastIndex);
        }

        public void UpdateSelectCursor(List<int> targetIndexes)
        {
            partyUnitList.UpdateSelectIndexList(targetIndexes);
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

        public void StartFormation()
        {
            SetActivate(partyUnitList);
            partyUnitList.UpdateSelectIndex(0);
        }

        public void EndFormation()
        {
            partyUnitList.UpdateSelectIndex(-1);
            SetActivate(null);
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
            } else
            if (keyTypes.Contains(InputKeyType.SideRight1))
            {
                CallViewEvent(CommandType.Heal);
            } else
            if (keyTypes.Contains(InputKeyType.SideLeft1))
            {
                CallViewEvent(CommandType.Formation);
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
            Formation,
            SelectCharacter,
            EndFormation,
            SelectSideMenu
        }
    }
}