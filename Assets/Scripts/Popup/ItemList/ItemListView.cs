using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.ItemList;

namespace Ryneus
{
    public class ItemListView : BaseView
    {
        [SerializeField] private BaseList itemList = null;
        [SerializeField] private OnOffButton presentButton = null;
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
            SetViewCommandSceneType(ViewCommandSceneType.ItemList);
            InitializeItemList();
            if (presentButton != null)
            {
                presentButton.OnClickAddListener(() => CallViewEvent(CommandType.DecideItem, itemList.ListItemData<ItemInfo>()));
            }
            if (detailButton != null)
            {
                detailButton.OnClickAddListener(() => CallViewEvent(CommandType.DetailItem, itemList.ListItemData<ItemInfo>()));
            }
            SetBaseAnimation(popupAnimation);
            _ = new ItemListPresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => {});
        }

        private void InitializeItemList()
        {
            itemList.Initialize();
            itemList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            itemList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CommandType.DecideItem, itemList.ListItemData<ItemInfo>()));
            itemList.SetInputHandler(InputKeyType.Option1, () => CallViewEvent(CommandType.DetailItem, itemList.ListItemData<ItemInfo>()));
            itemList.SetInputHandler(InputKeyType.Right, () => CallViewEvent(CommandType.PlusUseNum, itemList.ListItemData<ItemInfo>()?.Id.Value));
            itemList.SetInputHandler(InputKeyType.Left, () => CallViewEvent(CommandType.MinusUseNum, itemList.ListItemData<ItemInfo>()?.Id.Value));
            itemList.SetInputHandler(InputKeyType.Down, CheckItemDetailButtonActive);
            itemList.SetInputHandler(InputKeyType.Up, CheckItemDetailButtonActive);
            AddViewActives(itemList);
        }

        public void SetItemList(List<ListData> achievementLists)
        {
            itemList.SetData(achievementLists, false, () =>
            {
                foreach (var prefab in itemList.ItemPrefabList)
                {
                    var comp = prefab.GetComponent<ItemListItem>();
                    if (comp != null)
                    {
                        comp.SetUseCountEvent((a) =>
                        {
                            var itemData = itemList.ListItemData<ItemInfo>();
                            if (itemData != null)
                            {
                                if (a)
                                {
                                    CallViewEvent(CommandType.PlusUseNum, itemData.Id.Value);
                                } else
                                {
                                    CallViewEvent(CommandType.MinusUseNum, itemData.Id.Value);
                                }
                            }
                        }, (a) => CallViewEvent(CommandType.DetailItem, a));
                    }
                }
            });
        }

        public void CheckItemDetailButtonActive()
        {
            if (detailButton == null)
            {
                return;
            }
            var isActive = false;
            var itemInfo = itemList.ListItemData<ItemInfo>();
            if (itemInfo != null && itemInfo.Master.ItemType == ItemType.RandumAddSkill)
            {
                isActive = true;
            }
            detailButton.gameObject.SetActive(isActive);
        }

        public void ActivateItemList(bool isActivate)
        {
            SetActivate(isActivate ? itemList : null);
        }
    }

    namespace ItemList
    {
        public enum CommandType
        {
            Initialize,
            DecideItem,
            PlusUseNum,
            MinusUseNum,
            DetailItem,
        }
    }
}
