using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EnemyInfo;

namespace Ryneus
{
    public class EnemyInfoView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private StatusInfoComponent paramstatusInfoComponent = null;
        [SerializeField] private StatusInfoComponent statusInfoComponent = null;
        [SerializeField] private ActorInfoComponent actorInfoComponent = null;
        [SerializeField] private BattleThumb battleThumb = null;
        [SerializeField] private EnemyInfoComponent enemyInfoComponent = null;
        [SerializeField] private GameObject magicListRoot = null;
        [SerializeField] private BaseList magicList = null;
        [SerializeField] private GameObject conditionListRoot = null;
        [SerializeField] private BaseList conditionList = null;
        [SerializeField] private Button leftArrowButton = null;
        [SerializeField] private InputInfoComponent leftArrowButtonInput = null;
        [SerializeField] private Button rightArrowButton = null;
        [SerializeField] private InputInfoComponent rightArrowButtonInput = null;
        [SerializeField] private Button leftBattlerButton = null;
        [SerializeField] private Button rightBattlerButton = null;
        [SerializeField] private TextMeshProUGUI displayCategory = null;

        private System.Action _backEvent = null;



        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Status);
            //InitializeEnemyList();
            InitializeMagicList();
            InitializeConditionList();
            if (leftArrowButton != null)
            {
                leftArrowButton.onClick.AddListener(() => CallViewEvent(CommandType.CallMagicList));
            }
            if (rightArrowButton != null)
            {
                rightArrowButton.onClick.AddListener(() => CallViewEvent(CommandType.CallConditionList));
            }
            if (leftArrowButtonInput != null)
            {
                leftArrowButtonInput.UpdateGuideIcon(InputKeyType.SideLeft1);
            }
            if (rightArrowButtonInput != null)
            {
                rightArrowButtonInput.UpdateGuideIcon(InputKeyType.SideRight1);
            }
            if (leftBattlerButton != null)
            {
                leftBattlerButton.onClick.AddListener(() => CallViewEvent(CommandType.LeftBattler));
            }
            if (rightBattlerButton != null)
            {
                rightBattlerButton.onClick.AddListener(() => CallViewEvent(CommandType.RightBattler));
            }
            CallMagicList();
            _ = new EnemyInfoPresenter(this);
        }

        private void InitializeMagicList()
        {
            magicList.Initialize();
            AddViewActives(magicList);
        }

        private void InitializeConditionList()
        {
            conditionList.Initialize();
            AddViewActives(conditionList);
        }

        public void SetActiveSelector(bool isActive)
        {
            UIComponent.SetActive(leftBattlerButton, isActive);
            UIComponent.SetActive(rightBattlerButton, isActive);
            UIComponent.SetActive(leftArrowButtonInput.gameObject, isActive);
            UIComponent.SetActive(rightArrowButtonInput.gameObject, isActive);
        }

        public void CallMagicList()
        {
            UIComponent.SetActive(magicListRoot, true);
            UIComponent.SetActive(conditionListRoot, false);
            UIComponent.SetText(displayCategory, DataSystem.GetText(15010));
            SetActivate(magicList);
        }

        public void CallConditionList()
        {
            UIComponent.SetActive(magicListRoot, false);
            UIComponent.SetActive(conditionListRoot, true);
            UIComponent.SetText(displayCategory, DataSystem.GetText(15020));
            SetActivate(conditionList);
        }

        public void CommandRefreshStatus(List<ListData> skillInfos, BattlerInfo battlerInfo)
        {
            magicList.SetData(skillInfos);
            paramstatusInfoComponent.UpdateInfo(battlerInfo.Status);
            paramstatusInfoComponent.UpdateHp(battlerInfo.Hp.Value, battlerInfo.MaxHp);
            //statusInfoComponent.UpdateInfo(battlerInfo.Status);
            statusInfoComponent.UpdateHp(battlerInfo.Hp.Value, battlerInfo.MaxHp);
            if (battlerInfo.IsActor)
            {
                enemyInfoComponent.Clear();
                battleThumb.ShowThumb(battlerInfo);
                UIComponent.SetActive(battleThumb.gameObject, true);
                actorInfoComponent.UpdateInfo(battlerInfo.ActorInfo, new List<ActorInfo>());
            } else
            {
                actorInfoComponent.Clear();
                battleThumb.HideThumb();
                UIComponent.SetActive(battleThumb.gameObject, false);
                enemyInfoComponent.UpdateInfo(battlerInfo);
            }
        }

        private void OnClickBack()
        {
            CallViewEvent(CommandType.Back);
        }

        public void SetHelpWindow()
        {
            HelpWindow.SetHelpText(DataSystem.GetHelp(809));
            if (true)
            {
                HelpWindow.SetInputInfo("ENEMYINFO_BATTLE");
            } else
            {
                HelpWindow.SetInputInfo("ENEMYINFO");
            }
        }

        public void SetCondition(List<ListData> conditions)
        {
            conditionList.SetData(conditions);
        }

        public new void SetBackEvent(System.Action backEvent)
        {
            _backEvent = backEvent;
            //CallViewEvent(CommandType.Back);
            ChangeBackCommandActive(true);
        }

        public void CommandBack()
        {
            _backEvent?.Invoke();
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (InputSystem.GetInputDate(InputKeyType.Cancel).IsDownTrigger())
            {
                CommandBack();
            }
            if (InputSystem.GetInputDate(InputKeyType.SideRight1).IsDownTrigger())
            {
                CallViewEvent(CommandType.RightBattler);
            }
            if (InputSystem.GetInputDate(InputKeyType.SideLeft1).IsDownTrigger())
            {
                CallViewEvent(CommandType.LeftBattler);
            }
            if (InputSystem.GetInputDate(InputKeyType.Left).IsDownTrigger())
            {
                CallViewEvent(CommandType.CallMagicList);
            }
            if (InputSystem.GetInputDate(InputKeyType.Right).IsDownTrigger())
            {
                CallViewEvent(CommandType.CallConditionList);
            }
        }


        public new void MouseCancelHandler()
        {
            CommandBack();
        }
    }
}

namespace EnemyInfo
{
    public enum CommandType
    {
        None = 0,
        Back,
        CallMagicList,
        CallConditionList,
        LeftBattler,
        RightBattler,
        SelectEnemy,
    }
}