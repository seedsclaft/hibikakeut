using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.MainMenu;
using System;

namespace Ryneus
{
    public class MainMenuView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private PartyInfoComponent component;
        [SerializeField] private BaseList commandList;
        [SerializeField] private TacticsCharaLayer tacticsCharaLayer;
        [SerializeField] private AlcanaInfoComponent alcanaInfoComponent;
        [SerializeField] private OnOffButton alcanaInfoButton;
        [SerializeField] private MainMenuStartAnim mainMenuStartAnim;
        [SerializeField] private MainMenuReliefAnimation reliefAnim;
        [SerializeField] private Button startAnimButton = null;
        [SerializeField] private GameObject particleObject;
        [SerializeField] private GameObject battleFieldNotice;
        [SerializeField] private GameObject sideMenuBatch;
        [SerializeField] private InputInfoComponent sideMenuInput = null;
        [SerializeField] private OnOffButton partyInfoButton = null;
        [SerializeField] private OnOffButton saveButton = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.MainMenu);
            InitializeCommandList();
            SideMenuButton.OnClickAddListener(() =>
            {
                CallSideMenu();
            });
            sideMenuInput.UpdateGuideIcon(InputKeyType.SideRight1);
            if (alcanaInfoButton != null)
            {
                alcanaInfoButton.OnClickAddListener(() => CallViewEvent(CommandType.Aritifact));
            }
            if (partyInfoButton != null)
            {
                partyInfoButton.OnClickAddListener(() =>
                {
                    CallPartyInfo();
                });
            }
            if (saveButton != null)
            {
                saveButton.OnClickAddListener(() =>
                {
                    CallSaveCommand();
                });
            }
            SetInputHandler(gameObject);
            CommandRefresh();
            if (mainMenuStartAnim != null)
            {
                mainMenuStartAnim.Reset();
                mainMenuStartAnim.gameObject.SetActive(false);
            }
            if (startAnimButton != null)
            {
                startAnimButton.onClick.AddListener(() => EndAnimation());
            }
            if (reliefAnim != null)
            {
                reliefAnim.Initialize();
            }
            _ = new MainMenuPresenter(this);
        }

        private void InitializeCommandList()
        {
            commandList.Initialize();
            commandList.SetInputHandler(InputKeyType.Decide, () => CallMainMenuCommand());
            //commandList.SetInputHandler(InputKeyType.SideLeft1, () => CallViewEvent(CommandType.Aritifact));
            commandList.SetInputHandler(InputKeyType.SideRight1, () => CallSideMenu());
            commandList.SetInputHandler(InputKeyType.Option2, () => CallPartyInfo());
            commandList.SetInputHandler(InputKeyType.Option1, () => CallSaveCommand());
            AddViewActives(commandList);
        }

        public void UpdateCommandList(List<ListData> listDatas)
        {
            commandList.RefreshListData(listDatas);
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

        private void CallPartyInfo()
        {
            CallViewEvent(CommandType.PartyInfo);
        }

        private void CallSaveCommand()
        {
            CallViewEvent(CommandType.SaveCommand);
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
                SetDeactivate();
            }
        }

        public void SetActiveParticleObject(bool isActive)
        {
            particleObject.SetActive(isActive);
        }

        public void UpdateBattleFieldNotice(bool isActive)
        {
            battleFieldNotice.SetActive(isActive);
        }

        public void UpdateSidemenuBatch(bool isActive)
        {
            if (sideMenuBatch == null)
            {
                return;
            }
            sideMenuBatch.SetActive(isActive);
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (InputSystem.GetInputDate(InputKeyType.Decide).IsDownTrigger() || InputSystem.GetInputDate(InputKeyType.Cancel).IsDownTrigger())
            {
                EndAnimation();
            }
        }

        public void MainMenuStartAnim(int chapter, int period, int periodMax, int remain)
        {
            mainMenuStartAnim.SetText(chapter, period, periodMax, remain);
            mainMenuStartAnim.gameObject.SetActive(true);
            mainMenuStartAnim.StartAnim(0);
        }

        private void EndAnimation()
        {
            if (!mainMenuStartAnim.gameObject.activeSelf)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            mainMenuStartAnim.EndAnimation();
            ClearMainMenuStart();
            CallViewEvent(CommandType.EndAnimation);
        }

        public void ClearMainMenuStart()
        {
            mainMenuStartAnim.gameObject.SetActive(false);
            startAnimButton.gameObject.SetActive(false);
        }

        public void StartReliefAnimation(Action endEvent, ActorInfo actorInfo, List<ActorInfo> releifActorInfos)
        {
            reliefAnim.PlayAnimation(endEvent, actorInfo, releifActorInfos);
        }
    }

    namespace MainMenu
    {
        public enum CommandType
        {
            None = 0,
            StartAnimation,
            EndAnimation,
            MainMenuCommand,
            SelectSideMenu,
            PartyInfo,
            SaveCommand,
            Aritifact
        }
    }
}