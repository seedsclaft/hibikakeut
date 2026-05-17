namespace Ryneus
{
    [System.Serializable]
    public class TradeItemInfo
    {
        private GetItemInfo _getItemInfo = null;
        public GetItemInfo GetItemInfo => _getItemInfo;
        public ParameterInt Cost = new();
        public ParameterInt GetCount = new();
        public ParameterBool Selected = new();
        public TradeItemInfo(GetItemData getItemData, int cost)
        {
            _getItemInfo = new GetItemInfo(getItemData);
            Cost.SetValue(cost);
        }

        public SkillInfo SkillInfo()
        {
            if (_getItemInfo.Master.Type == GetItemType.Skill)
            {
                var skillInfo = new SkillInfo(_getItemInfo.Param1);
                return skillInfo;
            }
            return null;
        }

        public ItemInfo ItemInfo()
        {
            if (_getItemInfo.Master.Type == GetItemType.Item)
            {
                var itemInfo = new ItemInfo(_getItemInfo.Param1, 1);
                return itemInfo;
            }
            return null;
        }

        public EquipmentInfo EquipmentInfo()
        {
            if (_getItemInfo.Master.Type == GetItemType.Equipment)
            {
                var equipmentInfo = new EquipmentInfo(_getItemInfo.Param1);
                return equipmentInfo;
            }
            return null;
        }

    }
}
