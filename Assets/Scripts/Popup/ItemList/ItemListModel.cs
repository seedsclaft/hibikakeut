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

        public bool CanChangeUseNum(int itemId, bool plus)
        {
            var ownCount = PartyInfo.Items[itemId];
            if (!_useCount.ContainsKey(itemId))
            {
                _useCount[itemId] = new();
            }
            var useCount = _useCount[itemId];
            if (plus)
            {
                return ownCount.Value - useCount.Value > 0;
            }
            return useCount.Value > 0;
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
            }
            else
            {
                useCount.GainValue(-1, 0, ownCount.Value);
            }
        }

        public bool CanPresent()
        {
            return _useCount.Where(a => a.Value.Value > 0).Count() > 0;
        }

        public List<ItemData> SelectEquipmentItems()
        {
            var list = new List<ItemData>();
            foreach (var useCount in _useCount)
            {
                var getItemInfos = MakeItemGetItemInfos(useCount.Key, useCount.Value.Value);
                if (getItemInfos.Count > 0 && getItemInfos[0].GetItemType == GetItemType.SelectEquipment)
                {
                    foreach (var getItemInfo in getItemInfos)
                    {
                        list.Add(DataSystem.FindItem(useCount.Key));
                    }
                    return list;
                } 
            }
            return list;
        }

        public bool SelectEquipmentZero(ItemData itemData)
        {
            var list = new List<EquipmentInfo>();
            var selectEquipmentItem = itemData;
            // Param1 = Rank以下全て
            // Param2 = -1 属性区別なし, 1-5 属性縛り
            foreach (var equipment in DataSystem.Dates[DataType.Equipment].ToList<EquipmentData>())
            {
                if (equipment.Id == DataSystem.System.InitEquipmentId)
                {
                    continue;
                }
                if (equipment.Rank > selectEquipmentItem.Param1)
                {
                    continue;
                }
                if (selectEquipmentItem.Param2 != -1 && (AttributeType)selectEquipmentItem.Param2 != equipment.Attribute)
                {
                    continue;
                }
                if (PartyInfo.EquipmentIds.Contains(equipment.Id))
                {
                    continue;
                }
                var equipmentInfo = new EquipmentInfo(equipment.Id);
                list.Add(equipmentInfo);
            }
            return list.Count == 0;
        }

        public List<EquipmentInfo> SelectEquipmentInfos()
        {
            var selectEquipmentItem = SelectEquipmentItems()[0];
            return SelectEquipmentInfos(selectEquipmentItem);
        }

        public List<EquipmentInfo> SelectEquipmentInfos(ItemData itemData)
        {
            var list = new List<EquipmentInfo>();
            // Param1 = Rank以下全て
            // Param2 = -1 属性区別なし, 1-5 属性縛り
            foreach (var equipment in DataSystem.Dates[DataType.Equipment].ToList<EquipmentData>())
            {
                if (equipment.Id == DataSystem.System.InitEquipmentId)
                {
                    continue;
                }
                if (equipment.Rank > itemData.Param1)
                {
                    continue;
                }
                if (itemData.Param2 != -1 && (AttributeType)itemData.Param2 != equipment.Attribute)
                {
                    continue;
                }
                var equipmentInfo = new EquipmentInfo(equipment.Id);
                list.Add(equipmentInfo);
            }
            return list;
        }

        public List<GetItemInfo> PresentGetItemInfos(List<EquipmentInfo> selectedEquipmentInfos = null, ItemData itemData = null)
        {
            var list = new List<GetItemInfo>();
            // アイテムを消費
            foreach (var useCount in _useCount)
            {
                if (itemData != null && itemData.Id != useCount.Key)
                {
                    continue;
                }
                var getItemInfos = MakeItemGetItemInfos(useCount.Key, useCount.Value.Value);
                if (getItemInfos.Count > 0 && getItemInfos[0].GetItemType != GetItemType.SelectEquipment)
                {
                    list.AddRange(getItemInfos);
                }
                PartyInfo.ConsuneItemNum(useCount.Key, useCount.Value.Value);
            }
            // 選択した装備
            if (selectedEquipmentInfos != null)
            {
                foreach (var equipmentInfo in selectedEquipmentInfos)
                {
                    var getItemData = new GetItemData
                    {
                        Type = GetItemType.Equipment,
                        Param1 = equipmentInfo.EquipmentId.Value
                    };
                    var getItemInfo = new GetItemInfo(getItemData);
                    list.Add(getItemInfo);
                }
            }
            return list;
        }

        public List<GetItemInfo> MakeItemGetItemInfos(int itemId, int num)
        {
            var getItemInfos = new List<GetItemInfo>();
            var itemData = DataSystem.FindItem(itemId);
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
            var skillDates = DataSystem.Dates[DataType.Skills].FindAll<SkillData>(a => SkillData.ConvertRankCost(a.Rank) == itemInfo.Master.Param1 && a.Rank != RankType.PassiveEnhanceRank1 && a.IsRandumAddSkill() && !PartyInfo.LearningSkillIds.Contains(a.Id));
            if (attribute > 0)
            {
                skillDates = skillDates.FindAll(a => (int)a.Attribute == attribute);
            }

            foreach (var skillData in skillDates)
            {
                list.Add(new SkillInfo(skillData.Id));
            }
            return list;
        }
    }
}