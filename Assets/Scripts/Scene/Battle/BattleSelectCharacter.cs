using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{

    public class BattleSelectCharacter : BaseView, IInputHandlerEvent
    {

        [SerializeField] private MagicList magicList;
        public MagicList MagicList => magicList;
        [SerializeField] private BaseList conditionList;
        [SerializeField] private BaseList attributeList = null;
        public BaseList AttributeList => attributeList;
        [SerializeField] private ToggleSelect toggleSelect;

        [SerializeField] private StatusInfoComponent statusInfoComponent;
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private OnOffButton lvResetButton;
        [SerializeField] private SkillTriggerList skillTriggerList;

        [SerializeField] private Button statusButton;
        [SerializeField] private BaseList kindList;
        private bool _isInit = false;

        public int SelectedTabIndex => toggleSelect.SelectTabIndex;


        public SkillInfo ActionData
        {
            get

            {
                if (magicList.ListData != null)
                {
                    return (SkillInfo)magicList.ListData.Data;
                }
                return null;
            }
        }

        public AttributeType AttributeType
        {
            get

            {
                if (attributeList.ListData != null)
                {
                    return (AttributeType)attributeList.ListData.Data;
                }
                return AttributeType.None;
            }
        }

        public new void Initialize()
        {
            if (_isInit == true)
            {
                return;
            }
            base.Initialize();
            SetInputHandlerAction(InputKeyType.SideLeft2, () =>

            {
                CommandTabSelect(InputKeyType.SideLeft2);
            });
            SetInputHandlerAction(InputKeyType.SideRight2, () =>

            {
                CommandTabSelect(InputKeyType.SideRight2);
            });
            magicList.Initialize();
            SetInputHandler(magicList.gameObject);
            SetBusy(true);
            conditionList.Initialize();
            skillTriggerList?.Initialize();
            attributeList?.Initialize();
            kindList?.Initialize();
            UIComponent.SetActive(kindList?.gameObject, kindList != null);
            //SetInputHandler(attributeList.GetComponent<IInputHandlerEvent>());
            UIComponent.SetActive(attributeList?.gameObject, false);
            _isInit = true;
            toggleSelect.Initialize(new List<string>()
            {
                DataSystem.GetText(421),
                DataSystem.GetText(420),
                DataSystem.GetText(402),
                DataSystem.GetText(422),
            });
            toggleSelect.SetSelectTabHandler(() =>

            {
                skillTriggerList.UpdateHelpWindow();
            });
            UIComponent.SetActive(gameObject, false);
            UIComponent.SetActive(lvResetButton?.gameObject, false);
            UpdateTabs();
        }

        public void InitializeLvReset(System.Action lvResetEvent)
        {
            UIComponent.SetActive(lvResetButton?.gameObject, true);
            lvResetButton?.OnClickAddListener(() =>
            {
                lvResetEvent?.Invoke();
            });
        }

        public void SetStatusButtonEvent(System.Action statusEvent)
        {
            UIComponent.SetActive(statusButton?.gameObject, true);
            statusButton.onClick.AddListener(() =>
            {
                statusEvent?.Invoke();
            });
        }

        public void SelectCharacterTab(int tabIndex, bool needSe = true)
        {
            toggleSelect.SetSelectTabIndex(tabIndex, needSe);
        }

        public void SelectCharacterTabSmooth(int index)
        {
            toggleSelect.SelectCharacterTabSmooth(index);
        }

        private void UpdateTabs()
        {
            toggleSelect.UpdateTabs();
        }


        public void SetActiveTab(SelectCharacterTabType selectCharacterTabType, bool isActive)
        {

            toggleSelect.SetActiveTab((int)selectCharacterTabType, isActive);
        }

        public void UpdateStatus(ActorInfo actorInfo)
        {
            var currentStatus = actorInfo.CurrentStatus;
            statusInfoComponent.UpdateInfo(currentStatus);
            statusInfoComponent.UpdateHp(actorInfo.MaxHp, currentStatus.Hp);
            statusInfoComponent.UpdateMp(actorInfo.MaxMp, currentStatus.Mp);
        }

        public void UpdateStatus(BattlerInfo battlerInfo)
        {
            /*
            battlerInfoComponent.UpdateInfo(battlerInfo);
            var baseStatus = battlerInfo.CurrentStatus(true);
            var currentStatus = battlerInfo.CurrentStatus(false);
            statusInfoComponent.UpdateInfo(currentStatus, baseStatus);
            statusInfoComponent.UpdateHp(battlerInfo.MaxHp, currentStatus.Hp);
            statusInfoComponent.UpdateMp(battlerInfo.MaxMp, currentStatus.Mp);
            */
        }

        public void HideStatus()
        {
            UIComponent.SetActive(statusInfoComponent?.gameObject, false);
        }

        public void SetSkillInfos(List<ListData> skillInfoData)
        {
            magicList.SetData(skillInfoData);
            SelectCharacterTab(toggleSelect.SelectTabIndex);
        }

        public void SetAttributeList(List<ListData> list)
        {
            attributeList.SetData(list);
        }

        public void SetConditionList(List<ListData> conditionData)
        {
            conditionList.SetData(conditionData);
        }

        public void SetSkillTriggerList(List<ListData> skillTriggerLists)
        {
            skillTriggerList.SetData(skillTriggerLists);
        }

        public void SetActorInfo(ActorInfo actorInfo, List<ActorInfo> party)
        {
            actorInfoComponent.UpdateInfo(actorInfo, party);
        }

        public void SetEnemyBattlerInfo(BattlerInfo enemyInfo)
        {
            battlerInfoComponent.UpdateInfo(enemyInfo);
            kindList?.SetData(ListData.MakeListData(enemyInfo.Kinds));
        }

        public void SetInputHandlerAction(InputKeyType keyType, System.Action callEvent)
        {
            magicList.SetInputHandler(keyType, callEvent);
            conditionList.SetInputHandler(keyType, callEvent);
        }

        public void RefreshAction(int selectIndex = 0)
        {
            magicList.Refresh(selectIndex);
        }

        public void RefreshCostInfo()
        {
            magicList.UpdateAllItems();
        }

        public void ShowActionList()
        {
            UIComponent.SetActive(gameObject, true);
        }

        public void HideActionList()
        {
            UIComponent.SetActive(gameObject, false);
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (attributeList != null && !attributeList.gameObject.activeSelf)
            {
                // タブ選択操作
                if (keyTypes.Contains(InputKeyType.SideLeft2))
                {
                    toggleSelect.SelectPrev();
                }
                else
                if (keyTypes.Contains(InputKeyType.SideRight2))
                {
                    toggleSelect.SelectNext();
                }
            }
        }

        private void CommandTabSelect(InputKeyType keyType)
        {
            if (attributeList.gameObject.activeSelf)
            {
                // 属性切替操作
                var selectAttribute = attributeList.Index;
                if (keyType == InputKeyType.SideLeft2)
                {
                    selectAttribute--;
                }
                else
                if (keyType == InputKeyType.SideRight2)
                {
                    selectAttribute++;
                }
                if (selectAttribute < 0)
                {
                    selectAttribute = attributeList.DataCount - 1;
                }
                else
                if (selectAttribute >= attributeList.DataCount)
                {
                    selectAttribute = 0;
                }
                attributeList.UpdateSelectIndex(selectAttribute);
            }
            else
            {
            }
        }
    }

    public enum SelectCharacterTabType
    {
        Detail = 0,
        Magic = 1,
        Condition = 2,
        SkillTrigger = 3
    }
}