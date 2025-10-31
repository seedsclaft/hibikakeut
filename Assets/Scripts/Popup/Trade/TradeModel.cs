using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class TradeModel : BaseModel
    {
        private Dictionary<TradeItemInfo, int> _getTradeItems = new();
        public Dictionary<TradeItemInfo, int> GetTradeItems => _getTradeItems;
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
            var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == 50000 + (PartyInfo.Chapter.Value * 10));
            foreach (var prizeSet in prizeSets)
            {
                TradeItemInfo tradeItemInfo;
                switch (prizeSet.GetItem.Type)
                {
                    case GetItemType.Item:
                        // アイテム1つ
                        GetItemData getItemData = new()
                        {
                            Type = prizeSet.GetItem.Type,
                            Param1 = prizeSet.GetItem.Param1,
                            Param2 = 1 // 1つ単位で取引
                        };
                        tradeItemInfo = new TradeItemInfo(getItemData, prizeSet.GetItem.Param2);
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
            /*
            if (!_getItems.Contains(getItemInfo))
            {
                getItemInfo.Selected.SetValue(true);
                var cost = (int)(getItemInfo.Cost.Value * PartyInfo.TradeDownRate());
                PayCost.GainValue((int)cost);
                _getItems.Add(getItemInfo);
            }
            */
        }

        public void RemoveTradeItem(TradeItemInfo tradeItemInfo)
        {
            if (_getTradeItems.ContainsKey(tradeItemInfo))
            {
                var cost = (int)(tradeItemInfo.Cost.Value * PartyInfo.TradeDownRate());
                PayCost.GainValue((int)cost * -1 * _getTradeItems[tradeItemInfo]);
                _getTradeItems[tradeItemInfo] = 0;
                tradeItemInfo.GetCount.SetValue(0);
            }
            /*
            if (_getItems.Contains(getItemInfo))
            {
                getItemInfo.Selected.SetValue(false);
                var cost = (int)(getItemInfo.Cost.Value * PartyInfo.TradeDownRate());
                PayCost.GainValue((int)cost * -1);
                _getItems.Remove(getItemInfo);
            }
            */
        }

        public void ChangeTradeItemNum(TradeItemInfo tradeItemInfo, bool plus)
        {
            var num = 0;
            if (_getTradeItems.ContainsKey(tradeItemInfo))
            {
                num = _getTradeItems[tradeItemInfo];
            } else
            {
                _getTradeItems[tradeItemInfo] = 0;
            }
            num += plus ? 1 : -1;
            if (num >= 0)
            {
                var cost = (int)(tradeItemInfo.Cost.Value * PartyInfo.TradeDownRate());
                cost *= plus ? 1 : -1;
                PayCost.GainValue((int)cost);
                _getTradeItems[tradeItemInfo] = num;
                tradeItemInfo.GetCount.SetValue(num);
            }
        }

        public List<GetItemInfo> GetTradeGetItemInfos()
        {
            var list = new List<GetItemInfo>();
            foreach (var getTradeItems in _getTradeItems)
            {
                for (int i = 0; i < getTradeItems.Value;i++)
                {
                    list.Add(getTradeItems.Key.GetItemInfo);
                }
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
            PartyInfo.TradeItemInfos.ForEach(a => a.GetCount.SetValue(0));
            //PartyInfo.RemoveTradeItemInfos(_getTradeItems);
        }

        public List<SkillInfo> GetRandumAddSkillInfos(ItemInfo itemInfo)
        {
            var list = new List<SkillInfo>();
            var rank = itemInfo.Master.Param1;
            var attribute = itemInfo.Master.Param2;
            var skillDates = DataSystem.Skills.Where(a => SkillData.ConvertRankCost(a.Value.Rank) == itemInfo.Master.Param1 && a.Value.Rank != RankType.PassiveEnhanceRank1 && a.Value.IsRandumAddSkill() && !PartyInfo.LearningSkillIds.Contains(a.Key)).ToList();
            if (attribute > 0)
            {
                skillDates = skillDates.FindAll(a => (int)a.Value.Attribute == attribute);
            }

            foreach (var skillData in skillDates)
            {
                list.Add(new SkillInfo(skillData.Key));
            }
            return list;
        }
    }
}