using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Ryneus.Status;

namespace Ryneus
{
    public class StatusView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private Button helpButton = null;
        [SerializeField] private MagicList equipSkillList = null;
        [SerializeField] private MagicList changeSkillList = null;
        [SerializeField] private GameObject filterRoot = null;
        [SerializeField] private TextMeshProUGUI filterAttribute = null;
        [SerializeField] private Button filterPlusButton = null;
        [SerializeField] private InputInfoComponent filterPlusInput = null;
        [SerializeField] private Button filterMinusButton = null;
        [SerializeField] private InputInfoComponent filterMinusInput = null;
        [SerializeField] private UseItemList useItemList = null;
        [SerializeField] private GameObject magicListRoot = null;
        [SerializeField] private OnOffButton magicListButton = null;
        [SerializeField] private GameObject useItemRoot = null;
        [SerializeField] private OnOffButton useItemButton = null;
        [SerializeField] private GameObject statusLevelUpRoot = null;
        [SerializeField] private StatusLevelUp statusLevelUp = null;
        [SerializeField] private ActorInfoComponent selectingActorInfoComponent = null;
        [SerializeField] private Button leftArrowButton = null;
        [SerializeField] private Button rightArrowButton = null;
        [SerializeField] private Button decideButton = null;
        [SerializeField] private GameObject decideAnimation = null;
        [SerializeField] private OnOffButton characterListButton = null;
        [SerializeField] private GameObject useItemBatch = null;
        [SerializeField] private GameObject changeSkillBatch = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Status);

            InitializeEquipSkillList();
            InitializeChangeSkillList();
            InitializeUseItemList();
            if (statusLevelUp != null)
            {
                statusLevelUp.Initialize(() => CallViewEvent(CommandType.LevelUp));
            }

            if (leftArrowButton != null)
            {
                leftArrowButton.onClick.AddListener(CallLeftActor);
            }
            if (rightArrowButton != null)
            {
                rightArrowButton.onClick.AddListener(CallRightActor);
            }
            if (decideButton != null)
            {
                decideButton.onClick.AddListener(() => CallViewEvent(CommandType.DecideActor));
            }
            if (characterListButton != null)
            {
                characterListButton.OnClickAddListener(CallCharacterList);
            }
            if (filterPlusButton != null)
            {
                filterPlusButton.onClick.AddListener(() => CallViewEvent(CommandType.FilterPlus));
            }
            if (filterMinusButton != null)
            {
                filterMinusButton.onClick.AddListener(() => CallViewEvent(CommandType.FilterMinus));
            }
            if (filterPlusInput != null)
            {
                filterPlusInput.UpdateGuideIcon(InputKeyType.SideRight1);
            }
            if (filterMinusInput != null)
            {
                filterMinusInput.UpdateGuideIcon(InputKeyType.SideLeft1);
            }
            if (useItemButton != null)
            {
                useItemButton.OnClickAddListener(() => CallViewEvent(CommandType.ShowUseItem));
            }
            if (magicListButton != null)
            {
                magicListButton.OnClickAddListener(() => CallViewEvent(CommandType.AutoSetSkill));
            }
            SetBackCommand(() => OnClickBack());
            _ = new StatusPresenter(this);
        }

        private void InitializeEquipSkillList()
        {
            equipSkillList.Initialize();
            equipSkillList.SetInputHandler(InputKeyType.Decide, OnSelectEquipSkill);
            equipSkillList.SetInputHandler(InputKeyType.Cancel, () => CallViewEvent(CommandType.Back));
            equipSkillList.SetInputHandler(InputKeyType.SideLeft1, CallLeftActor);
            equipSkillList.SetInputHandler(InputKeyType.SideRight1, CallRightActor);
            equipSkillList.SetInputHandler(InputKeyType.Option1, CallCharacterList);
            equipSkillList.SetInputHandler(InputKeyType.SideLeft2, CommandScrollUpSkillHelp);
            equipSkillList.SetInputHandler(InputKeyType.SideRight2, CommandScrollDownSkillHelp);
            //equipSkillList.SetInputHandler(InputKeyType.Option1,() => CallViewEvent(CommandType.LevelUp));
            AddViewActives(equipSkillList);
        }

        private void CallLeftActor()
        {
            if (changeSkillList.gameObject.activeSelf)
            {
                return;
            }
            CallViewEvent(CommandType.LeftActor);
        }

        private void CallRightActor()
        {
            if (changeSkillList.gameObject.activeSelf)
            {
                return;
            }
            CallViewEvent(CommandType.RightActor);
        }

        private void CallCharacterList()
        {
            if (!characterListButton.gameObject.activeSelf)
            {
                return;
            }
            CallViewEvent(CommandType.CharacterList);
        }

        public void SetEquipSkillList(List<ListData> skillInfos, bool resetListIndex)
        {
            equipSkillList.SetData(skillInfos, resetListIndex);
        }

        public void SetActorInfo(ActorInfo actorInfo, List<ActorInfo> partyInfo)
        {
            selectingActorInfoComponent.UpdateInfo(actorInfo, partyInfo);
        }

        public void SetActiveActorInfo(bool isActive)
        {
            selectingActorInfoComponent.gameObject.SetActive(isActive);
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
                CallViewEvent(CommandType.SelectEquipSkill, data);
            }
        }

        private void OnCancelEquipSkill()
        {
            CallViewEvent(CommandType.CancelEquipSkill);
        }

        private void InitializeChangeSkillList()
        {
            changeSkillList.Initialize();
            changeSkillList.SetInputHandler(InputKeyType.Decide, OnSelectChangeSkill);
            changeSkillList.SetInputHandler(InputKeyType.Cancel, OnCancelEquipSkill);
            changeSkillList.SetInputHandler(InputKeyType.SideLeft2, CommandScrollUpSkillHelp);
            changeSkillList.SetInputHandler(InputKeyType.SideRight2, CommandScrollDownSkillHelp);
            //changeSkillList.SetInputHandler(InputKeyType.SideLeft1,() => CallViewEvent(CommandType.FilterMinus));
            //changeSkillList.SetInputHandler(InputKeyType.SideRight1,() => CallViewEvent(CommandType.FilterPlus));
            AddViewActives(changeSkillList);
        }

        public void SetChangeSkillList(List<ListData> skillInfos, string filterText)
        {
            changeSkillList.SetData(skillInfos);
            if (filterAttribute != null)
            {
                filterAttribute.SetText(filterText);
            }
        }

        private void OnSelectChangeSkill()
        {
            var data = changeSkillList.ListItemData<SkillInfo>();
            if (data != null)
            {
                CallViewEvent(CommandType.SelectChangeSkill, data);
            }
        }

        private void InitializeUseItemList()
        {
            /*
            useItemList.Initialize();
            useItemList.SetInputHandler(InputKeyType.Decide, OnUseItem);
            useItemList.SetInputHandler(InputKeyType.Cancel, OnCancelUseItem);
            AddViewActives(useItemList);
            */
        }

        public void SetUseItemList(List<ListData> itemInfos)
        {
            //useItemList.SetData(itemInfos, false);
        }

        private void OnUseItem()
        {
            /*
            var data = useItemList.ListItemData<ItemInfo>();
            if (data != null)
            {
                CallViewEvent(CommandType.UseItem, data);
            }
            */
        }

        private void OnCancelUseItem()
        {
            CallViewEvent(CommandType.CancelUseItem);
        }

        public void SetActiveArrows(bool isActive)
        {
            leftArrowButton.gameObject.SetActive(isActive);
            rightArrowButton.gameObject.SetActive(isActive);
        }

        public void CallEquipSkillList(bool isDecide)
        {
            magicListRoot.SetActive(true);
            useItemRoot.SetActive(false);
            SetActivate(equipSkillList);
            equipSkillList.gameObject.SetActive(true);
            if (!isDecide)
            {
                magicListButton.gameObject.SetActive(true);
                useItemButton.gameObject.SetActive(true);
            }
            changeSkillList.gameObject.SetActive(false);
            //useItemList.gameObject.SetActive(false);
            filterRoot.SetActive(false);
        }

        public void CallChangeSkillList()
        {
            SetActivate(changeSkillList);
            changeSkillList.gameObject.SetActive(true);
            equipSkillList.gameObject.SetActive(false);
            magicListButton.gameObject.SetActive(false);
            useItemButton.gameObject.SetActive(false);
            //useItemList.gameObject.SetActive(false);
            filterRoot.SetActive(true);
        }

        public void CallUseItemList()
        {
            SetDeactivate();
            /*
            magicListRoot.SetActive(false);
            useItemRoot.SetActive(true);
            SetActivate(useItemList);
            changeSkillList.gameObject.SetActive(false);
            equipSkillList.gameObject.SetActive(false);
            useItemList.gameObject.SetActive(true);
            filterRoot.SetActive(false);
            */
        }

        public void OpenAnimation(Action endEvent)
        {
        }

        public void SetHelpWindow(string helpText)
        {
        }

        public void CommandBack(Action endEvent)
        {
            endEvent?.Invoke();
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
                magicListButton.gameObject.SetActive(false);
                changeSkillBatch.gameObject.SetActive(false);
                useItemButton.gameObject.SetActive(false);
                useItemBatch.gameObject.SetActive(false);
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
                .Append(rect.DOScaleX(1.25f, duration))
                .Join(rect.DOScaleY(1.1f, duration))
                .Join(canvasGroup.DOFade(0, duration))
                .Append(canvasGroup.DOFade(0, duration)
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

        public void SetLvUpInfo(int cost, int currency)
        {
            if (statusLevelUp == null)
            {
                return;
            }
            statusLevelUp.SetLvUpInfo(cost, currency);
        }

        public void SetLvUpExpInfo(int before, int after)
        {
            if (statusLevelUp == null)
            {
                return;
            }
            statusLevelUp.SetLvUpExpInfo(before, after);
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

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (InputSystem.GetInputDate(InputKeyType.Option1).IsDownTrigger())
            {
                if (magicListButton.gameObject.activeSelf)
                {
                    CallViewEvent(CommandType.AutoSetSkill);
                }
            }
            if (InputSystem.GetInputDate(InputKeyType.Option2).IsDownTrigger())
            {
                if (useItemButton.gameObject.activeSelf)
                {
                    CallViewEvent(CommandType.ShowUseItem);
                }
            }
            if (!changeSkillList.gameObject.activeSelf)
            {
                return;
            }
            if (InputSystem.GetInputDate(InputKeyType.SideRight1).IsDownTrigger())
            {
                CallViewEvent(CommandType.FilterPlus);
            }
            if (InputSystem.GetInputDate(InputKeyType.SideLeft1).IsDownTrigger())
            {
                CallViewEvent(CommandType.FilterMinus);
            }
        }

        public new void MouseCancelHandler()
        {
        }

        public void UpdateUseItemBatch(bool isActive)
        {
            if (useItemBatch == null)
            {
                return;
            }
            useItemBatch.SetActive(isActive);
        }

        public void UpdateChangeSkillBatch(bool isActive)
        {
            if (changeSkillBatch == null)
            {
                return;
            }
            //changeSkillBatch.SetActive(isActive);
        }

        public void CommandScrollUpSkillHelp()
        {
            if (equipSkillList.Active)
            {
                equipSkillList.ScrollUpSkillHelp();
            }
            if (changeSkillList.Active)
            {
                changeSkillList.ScrollUpSkillHelp();
            }
        }

        public void CommandScrollDownSkillHelp()
        {
            if (equipSkillList.Active)
            {
                equipSkillList.ScrollDownSkillHelp();
            }
            if (changeSkillList.Active)
            {
                changeSkillList.ScrollDownSkillHelp();
            }
        }
    }

    public class StatusViewInfo
    {
        private Action _backEvent = null;
        public Action BackEvent => _backEvent;
        public ParameterBool AddActor = new();
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
            ShowUseItem,
            UseItem,
            CancelUseItem,
            AutoSetSkill,
            CharacterList,
            SelectCharacter,
            SelectCommandList,
            LvReset,
            LevelUp,
            ShowLearnMagic,
            LearnMagic,
            HideLearnMagic,
            FilterPlus,
            FilterMinus,
            CallHelp,
            Back
        }
    }
}