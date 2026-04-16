using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public abstract partial class ListWindow : MonoBehaviour
    {
        private bool _active = true;
        public bool Active => _active;

        private int _index = 0;
        public int Index => _index;
        private List<int> _selectIndexes = new();
        public List<int> SelectIndexes => _selectIndexes;
        public void SetSelectIndexes(List<int> selectIndexes)
        {
            _selectIndexes = selectIndexes;
        }

        private int _listMoveInputFrameFirst = 8;
        private int _listMoveGamePadFrameFirst = 8;
        private int _listMoveInputFrame = 3;
        private int _listMoveGamePadFrame = 3;
        private int _inputBusyFrame = 0;
        [SerializeField] private bool reverse = false;
        [SerializeField] private bool warpMode = true;
        [SerializeField] private GameObject itemPrefab = null;

        private ScrollRect _scrollRect = null;
        public ScrollRect ScrollRect => _scrollRect;
        private bool _horizontal => _scrollRect.horizontal;
        private List<GameObject> _itemPrefabList = new();
        public List<GameObject> ItemPrefabList => _itemPrefabList;
        private GameObject _prefabPool = null;
        private List<ListData> _listDates = new();
        public List<ListData> ListDates => _listDates;
        public void SetListData(List<ListData> listData)
        {
            if (reverse)
            {
                listData.Reverse();
            }
            _listDates = listData;
        }
        public int DataCount => _listDates.Count;
        private Vector2 _itemSize;
        private int _lastStartIndexX = 0;
        private int _lastStartIndexY = 0;
        private LinkedList<IListViewItem> _itemList = new();
        private List<GameObject> _objectList = new();
        public List<GameObject> ObjectList => _objectList;
        public int ObjectListCount => _objectList.FindAll(a => a.activeSelf).Count;

        private Action<List<InputKeyType>> _inputCallHandler = null;
        private Dictionary<InputKeyType, Action> _inputHandler = new();

        private Action _selectedHandler = null;

        public HelpWindow _helpWindow = null;

        private Action _cancelEvent = null;

        private GameObject _blankObject;
        public void SetCancelEvent(Action cancelEvent)
        {
            _cancelEvent = cancelEvent;
        }

        public void Activate()
        {
            ResetInputFrame(1);
            _active = true;
        }

        public void Deactivate()
        {
            _active = false;
        }

        private void DestroyListChildren()
        {
            foreach (Transform child in _scrollRect.content.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void InitializeListView()
        {
            var _basePrefab = new GameObject();
            _prefabPool = Instantiate(_basePrefab);
            Destroy(_basePrefab);
            _prefabPool.name = "prefab pool";
            _prefabPool.transform.SetParent(gameObject.transform, false);
            _scrollRect = GetComponentInChildren<ScrollRect>();
            DestroyListChildren();
            _objectList = new();
            SetValueChangedEvent();
            SetItemSize();
            _inputCallHandler = null;
            _scrollRect.scrollSensitivity = 10;
        }

        private void SetValueChangedEvent()
        {
            _scrollRect.onValueChanged.AddListener(ValueChanged);
        }

        private void ValueChanged(Vector2 scrollPosition)
        {
            if (EnableValueChanged())
            {
                UpdateListItem();
                UpdateSelectIndex(Index);
            }
        }

        private bool EnableValueChanged()
        {
            int startIndex = GetStartIndex(_horizontal);
            int gridIndex = GetStartIndex(!_horizontal);
            if (_grid)
            {
                if (gridIndex >= 0 && _lastStartIndexY != gridIndex)
                {
                    return true;
                }
            }
            if (startIndex < 0 || _lastStartIndexX == startIndex)
            {
                return false;
            }
            return true;
        }

        public void SetItemSize()
        {
            _itemSize = itemPrefab.GetComponent<RectTransform>().sizeDelta;
        }

        public void CreateList()
        {
            if (_itemPrefabList.Count > 0)
            {
                var listCount = ListItemCount();
                if (_itemPrefabList.Count < listCount)
                {
                    var createCount = listCount - _itemPrefabList.Count;
                    CreateListPrefab(createCount);
                }
                return;
            }
            CreateObjectPrefab();
            CreateListItemPrefab();
        }

        public void UpdateObjectList()
        {
            for (var i = 0; i < _objectList.Count; i++)
            {
                UIComponent.SetActive(_objectList[i], _listDates.Count > i);
            }
        }

        private void CreateObjectPrefab()
        {
            _blankObject = new GameObject("blank");
            _blankObject.AddComponent<RectTransform>();
            _blankObject.transform.SetParent(_scrollRect.content, false);
            _blankObject.name = "blank";
            _objectList.Add(_blankObject);
            var rect = _blankObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector3(_itemSize.x, _itemSize.y, 0);
            rect.pivot = new Vector3(0, 1, 0);
            int createCount = _listDates.Count;
            for (var i = 0; i < createCount - 1; i++)
            {
                var prefab = Instantiate(_blankObject);
                prefab.transform.SetParent(_scrollRect.content, false);
                prefab.name = "blank Object : " + i;
                _objectList.Add(prefab);
            }
            if (reverse)
            {
                _objectList.Reverse();
            }
        }

        private void CreateListItemPrefab()
        {
            var listCount = ListItemCount();
            CreateListPrefab(listCount);
        }

        private void CreateListPrefab(int createCount)
        {
            // 上下用に1つ多く作成
            if (GetVerticalCount() < _listDates.Count)
            {
                createCount++;
            }
            for (var i = 0; i < createCount; i++)
            {
                var prefab = Instantiate(itemPrefab);
                prefab.name += "_" + i.ToString();
                _itemPrefabList.Add(prefab);
                var view = prefab.GetComponent<IListViewItem>();
                if (view != null)
                {
                    _itemList.AddLast(view);
                }
            }
        }

        public void UpdateItemPrefab()
        {
            var startIndex = GetStartIndex(_horizontal);
            var itemStartIndex = startIndex % _itemPrefabList.Count;
            if (_grid)
            {
                UpdateListGridItem(itemStartIndex);
                return;
            }
            for (int i = 0; i < _itemPrefabList.Count; i++)
            {
                var itemPrefab = _itemPrefabList[i];
                if (itemStartIndex > -1)
                {
                    var tempIndex = (i + itemStartIndex) % _itemPrefabList.Count;
                    itemPrefab = _itemPrefabList[tempIndex];
                }
                var itemIndex = i + startIndex;
                var listItem = itemPrefab.GetComponent<ListItem>();
                if (_listDates.Count <= itemIndex || ObjectListCount <= itemIndex || itemIndex < 0)
                {
                    if (itemStartIndex > -1)
                    {
                        // 1つ上のグループの最後に設定
                        itemIndex -= _itemPrefabList.Count;
                        if (itemIndex < 0)
                        {
                            UIComponent.SetActive(itemPrefab, false);
                            itemPrefab.transform.SetParent(_prefabPool.transform, false);
                            continue;
                        }
                    }
                    else
                    {
                        UIComponent.SetActive(itemPrefab, false);
                        listItem.SetListData(null, -1);
                        listItem.SetUnSelect();
                        continue;
                    }
                }
                listItem.SetListData(_listDates[itemIndex], itemIndex);
                //Debug.Log("itemIndex:" + i + "がobjectIndex: " + itemIndex);
                itemPrefab.transform.SetParent(_objectList[itemIndex].transform, false);
                UIComponent.SetActive(itemPrefab, true);
            }
        }

        public void Refresh(int selectIndex = 0)
        {
            UpdateItemPrefab();
            UpdateAllItems();

            UpdateScrollRect(selectIndex);
        }

        public void AddCreateList(int count)
        {
            int createCount = count;
            foreach (var objectList in _objectList)
            {
                for (int i = 0; i < objectList.transform.childCount; i++)
                {
                    var child = objectList.transform.GetChild(i);
                    child.transform.SetParent(_scrollRect.content, false);
                }
            }
            for (var i = 0; i < createCount; i++)
            {
                var prefab = Instantiate(_blankObject);
                prefab.name = "blank Object";
                prefab.transform.SetParent(_scrollRect.content, false);
                _objectList.Add(prefab);
            }
            var startIndex = 0;
            for (int i = startIndex; i < _itemPrefabList.Count; i++)
            {
                if (_listDates.Count <= i)
                {
                    continue;
                }
                _itemPrefabList[i].transform.SetParent(_objectList[i].transform, false);
            }
        }

        public InputKeyType GetPlusKey()
        {
            return _horizontal ? InputKeyType.Right : InputKeyType.Down;
        }

        public InputKeyType GetMinusKey()
        {
            return _horizontal ? InputKeyType.Left : InputKeyType.Up;
        }

        public InputKeyType GetPageUpKey()
        {
            return _horizontal ? InputKeyType.Down : InputKeyType.Right;
        }

        public InputKeyType GetPageDownKey()
        {
            return _horizontal ? InputKeyType.Up : InputKeyType.Left;
        }

        public bool InputDir4(List<InputKeyType> keyTypes)
        {
            var findAll = keyTypes.FindAll(a => a is InputKeyType.Up or InputKeyType.Down or InputKeyType.Left or InputKeyType.Right);
            return findAll.Count > 0;
        }

        private float GetViewPortWidth()
        {
            return _scrollRect.viewport.rect.width;
        }

        private float GetViewPortHeight()
        {
            return _scrollRect.viewport.rect.height;
        }

        private float GetVisibleRectHeight()
        {
            return _scrollRect.content.rect.height - GetViewPortHeight();
        }

        private float GetScrolledWidth()
        {
            return (_scrollRect.content.rect.width - GetViewPortWidth()) * _scrollRect.normalizedPosition.x;
        }

        private float GetScrolledHeight()
        {
            return (_scrollRect.content.rect.height - GetViewPortHeight()) * (1.0f - _scrollRect.normalizedPosition.y);
        }

        private int Cols()
        {
            var cols = (int)Math.Floor(GetViewPortWidth() / _itemSize.x);
            return cols > 0 ? cols : 1;
        }

        private int Rows()
        {
            var rows = (int)Math.Floor(GetViewPortHeight() / _itemSize.y);
            return rows > 0 ? rows : 1;
        }

        private int GridCols()
        {
            var cols = (int)Math.Floor(GetViewPortWidth() / _itemSize.x);
            var gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();
            if (gridLayoutGroup != null)
            {
                return gridLayoutGroup.constraintCount;
            }
            return cols > 0 ? cols : 1;
        }

        private int GridRows()
        {
            var rows = (int)Math.Floor(GetViewPortHeight() / _itemSize.y);
            var gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();
            if (gridLayoutGroup != null)
            {
                return gridLayoutGroup.constraintCount;
            }
            return rows > 0 ? rows : 1;
        }

        private float ItemSpaceTop(bool isHorizontal)
        {
            if (isHorizontal)
            {
                var horizontal = GetComponentInChildren<HorizontalLayoutGroup>();
                if (horizontal != null)
                {
                    return horizontal.padding.left;
                }
            }
            else
            {
                var vertical = GetComponentInChildren<VerticalLayoutGroup>();
                if (vertical != null)
                {
                    return vertical.padding.top;
                }
            }
            return 0;
        }

        private float ItemSpace(bool isHorizontal)
        {
            if (isHorizontal)
            {
                var horizontal = GetComponentInChildren<HorizontalLayoutGroup>();
                if (horizontal != null)
                {
                    return horizontal.spacing;
                }
            }
            else
            {
                var vertical = GetComponentInChildren<VerticalLayoutGroup>();
                if (vertical != null)
                {
                    return vertical.spacing;
                }
            }
            return 0;
        }

        private float ListMargin(bool isHorizontal)
        {
            if (isHorizontal)
            {
                var horizontal = GetComponentInChildren<HorizontalLayoutGroup>();
                if (horizontal != null)
                {
                    return horizontal.padding.left + horizontal.padding.right;
                }
            }
            else
            {
                var vertical = GetComponentInChildren<VerticalLayoutGroup>();
                if (vertical != null)
                {
                    return vertical.padding.top + vertical.padding.bottom;
                }
            }
            return 0;
        }

        /// <summary>
        /// 始点リスト番号
        /// </summary>
        /// <returns></returns>
        public int GetStartIndex(bool horizontal)
        {
            var itemSpace = ItemSpace(horizontal);
            var listMargin = ListMargin(horizontal);
            var itemSize = horizontal ? _itemSize.x : _itemSize.y;
            var rectSize = horizontal ? GetScrolledWidth() : Math.Max(0, GetScrolledHeight());
            var index = (int)Math.Floor((Math.Round(rectSize) - 0 - listMargin + 0) / (itemSize + itemSpace));
            return Math.Max(0, index);
        }

        public void UpdateListItem()
        {
            if (_grid)
            {
                var update = UpdateListGrid();
                if (update)
                {
                    return;
                }
            }
            var horizontalCount = GetHorizonalCount();
            var verticalCount = GetVerticalCount();
            horizontalCount += 1;
            int startIndex = GetStartIndex(_horizontal);
            int gridIndex = GetStartIndex(!_horizontal);

            if (startIndex != _lastStartIndexX && _itemPrefabList.Count > 0)
            {
                if (startIndex - 1 == _lastStartIndexX && _lastStartIndexX > -1)
                {
                    UpdateListDown(startIndex);
                    _selectedHandler?.Invoke();
                    return;
                }
                else
                if (startIndex + 1 == _lastStartIndexX && _lastStartIndexX > -1)
                {
                    UpdateListUp(startIndex);
                    _selectedHandler?.Invoke();
                    return;
                }
                UpdateItemPrefab();
                UpdateAllItems();
                _lastStartIndexX = startIndex;
                _selectedHandler?.Invoke();
            }
        }

        private void UpdateListDown(int startIndex)
        {
            _lastStartIndexX = startIndex;
            var itemIndex = (startIndex - 1) % _itemPrefabList.Count;
            var objectIndex = _itemPrefabList.Count + startIndex - 1;
            if (objectIndex > ObjectListCount - 1)
            {
                return;
            }
            Debug.Log("itemIndex:" + itemIndex + "がobjectIndex: " + objectIndex);
            UpdateListItem(itemIndex, objectIndex);
        }

        private void UpdateListUp(int startIndex)
        {
            _lastStartIndexX = startIndex;
            var itemIndex = (startIndex - 0) % _itemPrefabList.Count;
            if (itemIndex < 0)
            {
                return;
            }
            var objectIndex = startIndex - 0;
            if (objectIndex < 0)
            {
                return;
            }
            //Debug.Log("itemIndex:" + itemIndex + "がobjectIndex: " + objectIndex);
            UpdateListItem(itemIndex, objectIndex);
        }

        private void UpdateListItem(int itemIndex, int objectIndex)
        {
            var itemPrefab = _itemPrefabList[itemIndex];
            itemPrefab.transform.SetParent(_objectList[objectIndex].transform, false);
            var listItem = itemPrefab.GetComponent<ListItem>();
            if (listItem != null)
            {
                listItem.SetListData(_listDates[objectIndex], objectIndex);
                var view = itemPrefab.GetComponent<IListViewItem>();
                view.UpdateViewItem();
            }
        }

        public void UpdateAllItems()
        {
            foreach (var item in _itemList)
            {
                item.UpdateViewItem();
            }
        }

        public void SelectIndex(int selectIndex)
        {
            var callHandler = _index != selectIndex;
            _index = selectIndex;
            if (callHandler)
            {
                _selectedHandler?.Invoke();
            }
        }

        public void Update()
        {
            UpdateInputFrame();
        }

        private void UpdateInputFrame()
        {
            if (_inputBusyFrame > 0)
            {
                _inputBusyFrame--;
            }
        }

        public bool IsInputEnable()
        {
            if (this == null)
            {
                return false;
            }
            if (GameSystem.Instance != null && GameSystem.Instance.TutorialBusy)
            {
                return false;
            }
            if (!_active || !gameObject || !gameObject.activeSelf)
            {
                return false;
            }
            if (_inputBusyFrame > 0)
            {
                return false;
            }
            return true;
        }

        public void ResetInputFrame(int plusValue)
        {
            _inputBusyFrame = plusValue;
        }

        public void SetHelpWindow(HelpWindow helpWindow)
        {
            _helpWindow = helpWindow;
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (keyTypes == null)
            {
                //ResetInputOneFrame();
            }
            if (_inputBusyFrame > 0 && keyTypes.Count == 0)
            {
                _inputBusyFrame = 0;
            }
            if (!IsInputEnable())
            {
                return;
            }
            if (InputDir4(keyTypes))
            {
                InputSelectIndex(keyTypes);
            }
            InputCallEvent(keyTypes);
            int plusValue = 0;
            if (InputDir4(keyTypes))
            {
                if (InputSystem.IsGamePad)
                {
                    plusValue = pressed ? _listMoveGamePadFrame : _listMoveGamePadFrameFirst;
                }
                else
                {
                    plusValue = pressed ? _listMoveInputFrame : _listMoveInputFrameFirst;
                }
            }
            else
            {
                plusValue = 0;
            }
            ResetInputFrame(plusValue);
        }

        public void InputSelectIndex(List<InputKeyType> keyTypes)
        {
            if (_grid)
            {
                InputSelectGridIndex(keyTypes);
                return;
            }
            var currentIndex = Index;
            var selectIndex = Index;
            var plusKey = GetPlusKey();
            var minusKey = GetMinusKey();
            var pageUpKey = GetPageUpKey();
            var pageDownKey = GetPageDownKey();
            var nextIndex = Index;
            if (keyTypes.Contains(plusKey) || keyTypes.Contains(minusKey))
            {
                for (int i = 0; i < _listDates.Count; i++)
                {
                    if (keyTypes.Contains(plusKey))
                    {
                        nextIndex = Index + i + 1;
                        if (nextIndex >= _listDates.Count)
                        {
                            nextIndex -= _listDates.Count;
                        }
                    }
                    else
                    if (keyTypes.Contains(minusKey))
                    {
                        nextIndex = Index - i - 1;
                        if (nextIndex < 0)
                        {
                            nextIndex += _listDates.Count;
                        }
                    }
                    var listItem = _objectList[nextIndex].GetComponent<ListItem>();
                    if (listItem == null || listItem.Disable == null)
                    {
                        break;
                    }
                    if (listItem.Disable != null && !listItem.Disable.activeSelf)
                    {
                        break;
                    }
                }
            }
            else
            if (keyTypes.Contains(pageUpKey) || keyTypes.Contains(pageDownKey))
            {
                // 列移動
                var lines = _horizontal ? Cols() : Rows();
                var singleLine = _horizontal ? Rows() : Cols();
                if (lines > 1 && singleLine > 1)
                {
                    for (int i = 0; i < lines; i++)
                    {
                        if (keyTypes.Contains(pageUpKey))
                        {
                            nextIndex = Index + (i + 1) * lines;
                        }
                        else
                        if (keyTypes.Contains(pageDownKey))
                        {
                            nextIndex = Index + (i + 1) * -1 * lines;
                        }
                        if (nextIndex < 0 || nextIndex > _listDates.Count - 1)
                        {
                            nextIndex = Index;
                            break;
                        }
                        var listItem = _objectList[nextIndex].GetComponent<ListItem>();
                        if (listItem == null || listItem.Disable == null)
                        {
                            break;
                        }
                        if (listItem.Disable != null && listItem.Disable.activeSelf == false)
                        {
                            break;
                        }
                    }
                }
            }

            selectIndex = nextIndex;
            if (warpMode)
            {
                if (selectIndex >= _listDates.Count)
                {
                    selectIndex = 0;
                }
                else
                if (selectIndex < 0)
                {
                    selectIndex = _listDates.Count - 1;
                }
            }

            if (currentIndex != selectIndex)
            {
                SoundManager.Instance.PlayStaticSe(SEType.CursorMove);
                UpdateSelectIndex(selectIndex);
            }
        }

        public void UpdateSelectIndex(int index)
        {
            //Debug.Log(this.name + " " + index);
            SelectIndex(index);
            UpdateHelpWindow();
            foreach (var objectList in _objectList)
            {
                if (objectList == null)
                {
                    continue;
                }
                var listItem = objectList.GetComponentInChildren<ListItem>();
                if (listItem == null)
                {
                    continue;
                }
                if (_active && (index == listItem.Index || _selectIndexes.Contains(listItem.Index)))
                {
                    listItem.SetSelect();
                }
                else
                {
                    listItem.SetUnSelect();
                }
            }
        }

        public void UnselectAll()
        {
            foreach (var objectList in _objectList)
            {
                if (objectList == null)
                {
                    continue;
                }
                var listItem = objectList.GetComponentInChildren<ListItem>();
                if (listItem == null)
                {
                    continue;
                }
                listItem.SetUnSelect();
            }

            _index = -1;
        }

        public void SetInputCallHandler(Action<List<InputKeyType>> callHandler)
        {
            _inputCallHandler = callHandler;
        }

        public void SetInputHandler(InputKeyType keyType, Action handler)
        {
            _inputHandler[keyType] = handler;
            if (keyType == InputKeyType.Cancel)
            {
                SetCancelEvent(handler);
            }
        }

        public void SetSelectedHandler(Action selectedHandler)
        {
            _selectedHandler = selectedHandler;
        }

        public void CallListInputHandler(InputKeyType keyType)
        {
            if (!IsInputEnable())
            {
                return;
            }
            if (_inputHandler.ContainsKey(keyType))
            {
                _inputHandler[keyType]?.Invoke();
            }
        }

        private void InputCallEvent(List<InputKeyType> keyTypes)
        {
            if (!IsInputEnable())
            {
                return;
            }
            _inputCallHandler?.Invoke(keyTypes);
            if (keyTypes.Count == 0)
            {
                return;
            }
            foreach (var keyType in keyTypes)
            {
                CallListInputHandler(keyType);
            }
        }

        public void MouseCancelHandler()
        {
            if (!IsInputEnable())
            {
                return;
            }
            _cancelEvent?.Invoke();
        }

        public void MouseMoveHandler(Vector3 position)
        {

        }

        public void MouseWheelHandler(Vector2 position)
        {

        }

        public virtual void UpdateHelpWindow()
        {
        }

        public void CallSelectHandler(List<InputKeyType> keyTypes)
        {
            if (InputDir4(keyTypes))
            {
                UpdateScrollRect(keyTypes);
            }
        }

        private void UpdateScrollRect(List<InputKeyType> keyTypes)
        {
            if (_index < 0)
            {
                return;
            }
            if (ObjectListCount <= _index)
            {
                return;
            }
            if (_grid)
            {
                UpdateGridScrollRect(keyTypes);
                return;
            }
            if (_horizontal)
            {
                // 並行は未実装
                return;
            }
            float verticalCount = GetVerticalCount();
            // 全てリスト内に収まる
            if (verticalCount > _listDates.Count)
            {
                return;
            }
            var plusKey = GetPlusKey();
            var minusKey = GetMinusKey();
            var selectItem = _objectList[_index];
            var itemPosition = Math.Round(GetCornerPosition(selectItem, 0, false));
            var positionY = 0f;
            var viewPortPosition = Math.Round(GetCornerPosition(_scrollRect.viewport.gameObject, 0, false));
            var update = false;
            if (keyTypes.Contains(plusKey))
            {
                // 位置を更新する
                if (itemPosition < viewPortPosition)
                {
                    // 最下段
                    if (_index == _listDates.Count - 1)
                    {
                        positionY = ScrollRect.content.rect.height - ScrollRect.viewport.rect.height;
                    }
                    else
                    {
                        var c = _index - verticalCount + 1;
                        positionY = c * (_itemSize.y + ItemSpace(false));
                    }
                    update = true;
                }
                else
                if (warpMode && _index == 0)
                {
                    positionY = 0;
                    update = true;
                }
            }
            else
            if (keyTypes.Contains(minusKey))
            {
                // 位置を更新する
                if (itemPosition >= (GetViewPortHeight() + viewPortPosition - _itemSize.y))
                {
                    var c = _index;
                    positionY = c * (_itemSize.y + ItemSpace(false));
                    update = true;
                }
                else
                if (warpMode && _index == _listDates.Count - 1)
                {
                    positionY = ScrollRect.content.rect.height - ScrollRect.viewport.rect.height;
                    update = true;
                }
            }
            if (!update)
            {
                return;
            }
            ScrollRect.content.SetAnchoredPositionY(positionY);
        }

        /// <summary>
        /// 高さが均等に収まるリストか
        /// </summary>
        /// <returns></returns>
        private bool IsExtraHeightRectSize(float verticalCount)
        {
            return GetViewPortHeight() == (_itemSize.y + ItemSpace(false)) * verticalCount;
        }

        public void UpdateScrollRect(int selectIndex)
        {
            if (_index < 0)
            {
                return;
            }
            var update = selectIndex != _index;
            LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.content);
            UpdateSelectIndex(selectIndex);
            if (_grid)
            {
                UpdateGridScrollRect(selectIndex);
                return;
            }
            if (!update)
            {
                return;
            }
            if (_horizontal)
            {
                return;
            }
            float visibleCount = _horizontal ? GetHorizonalCount() : GetVerticalCount();
            var p = ObjectListCount - visibleCount;
            if (p == 0)
            {
                return;
            }
            var c = _index - visibleCount + 1;
            if (c < 0)
            {
                c = 0;
            }
            var positionY = c * (_itemSize.y + ItemSpace(false));
            ScrollRect.content.SetAnchoredPositionY(positionY);
        }

        private int ListItemCount()
        {
            var horizontalCount = GetHorizonalCount();
            var verticalCount = GetVerticalCount();
            if (_grid)
            {
                return horizontalCount * (verticalCount + 1) + verticalCount;
            }
            else
            if (_horizontal)
            {
                return horizontalCount * Rows();
            }
            return verticalCount * Cols();
        }

        public void ResetScrollRect()
        {
            ScrollRect.normalizedPosition = _horizontal ? new Vector2(1, 0) : new Vector2(0, 1);
            _lastStartIndexX = -1;
        }

        public void Release()
        {
            OnDestroy();
        }

        private void OnDestroy()
        {
            for (int i = _itemPrefabList.Count - 1; 0 <= i; i--)
            {
                Destroy(_itemPrefabList[i]);
            }
            for (int i = _objectList.Count - 1; 0 <= i; i--)
            {
                Destroy(_objectList[i]);
            }
            if (_prefabPool != null)
            {
                Destroy(_prefabPool);
            }
            if (_blankObject != null)
            {
                Destroy(_blankObject);
            }
        }
    }
}