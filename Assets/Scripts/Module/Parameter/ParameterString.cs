namespace Ryneus
{
    [System.Serializable]
    public class ParameterString
    {
        [UnityEngine.SerializeField] private string _value = "";
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
