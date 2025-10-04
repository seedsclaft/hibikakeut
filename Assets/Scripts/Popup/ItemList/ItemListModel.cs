using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class ItemListModel : BaseModel
    {
        private Dictionary<int, ParameterInt> _useCount = new();
        public ItemListModel()
        {
        }

        public List<ItemInfo> ItemInfos()
        {
            var list = new List<ItemInfo>();
            foreach (var item in PartyInfo.Items)
            {
                if (item.Value.Value <= 0)
                {
                    continue;
                }
                var itemInfo = new ItemInfo(item.Key, item.Value.Value);
                if (itemInfo.Master.ItemType == ItemType.UseItem || itemInfo.Master.ItemType == ItemType.DungeonItem)
                {
                    continue;
                }
                if (_useCount.ContainsKey(item.Key))
                {
                    itemInfo.UseNum.SetValue(_useCount[item.Key].Value);
                }
                list.Add(itemInfo);
            }
            list.Sort((a, b) => a.Id.Value - b.Id.Value > 0 ? 1 : -1);
            return list;
        }

        public void ChangeUseNum(int itemId, bool plus)
        {
            var ownCount = PartyInfo.Items[itemId];
            if (!_useCount.ContainsKey(itemId))
            {
                _useCount[itemId] = new();
            }
            var useCount = _useCount[itemId];
            if (plus)
            {
                useCount.GainValue(1, 0, ownCount.Value);
            } else
            {
                useCount.GainValue(-1, 0, ownCount.Value);
            }
        }

        public bool CanPresent()
        {
            return _useCount.Where(a => a.Value.Value > 0).Count() > 0;
        }

        public List<GetItemInfo> PresentGetItemInfos()
        {
            var list = new List<GetItemInfo>();
            // アイテムを消費
            foreach (var useCount in _useCount)
            {
                var getItemInfos = MakeItemGetItemInfos(useCount.Key, useCount.Value.Value);
                list.AddRange(getItemInfos);
                PartyInfo.ConsuneItemNum(useCount.Key, useCount.Value.Value);
            }
            return list;
        }

        public List<GetItemInfo> MakeItemGetItemInfos(int itemId, int num)
        {
            var getItemInfos = new List<GetItemInfo>();
            var itemData = DataSystem.Items.Find(a => a.Id == itemId);
            if (itemData != null)
            {
                var count = 0;
                while (getItemInfos.Count != num)
                {
                    var getItemInfo = MakeItemGetItemInfo(itemData);
                    if (getItemInfo == null)
                    {
                        continue;
                    }
                    if (getItemInfo.GetItemType == GetItemType.Skill)
                    {
                        if (getItemInfos.Find(a => a.Param1 == getItemInfo.Param1) == null)
                        {
                            getItemInfos.Add(getItemInfo);
                        }
                    } else
                    {
                        getItemInfos.Add(getItemInfo);
                    }
                    count++;
                    if (count > 99)
                    {
                        getItemInfo = MakeGetItemInfo(GetItemType.Currency, 10, 0);
                        getItemInfos.Add(getItemInfo);
                    }
                }
            }
            return getItemInfos;
        }

        public List<SkillInfo> GetRandumAddSkillInfos(ItemInfo itemInfo)
        {
            var list = new List<SkillInfo>();
            var rank = itemInfo.Master.Param1;
            var attribute = itemInfo.Master.Param2;
            var skillDates = DataSystem.Skills.Where(a => SkillData.ConvertRankCost(a.Value.Rank) == itemInfo.Master.Param1 && a.Value.IsRandumAddSkill() && !PartyInfo.LearningSkillIds.Contains(a.Key)).ToList();
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