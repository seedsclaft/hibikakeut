using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.SelectEquipment;
using System;

namespace Ryneus
{
    public class SelectEquipmentView : BaseView
    {
        [SerializeField] private EquipmentList equipmentList = null;
        public int SelectIndex => equipmentList.Index;
        [SerializeField] private OnOffButton detailButton = null;
        [SerializeField] private PopupAnimation popupAnimation = null;

        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.SelectEquipment);
            InitializeSelectEquipment();
            if (detailButton != null)
            {
                detailButton.OnClickAddListener(() => CallViewEvent(CommandType.DetailItem, equipmentList.ListItemData<EquipmentInfo>()));
            }
            SetBaseAnimation(popupAnimation);
            _ = new SelectEquipmentPresenter(this);
        }

        public void OpenAnimation(Action initializeAfter)
        {
            popupAnimation.OpenAnimation(UiRoot.transform, initializeAfter);
        }

        private void InitializeSelectEquipment()
        {
            equipmentList.Initialize();
            equipmentList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            equipmentList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CommandType.DecideItem, equipmentList.ListItemData<EquipmentInfo>()));
            equipmentList.SetInputHandler(InputKeyType.Option1, () => CallViewEvent(CommandType.DetailItem, equipmentList.ListItemData<EquipmentInfo>()));
            AddViewActives(equipmentList);
        }

        public void SetSelectEquipment(List<ListData> equiomentLists)
        {
            equipmentList.SetData(equiomentLists);
            equipmentList.UpdateHelpWindow();
        }

        public void UpdateEquipmentList(List<ListData> equiomentLists)
        {
            equipmentList.RefreshListData(equiomentLists);
            equipmentList.UpdateHelpWindow();
        }

        public void CheckItemDetailButtonActive()
        {
            if (detailButton == null)
            {
                return;
            }
            UIComponent.SetActive(detailButton?.gameObject, true);
        }

        public void ActivateSelectEquipment(bool isActivate)
        {
            SetActivate(isActivate ? equipmentList : null);
        }
    }

    namespace SelectEquipment
    {
        public enum CommandType
        {
            Initialize,
            DecideItem,
            DetailItem,
        }
    }
}
