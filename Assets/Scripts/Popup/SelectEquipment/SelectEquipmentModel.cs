using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class SelectEquipmentModel : BaseModel
    {
        private SelectEquipmentSceneInfo _sceneParam;
        private List<int> _selectEquipments = new();
        public List<int> SelectEquipments => _selectEquipments;
        public SelectEquipmentModel()
        {
            _sceneParam = (SelectEquipmentSceneInfo)GameSystem.SceneStackManager.LastPopupInfo.template;
        }

        public void SelectEquipment(int equipmentId)
        {
            if (PartyInfo.EquipmentIds.Contains(equipmentId))
            {
                return;
            }
            if (_selectEquipments.Contains(equipmentId))
            {
                _selectEquipments.Remove(equipmentId);
            } else
            {
                if (_selectEquipments.Count < _sceneParam.SelectCount)
                {
                    _selectEquipments.Add(equipmentId);
                }
            }
        }
        
        public bool SelectedEquipment()
        {
            return _selectEquipments.Count == _sceneParam.SelectCount;
        }

        public List<EquipmentInfo> EquipmentInfos()
        {
            return _sceneParam.SelectEquipments;
        }

        public void DecideEquipmentInfos()
        {
            foreach (var selectEquipment in _selectEquipments)
            {
                var equipmentInfo = new EquipmentInfo(selectEquipment);
                _sceneParam.SelectedEquipments.Add(equipmentInfo);
            }
        }
    }

    public class SelectEquipmentSceneInfo
    {
        public int SelectCount;
        public List<EquipmentInfo> SelectEquipments;
        public List<EquipmentInfo> SelectedEquipments = new();
    }
}