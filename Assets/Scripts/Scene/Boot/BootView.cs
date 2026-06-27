using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ryneus.Boot;

namespace Ryneus
{
    public class BootView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private Button logoButton = null;
        [SerializeField] private TextMeshProUGUI titleCaution = null;
        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Boot);
            _ = new BootPresenter(this);
            if (!TestMode)
            {
                logoButton.onClick.AddListener(() => CallLogoClick());
            }
            UIComponent.SetActive(logoButton, !TestMode);
        }

        private void CallLogoClick()
        {
            CallViewEvent(CommandType.LogoClick);
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (keyTypes.Count > 0)
            {
                CallLogoClick();
            }
        }

        public void SetTitleCaution(string text)
        {
            UIComponent.SetText(titleCaution, text);
        }

    }

    namespace Boot
    {
        public enum CommandType
        {
            None = 0,
            LogoClick,
        }
    }
}