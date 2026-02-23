using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.Tutorial;
using TMPro;
using System;

namespace Ryneus
{
    public class TutorialView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private Button backImage = null;
        [SerializeField] private Button helpButton = null;
        [SerializeField] private Image focusImage = null;
        [SerializeField] private Image focusBgImage = null;
        [SerializeField] private GameObject frameObj = null;
        [SerializeField] private TextMeshProUGUI tutorialText = null;
        [SerializeField] private TextMeshProUGUI focusText = null;
        [SerializeField] private TextMeshProUGUI focusText2 = null;
        [SerializeField] private GameObject toggleObj = null;
        [SerializeField] private Toggle checkToggle = null;
        [SerializeField] private OnOffButton toggleButton = null;
        public bool CheckToggle => checkToggle.isOn;

        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Tutorial);
            toggleButton.OnClickAddListener(() =>
            {
                ChangeToggle();
            });
            //SetBackCommand(() => OnClickBack());
            _ = new TutorialPresenter(this);
        }

        public void SetTutorialData(TutorialData tutorialData)
        {
            var frameType = tutorialData.Type;
            var frameTypeWindow = frameType == FrameType.Window;
            var frameTypeFocus = frameType == FrameType.Focus;
            ChangeBackCommandActive(frameTypeWindow || tutorialData.Param2 == 1);
            // 最初だけ
            toggleObj.SetActive(tutorialData.Id == 1000);
            frameObj.SetActive(frameTypeWindow);
            if (frameTypeWindow)
            {
                var rect = frameObj.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(tutorialData.X, tutorialData.Y, 0);
                rect.sizeDelta = new Vector3(tutorialData.Width, tutorialData.Height);
            }
            tutorialText.SetText(tutorialData.Help);
            focusImage.gameObject.SetActive(frameTypeFocus);
            focusText.gameObject.SetActive(frameTypeFocus);
            if (frameTypeFocus)
            {
                ShowFocusImage(tutorialData);
            }
        }

        public void ShowFocusImage(TutorialData tutorialData)
        {
            //gameObject.SetActive(true);
            if (focusImage == null)
            {
                return;
            }
            var rect = focusImage.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(tutorialData.X, tutorialData.Y, 0);
            rect.sizeDelta = new Vector3(tutorialData.Width, tutorialData.Height);

            var focusRect = focusText.GetComponent<RectTransform>();
            focusRect.localPosition = new Vector3(tutorialData.FocusX, tutorialData.FocusY, 0);
            focusText.SetText(tutorialData.Help);
            focusText2.SetText(tutorialData.Help);
            var bgRect = focusBgImage.GetComponent<RectTransform>();
            bgRect.localPosition = new Vector3(tutorialData.X * -1, tutorialData.Y * -1, 0);
        }

        public void CommandBack()
        {
            BackEvent?.Invoke();
        }

        public void OnClickBack()
        {
            CallViewEvent(CommandType.Back);
        }

        private void ChangeToggle()
        {
            if (toggleObj.activeSelf)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                checkToggle.isOn = !checkToggle.isOn;
            }
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (InputSystem.GetInputDate(InputKeyType.Decide).IsDownTrigger())
            {
                CommandBack();
            }
            if (keyTypes.Contains(InputKeyType.Option1))
            {
                ChangeToggle();
            }
        }

        public new void MouseCancelHandler()
        {
            CallViewEvent(CommandType.Back);
        }
    }

    namespace Tutorial
    {
        public enum CommandType
        {
            Initialize = 0,
            CallTutorialData,
            Back
        }
    }

    public class TutorialViewInfo
    {
        public int SceneType;
        public Func<TutorialData, bool> CheckEndMethod;
        public Func<TutorialData, bool> CheckMethod;
        public Action<TutorialData> CheckTrueAction;
        public Action EndEvent;
    }
}