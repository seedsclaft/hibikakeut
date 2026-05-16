using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
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
            var rate = TacticsCostRate(actorInfo);
            float remainPer = 1f - (actorInfo.Exp.Value % 100 * 0.01f);
            var needPoint = ((actorInfo.Level - 1) * 0.5f) + 10;
            return (int)MathF.Floor(needPoint * remainPer * rate) + 1;
        }

        public static int GetExpCurrency(ActorInfo actorInfo, int currency)
        {
            var baseValue = 20 - (int)MathF.Floor((actorInfo.Level - 1) * 0.5f);
            return baseValue * currency;
        }

        public static int TrainCost(int level, ActorInfo actorInfo)
        {
            return level * TacticsCostRate(actorInfo);
        }

        public static int RecoveryCost(ActorInfo actorInfo, bool checkAlcana = false)
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

        public static int EquipAttributeRankCost(AttributeRank attributeRank)
        {
            var cost = 1;
            switch (attributeRank)
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
            return cost;
        }

        public static float AttributeRankSkillExp(AttributeRank attributeRank)
        {
            var rate = 1f;
            switch (attributeRank)
            {
                case AttributeRank.S:
                    rate = 1.25f;
                    break;
                case AttributeRank.A:
                    rate = 1;
                    break;
                case AttributeRank.B:
                    rate = 0.8f;
                    break;
                case AttributeRank.C:
                    rate = 0.6f;
                    break;
                case AttributeRank.D:
                    rate = 0.4f;
                    break;
                case AttributeRank.E:
                    rate = 0.2f;
                    break;
                case AttributeRank.F:
                    rate = 0.1f;
                    break;
                case AttributeRank.G:
                    rate = 0;
                    break;
            }
            return rate;
        }
    }
}