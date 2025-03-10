
namespace Ryneus
{
    [System.Serializable]
    public class BattleExamine
    {        
        public ParameterInt ChainSuccessCount = new();
        public ParameterInt PayBattleMp = new();
        public ParameterInt AttackedCount = new();
        public ParameterInt MaxDamage = new();
        public ParameterInt DodgeCount = new();
        public ParameterInt HealCount = new();
        public ParameterInt BeCriticalCount = new();
        public ParameterInt DamagedValue = new();
        
        public void ResetData()
        {
            ChainSuccessCount.SetValue(0);
            PayBattleMp.SetValue(0);
            AttackedCount.SetValue(0);
            HealCount.SetValue(0);
            BeCriticalCount.SetValue(0);
            DodgeCount.SetValue(0);
            DamagedValue.SetValue(0);
        }
        
        public void GainMaxDamage(int value)
        {
            if (value > MaxDamage.Value)
            {
                MaxDamage.SetValue(value);
            }
        }
    }
}
