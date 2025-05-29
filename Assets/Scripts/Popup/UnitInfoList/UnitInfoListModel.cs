using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class UnitInfoListModel : BaseModel
    {
    }

    public class UnitInfoListInfo
    {
        private System.Action<UnitInfo> _callEvent;
        public System.Action<UnitInfo> CallEvent => _callEvent;
        public UnitInfoListInfo(System.Action<UnitInfo> callEvent,System.Action backEvent)
        {
            _callEvent = callEvent;
            _backEvent = backEvent;
        }
        private System.Action _backEvent;
        public System.Action BackEvent => _backEvent;

        private List<UnitInfo> _unitInfos;
        public List<UnitInfo> UnitInfos => _unitInfos;
        public void SetUnitInfos(List<UnitInfo> unitInfos)
        {
            _unitInfos = unitInfos;
        }

        public ParameterBool IsUnitEdit = new();
        public ParameterBool IsDepatureEdit = new();
    }
}