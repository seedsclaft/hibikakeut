using System;
using UnityEngine;

namespace Ryneus
{
    public partial class ActionResultInfo
    {
        private float BaseHpDamage(BattlerInfo subject, BattlerInfo target, float featureValue, bool isNoEffect, bool isOneTarget)
        {
            // ダメージ＝(攻撃力-防御力)×(威力÷100)×(100%+会心ダメージ)×(100%-ガード軽減率)
            float atkValue = CurrentAttack(subject, isNoEffect);
            float defValue = CurrentDefense(subject, target, isNoEffect);
            return (atkValue - defValue) * (featureValue * 0.01f);
        }

        private float CalcHpDamage(BattlerInfo subject, BattlerInfo target, float featureValue, bool isNoEffect, bool isOneTarget)
        {
            float hpDamage = BaseHpDamage(subject, target, featureValue, isNoEffect, isOneTarget);

            // 効果補正
            return CalcDamageEffect(hpDamage, subject, target, isNoEffect);
        }
        
        private float CalcDamageEffect(float hpDamage, BattlerInfo subject, BattlerInfo target, bool isNoEffect, int plusRate = 0)
        {
            if (IsCritical(subject, target))
            {
                hpDamage *= 1.5f + CriticalDamageRate(subject);
            }

            // 有利属性なら1.5倍
            var skillData = DataSystem.FindSkill(SkillId.Value);
            if (target.Kinds.Contains((KindType)skillData.Attribute))
            {
                hpDamage *= DataSystem.System.WeakPointRate * 0.01f;
                _weakPoint = true;
            }
            hpDamage = ApplyVariance(hpDamage);
            hpDamage -= CalcDamageShell(target);
            hpDamage = CalcAddDamage(subject, target, hpDamage);
            CalcAddState(subject, target);
            hpDamage = Mathf.Max(1, hpDamage);
            if (IsNoDamage(target, isNoEffect))
            {
                hpDamage = 0;
            }
            if (IsDeadlyDamage(subject, target, isNoEffect))
            {
                // 対象がボスの場合は残りHpの50%ダメージ
                if (target.Kinds.Contains(KindType.Boss))
                {
                    hpDamage = Math.Max(target.Hp.Value / 2, hpDamage);
                }
                else
                {
                    hpDamage = target.Hp.Value;
                }
            }
            if (!isNoEffect)
            {
                CalcCounterDamage(subject, target, hpDamage);
            }
            CalcFreezeDamage(subject, hpDamage);
            ReHeal.GainValue(CalcDrainValue(subject, hpDamage));
            return CalcDamageShield(subject, target, hpDamage);
        }
    }
}
