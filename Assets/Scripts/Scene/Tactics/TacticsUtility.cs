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

        public static int LearningMagicCost(ActorInfo actorInfo,AttributeType attributeType,List<ActorInfo> stageMembers,RankType rank = RankType.None)
        {
            if (attributeType == AttributeType.None)
            {
                return 0;
            }
            var cost = 1;
            var rankCost = ConvertRankCost(rank);
            var param = actorInfo.AttributeRanks(stageMembers)[(int)attributeType-1];
            switch (param)
            {
                case AttributeRank.S:
                    cost = 1;
                    break;
                case AttributeRank.A:
                    cost = 2;
                    break;
                case AttributeRank.B:
                    cost = 3;
                    break;
                case AttributeRank.C:
                    cost = 4;
                    break;
                case AttributeRank.D:
                    cost = 5;
                    break;
                case AttributeRank.E:
                    cost = 6;
                    break;
                case AttributeRank.F:
                    cost = 7;
                    break;
                case AttributeRank.G:
                    cost = 8;
                    break;
            }
            
            return Mathf.FloorToInt(cost * TacticsCostRate(actorInfo) * rankCost);
        }

        private static int ConvertRankCost(RankType rankType)
        {
            switch (rankType)
            {
                case RankType.ActiveRank1:
                    return 0;
                case RankType.PassiveRank1:
                case RankType.EnhanceRank1:
                    return 1;
                case RankType.ActiveRank2:
                case RankType.PassiveRank2:
                case RankType.EnhanceRank2:
                    return 2;
            }
            return 1;
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