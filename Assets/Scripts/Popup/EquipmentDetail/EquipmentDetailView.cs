using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ryneus.EquipmentDetail;

namespace Ryneus
{
    public class EquipmentDetailView : BaseView
    {
        [SerializeField] private EquipmentList equipmentList = null;
        [SerializeField] private TextMeshProUGUI title = null;
        [SerializeField] private PopupAnimation popupAnimation = null;

        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.EquipmentDetail);
            InitializeEquipmentDetail();
            SetBaseAnimation(popupAnimation);
            _ = new EquipmentDetailPresenter(this);
        }

        public void OpenAnimation(Action initializeAfter)
        {
            popupAnimation.OpenAnimation(UiRoot.transform, initializeAfter);
        }

        private void InitializeEquipmentDetail()
        {
            equipmentList.Initialize();
            equipmentList.SetInputHandler(InputKeyType.Decide, () => BackEvent());
            equipmentList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            equipmentList.SetInputHandler(InputKeyType.Option1, () => CallViewEvent(CommandType.DetailItem, equipmentList.ListItemData<EquipmentInfo>()));
            
            AddViewActives(equipmentList);
        }

        public void SetTitle(string text)
        {
            UIComponent.SetText(title, text);
        }

        public void SetEquipmentDetail(List<ListData> achievementLists, bool updateSelectIndex)
        {
            equipmentList.SetData(achievementLists);
            equipmentList.UpdateHelpWindow();
            SetActivate(equipmentList);
        }
    }

    namespace EquipmentDetail
    {
        public enum CommandType
        {
            Initialize,
            DetailItem,
        }
    }
}
