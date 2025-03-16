using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class TacticsUtility 
    {
        private static int TacticsCostRate(ActorInfo actorInfo)
        {
            return 1;
        }

        public static int TrainCost(ActorInfo actorInfo)
        {
            return TacticsCostRate(actorInfo);
        }

        public static int TrainCost(int level,ActorInfo actorInfo)
        {
            return level * TacticsCostRate(actorInfo);
        }

        public static int RecoveryCost(ActorInfo actorInfo,bool checkAlcana = false)
        {
            return (int)Mathf.Ceil(actorInfo.Level * 0.1f) * TacticsCostRate(actorInfo);
        }

        public static int RemainRecoveryCost(ActorInfo actorInfo,bool checkAlcana = false)
        {
            int hpCost = (int)Mathf.Ceil((actorInfo.MaxHp - actorInfo.CurrentHp.Value) * 0.1f) * TacticsCostRate(actorInfo);
            int mpCost = (int)Mathf.Ceil((actorInfo.MaxMp - actorInfo.CurrentMp.Value) * 0.1f) * TacticsCostRate(actorInfo);
            return hpCost > mpCost ? hpCost : mpCost;
        }

        public static int ResourceCost(ActorInfo actorInfo)
        {
            return 0;
        }

        public static int ResourceGain(ActorInfo actorInfo)
        {
            return actorInfo.Level;
        }
    }
}