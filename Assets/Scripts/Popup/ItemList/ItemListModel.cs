using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class ItemListModel : BaseModel
    {
        private Dictionary<int,ParameterInt> _useCount = new();
        public ItemListModel()
        {
        }

        public List<ItemInfo> ItemInfoss()
        {
            var list = new List<ItemInfo>();
            foreach (var item in PartyInfo.Items)
            {
                if (item.Value.Value <= 0)
                {
                    continue;
                }
                var itemInfo = new ItemInfo(item.Key,item.Value.Value);
                if (_useCount.ContainsKey(item.Key))
                {
                    itemInfo.UseNum.SetValue(_useCount[item.Key].Value);
                }
                list.Add(itemInfo);
            }
            return list;
        }

        public void ChangeUseNum(int itemId,bool plus)
        {
            var ownCount = PartyInfo.Items[itemId];
            if (!_useCount.ContainsKey(itemId))
            {
                _useCount[itemId] = new();
            }
            var useCount = _useCount[itemId];
            if (plus)
            {
                useCount.GainValue(1,0,ownCount.Value);
            } else
            {
                useCount.GainValue(-1,0,ownCount.Value);
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
                var getItemInfos = MakeItemGetItemInfos(useCount.Key,useCount.Value.Value);
                list.AddRange(getItemInfos);
                PartyInfo.ConsuneItemNum(useCount.Key,useCount.Value.Value);
            }
            return list;
        }

        public List<GetItemInfo> MakeItemGetItemInfos(int itemId,int num)
        {
            var list = new List<GetItemInfo>();
            var itemData = DataSystem.Items.Find(a => a.Id == itemId);
            if (itemData != null)
            {
                for (int i = 0;i < num;i++)
                {
                    var getItemInfo = MakeItemGetItemInfo(itemData);
                    if (getItemInfo != null)
                    {
                        list.Add(getItemInfo);
                    }
                }
            }
            return list;
        }

        private GetItemInfo MakeItemGetItemInfo(ItemData itemData)
        {
            switch (itemData.ItemType)
            {
                case ItemType.RandumAddSkill:
                    // ランダムでparam2属性のparam1Rankを入手
                    var candidateSkills = DataSystem.Skills.Where(a => (int)a.Value.Rank == itemData.Param1).ToList();
                    if (itemData.Param2 != -1)
                    {
                        candidateSkills = candidateSkills.Where(a => (int)a.Value.Attribute == itemData.Param2).ToList();
                    }
                    var rand = UnityEngine.Random.Range(0,candidateSkills.Count);
                    // 報酬設定
                    return MakeGetItemInfo(GetItemType.Skill,candidateSkills[rand].Value.Id);
            }
            return null;
        }
    }
}