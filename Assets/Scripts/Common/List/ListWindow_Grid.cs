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
        private int _gridColumnCount = 1;
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
            var height = GetViewPortHeight();
            var listMargin = ListMargin(false);
            var space = ItemSpace(false);
            return (int)Math.Floor((height - listMargin) / (_itemSize.y + space));
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
                    Debug.Log("itemIndex:" + i + "がobjectIndex: " + itemIndex);
                    itemPrefab.transform.SetParent(_objectList[itemIndex].transform, false);
                    itemPrefab.SetActive(true);
                }
            }
        }

        private void UpdateGridScrollRect(List<InputKeyType> keyTypes)
        {
            /*
            if (keyTypes.Contains(InputKeyType.Down))
            {
                UpdateGridDown();
            }
            else
            if (keyTypes.Contains(InputKeyType.Up))
            {
                UpdateGridUp();
            }

            if (keyTypes.Contains(InputKeyType.Right))
            {
                UpdateGridRight();
            }
            else
            if (keyTypes.Contains(InputKeyType.Left))
            {
                UpdateGridLeft();
            }
            */
        }

        private bool UpdateListGrid()
        {
            int startIndex = GetStartIndex(_horizontal);
            int gridIndex = GetStartIndex(!_horizontal);
            var update = false;
            if (gridIndex != _lastStartIndexY)
            {
                if (gridIndex > _lastStartIndexY && gridIndex > 0)
                {
                    UpdateListDownGrid(gridIndex);
                    update = true;
                }
                else
                if (gridIndex < _lastStartIndexY && gridIndex >= 0)
                {
                    UpdateListUpGrid(gridIndex);
                    update = true;
                }
            }
            if (startIndex > _lastStartIndexX)
            {
                UpdateListRightGrid(startIndex);
                update = true;
            }
            else
            if (startIndex < _lastStartIndexX)
            {
                UpdateListLeftGrid(startIndex);
                update = true;
            }
            if (update)
            {
                _selectedHandler?.Invoke();
            }
            return update;
        }

        private void UpdateListDownGrid(int gridIndex)
        {
            var lineIndex = gridIndex - _lastStartIndexY;
            _lastStartIndexY = gridIndex;
            int startIndex = GetStartIndex(_horizontal);
            var verticalCount = GetVerticalCount();
            var horizontalCount = GetHorizonalCount();
            var itemPerIndex = (gridIndex - 1) / (verticalCount + 1);
            var objectPerIndex = startIndex / (horizontalCount + 1);
            for (int j = 0; j < lineIndex; j++)
            {
                for (int i = 0; i <= horizontalCount; i++)
                {
                    var itemIndex = ((gridIndex - 1 - j) * (horizontalCount + 1)) + i;
                    if (itemPerIndex > 0)
                    {
                        itemIndex -= itemPerIndex * (verticalCount + 1) * (horizontalCount + 1);
                    }
                    if (!WithinItemIndex(itemIndex))
                    {
                        continue;
                    }
                    var objectIndex = (_gridColumnCount * verticalCount) + ((gridIndex - j) * _gridColumnCount) + i;
                    if (startIndex - (objectPerIndex * (horizontalCount + 1)) > i)
                    {
                        objectIndex += ((objectPerIndex + 1) * horizontalCount) + 1 + objectPerIndex;
                    }
                    else
                    {
                        objectIndex += objectPerIndex * (horizontalCount + 1);
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

        private void UpdateListUpGrid(int gridIndex)
        {
            var lineIndex = _lastStartIndexY - gridIndex;
            _lastStartIndexY = gridIndex;
            int startIndex = GetStartIndex(_horizontal);
            var verticalCount = GetVerticalCount();
            var horizontalCount = GetHorizonalCount();
            var itemPerIndex = gridIndex / (verticalCount + 1);
            var objectPerIndex = startIndex / (horizontalCount + 1);
            for (int j = 0; j < lineIndex; j++)
            {
                for (int i = 0; i <= horizontalCount; i++)
                {
                    var itemIndex = ((gridIndex + j) * (horizontalCount + 1)) + i;
                    if (itemPerIndex > 0)
                    {
                        itemIndex -= itemPerIndex * (verticalCount + 1) * (horizontalCount + 1);
                    }
                    if (!WithinItemIndex(itemIndex))
                    {
                        continue;
                    }
                    var objectIndex = ((gridIndex + j) * _gridColumnCount) + i;
                    if (startIndex - (objectPerIndex * (horizontalCount + 1)) > i)
                    {
                        objectIndex += ((objectPerIndex + 1) * horizontalCount) + 1 + objectPerIndex;
                    }
                    else
                    {
                        objectIndex += objectPerIndex * (horizontalCount + 1);
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
                    //Debug.Log("itemIndex:" + itemIndex + "がobjectIndex: " + objectIndex);
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
                var lines = _horizontal ? Cols() : Rows();
                var singleLine = _horizontal ? Rows() : Cols();
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

/*
        private void UpdateGridDown()
        {
            if (_objectList.Count <= _index + _gridColumnCount)
            {
                return;
            }
            var selectItem = _objectList[_index + _gridColumnCount];
            var itemPosition = GetCornerPosition(selectItem, 0, false);
            var col = (_index / _gridColumnCount) + 2f;
            if (_index % 2 == 1)
            {
                col += 0.5f;
            }
            if (itemPosition < 0)
            {
                float verticalCount = GetVerticalCount();
                if (col != verticalCount)
                {
                    var c = col - verticalCount;
                    var p = GetGridRowCount() - verticalCount;
                    var per = 1f - (c / p);

                    ScrollRect.verticalNormalizedPosition = Math.Max(per, 0);
                }
            }
        }

        private void UpdateGridUp()
        {
            if (0 > _index - _gridColumnCount)
            {
                return;
            }
            var selectItem = _objectList[_index - _gridColumnCount];
            var itemPosition = GetCornerPosition(selectItem, 0, false);
            var col = (_index / _gridColumnCount) - 2f;
            if (_index % 2 == 1)
            {
                col += 0.5f;
            }
            if (itemPosition > (720 - _itemSize.y))
            {
                float verticalCount = GetVerticalCount();

                var c = GetGridRowCount() - verticalCount - col;
                var p = GetGridRowCount() - verticalCount;
                var per = (c / p);

                ScrollRect.verticalNormalizedPosition = Math.Min(1, per);
            }
        }

        private void UpdateGridRight()
        {
            var row = _index % _gridColumnCount;
            var width = GetViewPortWidth();
            var horizontalCount = GetHorizonalCount();

            if (row + 2 < horizontalCount)
            {
                return;
            }
            var nextRow = _index + 1;
            if (nextRow % _gridColumnCount < _index % _gridColumnCount)
            {
                return;
            }
            var selectItem = _objectList[nextRow];
            var itemPosition = GetCornerPosition(selectItem, 2, true);
            if (itemPosition > width)
            {
                float c = (row + 2) - horizontalCount;
                float p = _gridColumnCount - horizontalCount;
                var per = (c / p);

                ScrollRect.horizontalNormalizedPosition = Math.Max(per, 0);
            }
        }

        private void UpdateGridLeft()
        {
            var nextRow = _index - 1;
            if (nextRow % _gridColumnCount > _index % _gridColumnCount)
            {
                return;
            }
            if (nextRow < 0)
            {
                return;
            }
            var selectItem = _objectList[nextRow];
            var itemPosition = GetCornerPosition(selectItem, 2, true);
            var row = (_index % _gridColumnCount) - 2f;
            if (itemPosition < _itemSize.x)
            {
                var horizontalCount = GetHorizonalCount();

                float c = _gridColumnCount - horizontalCount - row;
                float p = _gridColumnCount - horizontalCount;
                var per = 1f - (c / p);

                ScrollRect.horizontalNormalizedPosition = Math.Max(0, per);
            }
        }
*/

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
            UpdateGridPositionX(selectIndex);
            UpdateGridPositionY(selectIndex);
        }

        private void UpdateGridPositionX(int selectIndex)
        {
            float nextX = selectIndex % _gridColumnCount;
            var horizontalCount = GetHorizonalCount();
            var horizontarmargin = (_gridColumnCount - horizontalCount) / 2;
            var perX = 0f;
            // 右区間
            if ((_gridColumnCount - nextX) <= horizontarmargin)
            {
                perX = 1;
            }
            else
            // 左区間
            if (nextX < horizontarmargin)
            {

            }
            else
            // 中区間
            {
                perX = (nextX - horizontarmargin) / (_gridColumnCount - (horizontarmargin * 2));
            }
            ScrollRect.horizontalNormalizedPosition = Math.Max(perX, 0);
        }

        private void UpdateGridPositionY(int selectIndex)
        {
            float nextY = selectIndex / _gridColumnCount;
            var verticalCount = GetVerticalCount();
            var verticalmargin = verticalCount / 2;
            var perY = 1f;
            // 下区間
            if ((GetGridRowCount() - nextY) <= verticalmargin)
            {
                perY = 0;
            }
            else
            // 上区間
            if (nextY < verticalmargin)
            {

            }
            else
            // 中区間
            {
                perY = 1f - ((nextY - verticalmargin) / (GetGridRowCount() - (verticalmargin * 2)));
            }
            ScrollRect.verticalNormalizedPosition = Math.Max(0, perY);
        }
    }
}
