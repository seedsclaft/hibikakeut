using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class BattleRecord
    {
        private Dictionary<int, int> _useSkillCountDict = new();
        public Dictionary<int, int> UseSkillCountDict => _useSkillCountDict;
        private int _battlerIndex = -1;
        public int BattlerIndex => _battlerIndex;
        private int _attackValue = 0;
        public int AttackValue => _attackValue;
        private int _damagedValue = 0;
        public int DamagedValue => _damagedValue;
        private int _maxDamage = 0;
        public int MaxDamage => _maxDamage;
        private int _healValue = 0;
        public int HealValue => _healValue;
        public ParameterInt WeakAttackCount = new();
        public BattleRecord(int battlerIndex)
        {
            _battlerIndex = battlerIndex;
        }

        public void GainAttackValue(int attackValue)
        {
            _attackValue += attackValue;
            SetMaxAttack(attackValue);
        }

        public void GainDamagedValue(int damagedValue)
        {
            _damagedValue += damagedValue;
        }

        public void GainHealValue(int healValue)
        {
            _healValue += healValue;
        }

        public void SetMaxAttack(int attackValue)
        {
            if (attackValue > _maxDamage)
            {
                _maxDamage = attackValue;
            }
        }

        public void GainUseSkillCount(int skillId,int gain)
        {
            if (!_useSkillCountDict.ContainsKey(skillId))
            {
                _useSkillCountDict[skillId] = 0;
            }
            _useSkillCountDict[skillId] += gain;
        }
    }
}