namespace Ryneus
{
    [System.Serializable]
    public class TradeItemInfo
    {
        private GetItemInfo _getItemInfo = null;
        public GetItemInfo GetItemInfo => _getItemInfo;
        public ParameterInt Cost = new();
        public ParameterBool Selected = new();
        public TradeItemInfo(GetItemData getItemData, int cost)
        {
            _getItemInfo = new GetItemInfo(getItemData);
            Cost.SetValue(cost);
        }

        public string Destriction()
        {
            return "魔法を入手する";
        }
    }
}
