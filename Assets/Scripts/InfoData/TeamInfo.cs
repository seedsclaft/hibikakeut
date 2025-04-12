using System;
using System.Collections.Generic;
using System.Numerics;

namespace Ryneus
{
    [Serializable]
    public class TeamInfo
    {
        public ParameterInt TeamId = new();
        // 所持ユニット
        private List<HexUnitInfo> _unitInfos = new();
        public List<HexUnitInfo> UnitInfos => _unitInfos;
        public List<HexUnitInfo> GetOnFieldUnitInfos(int x,int y)
        {
            return _unitInfos.FindAll(a => a.OnField(x,y));
        }
        public void AddUnitInfos(HexUnitInfo unitInfo)
        {
            _unitInfos.Add(unitInfo);
        }
        public void RemoveUnitInfos(HexUnitInfo unitInfo)
        {
            if (_unitInfos.Contains(unitInfo))
            {
                _unitInfos.Remove(unitInfo);
            }
        }

        // 行動可能回数
        public ParameterInt ActPoint = new(1);
        // 残り行動回数
        public ParameterInt CurrentActPoint = new(1);
        // 最後に選択していたマス
        public ParameterInt LastSelectHexX = new(0);
        public ParameterInt LastSelectHexY = new(0);
        public void SetLastSelectHex(int x,int y)
        {
            LastSelectHexX.SetValue(x);
            LastSelectHexY.SetValue(y);
        }

        /// <summary>
        /// オート行動状態を取得
        /// </summary>
        /// <returns></returns>
        public TeamState GetTeamState()
        {
            if (CurrentActPoint.Value > 0)
            {
                return TeamState.MoveBattler;
            }
            return TeamState.TurnEnd;
        }

        /// <summary>
        /// 行動可能なユニットを取得
        /// </summary>
        /// <returns></returns>
        public HexUnitInfo GetMoveBattlerUnit()
        {
            if (_unitInfos.Count > 0)
            {
                return _unitInfos[0];
            }
            return null;
        }

        /// <summary>
        /// バトル結果と同期
        /// </summary>
        /// <param name="actorInfo"></param>
        public void UpdateUnitStatus(BattlerInfo battlerInfo)
        {
            var unit = _unitInfos.Find(a => a.UnitInfo.BattlerInfos.Find(b => b.Index == battlerInfo.Index) != null);
            if (unit != null)
            {
                foreach (var enemy in unit.UnitInfo.BattlerInfos)
                {
                    if (enemy.Index.Value == battlerInfo.Index.Value)
                    {
                        enemy.SetHp(battlerInfo.Hp.Value);
                    }
                }
            }
        }
    }

    public enum TeamState
    {
        None = 0,
        MoveBattler = 1, // 行動可能
        TurnEnd = 99 // 行動終了
    }    
}
