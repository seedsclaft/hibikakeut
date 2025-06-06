using UnityEngine;

namespace Ryneus
{
    [System.Serializable]
    public class ParameterFloat
    {
#if UNITY_EDITOR
        [SerializeField] private float _value = 0;
#else
        private float _value = 0;
#endif
        public float Value => _value;

        public ParameterFloat(float value = 0)
        {
            _value = value;
        }

        public void SetValue(float value)
        {
            _value = value;
        }

        public void GainValue(float value)
        {
            _value += value;
        }

        public void GainValue(float value,float minValue)
        {
            _value += value;
            if (_value < minValue)
            {
                _value = minValue;
            }
        }

        public void GainValue(float value,float minValue,float maxValue)
        {
            _value += value;
            if (_value < minValue)
            {
                _value = minValue;
            }
        }
    }
}
