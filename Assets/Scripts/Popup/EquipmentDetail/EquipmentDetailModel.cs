using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class EquipmentDetailModel : BaseModel
    {
        private EquipmentDetailViewInfo _sceneParam;
        public EquipmentDetailViewInfo SceneParam => _sceneParam;
        public EquipmentDetailModel()
        {
            _sceneParam = (EquipmentDetailViewInfo)GameSystem.SceneStackManager.LastPopupInfo.template;

        }

        public List<EquipmentInfo> EquipmentInfos()
        {
            return _sceneParam.EquipmentInfos;
        }
    }

    public class EquipmentDetailViewInfo
    {
        public ParameterString Title = new();
        public List<EquipmentInfo> EquipmentInfos = new();
    }
}