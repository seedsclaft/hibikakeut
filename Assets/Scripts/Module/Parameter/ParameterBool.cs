namespace Ryneus
{
    [System.Serializable]
    public class ParameterBool
    {
        [UnityEngine.SerializeField] private bool _value = false;
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
