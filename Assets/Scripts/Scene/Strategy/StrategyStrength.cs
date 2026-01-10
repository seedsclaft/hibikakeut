// Lvアップステータスで使用
using System.Collections.Generic;
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
        public static List<StrategyStrengthInfo> BasicStrategyStrengthInfos(ActorInfo actorInfo)
        {
            var strategyStrengthInfos = new List<StrategyStrengthInfo>
            {
                new()
                {
                    ActorInfo = actorInfo,
                    StatusParamType = StatusParamType.Hp,
                },
                new()
                {
                    ActorInfo = actorInfo,
                    StatusParamType = StatusParamType.Atk,
                },
                new()
                {
                    ActorInfo = actorInfo,
                    StatusParamType = StatusParamType.Def,
                },
                new()
                {
                    ActorInfo = actorInfo,
                    StatusParamType = StatusParamType.Spd,
                },
                new()
                {
                    ActorInfo = actorInfo,
                    StatusParamType = StatusParamType.Cost,
                }
            };
            return strategyStrengthInfos;
        }
    }
}