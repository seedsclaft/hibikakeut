using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class SelectEquipmentModel : BaseModel
    {
        private SelectEquipmentSceneInfo _sceneParam;
        private List<int> _selectEquipmentIds = new();
        public List<int> SelectEquipmentIds => _selectEquipmentIds;
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
            if (_selectEquipmentIds.Contains(equipmentId))
            {
                _selectEquipmentIds.Remove(equipmentId);
            } else
            {
                if (_selectEquipmentIds.Count < _sceneParam.SelectCount)
                {
                    _selectEquipmentIds.Add(equipmentId);
                }
            }
        }
        
        public bool SelectedEquipment()
        {
            return _selectEquipmentIds.Count == _sceneParam.SelectCount;
        }

        public List<EquipmentInfo> EquipmentInfos()
        {
            var list = new List<EquipmentInfo>();
            foreach (var equipmentInfo in _sceneParam.SelectEquipments)
            {
                equipmentInfo.Selected.SetValue(_selectEquipmentIds.Contains(equipmentInfo.EquipmentId.Value));
                list.Add(equipmentInfo);
            }
            return list;
        }

        public void DecideEquipmentInfos()
        {
            foreach (var selectEquipment in _selectEquipmentIds)
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