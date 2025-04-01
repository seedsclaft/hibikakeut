using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class HexUnitInfo
    {
        public HexUnitInfo(int index,StageSymbolData stageSymbolData)
        {
            Index.SetValue(index);
            SetHexUnitType(stageSymbolData.UnitType);
            SetPosition(stageSymbolData.InitX,stageSymbolData.InitY);
        }

        private HexField _hexField = new();
        public HexField HexField => _hexField;        
        
        public ParameterInt Index = new();
        public void SetPosition(int x,int y)
        {
            _hexField.X = x;
            _hexField.Y = y;
        }

        public bool IsUnit => _hexUnitType == HexUnitType.Battler;
        public bool IsSelectArea => _hexUnitType == HexUnitType.Reach;
        private HexUnitType _hexUnitType = HexUnitType.None;
        public HexUnitType HexUnitType => _hexUnitType;
        public void SetHexUnitType(HexUnitType hexUnitType) => _hexUnitType = hexUnitType;

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
            return _hexUnitType == HexUnitType.Battler || _hexUnitType == HexUnitType.Boss;
        }
    }

    public enum HexUnitType
    {
        None = 0,
        Battler = 10,
        Boss = 11,
        Basement = 20,
        Departure = 21, // 出撃する場所
        Alcana = 30,
        Actor = 40,
        Resource = 50,
        SelectActor = 60,
        Shop = 70,
        Group = 99, // 99以上はグループ指定
        Reach = 1000,
    }
}
