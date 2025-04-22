using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace Ryneus
{
    using Status;
    public class StatusView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private Button helpButton = null;
        [SerializeField] private MagicList equipSkillList = null;
        [SerializeField] private MagicList changeSkillList = null;
        [SerializeField] private ActorInfoComponent selectingActorInfoComponent = null;
        [SerializeField] private Button leftArrowButton = null;
        [SerializeField] private Button rightArrowButton = null;

        private StatusViewInfo _statusViewInfo = null; 

        private Action _backEvent = null;
        private bool _isDisplayDecide => _statusViewInfo != null && _statusViewInfo.DisplayDecideButton;
        public bool DisplayDecide => _isDisplayDecide;
        private string _helpText;
        public bool IsRanking => _statusViewInfo != null && _statusViewInfo.IsRanking;
        public override void Initialize() 
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Status);
            
            InitializeEquipSkillList();
            InitializeChangeSkillList();

            if (leftArrowButton != null)
            {
                leftArrowButton.onClick.AddListener(() => CallViewEvent(CommandType.LeftActor));
            }
            if (rightArrowButton != null)
            {
                rightArrowButton.onClick.AddListener(() => CallViewEvent(CommandType.RightActor));
            }
            new StatusPresenter(this);
        }

        private void InitializeEquipSkillList()
        {
            equipSkillList.Initialize();
            equipSkillList.SetInputHandler(InputKeyType.Decide,OnSelectEquipSkill);
            equipSkillList.SetInputHandler(InputKeyType.Cancel,OnCancelEquipSkill);
            SetInputHandler(equipSkillList.gameObject);
            AddViewActives(equipSkillList);
        }

        public void SetEquipSkillList(List<ListData> skillInfos)
        {
            equipSkillList.SetData(skillInfos);
        }

        public void SetActorInfo(ActorInfo actorInfo,List<ActorInfo> partyInfo)
        {
            selectingActorInfoComponent.UpdateInfo(actorInfo,partyInfo);
        }

        private void OnSelectEquipSkill()
        {
            var data = equipSkillList.ListItemData<SkillInfo>();
            if (data != null)
            {
                CallViewEvent(CommandType.SelectEquipSkill,data);
            }
        }

        private void OnCancelEquipSkill()
        {
            CallViewEvent(CommandType.CancelEquipSkill);
        }

        private void InitializeChangeSkillList()
        {
            changeSkillList.Initialize();
            changeSkillList.SetInputHandler(InputKeyType.Decide,OnSelectChangeSkill);
            changeSkillList.SetInputHandler(InputKeyType.Cancel,OnCancelEquipSkill);
            changeSkillList.SetInputHandler(InputKeyType.SideLeft1,() => CallViewEvent(CommandType.LeftActor));
            changeSkillList.SetInputHandler(InputKeyType.SideRight1,() => CallViewEvent(CommandType.RightActor));
            SetInputHandler(changeSkillList.gameObject);
            AddViewActives(changeSkillList);
        }

        public void SetChangeSkillList(List<ListData> skillInfos)
        {
            changeSkillList.SetData(skillInfos);
        }

        private void OnSelectChangeSkill()
        {
            var data = changeSkillList.ListItemData<SkillInfo>();
            if (data != null)
            {
                CallViewEvent(CommandType.SelectChangeSkill,data);
            }
        }

        public void SetActiveArrows(bool isActive)
        {
            leftArrowButton.gameObject.SetActive(isActive);
            rightArrowButton.gameObject.SetActive(isActive);
        }

        public void CallEquipSkillList()
        {
            SetActivate(equipSkillList);
            equipSkillList.gameObject.SetActive(true);
            changeSkillList.gameObject.SetActive(false);
        }

        public void CallChangeSkillList()
        {
            SetActivate(changeSkillList);
            changeSkillList.gameObject.SetActive(true);
            equipSkillList.gameObject.SetActive(false);
        }

        public void OpenAnimation(Action endEvent)
        {
        }

        public void SetHelpWindow(string helpText)
        {
            _helpText = helpText;
        }

        public void SetViewInfo(StatusViewInfo statusViewInfo)
        {
            _statusViewInfo = statusViewInfo;
            _backEvent = statusViewInfo.BackEvent;
            SetBackEvent(statusViewInfo.BackEvent);
            if (statusViewInfo.StartIndex != -1)
            {
                CallViewEvent(CommandType.SelectCharacter,statusViewInfo.StartIndex);
            }
        }

        public void CommandBack()
        {
            _backEvent?.Invoke();
        }

        public new void SetBusy(bool busy)
        {
            base.SetBusy(busy);
        }

        private void OnClickBack()
        {
            CallViewEvent(CommandType.Back);
        }

        private void OnClickHelp()
        {
            CallViewEvent(CommandType.CallHelp);
        }

        public int SelectedSkillId()
        {
            return -1;
        }

        public void CommandRefresh()
        {
            if (_isDisplayDecide)
            {
                SetHelpText(_helpText);
                SetHelpInputInfo("SELECT_HEROINE");
            } else
            {
                SetHelpText(DataSystem.GetHelp(202));
                SetHelpInputInfo("STATUS");
            }
        }

        public void InputHandler(List<InputKeyType> keyTypes,bool pressed)
        {
        }

        public new void MouseCancelHandler()
        {
        }
    }

    public class StatusViewInfo
    {
        private Action _backEvent = null;
        public Action BackEvent => _backEvent;
        private bool _displayDecideButton = false;
        public bool DisplayDecideButton => _displayDecideButton;
        private bool _displayBackButton = true;
        public bool DisplayBackButton => _displayBackButton;
        private bool _displayCharacterList = true;
        public bool DisplayCharacterList => _displayCharacterList;
        private bool _displayLvResetButton = false;
        public bool DisplayLvResetButton => _displayLvResetButton;
        private List<ActorInfo> _actorInfos = null;
        public List<ActorInfo> ActorInfos => _actorInfos;
        private List<BattlerInfo> _enemyInfos = null;
        public List<BattlerInfo> EnemyInfos => _enemyInfos;
        private bool _isBattle = false;
        public bool IsBattle => _isBattle;
        private bool _isRanking = false;
        public bool IsRanking => _isRanking;
        private int _startIndex = -1;
        public int StartIndex => _startIndex;
        private Action<int> _charaLayerEvent = null;
        public Action<int> CharaLayerEvent => _charaLayerEvent;
        
        public StatusViewInfo(Action backEvent)
        {
            _backEvent = backEvent;
        }

        public void SetDisplayDecideButton(bool isDisplay)
        {
            _displayDecideButton = isDisplay;
        }
        
        public void SetDisplayBackButton(bool isDisplay)
        {
            _displayBackButton = isDisplay;
        }
        
        public void SetDisplayCharacterList(bool isDisplay)
        {
            _displayCharacterList = isDisplay;
        }

        public void SetDisplayLevelResetButton(bool isDisplay)
        {
            _displayLvResetButton = isDisplay;
        }

        public void SetEnemyInfos(List<BattlerInfo> enemyInfos,bool isBattle)
        {
            _enemyInfos = enemyInfos;
            _isBattle = isBattle;
        }

        public void SetActorInfos(List<ActorInfo> actorInfos,bool isBattle)
        {
            _actorInfos = actorInfos;
            _isBattle = isBattle;
        }

        public void SetStartIndex(int actorIndex)
        {
            _startIndex = actorIndex;
        }        
        
        public void SetCharaLayerEvent(System.Action<int> charaLayerEvent)
        {
            _charaLayerEvent = charaLayerEvent;
        }

        public void SetIsRanking(bool isRanking)
        {
            _isRanking = isRanking;
        }        
    }
    
    namespace Status
    {
        public enum CommandType
        {
            None = 0,
            SelectActor,
            CancelActor,
            LeftActor,
            RightActor,
            SelectEquipSkill,
            CancelEquipSkill,
            SelectChangeSkill,
            DecideStage,
            CharacterList,
            SelectCharacter,
            SelectCommandList,
            LvReset,
            LevelUp,
            ShowLearnMagic,
            LearnMagic,
            HideLearnMagic,
            CallHelp,
            Back
        }
    }
}