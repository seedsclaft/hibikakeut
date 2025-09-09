using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ryneus.UseItem;

namespace Ryneus
{
    public class UseItemView : BaseView
    {
        [SerializeField] private UseItemList useItemList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.UseItem);
            InitializeUseItem();
            SetBaseAnimation(popupAnimation);
            _ = new UseItemPresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => {});
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

        public void RefreshUseItemList(List<ListData> getItemInfos)
        {
            useItemList.RefreshListData(getItemInfos);
        }
    }

    namespace UseItem
    {
        public enum CommandType
        {
            DecideUseItem = 0,
            CommandBack
        }
    }
}
