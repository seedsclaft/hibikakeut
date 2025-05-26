using System.Collections;
using System.Collections.Generic;

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
    }
}