using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public abstract partial class ListWindow : MonoBehaviour
    {
        [SerializeField] private bool _grid = false;
        private int _gridColumnCount = 16;
        private int _calcVerticalCount = -1;
        public void SetGridColumnCount(int columnCount)
        {
            _gridColumnCount = columnCount;
            GetComponentInChildren<GridLayoutGroup>().constraintCount = columnCount;
        }

        public int GridColumnCount()
        {
            return _grid ? _gridColumnCount : 1;
        }

        public int GetVerticalCount()
        {
            if (_calcVerticalCount != -1)
            {
                return _calcVerticalCount;
            }
            var height = GetViewPortHeight();
            var listMargin = ListMargin(false);
            height -= listMargin;
            var itemHeight = 0f;
            var count = 0;
            while (itemHeight < height)
            {
                if (count == 0)
                {
                    itemHeight += _itemSize.y;
                }
                else
                {
                    itemHeight += _itemSize.y + ItemSpace(false);
                }
                count++;
            }
            _calcVerticalCount = count;
            return count;
        }

        public int GetHorizonalCount()
        {
            var width = GetViewPortWidth();
            var listMargin = ListMargin(true);
            var space = ItemSpace(true);
            return (int)Math.Floor((width - listMargin) / (_itemSize.x + space));
        }

        public int GetGridRowCount()
        {
            return ObjectListCount / _gridColumnCount;
        }

        public void UpdateListGridItem(int itemStartIndex = -1)
        {
            if (_grid)
            {
                var update = UpdateListGrid();
                if (update)
                {
                    return;
                }
            }
            var startIndex = GetStartIndex(_horizontal);

            for (int i = 0; i < _itemPrefabList.Count; i++)
            {
                var itemPrefab = _itemPrefabList[i];
                if (itemStartIndex > -1)
                {
                    //
                    var tempIndex = (i + itemStartIndex) % _itemPrefabList.Count;
                    itemPrefab = _itemPrefabList[tempIndex];
                }
                var itemIndex = i + startIndex;
                if (_listDates.Count > itemIndex)
                {
                    var listItem = itemPrefab.GetComponent<ListItem>();
                    listItem.SetListData(_listDates[itemIndex], itemIndex);
                    //Debug.Log("itemIndex:" + i + "がobjectIndex: " + itemIndex);
                    itemPrefab.transform.SetParent(_objectList[itemIndex].transform, false);
                    UIComponent.SetActive(itemPrefab, true);
                }
            }
        }

        private void UpdateGridScrollRect(List<InputKeyType> keyTypes)
        {
            var plusKey = GetPlusKey();
            var minusKey = GetMinusKey();
            var pageUpKey = GetPageUpKey();
            var pageDownKey = GetPageDownKey();
            float verticalCount = GetVerticalCount();
            float horizonalCount = GetHorizonalCount();
            var selectItem = _objectList[_index];
            var itemPosition = Math.Round(GetCornerPosition(selectItem, 0, false));
            var positionY = 0f;
            var viewPortPosition = Math.Round(GetCornerPosition(_scrollRect.viewport.gameObject, 0, false));
            var update = false;
            // 全てリスト内に収まる
            if (verticalCount > _listDates.Count / horizonalCount)
            {
                return;
            }
            if (keyTypes.Contains(plusKey) || keyTypes.Contains(pageDownKey))
            {
                // 位置を更新する
                if (itemPosition < viewPortPosition)
                {
                    // 最下段
                    var verticalListCount = Math.Ceiling(_listDates.Count / horizonalCount);
                    var index = Math.Ceiling((_index - _index % horizonalCount) / horizonalCount);
                    if (index >= verticalListCount - 1)
                    {
                        positionY = ScrollRect.content.rect.height - ScrollRect.viewport.rect.height;
                    }
                    else
                    {
                        var c = (int)Math.Floor((_index / horizonalCount) - (verticalCount) + 1);
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
            if (keyTypes.Contains(minusKey) || keyTypes.Contains(pageUpKey))
            {
                // 位置を更新する
                if (itemPosition >= (GetViewPortHeight() + viewPortPosition - _itemSize.y))
                {
                    var c = (int)Math.Floor(_index / horizonalCount);
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
            /*
            int startIndex = GetStartIndex(_horizontal);
            int gridIndex = GetStartIndex(!_horizontal);
            if (keyTypes.Contains(InputKeyType.Down))
            {
                UpdateListDownGrid(gridIndex + 1);
            }
            else
            if (keyTypes.Contains(InputKeyType.Up))
            {
                UpdateListUpGrid(gridIndex);
            }

            if (keyTypes.Contains(InputKeyType.Right))
            {
                UpdateListRightGrid(startIndex);
            }
            else
            if (keyTypes.Contains(InputKeyType.Left))
            {
                UpdateListLeftGrid(startIndex);
            }
            */
        }

        private bool UpdateListGrid()
        {
            int startIndex = GetStartIndex(_horizontal);
            int gridIndex = GetStartIndex(!_horizontal);
            var update = false;
            /*
            if (gridIndex != _lastStartIndexY)
            {
                if (gridIndex > _lastStartIndexY && gridIndex > 0)
                {
                    //UpdateListDownGrid(gridIndex);
                    update = true;
                }
                else
                if (gridIndex < _lastStartIndexY && gridIndex >= 0)
                {
                    //UpdateListUpGrid(gridIndex);
                    update = true;
                }
            }
            */
            if (startIndex > _lastStartIndexY)
            {
                UpdateListDownGrid(startIndex);
                update = true;
            }
            else
            if (startIndex < _lastStartIndexY)
            {
                UpdateListUpGrid(startIndex);
                update = true;
            }
            if (update)
            {
                _selectedHandler?.Invoke();
            }
            return update;
        }

        private void UpdateListDownGrid(int startIndex)
        {
            var lineIndex = startIndex - _lastStartIndexY;
            var lastStartIndexY = _lastStartIndexY;
            _lastStartIndexY = startIndex;
            var verticalCount = GetVerticalCount();
            var horizontalCount = GetHorizonalCount();
            for (int j = 0; j < lineIndex; j++)
            {
                for (int i = 0; i < horizontalCount; i++)
                {
                    var itemIndex = (lastStartIndexY * horizontalCount) + (j * horizontalCount) + i;
                    if (!WithinItemIndex(itemIndex))
                    {
                        continue;
                    }
                    var objectIndex = ((verticalCount + 1) * (horizontalCount + 1)) + itemIndex;
                    if (!WithinObjectIndex(objectIndex))
                    {
                        continue;
                    }
                    //Debug.Log("itemIndex:" + itemIndex + "がobjectIndex: " + objectIndex);
                    UpdateListItem(itemIndex, objectIndex);
                }
            }
        }

        private void UpdateListUpGrid(int startIndex)
        {
            var lineIndex = _lastStartIndexY - startIndex;
            var lastStartIndexY = _lastStartIndexY;
            _lastStartIndexY = startIndex;
            var verticalCount = GetVerticalCount();
            var horizontalCount = GetHorizonalCount();
            for (int j = 0; j < lineIndex; j++)
            {
                for (int i = 0; i < horizontalCount; i++)
                {
                    var itemIndex = ((lastStartIndexY - lineIndex) * horizontalCount) + (j * horizontalCount) + i;
                    if (!WithinItemIndex(itemIndex))
                    {
                        continue;
                    }
                    var objectIndex = itemIndex;
                    if (!WithinObjectIndex(objectIndex))
                    {
                        continue;
                    }
                    //Debug.Log("itemIndex:" + itemIndex + "がobjectIndex: " + objectIndex);
                    UpdateListItem(itemIndex, objectIndex);
                }
            }
        }

        private void UpdateListRightGrid(int startIndex)
        {
            var lineIndex = startIndex - _lastStartIndexX;
            _lastStartIndexX = startIndex;
            int gridIndex = GetStartIndex(!_horizontal);
            var verticalCount = GetVerticalCount();
            var horizontalCount = GetHorizonalCount();
            var itemPerIndex = (startIndex - 1) / (horizontalCount + 1);
            var objectPerIndex = (gridIndex - 1) / (verticalCount + 1);
            for (int j = 0; j < lineIndex; j++)
            {
                for (int i = 0; i <= verticalCount; i++)
                {
                    var itemIndex = startIndex - 1 - j + (i * (horizontalCount + 1));
                    if (itemPerIndex > 0)
                    {
                        itemIndex -= itemPerIndex * (horizontalCount + 1);
                    }
                    if (!WithinItemIndex(itemIndex))
                    {
                        continue;
                    }
                    var objectIndex = startIndex - j + horizontalCount + (i * _gridColumnCount) + (objectPerIndex * (_gridColumnCount) * (verticalCount + 1));// + gridIndex * _gridColumnCount;
                    if (gridIndex - (objectPerIndex * (verticalCount + 1)) > i)
                    {
                        objectIndex += _gridColumnCount * (verticalCount + 1);
                    }
                    if (!WithinObjectIndex(objectIndex))
                    {
                        continue;
                    }
                    Debug.Log("itemIndex:" + itemIndex + "がobjectIndex: " + objectIndex);
                    UpdateListItem(itemIndex, objectIndex);
                }
            }
        }

        private void UpdateListLeftGrid(int startIndex)
        {
            var lineIndex = _lastStartIndexX - startIndex;
            _lastStartIndexX = startIndex;
            int gridIndex = GetStartIndex(!_horizontal);
            var verticalCount = GetVerticalCount();
            var horizontalCount = GetHorizonalCount();
            var itemPerIndex = startIndex / (horizontalCount + 1);
            var objectPerIndex = (gridIndex - 1) / (verticalCount + 1);
            for (int j = 0; j < lineIndex; j++)
            {
                for (int i = 0; i <= verticalCount; i++)
                {
                    var itemIndex = startIndex + j + (i * (horizontalCount + 1));
                    if (itemPerIndex > 0)
                    {
                        itemIndex -= itemPerIndex * (horizontalCount + 1);
                    }
                    if (!WithinItemIndex(itemIndex))
                    {
                        continue;
                    }
                    var objectIndex = startIndex + j + (i * _gridColumnCount) + (objectPerIndex * (_gridColumnCount) * (verticalCount + 1));
                    if (gridIndex - (objectPerIndex * (verticalCount + 1)) > i)
                    {
                        objectIndex += _gridColumnCount * (verticalCount + 1);
                    }
                    if (!WithinObjectIndex(objectIndex))
                    {
                        continue;
                    }
                    //Debug.Log("itemIndex:" + itemIndex + "がobjectIndex: " + objectIndex);
                    UpdateListItem(itemIndex, objectIndex);
                }
            }
        }

        private bool WithinItemIndex(int itemIndex)
        {
            return itemIndex >= 0 && itemIndex < _itemPrefabList.Count;
        }

        private bool WithinObjectIndex(int objectIndex)
        {
            return objectIndex >= 0 && objectIndex < _objectList.Count;
        }

        public void InputSelectGridIndex(List<InputKeyType> keyTypes)
        {
            var currentIndex = Index;
            var selectIndex = Index;
            var plusKey = GetPlusKey();
            var minusKey = GetMinusKey();
            var pageUpKey = GetPageUpKey();
            var pageDownKey = GetPageDownKey();
            var nextIndex = Index;
            if (keyTypes.Contains(pageUpKey) || keyTypes.Contains(pageDownKey))
            {
                for (int i = 0; i < _listDates.Count; i++)
                {
                    if (keyTypes.Contains(pageDownKey))
                    {
                        nextIndex = Index + i + 1;
                        if (nextIndex >= _listDates.Count)
                        {
                            nextIndex -= _listDates.Count;
                        }
                    }
                    else
                    if (keyTypes.Contains(pageUpKey))
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
            } else
            if (keyTypes.Contains(plusKey) || keyTypes.Contains(minusKey))
            {
                // 列移動
                var lines = _horizontal ? GridCols() : GridRows();
                var singleLine = _horizontal ? GridRows() : GridCols();
                if (lines > 1 && singleLine > 1)
                {
                    for (int i = 0; i < lines; i++)
                    {
                        if (keyTypes.Contains(plusKey))
                        {
                            nextIndex = Index + (i + 1) * lines;
                        }
                        else
                        if (keyTypes.Contains(minusKey))
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

        private float GetCornerPosition(GameObject gameObject, int index, bool isHorizontal)
        {
            var corners = new Vector3[4];
            gameObject.GetComponent<RectTransform>().GetWorldCorners(corners);
            corners[index] = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[index]);
            if (isHorizontal)
            {
                return corners[index].x * 1280 / Screen.width;
            }
            return corners[index].y * 720 / Screen.height;
        }

        private void UpdateGridScrollRect(int selectIndex)
        {
            float visibleCount = _horizontal ? GetHorizonalCount() : GetVerticalCount();
            float gridCount = _horizontal ? GetVerticalCount() : GetHorizonalCount();
            var p = Math.Floor((ObjectListCount / gridCount) - visibleCount);
            if (p == 0)
            {
                return;
            }
            var c = (int)Math.Floor(selectIndex / gridCount) - visibleCount + 1;
            if (c < 0)
            {
                c = 0;
            }
            var positionY = c * (_itemSize.y + ItemSpace(false));
            ScrollRect.content.SetAnchoredPositionY(positionY);
        }
    }
}
