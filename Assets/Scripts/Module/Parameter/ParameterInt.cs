using System;

namespace Ryneus
{
    [Serializable]
    public class ParameterInt
    {
#if UNITY_EDITOR
        [UnityEngine.SerializeField] private int _value = 0;
#else
        private int _value = 0;
#endif
        public int Value => _value;

        public ParameterInt(int value = 0)
        {
            _value = value;
        }

        public void SetValue(int value)
        {
            _value = value;
        }

        public void SetValue(int value,int minValue,int maxValue)
        {
            var result = Math.Min(Math.Max(value,minValue),maxValue);
            SetValue(result);
        }

        public void GainValue(int value)
        {
            _value += value;
        }

        public void GainValue(int value,int minValue)
        {
            _value += value;
            _value = Math.Max(_value,minValue);
        }

        public void GainValue(int value,int minValue,int maxValue)
        {
            _value += value;
            _value = Math.Min(Math.Max(_value,minValue),maxValue);
        }

        public void GainValue(int value,int minValue,int maxValue,bool wrap = false)
        {
            _value += value;
            if (wrap)
            {
                if (_value < minValue)
                {
                    _value = maxValue;
                } else
                if (_value > maxValue)
                {
                    _value = minValue;
                }
            } else
            {
                _value = Math.Min(Math.Max(_value,minValue),maxValue);
            }
        }
    }
}
