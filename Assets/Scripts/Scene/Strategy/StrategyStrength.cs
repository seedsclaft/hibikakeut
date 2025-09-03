// Lvアップステータスで使用
using UnityEngine;

namespace Ryneus
{
    public class StrategyStrength : ListItem, IListViewItem
    {
        [SerializeField] private StrengthComponent strengthComponent;

        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }

            var data = ListItemData<StrategyStrengthInfo>();
            strengthComponent.UpdateInfo(data.ActorInfo, data.StatusParamType);
        }
    }

    public class StrategyStrengthInfo
    {
        public ActorInfo ActorInfo;
        public StatusParamType StatusParamType;
    }
}