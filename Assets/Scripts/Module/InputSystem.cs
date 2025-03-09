using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ryneus
{
    public class InputSystem
    {
        public static bool IsGamePad = false;

        public static List<InputData> _inputDates = new();
        public static void Initialize()
        {
            _inputDates.Clear();
            var enums = System.Enum.GetValues(typeof(InputKeyType));
            foreach (var e in enums)
            {
                var keyData = new InputData((InputKeyType)e);
                _inputDates.Add(keyData);
            }
        }

        public static InputData GetInputDate(InputKeyType inputKeyType)
        {
            var data = _inputDates.Find(a => a.InputKeyType == inputKeyType);
            return data;
        }

        public List<InputKeyType> Update()
        {
            if (GameSystem.OptionData.InputType == InputType.MouseOnly)
            {
                return new();
            }
            UpdateGamePadData();
            var gamePadKey = UpdateGamePad();
            if (gamePadKey != null)
            {
                IsGamePad = true;
                return gamePadKey;
            }
            UpdateKeyBoardData();
            return UpdateKeyBoard();
        }

        private void UpdateInputKeyData(InputKeyType inputKeyType,UnityEngine.InputSystem.Controls.KeyControl keyControl,Key key)
        {
            var keyData = GetInputDate(inputKeyType);
            if((keyControl != null && keyControl.wasPressedThisFrame) || Keyboard.current[key].wasPressedThisFrame) 
            {
                keyData.OnDown();
            } else
            if((keyControl != null && keyControl.isPressed) || Keyboard.current[key].isPressed) 
            {
                keyData.OnPress();
            } else
            if((keyControl != null && keyControl.wasReleasedThisFrame) || Keyboard.current[key].wasReleasedThisFrame) 
            {
                keyData.OnLeft();
            } else
            {
                keyData.OnLeftEnd();
            }
        }

        private void UpdateKeyBoardData()
        {
            UpdateInputKeyData(InputKeyType.Up,Keyboard.current.upArrowKey,Key.W);
            UpdateInputKeyData(InputKeyType.Down,Keyboard.current.downArrowKey,Key.S);
            UpdateInputKeyData(InputKeyType.Left,Keyboard.current.leftArrowKey,Key.A);
            UpdateInputKeyData(InputKeyType.Right,Keyboard.current.rightArrowKey,Key.D);
            UpdateInputKeyData(InputKeyType.Decide,null,Key.Space);
            UpdateInputKeyData(InputKeyType.Cancel,null,Key.LeftShift);
            UpdateInputKeyData(InputKeyType.Option1,null,Key.R);
            UpdateInputKeyData(InputKeyType.Option2,null,Key.T);
            UpdateInputKeyData(InputKeyType.SideLeft1,null,Key.Q);
            UpdateInputKeyData(InputKeyType.SideRight1,null,Key.E);
            UpdateInputKeyData(InputKeyType.SideLeft2,null,Key.PageDown);
            UpdateInputKeyData(InputKeyType.SideRight2,null,Key.PageUp);
            UpdateInputKeyData(InputKeyType.Start,null,Key.Enter);
            UpdateInputKeyData(InputKeyType.Select,null,Key.RightShift);
        }

        private void UpdateGamePadData()
        {
            var gamePad = Gamepad.current;
            if (gamePad == null)
            {
                return;
            }
            UpdateInputGamePadData(InputKeyType.Up,gamePad.dpad.up,null);
            UpdateInputGamePadData(InputKeyType.Down,gamePad.dpad.down,null);
            UpdateInputGamePadData(InputKeyType.Left,gamePad.dpad.left,null);
            UpdateInputGamePadData(InputKeyType.Right,gamePad.dpad.right,null);
            UpdateInputGamePadData(InputKeyType.Decide,gamePad.bButton,null);
            UpdateInputGamePadData(InputKeyType.Cancel,gamePad.aButton,null);
            UpdateInputGamePadData(InputKeyType.Option1,gamePad.yButton,null);
            UpdateInputGamePadData(InputKeyType.Option2,gamePad.xButton,null);
            UpdateInputGamePadData(InputKeyType.SideLeft1,gamePad.leftTrigger,null);
            UpdateInputGamePadData(InputKeyType.SideRight1,gamePad.rightTrigger,null);
            UpdateInputGamePadData(InputKeyType.SideLeft2,gamePad.leftShoulder,null);
            UpdateInputGamePadData(InputKeyType.SideRight2,gamePad.rightShoulder,null);
            UpdateInputGamePadData(InputKeyType.Start,gamePad.startButton,null);
            UpdateInputGamePadData(InputKeyType.Select,gamePad.selectButton,null);
            UpdateInputStickGamePadData(InputKeyType.LeftStickUp,gamePad.leftStick);
            UpdateInputStickGamePadData(InputKeyType.LeftStickDown,gamePad.leftStick);
            UpdateInputStickGamePadData(InputKeyType.LeftStickLeft,gamePad.leftStick);
            UpdateInputStickGamePadData(InputKeyType.LeftStickRight,gamePad.leftStick);
            UpdateInputStickGamePadData(InputKeyType.RightStickUp,gamePad.rightStick);
            UpdateInputStickGamePadData(InputKeyType.RightStickDown,gamePad.rightStick);
            UpdateInputStickGamePadData(InputKeyType.RightStickLeft,gamePad.rightStick);
            UpdateInputStickGamePadData(InputKeyType.RightStickRight,gamePad.rightStick);
        }

        private List<InputKeyType> UpdateKeyBoard()
        {
            var keyTypes = new List<InputKeyType>();
            if(Keyboard.current.upArrowKey.isPressed || Keyboard.current[Key.W].isPressed) 
            {
                keyTypes.Add(InputKeyType.Up);
            }
            if(Keyboard.current.downArrowKey.isPressed || Keyboard.current[Key.S].isPressed) 
            {
                keyTypes.Add(InputKeyType.Down);
            }
            if(Keyboard.current.leftArrowKey.isPressed || Keyboard.current[Key.A].isPressed) 
            {
                keyTypes.Add(InputKeyType.Left);
            }
            if(Keyboard.current.rightArrowKey.isPressed || Keyboard.current[Key.D].isPressed) 
            {
                keyTypes.Add(InputKeyType.Right);
            }
            if(Keyboard.current[Key.Space].wasPressedThisFrame) 
            {
                keyTypes.Add(InputKeyType.Decide);
            }
            if(Keyboard.current[Key.LeftShift].wasPressedThisFrame || Keyboard.current[Key.Escape].wasPressedThisFrame) 
            {
                keyTypes.Add(InputKeyType.Cancel);
            }
            if(Keyboard.current[Key.R].wasPressedThisFrame) 
            {
                keyTypes.Add(InputKeyType.Option1);
            }
            if(Keyboard.current[Key.T].wasPressedThisFrame) 
            {
                keyTypes.Add(InputKeyType.Option2);
            }
            if(Keyboard.current[Key.Q].isPressed) 
            {
                keyTypes.Add(InputKeyType.SideLeft1);
            }
            if(Keyboard.current[Key.E].isPressed) 
            {
                keyTypes.Add(InputKeyType.SideRight1);
            }
            if(Keyboard.current[Key.PageDown].isPressed) 
            {
                keyTypes.Add(InputKeyType.SideLeft2);
            }
            if(Keyboard.current[Key.PageUp].isPressed) 
            {
                keyTypes.Add(InputKeyType.SideRight2);
            }
            if(Keyboard.current[Key.Enter].wasPressedThisFrame) 
            {
                keyTypes.Add(InputKeyType.Start);
            }
            if(Keyboard.current[Key.RightShift].wasPressedThisFrame) 
            {
                keyTypes.Add(InputKeyType.Select);
            }
            if (keyTypes.Count > 0)
            {
                IsGamePad = false;
            }
            return keyTypes;
        }

        private void UpdateInputGamePadData(InputKeyType inputKeyType,UnityEngine.InputSystem.Controls.ButtonControl keyControl,UnityEngine.InputSystem.Controls.ButtonControl stick)
        {
            var keyData = GetInputDate(inputKeyType);
            if((keyControl != null && keyControl.wasPressedThisFrame) || (stick != null && stick.wasPressedThisFrame)) 
            {
                keyData.OnDown();
            } else
            if((keyControl != null && keyControl.isPressed) || (stick != null && stick.isPressed)) 
            {
                keyData.OnPress();
            } else
            if((keyControl != null && keyControl.wasReleasedThisFrame) || (stick != null && stick.wasReleasedThisFrame)) 
            {
                keyData.OnLeft();
            } else
            {
                keyData.OnLeftEnd();
            }
        }
        
        private void UpdateInputStickGamePadData(InputKeyType inputKeyType,UnityEngine.InputSystem.Controls.StickControl stickControl)
        {
            var keyData = GetInputDate(inputKeyType);
            switch (inputKeyType)
            {
                case InputKeyType.LeftStickUp:
                case InputKeyType.RightStickUp:
                    if (stickControl.y.value > 0)
                    {
                        keyData.SetValue(stickControl.y.value);
                    } else
                    {
                        keyData.SetValue(0);
                    }
                    break;
                case InputKeyType.LeftStickDown:
                case InputKeyType.RightStickDown:
                    if (stickControl.y.value < 0)
                    {
                        keyData.SetValue(stickControl.y.value * -1);
                    } else
                    {
                        keyData.SetValue(0);
                    }
                    break;
                case InputKeyType.LeftStickLeft:
                case InputKeyType.RightStickLeft:
                    if (stickControl.x.value < 0)
                    {
                        keyData.SetValue(stickControl.x.value * -1);
                    } else
                    {
                        keyData.SetValue(0);
                    }
                    break;
                case InputKeyType.LeftStickRight:
                case InputKeyType.RightStickRight:
                    if (stickControl.x.value > 0)
                    {
                        keyData.SetValue(stickControl.x.value);
                    } else
                    {
                        keyData.SetValue(0);
                    }
                    break;
            }
        }

        private List<InputKeyType> UpdateGamePad()
        {
            var gamePad = Gamepad.current;
            if (gamePad == null)
            {
                return null;
            }
            var keyTypes = new List<InputKeyType>();
            // 十字
            if (gamePad.dpad.up.isPressed) 
            {
                keyTypes.Add(InputKeyType.Up);
            }
            if (gamePad.dpad.down.isPressed) 
            {
                keyTypes.Add(InputKeyType.Down);
            }
            if (gamePad.dpad.right.isPressed) 
            {
                keyTypes.Add(InputKeyType.Right);
            }
            if (gamePad.dpad.left.isPressed) 
            {
                keyTypes.Add(InputKeyType.Left);
            }

            if (gamePad.aButton.wasPressedThisFrame || gamePad.crossButton.wasPressedThisFrame) 
            {
                keyTypes.Add(InputKeyType.Decide);
            }
            if (gamePad.bButton.wasPressedThisFrame || gamePad.buttonEast.wasPressedThisFrame || gamePad.circleButton.wasPressedThisFrame)
            {
                keyTypes.Add(InputKeyType.Cancel);
            }
            if (gamePad.xButton.wasPressedThisFrame || gamePad.buttonWest.wasPressedThisFrame || gamePad.squareButton.wasPressedThisFrame) 
            {
                keyTypes.Add(InputKeyType.Option1);
            }
            if (gamePad.yButton.wasPressedThisFrame || gamePad.buttonNorth.wasPressedThisFrame || gamePad.triangleButton.wasPressedThisFrame)
            {
                keyTypes.Add(InputKeyType.Option2);
            }

            // start,select
            if (gamePad.startButton.wasPressedThisFrame) 
            {
                keyTypes.Add(InputKeyType.Start);
            }
            if (gamePad.selectButton.wasPressedThisFrame) 
            {
                keyTypes.Add(InputKeyType.Select);
            }


            // L1,R1
            if (gamePad.leftShoulder.wasPressedThisFrame) 
            {
                keyTypes.Add(InputKeyType.SideLeft1);
            }
            if (gamePad.rightShoulder.wasPressedThisFrame)
            {
                keyTypes.Add(InputKeyType.SideRight1);
            }

            // L2,R2
            if (gamePad.leftShoulder.isPressed)
            {
                keyTypes.Add(InputKeyType.SideLeft2);
            }
            if (gamePad.rightShoulder.isPressed)
            {
                keyTypes.Add(InputKeyType.SideRight2);
            }
            if (gamePad.leftStick.value.y > 0)
            {
                keyTypes.Add(InputKeyType.LeftStickUp);
            }
            if (gamePad.leftStick.value.y < 0)
            {
                keyTypes.Add(InputKeyType.LeftStickDown);
            }

            if (gamePad.leftStick.value.x < 0)
            {
                keyTypes.Add(InputKeyType.LeftStickLeft);
            }
            if (gamePad.leftStick.value.x > 0)
            {
                keyTypes.Add(InputKeyType.LeftStickRight);
            }
            return keyTypes;
        }

        public static bool IsMouseRightButtonDown()
        {
            if (IsPlatformStandAloneOrEditor() || EnableWebGLInput())
            { 
                return Input.GetMouseButtonDown(1);
            }
            return false;
        }
        
        public static Vector3 MouseMovePosition()
        {
            if (IsPlatformStandAloneOrEditor() || EnableWebGLInput())
            { 
                return Input.mousePosition;
            }
            return new Vector3(0,0,0);
        }

        public static Vector2 MouseWheelPosition()
        {
            if (IsPlatformStandAloneOrEditor() || EnableWebGLInput())
            { 
                return Input.mouseScrollDelta;
            }
            return new Vector2(0,0);
        }

        public static bool EnableWebGLInput()
        {
            return Application.platform == RuntimePlatform.WebGLPlayer;
        }

        public static bool IsPlatformStandAloneOrEditor()
        {
            return Application.isEditor || IsPlatformStandAlone();
        }

        public static bool IsPlatformStandAlone()
        {
            return Application.platform switch
            {
                RuntimePlatform.WindowsPlayer or RuntimePlatform.OSXPlayer or RuntimePlatform.LinuxPlayer => true,
                _ => false,
            };
        }
    }

    public enum InputKeyType
    {
        None = -1,
        Up = 1,
        Down = 0,
        Left = 3,
        Right = 2,
        Decide = 4,
        Cancel = 5,
        Option1 = 6, // □,A
        Option2 = 7, // △,S
        SideLeft1 = 9, // L1
        SideRight1 = 8, // R1
        SideLeft2 = 11, // L2
        SideRight2 = 10, // R2
        Start = 12,
        LeftStickUp,
        LeftStickDown,
        LeftStickLeft,
        LeftStickRight,
        RightStickUp,
        RightStickDown,
        RightStickLeft,
        RightStickRight,
        Select,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
    }
}