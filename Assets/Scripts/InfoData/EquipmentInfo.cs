using System.Collections.Generic;

namespace Ryneus
{
    public class EquipmentInfo
    {
        private EquipmentData _master = null;
        public EquipmentData Master => _master == null ? _master = DataSystem.FindEquipment(EquipmentId.Value) : _master;
        public ParameterInt EquipmentId = new();
        public List<EquipmentLearningInfo> LearningInfos = new();

        public EquipmentInfo(int equipmentId)
        {
            EquipmentId.SetValue(equipmentId);
            if (Master != null)
            {
                foreach (var learningDate in Master.LearningDates)
                {
                    EquipmentLearningInfo equipmentLearningInfo = new();
                    equipmentLearningInfo.SkillId.SetValue(learningDate.SkillId);
                    equipmentLearningInfo.LearningRate.SetValue(learningDate.Rate);
                    equipmentLearningInfo.EquipmentOnly.SetValue(learningDate.EquipmentOnly);
                    LearningInfos.Add(equipmentLearningInfo);
                }
            }
        }
        public List<SkillInfo> SkillInfos()
        {
            var list = new List<SkillInfo>();
            foreach (var learningInfo in LearningInfos)
            {
                list.Add(new SkillInfo(learningInfo.SkillId.Value));
            }
            return list;
        }
    }
}
