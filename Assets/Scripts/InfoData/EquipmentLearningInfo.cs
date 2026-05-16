namespace Ryneus
{
    public class EquipmentLearningInfo
    {
        
        private SkillData _master = null;
        public SkillData Master => _master == null ? DataSystem.FindSkill(SkillId.Value) : _master;
        
        public ParameterInt SkillId = new();
        public ParameterInt LearningRate = new();
        public ParameterFloat LearningExp = new();
    }
}
