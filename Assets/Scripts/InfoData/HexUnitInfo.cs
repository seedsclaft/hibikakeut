using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class HexUnitInfo
    {
        public HexUnitInfo(int index,StageSymbolData stageSymbolData,int teamId = 0)
        {
            Index.SetValue(index);
            SetHexUnitType(stageSymbolData.UnitType);
            SetHexMoveType(stageSymbolData.MoveType,stageSymbolData.MoveParam);
            SetPosition(stageSymbolData.InitX,stageSymbolData.InitY);
            if (_hexUnitType == HexUnitType.Battler)
            {
                _hexLayer = HexLayer.Unit;
            } else
            {
                _hexLayer = HexLayer.Field;
            }
            TeamId.SetValue(teamId);
        }

        private HexField _hexField = new();
        public HexField HexField => _hexField;

        private HexLayer _hexLayer = HexLayer.None;
        public HexLayer HexLayer => _hexLayer;
        
        public ParameterInt Index = new();
        public ParameterInt TeamId = new();
        public void SetPosition(int x,int y)
        {
            _hexField.X = x;
            _hexField.Y = y;
        }

        public bool IsUnit => _hexLayer == HexLayer.Unit;
        public bool IsWall => _hexUnitType == HexUnitType.Battler || _hexUnitType == HexUnitType.None;        public bool IsSelectArea => _hexUnitType == HexUnitType.Reach;
        private HexUnitType _hexUnitType = HexUnitType.None;
        public HexUnitType HexUnitType => _hexUnitType;
        public void SetHexUnitType(HexUnitType hexUnitType) => _hexUnitType = hexUnitType;
        
        private HexMoveType _hexMoveType = HexMoveType.None;
        private int _hexMoveParam = 0;
        public HexMoveType HexMoveType => _hexMoveType;
        public void SetHexMoveType(HexMoveType hexMoveType,int param)
        {
            _hexMoveType = hexMoveType;
            _hexMoveParam = param;
        }

        [UnityEngine.SerializeField] private List<GetItemInfo> _getItemInfos = new();
        public List<GetItemInfo> GetItemInfos => _getItemInfos;
        public void AddGetItemInfos(List<GetItemInfo> getItemInfos)
        {
            _getItemInfos.AddRange(getItemInfos);
        }        
        
        [UnityEngine.SerializeField] private TroopInfo _troopInfo = null;
        public TroopInfo TroopInfo => _troopInfo;
        public void SetTroopInfo(TroopInfo troopInfo)
        {
            _troopInfo = troopInfo;
        }

        [UnityEngine.SerializeField] private List<ActorInfo> _actorInfos = null;
        public List<ActorInfo> ActorInfos => _actorInfos;
        public void SetActorInfos(List<ActorInfo> actorInfos)
        {
            _actorInfos = actorInfos;
        }

        public int BattleEvaluate()
        {
            if (_troopInfo != null)
            {
                var evaluate = 0;
                foreach (var battlerInfo in _troopInfo.BattlerInfos)
                {
                    evaluate += battlerInfo.Evaluate();
                }
                return evaluate;
            }
            return 0;
        }
        
        public bool IsBattleSymbol()
        {
            return _hexUnitType == HexUnitType.Battler;
        }
    }

}
