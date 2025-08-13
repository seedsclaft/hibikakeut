using System;

namespace Ryneus
{
    [Serializable]
    public class ItemInfo
    {
        public ItemData Master => DataSystem.Items.Find(a => a.Id == Id.Value);
        public ParameterInt Id = new();
        public ParameterInt OwnNum = new();
        public ParameterInt UseNum = new();
        public ItemInfo(int id, int num)
        {
            Id.SetValue(id);
            OwnNum.SetValue(num);
        }
    }
}
