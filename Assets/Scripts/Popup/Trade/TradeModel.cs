using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class TradeModel : BaseModel
    {
        private List<TradeItemInfo> _getItems = new();
        public List<TradeItemInfo> GetItems => _getItems;
        public ParameterInt PayCost = new();
        public TradeModel()
        {
        }

        public List<TradeItemInfo> TradeGetItemInfos()
        {
            if (PartyInfo.TradeItemInfos.Count > 0)
            {
                return PartyInfo.TradeItemInfos;
            }
            var list = new List<TradeItemInfo>();
            var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == 50010 + (PartyInfo.Chapter.Value * 10));
            foreach (var prizeSet in prizeSets)
            {
                TradeItemInfo tradeItemInfo;
                switch (prizeSet.GetItem.Type)
                {
                    case GetItemType.Item:
                        // アイテム1つ
                        tradeItemInfo = new TradeItemInfo(prizeSet.GetItem, prizeSet.GetItem.Param2);
                        prizeSet.GetItem.Param2 = 1;
                        break;
                    case GetItemType.RandumItem:
                        // 使用アイテムを1つ抽選する
                        var randumItem = new ItemData
                        {
                            ItemType = ItemType.RandumAddItem,
                            Param1 = prizeSet.GetItem.Param1,
                            Param2 = -1
                        };
                        tradeItemInfo = new TradeItemInfo(MakeItemGetItemInfo(randumItem).Master, prizeSet.GetItem.Param2);
                        break;
                    case GetItemType.RandumMagic:
                        // 魔法を1つ抽選する
                        var randumMagic = new ItemData
                        {
                            ItemType = ItemType.RandumAddSkill,
                            Param1 = prizeSet.GetItem.Param1,
                            Param2 = -1
                        };
                        tradeItemInfo = new TradeItemInfo(MakeItemGetItemInfo(randumMagic).Master, prizeSet.GetItem.Param2);
                        break;
                    default:
                        tradeItemInfo = new TradeItemInfo(prizeSet.GetItem, prizeSet.GetItem.Param2);
                        break;
                }
                list.Add(tradeItemInfo);
            }
            PartyInfo.SetTardeItemInfos(list);
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

        public void PayCostTrade()
        {
            PartyInfo.Currency.GainValue(-1 * PayCost.Value);
            PartyInfo.RemoveTradeItemInfos(_getItems);
        }
    }
}