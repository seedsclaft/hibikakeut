﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ryneus
{
    using Boot;
    public class BootPresenter : BasePresenter
    {
        private BootView _view = null;
        private BootModel _model = null;
        private bool _busy = true;
        public BootPresenter(BootView view)
        {
            _view = view;
            _model = new BootModel();

            Initialize();
        }

        private void Initialize()
        {
            DataSystem.LoadData();
            SoundManager.Instance.Initialize();
            Debug.Log("Boot Success");
            Application.targetFrameRate = 60;
    #if UNITY_ANDROID && !UNITY_EDITOR
            var width = Screen.width;
            var height = Screen.height;
            var rate = 1280f / (float)width;
            Screen.SetResolution((int)(width * rate), (int)(height * rate), true);
    #endif
            Input.multiTouchEnabled = false;
            var gamePad = Gamepad.current;
            if (gamePad != null)
            {
                InputSystem.IsGamePad = true;
            }
            if (SaveSystem.ExistsOptionFile())
            {
                SaveSystem.LoadOptionStart();
            } else
            {
                _model.InitOptionInfo();
            }
            if (_view.TestMode)
            {
                _model.InitSaveInfo();
                _view.CommandGotoSceneChange(Scene.Battle);
            } else
            {
                _view.SetEvent((type) => UpdateCommand(type));
            }
            _busy = false;
            //SaveSystem.SaveStart();
        }
        
        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Boot)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.LogoClick:
                    CommandLogoClick();
                    break;
            }
        }

        private void CommandLogoClick()
        {
            _view.CommandGotoSceneChange(Scene.Title);
        }
    }
}