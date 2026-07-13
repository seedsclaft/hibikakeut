using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.UseItem;
using System;

namespace Ryneus
{
    public class UseItemView : BaseView
    {
        [SerializeField] private UseItemList useItemList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;

        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.UseItem);
            InitializeUseItem();
            SetBaseAnimation(popupAnimation);
            _ = new UseItemPresenter(this);
        }

        public void OpenAnimation(Action initializeAfter)
        {
            popupAnimation.OpenAnimation(UiRoot.transform, initializeAfter);
        }

        private void InitializeUseItem()
        {
            useItemList.Initialize();
            useItemList.SetInputHandler(InputKeyType.Cancel, () => CallViewEvent(CommandType.CommandBack));
            useItemList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CommandType.DecideUseItem, useItemList.ListItemData<ItemInfo>()));
            SetInputHandler(useItemList.gameObject);
        }

        public void SetUseItem(List<ListData> getItemInfos)
        {
            useItemList.SetData(getItemInfos, true);
            useItemList.Activate();
        }
    }

    namespace UseItem
    {
        public enum CommandType
        {
            Initialize,
            DecideUseItem,
            CommandBack
        }
    }
}
