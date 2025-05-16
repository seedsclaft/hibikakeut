using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class HexUnitInfo
    {
        private StageSymbolData _master;
        public StageSymbolData Master => _master;
        public ParameterInt Id = new();
        [UnityEngine.SerializeField] private HexField _hexField = new();
        public HexField HexField => _hexField;

        public ParameterInt Index = new();
        public ParameterInt TeamId = new();

        public bool IsUnit => _hexUnitType == HexUnitType.Battler;
        public bool IsWall => _hexUnitType == HexUnitType.None;
        public bool IsSelectArea => _hexUnitType == HexUnitType.Reach;
        public bool IsAttackableArea => _hexUnitType == HexUnitType.ReachAttack;
        public bool IsBasementUnit => _hexUnitType == HexUnitType.Basement;
        public bool IsBattlerUnit => _hexUnitType == HexUnitType.Battler;
        public bool IsAlcanaUnit => _hexUnitType == HexUnitType.Alcana;
        public bool IsGetItemUnit => _hexUnitType == HexUnitType.GetItem;
        public bool IsSelectActorUnit => _hexUnitType == HexUnitType.SelectActor;
        public bool IsGachaUnit => _hexUnitType == HexUnitType.Gacha;
        public bool IsReachUnit => _hexUnitType == HexUnitType.Reach || _hexUnitType == HexUnitType.ReachAttack;


        [UnityEngine.SerializeField] private HexUnitType _hexUnitType = HexUnitType.None;
        public HexUnitType HexUnitType => _hexUnitType;
        public void SetHexUnitType(HexUnitType hexUnitType) => _hexUnitType = hexUnitType;

        private UnitMoveType _hexMoveType = UnitMoveType.None;
        public UnitMoveType HexMoveType => _hexMoveType;
        private MoveTypeParam _hexMoveParam = null;
        public MoveTypeParam HexMoveParam => _hexMoveParam;
        public void SetHexMoveType(UnitMoveType hexMoveType,MoveTypeParam param)
        {
            _hexMoveType = hexMoveType;
            _hexMoveParam = param;
        }
        public void FilpMoveParamFlag()
        {
            _hexMoveParam.Flag = !_hexMoveParam.Flag;
        }

        [UnityEngine.SerializeField] private List<GetItemInfo> _getItemInfos = new();
        public List<GetItemInfo> GetItemInfos => _getItemInfos;
        public void AddGetItemInfos(List<GetItemInfo> getItemInfos)
        {
            _getItemInfos.AddRange(getItemInfos);
        }

        [UnityEngine.SerializeField] private UnitInfo _unitInfo = null;
        public UnitInfo UnitInfo => _unitInfo;
        public void SetUnitInfo(UnitInfo unitInfo)
        {
            _unitInfo = unitInfo;
        }

        public HexUnitInfo(int index,StageSymbolData stageSymbolData,int teamId = 0)
        {
            _master = stageSymbolData;
            Id.SetValue(stageSymbolData.Id);
            Index.SetValue(index);
            SetHexUnitType(stageSymbolData.UnitType);
            SetHexMoveType(stageSymbolData.MoveType,stageSymbolData.MoveTypeParam);
            SetPosition(stageSymbolData.InitX,stageSymbolData.InitY);
            TeamId.SetValue(teamId);
        }

        public bool OnField(int x,int y)
        {
            return _hexField.X == x && _hexField.Y == y;
        }

        public bool OnField(HexField hexField)
        {
            return _hexField.X == hexField.X && _hexField.Y == hexField.Y;
        }

        public bool IsFriend(int teamId)
        {
            return TeamId.Value == teamId;
        }

        public bool IsHomeUnit()
        {
            return TeamId.Value == (int)TeamIdType.Home;
        }

        public void SetPosition(int x,int y)
        {
            _hexField.X = x;
            _hexField.Y = y;
        }

        public int GetUnitMov()
        {
            if (IsBattlerUnit)
            {
                return UnitInfo.CurrentMov();
            }
            return 0;
        }

        public void SetBattlerIndex(int index)
        {
            if (_unitInfo.BattlerInfos == null)
            {
                return;
            }
            var battleIndex = index;
            if (TeamId.Value == (int)TeamIdType.Away)
            {
                battleIndex += 100;
            }

            _unitInfo.SetLineIndexes(battleIndex);
        }

        /// <summary>
        /// バトル終了時の隊列データに変更する
        /// </summary>
        public void UpdateBattlerIndexes()
        {
            _unitInfo.BattleEndSetLineIndexes();
        }

        public bool IsLostUnit()
        {
            if (!IsBattlerUnit)
            {
                return false;
            }
            if (_unitInfo.BattlerInfos.Find(a => a.Hp.Value > 0) == null)
            {
                return true;
            }
            return false;
        }

        public void Conquer(int teamIdType)
        {
            TeamId.SetValue(teamIdType);
        }

        public string FieldText()
        {
            switch (_hexUnitType)
            {
                case HexUnitType.Basement:
                    return "拠点：\nターン開始時に回復";
                case HexUnitType.Alcana:
                    return "？？？：\nランダムなイベントが発生";
                case HexUnitType.GetItem:
                    return "宝箱：\nアイテムを入手";
                case HexUnitType.SelectActor:
                    return "召喚：\n仲間を任意選択して加入";
                case HexUnitType.Gacha:
                    return "奇跡：\n様々な恩恵を受ける";
            }
            return "";
        }

        public int BattleEvaluate()
        {
            if (_unitInfo != null)
            {
                var evaluate = 0;
                foreach (var battlerInfo in _unitInfo.BattlerInfos)
                {
                    evaluate += battlerInfo.Evaluate();
                }
                return evaluate;
            }
            return 0;
        }
    }
}
