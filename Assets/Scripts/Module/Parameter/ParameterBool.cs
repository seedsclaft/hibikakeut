namespace Ryneus
{
    [System.Serializable]
    public class ParameterBool
    {
#if UNITY_EDITOR
        [UnityEngine.SerializeField] private bool _value = false;
#else
        private bool _value = false;
#endif
        public bool Value => _value;

        public ParameterBool(bool value = false)
        {
            _value = value;
        }

        public void SetValue(bool value)
        {
            _value = value;
        }

        public void FlipValue()
        {
            _value = !_value;
        }
    }
}
