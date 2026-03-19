using System;

namespace Ryneus
{
    [System.Serializable]
    public class ParameterAction
    {
        [UnityEngine.SerializeField] private Action _value = null;
        public Action Value => _value;

        public void SetValue(Action value)
        {
            _value = value;
        }

        public void Invoke()
        {
            _value?.Invoke();
        }
    }
}
