using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Ryneus
{
    public class InputSystemModel 
    {
        private List<IInputHandlerEvent> _inputHandler = new ();
        private int _inputBusyFrame = 0;
        private List<InputKeyType> _lastInputKeys = new();
        private int _pressedFrame = 0;
        readonly int _pressFrame = 30;
        private bool _busy = false;

        public void AddInputHandler(IInputHandlerEvent handler)
        {
            _inputHandler.Add(handler);
        }

        public void SetInputFrame(int frame)
        {
            _inputBusyFrame = frame;
        }

        private void InputHandler(List<InputKeyType> keyTypes,bool pressed)
        {
            if (_inputBusyFrame >= 0) return;
            foreach (var handler in _inputHandler)
            {
                //LogOutput.Log(keyType);
                handler?.InputHandler(keyTypes,pressed);
            }
        }

        public void CallMouseCancel()
        {
            if (_busy) return;
            foreach (var handler in _inputHandler)
            {
                handler?.MouseCancelHandler();
            }
        }

        public void CallMouseMove(UnityEngine.Vector3 position)
        {
            if (_busy) return;
            foreach (var handler in _inputHandler)
            {
                handler?.MouseMoveHandler(position);
            }
        }

        public void CallMouseWheel(UnityEngine.Vector2 position)
        {
            if (_busy) return;
            foreach (var handler in _inputHandler)
            {
                handler?.MouseWheelHandler(position);
            }
        }

        public void UpdateInputKeyType(List<InputKeyType> keyTypes)
        {
            var sequenceEqual = _lastInputKeys.SequenceEqual(keyTypes);
            if (sequenceEqual)
            {
                _pressedFrame += 1;
            } else
            {
                _lastInputKeys = keyTypes;
                _pressedFrame = 0;
            }
            InputHandler(keyTypes,_pressedFrame > _pressFrame);
            if (InputSystem.IsMouseRightButtonDown())
            {
                CallMouseCancel();
            }
            CallMouseMove(InputSystem.MouseMovePosition());
            CallMouseWheel(InputSystem.MouseWheelPosition());
            if (_inputBusyFrame >= 0)
            {
                _inputBusyFrame--;
            }
        }
    }
}
