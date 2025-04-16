using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Ryneus
{
    public class UnitInfoListModel : BaseModel
    {
        private UnitInfoListInfo _sceneParam;
        private List<UnitInfo> _unitInfos = new();
        public bool IsEdit => _sceneParam.IsUnitEdit.Value;
        private BattlerInfo _selectingBattlerInfo = null;
        public void SetSelectingBattlerInfo(BattlerInfo selectingBattlerInfo)
        {
            _selectingBattlerInfo = selectingBattlerInfo;
        }

        public void SwapUnitInfos(int actorId)
        {
            UnitInfo fromUnitInfo = null;
            BattlerInfo fromBattler = _selectingBattlerInfo;
            bool fromMain = false;

            BattlerInfo toBattler;
            if (actorId > 0)
            {
                toBattler = new BattlerInfo(StageMembers().Find(a => a.ActorId.Value == actorId),1);
            } else
            {
                toBattler = new BattlerInfo();
            }
            foreach (var unitInfo in _unitInfos)
            {
                var from = unitInfo.BattlerInfos.FindIndex(a => a == _selectingBattlerInfo);
                if (from > -1)
                {
                    fromUnitInfo = unitInfo;
                    fromBattler = unitInfo.BattlerInfos[from];
                    fromMain = from == 0;
                }
            }
            fromUnitInfo.BattlerInfos.Remove(fromBattler);
            if (fromMain)
            {
                fromUnitInfo.BattlerInfos.Insert(0,toBattler);
            } else
            {
                fromUnitInfo.BattlerInfos.Add(toBattler);
            }
            CurrentStage.GetTurnTeamInfo().SetDepatuerInfos(_unitInfos);
        }

        
        public UnitInfoListModel()
        {
            _sceneParam = (UnitInfoListInfo)GameSystem.SceneStackManager.LastTemplate;
            foreach (var unitInfo in _sceneParam.UnitInfos)
            {
                var copy = unitInfo.CopyData();
                if (copy.BattlerInfos.Count == 1)
                {
                    copy.BattlerInfos.Add(new BattlerInfo());
                }
                _unitInfos.Add(copy);
            }
            // 編成の場合新規作成用に１枠作る
            if (_sceneParam.IsUnitEdit.Value)
            {
                var unitInfo = new UnitInfo();
                unitInfo.SetBattlers(new List<BattlerInfo>(){new BattlerInfo(),new BattlerInfo()});
                _unitInfos.Add(unitInfo);
            }
        }

        public List<UnitInfo> GetUnitInfos()
        {
            return _unitInfos;
        }

        public void CallDecideEvent(UnitInfo unitInfo)
        {
            if (unitInfo == null)
            {
                return;
            }
            _sceneParam.CallEvent(unitInfo);
        }
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
    }
}