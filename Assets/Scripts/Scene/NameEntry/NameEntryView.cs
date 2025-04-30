using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.NameEntry;
using TMPro;

namespace Ryneus
{
    public class NameEntryView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private TMP_InputField inputField = null;
        [SerializeField] private Button decideButton = null;


        private int _inputLateUpdate = -1;
        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.NameEntry);
            new NameEntryPresenter(this);
            decideButton.onClick.AddListener(() => OnClickDecide());
            inputField.gameObject.SetActive(false);
            decideButton.gameObject.SetActive(false);
            SetInputHandler(gameObject);
        }

        public void SetHelpWindow()
        {
            HelpWindow.SetHelpText("");
            HelpWindow.SetInputInfo("");
        }

        private void OnClickDecide()
        {
            CallViewEvent(CommandType.EntryEnd);
            if (inputField != null)
            {
                inputField.gameObject.SetActive(false);
            }
            if (decideButton != null)
            {
                decideButton.gameObject.SetActive(false);
            }
        }

        public void ShowNameEntry(string defaultName)
        {
            inputField.text = defaultName;
        }

        public void StartNameEntry()
        {
            decideButton.gameObject.SetActive(true);
            inputField.gameObject.SetActive(true);
            inputField.Select();
            _inputLateUpdate = 1;
            HelpWindow.SetHelpText(DataSystem.GetText(5000));
            HelpWindow.SetInputInfo("NAMEENTRY");
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (inputField.gameObject.activeSelf && inputField.IsActive())
            {
                if (keyTypes.Contains(InputKeyType.Start))
                {
                    OnClickDecide();
                }
            }
        }

        private new void Update()
        {
            if (_inputLateUpdate > -1)
            {
                _inputLateUpdate--;
                if (_inputLateUpdate == -1)
                {
                    inputField.MoveTextEnd(true);
                }
            }
            else
            {
                base.Update();
            }
        }
    }

    namespace NameEntry
    {
        public enum CommandType
        {
            None = 0,
            StartEntry = 100,
            EntryEnd = 101,
        }
    }
}