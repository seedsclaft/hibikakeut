using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

namespace Ryneus
{
    public class BaseList : ListWindow, IInputHandlerEvent
    {
        [SerializeField] private bool beforeSelect = true;
        private bool _isInit = false;
        private int _beforeSelectIndex = -1;
        private int _oneFrameUpdate = -1;
        private Action _oneFrameAction = null;
        public ListData ListData
        {
            get
            {
                if (Index > -1 && ListDates.Count > Index)
                {
                    return ListDates[Index];
                }
                return null;
            }
        }

        public T ListItemData<T>()
        {
            return (T)ListData?.Data;
        }

        public void Initialize()
        {
            if (_isInit)
            {
                return;
            }
            InitializeListView();
            SetInputCallHandler((a) => CallSelectHandler(a));
            _beforeSelectIndex = -1;
            _isInit = true;
        }

        public async void SetData(List<ListData> listData, bool resetScrollRect = true, Action initializeAfterEvent = null, bool unselect = false)
        {
            if (resetScrollRect && listData != ListDates)
            {
                ResetScrollRect();
            }
            SetListData(listData);
            CreateList();
            if (ListDates.Count > ObjectList.Count)
            {
                AddCreateList(ListDates.Count - ObjectList.Count);
            }
            UpdateObjectList();

            var selectIndex = -1;
            if (!resetScrollRect)
            {
                selectIndex = Index;
            }
            else
            {
                selectIndex = listData.FindIndex(a => a.Selected.Value);
                if (selectIndex == -1 && !unselect)
                {
                    //selectIndex = 0;
                }
            }
            if (selectIndex > 0 || initializeAfterEvent != null)
            {
                _oneFrameUpdate = selectIndex;
                _oneFrameAction = initializeAfterEvent;
                //await UniTask.DelayFrame(1);
                return;
            }
            SetListCallHandler();
            Refresh(selectIndex);
            initializeAfterEvent?.Invoke();
        }

        /// <summary>
        /// リストの中身を更新する
        /// </summary>
        /// <param name="listData"></param>
        public void RefreshListData(List<ListData> listData)
        {
            var setData = ListDates.Count == 0;
            if (setData)
            {
                SetData(listData);
            }
            SetListData(listData);
            if (!setData)
            {
                Refresh(Index);
            }
        }

        private void SetListCallHandler()
        {
            foreach (var itemPrefab in ItemPrefabList)
            {
                if (itemPrefab == null)
                {
                    continue;
                }
                var listItem = itemPrefab.GetComponent<ListItem>();
                if (listItem == null)
                {
                    continue;
                }
                listItem.SetCallHandler(CallListInputHandlerDecide);
                listItem.SetSelectHandler((index) =>
                {
                    if (Active)
                    {
                        UpdateSelectIndex(index);
                    }
                });
                listItem.SetAddListenHandler(true);
            }
        }

        public new void Refresh(int selectIndex = 0)
        {
            base.Refresh(selectIndex);
            _beforeSelectIndex = selectIndex;
        }

        private void CallListInputHandlerDecide()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (beforeSelect)
            {
                if (Index != _beforeSelectIndex)
                {
                    _beforeSelectIndex = Index;
                    return;
                }
            }
#endif
            CallListInputHandler(InputKeyType.Decide);
        }

        public void SetDisableIds(List<int> disableIds)
        {
            for (int i = 0; i < ListDates.Count; i++)
            {
                if (disableIds.Contains(i))
                {
                    ListDates[i].Enable.SetValue(false);
                }
            }
        }

        public void RefreshCurrentMouseSelect()
        {
            var rayResults = new List<RaycastResult>();
            var currentPointData = new PointerEventData(EventSystem.current);
            currentPointData.position = Input.mousePosition;
            EventSystem.current.RaycastAll(currentPointData,rayResults);
            if (rayResults.Count > 0)
            {
                var listItem = rayResults[0].gameObject.GetComponent<ListItem>();
                if (listItem != null)
                {
                    UpdateSelectIndex(listItem.Index);
                    Refresh(listItem.Index);
                }
            }
        }

        private void LateUpdate()
        {
            if (_oneFrameUpdate > -1)
            {
                SetListCallHandler();
                Refresh(_oneFrameUpdate);
                _oneFrameAction?.Invoke();
                _oneFrameUpdate = -1;
            }
        }
    }
}