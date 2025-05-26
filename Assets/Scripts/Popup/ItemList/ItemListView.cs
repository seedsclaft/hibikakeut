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
        [SerializeField] private Button presentButton = null;
        [SerializeField] private PopupAnimation popupAnimation = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.ItemList);
            InitializeItemList();
            if (presentButton != null)
            {
                presentButton.onClick.AddListener(() => CallViewEvent(CommandType.DecideItem, itemList.ListItemData<ItemInfo>()));
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
            itemList.SetInputHandler(InputKeyType.Left, () => CallViewEvent(CommandType.PlusUseNum, itemList.ListItemData<ItemInfo>()?.Id.Value));
            itemList.SetInputHandler(InputKeyType.Right, () => CallViewEvent(CommandType.MinusUseNum, itemList.ListItemData<ItemInfo>()?.Id.Value));
            SetInputHandler(itemList.gameObject);
        }

        public void SetItemList(List<ListData> achievementLists)
        {
            itemList.SetData(achievementLists,false,() =>
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
                                    CallViewEvent(CommandType.PlusUseNum,itemData.Id.Value);
                                } else
                                {
                                    CallViewEvent(CommandType.MinusUseNum,itemData.Id.Value);
                                }
                            }
                        });
                    }
                }
            });
            itemList.Activate();
        }
    }

    namespace ItemList
    {
        public enum CommandType
        {
            DecideItem = 0,
            PlusUseNum,
            MinusUseNum,
        }
    }
}
