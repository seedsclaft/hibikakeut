using System.Collections.Generic;

namespace Ryneus
{
    public class EquipmentInfo
    {
        private EquipmentData _master = null;
        public EquipmentData Master => _master == null ? DataSystem.FindEquipment(EquipmentId.Value) : _master;
        public ParameterInt EquipmentId = new();
        public List<EquipmentLearningInfo> LearningInfos = new();
    }
}
