using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class StrategyActorList : BaseList
    {
        public void StartResultAnimation(int actorCount, List<ActorInfo> bonusActorInfos, Action callEvent)
        {
            for (int i = 0; i < ItemPrefabList.Count;i++)
            {
                UIComponent.SetActive(ItemPrefabList[i], false);
                if (i < actorCount)
                {
                    var StrategyActor = ItemPrefabList[i].GetComponent<StrategyActor>();
                    var actorInfo = StrategyActor.ListItemData<ActorInfo>();
                    var isBonus = bonusActorInfos.Find(a => a.ActorId.Value == actorInfo.ActorId.Value) != null;
                    UIComponent.SetActive(StrategyActor?.gameObject, true);
                    StrategyActor.StartResultAnimation(i, isBonus);
                    if (i == actorCount-1)
                    {
                        StrategyActor.SetEndCallEvent(callEvent);
                    }
                }
            }
        }

        public void StartGetExpAnimation(List<StrategyActorLevelUpInfo> levelUpInfos, Action callEvent)
        {
            for (int i = 0; i < ItemPrefabList.Count;i++)
            {
                if (i < levelUpInfos.Count)
                {
                    var StrategyActor = ItemPrefabList[i].GetComponent<StrategyActor>();
                    StrategyActor.StartGetExpAnimation(levelUpInfos[i]);
                    if (i == levelUpInfos.Count-1)
                    {
                        StrategyActor.SetEndCallEvent(callEvent);
                    }
                }
            }
        }

        public void SetShinyReflect(bool isEnable)
        {
            for (int i = 0; i < ItemPrefabList.Count;i++)
            {
                var StrategyActor = ItemPrefabList[i].GetComponent<StrategyActor>();
                StrategyActor.SetShinyReflect(isEnable);
            }
        }
    }
}