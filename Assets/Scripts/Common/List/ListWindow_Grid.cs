using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    abstract public partial class ListWindow : MonoBehaviour
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
            return (int)Math.Round((height - listMargin) / (_itemSize.y + space));
        }

        public int GetHorizonalCount()
        {
            var width = GetViewPortWidth();
            var listMargin = ListMargin(true);
            var space = ItemSpace(true);
            return (int)Math.Round((width - listMargin) / (_itemSize.x + space));
        }

        public int GetGridRowCount()
        {
            return _objectList.Count / _gridColumnCount;
        }

        private void UpdateGridScrollRect(List<InputKeyType> keyTypes)
        {
            if (keyTypes.Contains(InputKeyType.Down))
            {
                UpdateGridDown();
            } else
            if (keyTypes.Contains(InputKeyType.Up))
            {
                UpdateGridUp();
            }

            if (keyTypes.Contains(InputKeyType.Right))
            {
                UpdateGridRight();
            } else
            if (keyTypes.Contains(InputKeyType.Left))
            {
                UpdateGridLeft();
            }
        }

        private void UpdateListDownGrid(int gridIndex)
        {
            var lineIndex = gridIndex - _lastStartIndexY;
            _lastStartIndexY = gridIndex;
            int startIndex = GetStartIndex(_horizontal);
            var length = GetVerticalCount();
            var horizontalCount = GetHorizonalCount();
            for (int j = 0;j < lineIndex;j++)
            {
                for (int i = 0;i <= horizontalCount;i++)
                {
                    var itemIndex = (gridIndex - 1 - j) * (horizontalCount + 1) + i;
                    if (itemIndex < 0)
                    {
                        continue;
                    }
                    var itemPrefab = _itemPrefabList[itemIndex];
                    var objectIndex = _gridColumnCount * length + (gridIndex - j) * _gridColumnCount + i + startIndex;
                    if (startIndex > i)
                    {
                        objectIndex += horizontalCount - (startIndex - 1);
                    } else
                    {
                        objectIndex -= startIndex;
                    }
                    if (objectIndex > _objectList.Count-1)
                    {
                        continue;
                    }
                    Debug.Log("itemIndex:" + itemIndex + "がobjectIndex: "+ objectIndex);
                    itemPrefab.transform.SetParent(_objectList[objectIndex].transform,false);
                    var listItem = itemPrefab.GetComponent<ListItem>();
                    listItem.SetListData(_listDates[objectIndex],objectIndex);
                    var view = itemPrefab.GetComponent<IListViewItem>();
                    view.UpdateViewItem();
                }
            }
        }

        private void UpdateListUpGrid(int gridIndex)
        {
            var lineIndex = _lastStartIndexY - gridIndex;
            _lastStartIndexY = gridIndex;
            int startIndex = GetStartIndex(_horizontal);
            var horizontalCount = GetHorizonalCount();
            for (int j = 0;j < lineIndex;j++)
            {
                for (int i = 0;i <= horizontalCount;i++)
                {
                    var itemIndex = (gridIndex + j) * (horizontalCount + 1) + i;
                    if (itemIndex < 0)
                    {
                        continue;
                    }
                    if (itemIndex >= _itemPrefabList.Count)
                    {
                        continue;
                    }
                    var itemPrefab = _itemPrefabList[itemIndex];
                    var objectIndex = (gridIndex + j) * _gridColumnCount + i;
                    if (startIndex > i)
                    {
                        objectIndex += horizontalCount + 1;
                    }
                    if (objectIndex < 0)
                    {
                        continue;
                    }
                    Debug.Log("itemIndex:" + itemIndex + "がobjectIndex: "+ objectIndex);
                    itemPrefab.transform.SetParent(_objectList[objectIndex].transform,false);
                    var listItem = itemPrefab.GetComponent<ListItem>();
                    listItem.SetListData(_listDates[objectIndex],objectIndex);
                    var view = itemPrefab.GetComponent<IListViewItem>();
                    view.UpdateViewItem();
                }
            }
        }

        private void UpdateListRightGrid(int startIndex)
        {
            var lineIndex = startIndex - _lastStartIndexX;
            _lastStartIndexX = startIndex;
            int gridIndex = GetStartIndex(!_horizontal);
            var verticalCount = GetVerticalCount();
            var length = GetHorizonalCount();
            for (int j = 0;j < lineIndex;j++)
            {
                for (int i = 0;i < _gridColumnCount;i++)
                {
                    var itemIndex = startIndex - 1 - j + i * (length + 1);
                    if (itemIndex < 0)
                    {
                        continue;
                    }
                    if (itemIndex >= _itemPrefabList.Count)
                    {
                        continue;
                    }
                    var itemPrefab = _itemPrefabList[itemIndex];
                    var objectIndex = startIndex - j + length + i * _gridColumnCount;// + gridIndex * _gridColumnCount;
                    if (gridIndex > i)
                    {
                        objectIndex += _gridColumnCount * (verticalCount + 1);
                    }
                    if (objectIndex > _objectList.Count-1)
                    {
                        continue;
                    }
                    Debug.Log("itemIndex:" + itemIndex + "がobjectIndex: "+ objectIndex);
                    itemPrefab.transform.SetParent(_objectList[objectIndex].transform,false);
                    var listItem = itemPrefab.GetComponent<ListItem>();
                    listItem.SetListData(_listDates[objectIndex],objectIndex);
                    var view = itemPrefab.GetComponent<IListViewItem>();
                    view.UpdateViewItem();
                }
            }
        }

        private void UpdateListLeftGrid(int startIndex)
        {
            var lineIndex = _lastStartIndexX - startIndex;
            _lastStartIndexX = startIndex;
            int gridIndex = GetStartIndex(!_horizontal);
            var verticalCount = GetVerticalCount();
            var length = GetHorizonalCount();
            for (int j = 0;j < lineIndex;j++)
            {
                for (int i = 0;i < _gridColumnCount;i++)
                {
                    var itemIndex = startIndex + j + i * (length + 1);
                    if (itemIndex < 0)
                    {
                        continue;
                    }
                    if (itemIndex >= _itemPrefabList.Count)
                    {
                        continue;
                    }
                    var itemPrefab = _itemPrefabList[itemIndex];
                    var objectIndex = startIndex + j + i * _gridColumnCount;
                    if (gridIndex > i)
                    {
                        objectIndex += _gridColumnCount * (verticalCount + 1);
                    }
                    if (objectIndex <= -1)
                    {
                        continue;
                    }
                    if (objectIndex > _objectList.Count-1)
                    {
                        continue;
                    }
                    Debug.Log("itemIndex:" + itemIndex + "がobjectIndex: "+ objectIndex);
                    itemPrefab.transform.SetParent(_objectList[objectIndex].transform,false);
                    var listItem = itemPrefab.GetComponent<ListItem>();
                    listItem.SetListData(_listDates[objectIndex],objectIndex);
                    var view = itemPrefab.GetComponent<IListViewItem>();
                    view.UpdateViewItem();
                }
            }
        }

        private void UpdateGridDown()
        {
            if (_objectList.Count <= _index + _gridColumnCount)
            {
                return;
            }
            var selectItem = _objectList[_index + _gridColumnCount];
            var itemPosition = GetCornerPosition(selectItem,0,false);
            var col = (_index/_gridColumnCount) +2f;
            if (_index % 2 == 1)
            {
                col += 0.5f;
            }
            if (itemPosition < 0)
            {
                float verticalCount = GetVerticalCount();
                if (col != verticalCount)
                {
                    var c = col-verticalCount;
                    var p = GetGridRowCount() - verticalCount;
                    var per = 1f - (c / p);

                    ScrollRect.verticalNormalizedPosition = Math.Max(per,0);
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
            var itemPosition = GetCornerPosition(selectItem,0,false);
            var col = (_index/_gridColumnCount) -2f;
            if (_index % 2 == 1)
            {
                col += 0.5f;
            }
            if (itemPosition > (720-_itemSize.y))
            {
                float verticalCount = GetVerticalCount();

                var c = GetGridRowCount() - verticalCount - col;
                var p = GetGridRowCount() - verticalCount;
                var per = (c / p);

                ScrollRect.verticalNormalizedPosition = Math.Min(1,per);
            }
        }

        private void UpdateGridRight()
        {
            var row = _index % _gridColumnCount;
            var width = GetViewPortWidth();
            var horizontalCount = GetHorizonalCount();
            
            if (row+2 < horizontalCount)
            {
                return;
            }
            var nextRow = _index + 1;
            if (nextRow%_gridColumnCount < _index%_gridColumnCount)
            {
                return;
            }
            var selectItem = _objectList[nextRow];
            var itemPosition = GetCornerPosition(selectItem,2,true);
            if (itemPosition > width)
            {
                float c = (row+2) - horizontalCount;
                float p = _gridColumnCount - horizontalCount;
                var per = (c / p);

                ScrollRect.horizontalNormalizedPosition = Math.Max(per,0);
            }
        }

        private void UpdateGridLeft()
        {
            var nextRow = _index - 1;
            if (nextRow%_gridColumnCount > _index%_gridColumnCount)
            {
                return;
            }
            if (nextRow < 0)
            {
                return;
            }
            var selectItem = _objectList[nextRow];
            var itemPosition = GetCornerPosition(selectItem,2,true);
            var row = (_index%_gridColumnCount) -2f;
            if (itemPosition < _itemSize.x)
            {
                var horizontalCount = GetHorizonalCount();
            
                float c = _gridColumnCount - horizontalCount - row;
                float p = _gridColumnCount - horizontalCount;
                var per = 1f - (c / p);

                ScrollRect.horizontalNormalizedPosition = Math.Max(0,per);
            }
        }

        private float GetCornerPosition(GameObject gameObject,int index,bool isHorizontal)
        {
            var corners = new Vector3[4];
            gameObject.GetComponent<RectTransform>().GetWorldCorners(corners);
            corners[index] = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[index]);
            if (isHorizontal)
            {
                return corners[index].x;
            }
            return corners[index].y;
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
            } else
            // 左区間
            if (nextX < horizontarmargin)
            {

            } else
            // 中区間
            {
                perX = (nextX - horizontarmargin) / (_gridColumnCount - horizontarmargin*2);
            }
            ScrollRect.horizontalNormalizedPosition = Math.Max(perX,0);
        }

        private void UpdateGridPositionY(int selectIndex)
        {
            float nextY = selectIndex / _gridColumnCount;
            var verticalCount = GetVerticalCount();
            var verticalmargin = (GetGridRowCount() - verticalCount) / 2;
            var perY = 1f;
            // 下区間
            if ((GetGridRowCount() - nextY) <= verticalmargin)
            {
                perY = 0;
            } else
            // 上区間
            if (nextY < verticalmargin)
            {

            } else
            // 中区間
            {
                perY = 1f - ((nextY - verticalmargin) / (GetGridRowCount() - verticalmargin*2));
            }
            ScrollRect.verticalNormalizedPosition = Math.Max(0,perY);
        }
    }
}
