using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyInfo;

namespace Ryneus
{
    public class EnemyInfoView : BaseView,IInputHandlerEvent
    {
        [SerializeField] private BattleBattlerList battleEnemyLayer = null;
        [SerializeField] private BattleSelectCharacter selectCharacter = null;
        [SerializeField] private EnemyInfoComponent enemyInfoComponent = null;
        private new System.Action<EnemyInfoViewEvent> _commandData = null;
        private System.Action _backEvent = null;

        public int EnemyListIndex => battleEnemyLayer.Index;


        public override void Initialize()
        {
            base.Initialize();
            battleEnemyLayer.Initialize();
            battleEnemyLayer.SetSelectedHandler(() => 
            {
                var eventData = new EnemyInfoViewEvent(CommandType.SelectEnemy);
                _commandData(eventData);
            });
            
            selectCharacter.Initialize();
            SetInputHandler(selectCharacter.gameObject);
            InitializeSelectCharacter();
            new EnemyInfoPresenter(this);
            SetInputHandler(gameObject);
        }

        public void SetEnemies(List<ListData> battlerInfos)
        {
            battleEnemyLayer.SetData(battlerInfos);
            battleEnemyLayer.SetInputHandler(InputKeyType.Decide,() => {});
            battleEnemyLayer.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
            SetInputHandler(battleEnemyLayer.GetComponent<IInputHandlerEvent>());
        }

        private void InitializeSelectCharacter()
        {
            selectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => {});
            selectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
            selectCharacter.SetInputHandlerAction(InputKeyType.SideLeft1,() => 
            {
                selectCharacter.SelectCharacterTabSmooth(-1);
            });
            selectCharacter.SetInputHandlerAction(InputKeyType.SideRight1,() => 
            {
                selectCharacter.SelectCharacterTabSmooth(1);
            });
            SetInputHandler(selectCharacter.MagicList.GetComponent<IInputHandlerEvent>());
            selectCharacter.HideActionList();
            selectCharacter.SelectCharacterTab(0,false);
            selectCharacter.SetActiveTab(SelectCharacterTabType.Magic,false);
            selectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
        }

        public void CommandRefreshStatus(List<ListData> skillInfos,BattlerInfo battlerInfo,List<ListData> skillTriggerInfos,List<int> enemyIndexes,int lastSelectIndex)
        {
            selectCharacter.ShowActionList();
            selectCharacter.SetEnemyBattlerInfo(battlerInfo);
            //selectCharacter.SetSkillInfos(skillInfos);
            selectCharacter.SetSkillTriggerList(skillTriggerInfos);
            selectCharacter.RefreshAction(lastSelectIndex);
            enemyInfoComponent.Clear();
            enemyInfoComponent.UpdateInfo(battlerInfo);
        }

        public void UpdateEnemyList(int selectIndex)
        {
            battleEnemyLayer.UpdateSelectIndex(selectIndex);
        }

        private void OnClickBack()
        {
            var eventData = new EnemyInfoViewEvent(CommandType.Back);
            _commandData(eventData);
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

        public void SetEvent(System.Action<EnemyInfoViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetCondition(List<ListData> skillInfos)
        {
            selectCharacter.SetConditionList(skillInfos);
        }

        public new void SetBackEvent(System.Action backEvent)
        {
            _backEvent = backEvent;
            SetBackCommand(() => 
            {    
                var eventData = new EnemyInfoViewEvent(CommandType.Back);
                _commandData(eventData);
            });
            ChangeBackCommandActive(true);
        }

        public void CommandBack()
        {
            _backEvent?.Invoke();
        }

        public void InputHandler(List<InputKeyType> keyTypes,bool pressed)
        {
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
        LeftEnemy,
        RightEnemy,
        SelectEnemy,
    }
}
public class EnemyInfoViewEvent
{
    public CommandType commandType;
    public object template;

    public EnemyInfoViewEvent(CommandType type)
    {
        commandType = type;
    }
}