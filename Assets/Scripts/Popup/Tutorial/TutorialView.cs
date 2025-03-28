using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tutorial;
using TMPro;
using System;

namespace Ryneus
{
    public class TutorialView : BaseView ,IInputHandlerEvent
    {
        private new Action<TutorialViewEvent> _commandData = null;
        [SerializeField] private Button backImage = null;
        [SerializeField] private Button helpButton = null;
        [SerializeField] private Image focusImage = null;
        [SerializeField] private Image focusBgImage = null;
        [SerializeField] private GameObject frameObj = null;
        [SerializeField] private TextMeshProUGUI tutorialText = null;
        [SerializeField] private GameObject focusFrameObj = null;
        [SerializeField] private TextMeshProUGUI focusText = null;
        [SerializeField] private GameObject toggleObj = null;
        [SerializeField] private Toggle checkToggle = null;
        public bool CheckToggle => checkToggle.isOn;


        private Action _backEvent = null;
        public override void Initialize() 
        {
            base.Initialize();
            SetBackCommand(() => OnClickBack());
            new TutorialPresenter(this);
        }

        public void SetTutorialData(TutorialData tutorialData)
        {
            ChangeBackCommandActive(tutorialData.Type == 1 || tutorialData.Param2 == 1);
            // 最初だけ
            toggleObj.SetActive(tutorialData.Id == 1000);
            frameObj.SetActive(tutorialData.Type == 1);
            if (tutorialData.Type == 1)
            {
                var rect = frameObj.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(tutorialData.X,tutorialData.Y,0);
                rect.sizeDelta = new Vector3(tutorialData.Width,tutorialData.Height);
            }
            tutorialText.SetText(tutorialData.Help);
            focusImage.gameObject.SetActive(tutorialData.Type == 2);
            focusFrameObj.SetActive(tutorialData.Type == 2);
            if (tutorialData.Type == 2)
            {
                ShowFocusImage(tutorialData);
            } else
            {
                HideFocusImage();
            }
        }

        public void ShowFocusImage(TutorialData tutorialData)
        {
            //gameObject.SetActive(true);
            if (focusImage == null) return;
            var rect = focusImage.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(tutorialData.X,tutorialData.Y,0);
            rect.sizeDelta = new Vector3(tutorialData.Width,tutorialData.Height);
            
            
            var focusRect = focusFrameObj.GetComponent<RectTransform>();
            focusRect.localPosition = new Vector3(tutorialData.FocusX,tutorialData.FocusY,0);            
            focusRect.sizeDelta = new Vector3(tutorialData.FocusWidth,tutorialData.FocusHeight);
            focusText.SetText(tutorialData.Help);
            var bgRect = focusBgImage.GetComponent<RectTransform>();
            bgRect.localPosition = new Vector3(tutorialData.X * -1,tutorialData.Y * -1,0);
        }

        public void HideFocusImage()
        {
            //gameObject.SetActive(false);
        }
        
        public void SetUIButton()
        {
            //helpButton.onClick.AddListener(() => OnClickHelp());
        }

        public void SetEvent(System.Action<TutorialViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void CommandBack()
        {
            _backEvent?.Invoke();
        }

        public new void SetBusy(bool busy)
        {
            base.SetBusy(busy);
        }

        public void OnClickBack()
        {
            var eventData = new TutorialViewEvent(CommandType.Back);
            _commandData(eventData);
        }

        private void OnClickHelp()
        {
            var eventData = new TutorialViewEvent(CommandType.CallTutorialData);
            _commandData(eventData);
        }

        public void InputHandler(List<InputKeyType> keyType,bool pressed)
        {
        }

        public new void MouseCancelHandler()
        {
            var eventData = new TutorialViewEvent(CommandType.Back);
            _commandData(eventData);
        }
    }


    public class TutorialViewEvent
    {
        public CommandType commandType;
        public object template;

        public TutorialViewEvent(CommandType type)
        {
            commandType = type;
        }
    }

    public class TutorialViewInfo
    {
        public int SceneType;
        public Func<TutorialData,bool> CheckEndMethod;
        public Func<TutorialData, bool> CheckMethod;
        public Action CheckTrueAction;
        public Action EndEvent;
    }
}

namespace Tutorial
{
    public enum CommandType
    {
        None = 0,
        CallTutorialData,
        Back
    }
}