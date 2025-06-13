using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Ryneus.Status;

namespace Ryneus
{
    public class StatusView : BaseView,IInputHandlerEvent
    {
        [SerializeField] private Button helpButton = null;
        [SerializeField] private MagicList equipSkillList = null;
        [SerializeField] private MagicList changeSkillList = null;
        [SerializeField] private GameObject statusLevelUpRoot = null;
        [SerializeField] private StatusLevelUp statusLevelUp = null;
        [SerializeField] private ActorInfoComponent selectingActorInfoComponent = null;
        [SerializeField] private Button leftArrowButton = null;
        [SerializeField] private Button rightArrowButton = null;
        [SerializeField] private Button decideButton = null;
        [SerializeField] private GameObject decideAnimation = null;
        [SerializeField] private OnOffButton characterListButton = null;

        private Action _backEvent = null;
        private string _helpText;
        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Status);

            InitializeEquipSkillList();
            InitializeChangeSkillList();
            if (statusLevelUp != null)
            {
                statusLevelUp.Initialize(() => CallViewEvent(CommandType.LevelUp));
            }

            if (leftArrowButton != null)
            {
                leftArrowButton.onClick.AddListener(() => CallViewEvent(CommandType.LeftActor));
            }
            if (rightArrowButton != null)
            {
                rightArrowButton.onClick.AddListener(() => CallViewEvent(CommandType.RightActor));
            }
            if (decideButton != null)
            {
                decideButton.onClick.AddListener(() => CallViewEvent(CommandType.DecideActor));
            }
            if (characterListButton != null)
            {
                characterListButton.OnClickAddListener(() => CallViewEvent(CommandType.CharacterList));
            }
            _ = new StatusPresenter(this);
        }

        private void InitializeEquipSkillList()
        {
            equipSkillList.Initialize();
            equipSkillList.SetInputHandler(InputKeyType.Decide,OnSelectEquipSkill);
            equipSkillList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            equipSkillList.SetInputHandler(InputKeyType.SideLeft1,() => CallViewEvent(CommandType.LeftActor));
            equipSkillList.SetInputHandler(InputKeyType.SideRight1,() => CallViewEvent(CommandType.RightActor));
            equipSkillList.SetInputHandler(InputKeyType.Option1,() => CallViewEvent(CommandType.LevelUp));
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
            if (decideButton.gameObject.activeSelf)
            {
                // この場合は仲間決定動作
                CallViewEvent(CommandType.DecideActor);
                return;
            }
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

        public void CommandBack()
        {
            _backEvent?.Invoke();
        }

        public void SetActiveDecide(bool isActive)
        {
            if (decideButton == null)
            {
                return;
            }
            decideButton.gameObject.SetActive(isActive);
            if (isActive)
            {
                SetDecideAnimation();
            }
        }

        private void SetDecideAnimation()
        {
            if (decideAnimation == null)
            {
                return;
            }
            var rect = decideAnimation.GetComponent<RectTransform>();
            var canvasGroup = decideAnimation.GetComponent<CanvasGroup>();
            var duration = 1f;
            DOTween.Sequence()
                .Append(rect.DOScaleX(1.25f,duration))
                .Join(rect.DOScaleY(1.1f,duration))
                .Join(canvasGroup.DOFade(0,duration))
                .Append(canvasGroup.DOFade(0,duration)
                .SetEase(Ease.InOutQuad))
                .SetLoops(-1);
        }

        public void SetActiveLvUpInfo(bool isActive)
        {
            if (statusLevelUpRoot == null)
            {
                return;
            }
            statusLevelUpRoot.SetActive(isActive);
        }

        public void SetLvUpInfo(int cost,int currency)
        {
            if (statusLevelUp == null)
            {
                return;
            }
            statusLevelUp.SetLvUpInfo(cost,currency);
        }

        public void SetActiveCharacterList(bool isActive)
        {
            if (characterListButton == null)
            {
                return;
            }
            characterListButton.gameObject.SetActive(isActive);
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
            /*
            if (_isDisplayDecide)
            {
                SetHelpText(_helpText);
                SetHelpInputInfo("SELECT_HEROINE");
            } else
            {
                SetHelpText(DataSystem.GetHelp(202));
                SetHelpInputInfo("STATUS");
            }
            */
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
        public ParameterBool DisplayDecideButton = new();
        public ParameterBool DisplayBackButton = new(true);
        public ParameterBool DisplayCharacterList = new();
        public ParameterBool DisplayLvUpInfo = new();
        private List<ActorInfo> _actorInfos = null;
        public List<ActorInfo> ActorInfos => _actorInfos;
        private List<BattlerInfo> _enemyInfos = null;
        public List<BattlerInfo> EnemyInfos => _enemyInfos;
        public ParameterBool IsBattle = new();
        public ParameterBool IsRanking = new();
        public ParameterInt StartIndex = new(-1);
        private Action<int> _charaLayerEvent = null;
        public Action<int> CharaLayerEvent => _charaLayerEvent;

        public StatusViewInfo(Action backEvent)
        {
            _backEvent = backEvent;
        }

        public void SetEnemyInfos(List<BattlerInfo> enemyInfos,bool isBattle)
        {
            _enemyInfos = enemyInfos;
            IsBattle.SetValue(isBattle);
        }

        public void SetActorInfos(List<ActorInfo> actorInfos,bool isBattle)
        {
            _actorInfos = actorInfos;
            IsBattle.SetValue(isBattle);
        }

        public void SetCharaLayerEvent(Action<int> charaLayerEvent)
        {
            _charaLayerEvent = charaLayerEvent;
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
            DecideActor,
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