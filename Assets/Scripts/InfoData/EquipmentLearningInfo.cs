namespace Ryneus
{
    public class EquipmentLearningInfo
    {   
        private SkillData _skillData = null;
        public SkillData SkillData => _skillData == null ? _skillData = DataSystem.FindSkill(SkillId.Value) : _skillData;
        public EquipmentLearningData Master = new();
        public ParameterInt SkillId = new();
        public ParameterInt LearningRate = new();
        public ParameterBool EquipmentOnly = new(false);
        public ParameterFloat LearningExp = new();
        public EquipmentLearningInfo(EquipmentLearningData learningDate)
        {
            Master = learningDate;
            SkillId.SetValue(learningDate.SkillId);
            LearningRate.SetValue(learningDate.Rate);
            EquipmentOnly.SetValue(learningDate.EquipmentOnly);
        }

        public bool IsLearned()
        {
            if (EquipmentOnly.Value)
            {
                return false;
            }
            return LearningExp.Value >= 100;
        }

        public bool IsLearning()
        {
            if (EquipmentOnly.Value)
            {
                return false;
            }
            return LearningExp.Value < 100;
        }

        public bool IsComplete()
        {
            if (EquipmentOnly.Value)
            {
                return true;
            }
            return LearningExp.Value >= 100;
        }
    }
}
