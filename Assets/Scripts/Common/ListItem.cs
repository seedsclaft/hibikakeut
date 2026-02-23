using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    abstract public class ListItem : MonoBehaviour
    {
        public Button clickButton;
        private int _index = -1;
        public int Index => _index;
        public void SetIndex(int index)
        {
            _index = index;
        }
        private ListData _listData;
        public ListData ListData => _listData;
        public void SetListData(ListData listData, int index)
        {
            _listData = listData;
            _index = index;
        }
        [SerializeField] private GameObject cursor;
        public GameObject Cursor => cursor;
        [SerializeField] private GameObject selectArrow;
        [SerializeField] private bool changeCursorColor = true;
        [SerializeField] private CursorRectAnimation cursorRectAnimation = null;
        [SerializeField] private GameObject disable = null;
        public GameObject Disable => disable;
        [SerializeField] private GameObject batch;
        public GameObject Batch => batch;

        [SerializeField] private Color selectColor;
        [SerializeField] private Color unSelectColor;
        [SerializeField] private List<TextMeshProUGUI> textUguiList = new();
        [SerializeField] private List<Image> imageUguiList = new();

        private bool _addListenHandler = false;
        public void SetAddListenHandler(bool add)
        {
            _addListenHandler = add;
        }

        public void Awake()
        {
            if (changeCursorColor)
            {
                SetCursorColor();
            }
            if (cursorRectAnimation != null)
            {
                cursorRectAnimation.SelectAnimation();
            }
        }

        public T ListItemData<T>()
        {
            return (T)ListData.Data;
        }

        public void SetSelect()
        {
            if (cursor == null)
            {
                return;
            }
            if (!clickButton.enabled)
            {
                return;
            }
            //if (disable != null && disable.activeSelf) return;

            cursor.SetActive(true);
            SetSelectArrow(true);
            foreach (var text in textUguiList)
            {
                text.color = selectColor;
            }
            foreach (var image in imageUguiList)
            {
                image.color = selectColor;
            }
        }

        public void SetUnSelect()
        {
            if (cursor == null)
            {
                return;
            }
            cursor.SetActive(false);
            SetSelectArrow(false);
            foreach (var text in textUguiList)
            {
                text.color = unSelectColor;
            }
            foreach (var image in imageUguiList)
            {
                image.color = unSelectColor;
            }
        }

        public void SetSelectHandler(Action<int> handler, Action exitAction = null)
        {
            if (clickButton == null || _addListenHandler)
            {
                return;
            }
            var enterListener = clickButton.gameObject.AddComponent<ContentEnterListener>();
            enterListener.SetEnterEvent(() =>
            {
                if (_index != -1 && clickButton.enabled)
                {
                    handler(_index);
                }
            });
            enterListener.SetExitEvent(() =>
            {
                exitAction?.Invoke();
            });
        }

        public void SetCallHandler(Action handler)
        {
            if (clickButton == null || _addListenHandler)
            {
                return;
            }
            clickButton.onClick.AddListener(() => handler());
        }

        public void SetCursorColor()
        {
            if (cursor == null)
            {
                return;
            }
            var images = cursor.GetComponentsInChildren<Image>();
            var cursorColor = GameSystem.TempData.ColorSettings.CursorColor;
            foreach (var image in images)
            {
                if (image.sprite == null)
                {
                    var alpha = image.color.a;
                    image.color = new Color(cursorColor.r, cursorColor.g, cursorColor.b, alpha);
                }
            }
        }

        public void SetSelectArrow(bool isSelect)
        {
            if (selectArrow == null)
            {
                return;
            }
            selectArrow.SetActive(isSelect);
        }



        void OnDestroy()
        {
            if (cursorRectAnimation != null)
            {
                cursorRectAnimation.StopAnimation();
            }
        }
    }
}
