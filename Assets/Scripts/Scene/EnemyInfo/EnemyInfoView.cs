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
        [SerializeField] private BattleBattlerList battleEnemyLayer = null;
        [SerializeField] private EnemyInfoComponent enemyInfoComponent = null;
        [SerializeField] private GameObject magicListRoot = null;
        [SerializeField] private BaseList magicList = null;
        [SerializeField] private GameObject conditionListRoot = null;
        [SerializeField] private BaseList conditionList = null;
        [SerializeField] private Button leftArrowButton = null;
        [SerializeField] private InputInfoComponent leftArrowButtonInput = null;
        [SerializeField] private Button rightArrowButton = null;
        [SerializeField] private InputInfoComponent rightArrowButtonInput = null;
        [SerializeField] private TextMeshProUGUI displayCategory = null;

        private System.Action _backEvent = null;

        public int EnemyListIndex => battleEnemyLayer.Index;


        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Status);
            //InitializeEnemyList();
            InitializeMagicList();
            InitializeConditionList();
            InitializeSelectCharacter();
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

        public void CallMagicList()
        {
            magicListRoot.SetActive(true);
            conditionListRoot.SetActive(false);
            displayCategory.SetText(DataSystem.GetText(15010));
            SetActivate(magicList);
        }

        public void CallConditionList()
        {
            magicListRoot.SetActive(false);
            conditionListRoot.SetActive(true);
            displayCategory.SetText(DataSystem.GetText(15020));
            SetActivate(conditionList);
        }

        private void InitializeEnemyList()
        {
            /*
            battleEnemyLayer.Initialize();
            battleEnemyLayer.SetSelectedHandler(() => CallViewEvent(CommandType.SelectEnemy));
            SetInputHandler(battleEnemyLayer.gameObject);
            */
        }

        public void SetEnemies(List<ListData> battlerInfos)
        {
            /*
            battleEnemyLayer.SetData(battlerInfos);
            battleEnemyLayer.SetInputHandler(InputKeyType.Decide,() => {});
            battleEnemyLayer.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
            SetInputHandler(battleEnemyLayer.GetComponent<IInputHandlerEvent>());
            */
        }

        private void InitializeSelectCharacter()
        {
            /*
            selectCharacter.Initialize();
            selectCharacter.SetInputHandlerAction(InputKeyType.SideLeft1,() => 
            {
                selectCharacter.SelectCharacterTabSmooth(-1);
            });
            selectCharacter.SetInputHandlerAction(InputKeyType.SideRight1,() => 
            {
                selectCharacter.SelectCharacterTabSmooth(1);
            });
            SetInputHandler(selectCharacter.gameObject);
            SetInputHandler(selectCharacter.MagicList.gameObject);
            selectCharacter.HideActionList();
            selectCharacter.SelectCharacterTab(0,false);
            selectCharacter.SetActiveTab(SelectCharacterTabType.SkillTrigger,false);
            selectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
            */
        }

        public void CommandRefreshStatus(List<ListData> skillInfos, BattlerInfo battlerInfo, List<ListData> skillTriggerInfos, List<int> enemyIndexes, int lastSelectIndex)
        {
            /*
            selectCharacter.ShowActionList();
            selectCharacter.SetEnemyBattlerInfo(battlerInfo);
            selectCharacter.SetSkillInfos(skillInfos);
            selectCharacter.SetSkillTriggerList(skillTriggerInfos);
            selectCharacter.RefreshAction(lastSelectIndex);
            */
            magicList.SetData(skillInfos);
            enemyInfoComponent.Clear();
            enemyInfoComponent.UpdateInfo(battlerInfo);
        }

        public void UpdateEnemyList(int selectIndex)
        {
            //battleEnemyLayer.UpdateSelectIndex(selectIndex);
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
                CallViewEvent(CommandType.CallConditionList);
            }
            if (InputSystem.GetInputDate(InputKeyType.SideLeft1).IsDownTrigger())
            {
                CallViewEvent(CommandType.CallMagicList);
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
        LeftEnemy,
        RightEnemy,
        SelectEnemy,
    }
}