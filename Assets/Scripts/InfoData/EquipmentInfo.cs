using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class EquipmentInfo
    {
        private EquipmentData _master = null;
        public EquipmentData Master => _master == null ? _master = DataSystem.FindEquipment(EquipmentId.Value) : _master;
        public ParameterInt EquipmentId = new();
        public List<EquipmentLearningInfo> LearningInfos = new();
        public ActorInfo EquipmentActor = null;
        public ParameterBool Selected = new();
        public EquipmentInfo(int equipmentId)
        {
            EquipmentId.SetValue(equipmentId);
            if (Master != null)
            {
                foreach (var learningDate in Master.LearningDates)
                {
                    var equipmentLearningInfo = new EquipmentLearningInfo(learningDate);
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

        public (bool isLearnd, int rate, int id) SortKey => 
            (IsLearned(), LearningRate(), _master.Id);

        private bool IsLearned()
        {
            return LearningInfos.All(a => a.LearningExp.Value == 100);
        }

        private int LearningRate()
        {
            return (int)LearningInfos.Sum(a => a.LearningExp.Value) * -1;
        }
    }
}
