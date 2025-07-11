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
        [SerializeField] private OnOffButton formationButton = null;
        [SerializeField] private InputInfoComponent formationInpurKey = null;
        [SerializeField] private OnOffButton healButton = null;
        [SerializeField] private InputInfoComponent healInpurKey = null;
        [SerializeField] private OnOffButton decideButton = null;
        [SerializeField] private InputInfoComponent decideInpurKey = null;
        [SerializeField] private AlcanaInfoComponent alcanaInfoComponent;
        [SerializeField] private OnOffButton alcanaInfoButton;
        //[SerializeField] private OnOffButton healButton = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Dungeon);
            InitializePartyUnitList();
            if (decideButton != null)
            {
                decideButton.OnClickAddListener(() =>
                {
                    CallViewEvent(CommandType.DecideDirectEvent);
                });
            }
            if (decideInpurKey != null)
            {
                decideInpurKey.UpdateGuideIcon(4);
            }
            if (healButton != null)
            {
                healButton.OnClickAddListener(() =>
                {
                    CallViewEvent(CommandType.Heal);
                });
            }
            if (healInpurKey != null)
            {
                healInpurKey.UpdateGuideIcon(12);
            }
            if (formationButton != null)
            {
                formationButton.OnClickAddListener(() =>
                {
                    CallFormation();
                });
            }
            if (formationInpurKey != null)
            {
                formationInpurKey.UpdateGuideIcon(6);
            }
            if (alcanaInfoButton != null)
            {
                alcanaInfoButton.OnClickAddListener(() => CallViewEvent(CommandType.Aritifact));
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

        private void CallFormation()
        {
            if (partyUnitList.Active)
            {
                return;
            }
            CallViewEvent(CommandType.Formation);
        }

        private void CallSideMenu()
        {
            if (partyUnitList.Active)
            {
                return;
            }
            CallViewEvent(CommandType.SelectSideMenu);
        }

        public void CommandRefresh()
        {
            stageInfoComponent.UpdateCurrent();
            partyInfoComponent.UpdateCurrentInfo();
            alcanaInfoComponent.UpdateCurrentInfo();
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
            if (InputSystem.GetInputDate(InputKeyType.Start).IsDownTrigger())
            {
                CallViewEvent(CommandType.Heal);
            }
            if (InputSystem.GetInputDate(InputKeyType.SideLeft1).IsDownTrigger())
            {
                CallViewEvent(CommandType.Aritifact);
            }
            if (keyTypes.Contains(InputKeyType.Option2))
            {
                CallSideMenu();
            }else
            if (keyTypes.Contains(InputKeyType.Option1))
            {
                CallFormation();
            } else
            if (keyTypes.Contains(InputKeyType.Decide))
            {
                if (decideButton.gameObject.activeSelf && !partyUnitList.Active)
                {
                    CallViewEvent(CommandType.DecideDirectEvent);
                }
            }
            moveController.UpdateKey(keyTypes);
        }

        public void SetActiveDisplayEventKey(bool isActive)
        {
            if (decideButton == null)
            {
                return;
            }
            decideButton.gameObject.SetActive(isActive);
        }

        public void SetActiveHealButton(bool isActive)
        {
            if (healButton == null)
            {
                return;
            }
            healButton.gameObject.SetActive(isActive);
        }

        public void SetActiveFormationButton(bool isActive)
        {
            if (formationButton == null)
            {
                return;
            }
            formationButton.gameObject.SetActive(isActive);
        }

        public void SetActiveStageInfo(bool isActive)
        {
            if (stageInfoComponent == null)
            {
                return;
            }
            stageInfoComponent.gameObject.SetActive(isActive);
        }

        public void ChangeSkybox(Material material)
        {
            RenderSettings.skybox = material;
        }
    }

    namespace Dungeon
    {
        public enum CommandType
        {
            None = 0,
            MoveEnd,
            DecideDirectEvent,
            Heal,
            Formation,
            SelectCharacter,
            EndFormation,
            Aritifact,
            SelectSideMenu
        }
    }
}