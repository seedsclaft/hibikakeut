using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class TradeModel : BaseModel
    {
        private List<TradeItemInfo> _makeGetItems = new();
        private List<TradeItemInfo> _getItems = new();
        public List<TradeItemInfo> GetItems => _getItems;
        public ParameterInt PayCost = new();
        public TradeModel()
        {
        }

        public List<TradeItemInfo> TradeGetItemInfos()
        {
            if (_makeGetItems.Count > 0)
            {
                return _makeGetItems;
            }
            var list = new List<TradeItemInfo>();
            var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == 50010);
            foreach (var prizeSet in prizeSets)
            {
                var tardeItemInfo = new TradeItemInfo(prizeSet.GetItem, prizeSet.GetItem.Param2);
                list.Add(tardeItemInfo);
            }
            _makeGetItems = list;
            return list;
        }

        public bool CanPayCost(TradeItemInfo tradeItemInfo)
        {
            return (Currency - PayCost.Value) >= tradeItemInfo.Cost.Value;
        }

        public void AddTradeItem(TradeItemInfo getItemInfo)
        {
            if (!_getItems.Contains(getItemInfo))
            {
                getItemInfo.Selected.SetValue(true);
                PayCost.GainValue(getItemInfo.Cost.Value);
                _getItems.Add(getItemInfo);
            }
        }

        public void RemoveTradeItem(TradeItemInfo getItemInfo)
        {
            if (_getItems.Contains(getItemInfo))
            {
                getItemInfo.Selected.SetValue(false);
                PayCost.GainValue(getItemInfo.Cost.Value * -1);
                _getItems.Remove(getItemInfo);
            }
        }

        public List<GetItemInfo> GetTradeGetItemInfos()
        {
            var list = new List<GetItemInfo>();
            foreach (var getItem in _getItems)
            {
                list.Add(getItem.GetItemInfo);
            }
            return list;
        }

        public int AfterCurrency()
        {
            return Currency - PayCost.Value;
        }
    }
}