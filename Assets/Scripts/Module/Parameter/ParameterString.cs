namespace Ryneus
{
    [System.Serializable]
    public class ParameterString
    {
#if UNITY_EDITOR
        [UnityEngine.SerializeField] private string _value = "";
#else
        private string _value = "";
#endif
        public string Value => _value;

        public void SetValue(string value)
        {
            _value = value;
        }

        public void GainValue(string value)
        {
            _value += value;
        }
    }
}
