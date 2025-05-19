using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class UnitInfoListModel : BaseModel
    {
        private UnitInfoListInfo _sceneParam;
        private List<UnitInfo> _unitInfos = new();
        public bool IsEdit => _sceneParam.IsUnitEdit.Value;
        public bool IsDepature => _sceneParam.IsDepatureEdit.Value;
        private BattlerInfo _selectingBattlerInfo = null;
        public void SetSelectingBattlerInfo(BattlerInfo selectingBattlerInfo)
        {
            _selectingBattlerInfo = selectingBattlerInfo;
        }
        public bool CheckSelectingBattlerInfo()
        {
            var busyIndexes = new List<int>();
            foreach (var unitInfo in CurrentStage.GetTurnTeamInfo().UnitInfos)
            {
                busyIndexes.Add(unitInfo.Index.Value);
            }
            var selectUnit = _unitInfos.Find(a => a.BattlerInfos.Contains(_selectingBattlerInfo));
            return busyIndexes.Contains(selectUnit.Index.Value);
        }

        public UnitInfoListModel()
        {
            _sceneParam = (UnitInfoListInfo)GameSystem.SceneStackManager.LastTemplate;
            _unitInfos = _sceneParam.UnitInfos;
        }

        public void SwapUnitInfos(int actorId)
        {
            UnitInfo fromUnitInfo = null;
            BattlerInfo fromBattler = _selectingBattlerInfo;
            bool fromMain = false;

            UnitInfo toUnitInfo = null;
            BattlerInfo toBattler = null;
            bool toMain = false;
            foreach (var unitInfo in _unitInfos)
            {
                var to = unitInfo.BattlerInfos.FindIndex(a => a.ActorInfo != null && a.ActorInfo.ActorId.Value == actorId);
                if (to > -1)
                {
                    toBattler = unitInfo.BattlerInfos[to];
                    toUnitInfo = unitInfo;
                    toMain = to == 0;
                }
            }
            if (toBattler == null)
            {
                if (actorId > 0)
                {
                    toBattler = new BattlerInfo(StageMembers().Find(a => a.ActorId.Value == actorId),1);
                } else
                {
                    toBattler = new BattlerInfo();
                }
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
            if (toUnitInfo != null)
            {
                toUnitInfo.BattlerInfos.Remove(toBattler);
                if (toMain)
                {
                    toUnitInfo.BattlerInfos.Insert(0,fromBattler);
                } else
                {
                    toUnitInfo.BattlerInfos.Add(fromBattler);
                }
            }
            CurrentStage.GetTurnTeamInfo().SetDepatuerInfos(_unitInfos);
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

        public List<ActorInfo> StageActorInfos(List<BattlerInfo> battlerInfos)
        {
            var list = new List<ActorInfo>();
            foreach (var battlerInfo in battlerInfos)
            {
                if (battlerInfo.ActorInfo != null)
                {
                    var actorInfo = StageMembers().Find(a => a.ActorId == battlerInfo.ActorInfo.ActorId);
                    actorInfo.ChangeHp(battlerInfo.Hp.Value);
                    list.Add(actorInfo);
                }
            }
            return list;
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
        public ParameterBool IsDepatureEdit = new();
    }
}