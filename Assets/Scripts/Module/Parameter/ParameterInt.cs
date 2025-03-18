using System;

namespace Ryneus
{
    [System.Serializable]
    public class ParameterInt
    {
        [UnityEngine.SerializeField] private int _value = 0;
        public int Value => _value;

        public ParameterInt(int value = 0)
        {
            _value = value;
        }

        public void SetValue(int value) 
        {
            _value = value;
        }

        public void GainValue(int value)
        {
            _value += value;
        }

        public void GainValue(int value,int minValue)
        {
            _value += value;
            if (_value < minValue)
            {
                _value = minValue;
            }
        }

        public void GainValue(int value,int minValue,int maxValue)
        {
            _value += value;
            _value = Math.Min(Math.Max(_value,minValue),maxValue);
        }
    }
}
