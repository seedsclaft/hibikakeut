using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    using Boot;
    public class BootView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private Button logoButton = null;
        public override void Initialize() 
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Boot);
            new BootPresenter(this);
            if (TestMode == false)
            {
                logoButton.onClick.AddListener(() => CallLogoClick());
            }
            logoButton.gameObject.SetActive(TestMode == false);
        }


        private void CallLogoClick()
        {
            CallViewEvent(CommandType.LogoClick);
        }

        public void InputHandler(InputKeyType keyType, bool pressed)
        {
            if (keyType != InputKeyType.None)
            {
                CallLogoClick();
            }
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