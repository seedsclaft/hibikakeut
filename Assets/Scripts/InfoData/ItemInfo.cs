using System;

namespace Ryneus
{
    [Serializable]
    public class ItemInfo
    {
        public ItemData _master = null;
        public ItemData Master => _master != null ? _master : DataSystem.Items.Find(a => a.Id == Id.Value);
        public ParameterInt Id = new();
        public ParameterInt OwnNum = new();
        public ParameterInt UseNum = new();
        public ItemInfo(int id, int num)
        {
            Id.SetValue(id);
            _master = Master;
            OwnNum.SetValue(num);
        }
    }
}
