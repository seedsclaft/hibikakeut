using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Guide;

namespace Ryneus
{
    public class GuideView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private BaseList helpTextList = null;
        [SerializeField] private Image guideImage = null;
        [SerializeField] private Button leftButton = null;
        [SerializeField] private Button rightButton = null;
        [SerializeField] private Button helpButton = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        
        public override void Initialize() 
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Guide);
            SetBaseAnimation(popupAnimation);
            InitilizeHelpTextList();
            leftButton.onClick.AddListener(() => OnClickLeft());
            rightButton.onClick.AddListener(() => OnClickRight());
            helpButton.onClick.AddListener(() => OnClickHelp());
            _ = new GuidePresenter(this);
        }
        
        public void OpenAnimation(Action initializeAfter)
        {
            popupAnimation.OpenAnimation(UiRoot.transform, initializeAfter);
        }

        private void InitilizeHelpTextList()
        {
            helpTextList.Initialize();
            //helpTextList.SetInputHandler(InputKeyType.Cancel, () => BackEvent?.Invoke());
            //helpTextList.SetInputHandler(InputKeyType.Right, () => OnClickRight());
            //helpTextList.SetInputHandler(InputKeyType.Left, () => OnClickLeft());
            //AddViewActives(helpTextList);
        }

        private void OnClickLeft()
        {
            if (!leftButton.gameObject.activeSelf)
            {
                return;
            }
            CallViewEvent(CommandType.PageLeft);
        }

        private void OnClickRight()
        {
            if (!rightButton.gameObject.activeSelf)
            {
                return;
            }
            CallViewEvent(CommandType.PageRight);
        }

        private void OnClickPageScroll(bool up)
        {
            var y = helpTextList.ScrollRect.verticalNormalizedPosition;
            y += up ? 0.04f : -0.04f;
            y = Math.Max(Math.Min(y, 1), 0);
            helpTextList.ScrollRect.verticalNormalizedPosition = y;
        }

        private void OnClickHelp()
        {
            CallViewEvent(CommandType.CallHelp);
        }

        public void SetLeftRight(bool left,bool right)
        {
            UIComponent.SetActive(leftButton, left);
            UIComponent.SetActive(rightButton, right);
        }

        public void SetGuideImage(Sprite guideSprite)
        {
            guideImage.sprite = guideSprite;
        }

        public void SetHelpText(List<ListData> helpTexts)
        {
            helpTextList.SetData(helpTexts);
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (InputSystem.GetInputDate(InputKeyType.SideLeft2).IsDownTrigger() || InputSystem.GetInputDate(InputKeyType.SideLeft2).IsPress())
            {
                OnClickPageScroll(false);
            } else
            if (InputSystem.GetInputDate(InputKeyType.SideRight2).IsDownTrigger() || InputSystem.GetInputDate(InputKeyType.SideRight2).IsPress())
            {
                OnClickPageScroll(true);
            } else
            if (InputSystem.GetInputDate(InputKeyType.Down).IsDownTrigger() || InputSystem.GetInputDate(InputKeyType.Down).IsPress())
            {
                OnClickPageScroll(false);
            } else
            if (InputSystem.GetInputDate(InputKeyType.Up).IsDownTrigger() || InputSystem.GetInputDate(InputKeyType.Up).IsPress())
            {
                OnClickPageScroll(true);
            } else
            if (InputSystem.GetInputDate(InputKeyType.Right).IsDownTrigger())
            {
                OnClickRight();
            } else
            if (InputSystem.GetInputDate(InputKeyType.Left).IsDownTrigger())
            {
                OnClickLeft();
            } else
            if (InputSystem.GetInputDate(InputKeyType.Cancel).IsDownTrigger())
            {
                BackEvent?.Invoke();
            }
        }
    }
}

namespace Guide
{
    public enum CommandType
    {
        Initialize = 0,
        PageLeft,
        PageRight,
        CallHelp,
    }
}