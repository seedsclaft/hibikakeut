using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class TeamInfo
    {
        public ParameterInt TeamId = new();
        // 所持ユニット
        [SerializeField] private List<HexUnitInfo> _unitInfos = new();
        public List<HexUnitInfo> UnitInfos => _unitInfos;
        public List<HexUnitInfo> GetUnitInfos()
        {
            return _unitInfos;
        }
        public void AddUnitInfos(HexUnitInfo unitInfo)
        {
            _unitInfos.Add(unitInfo);
        }

        // 行動可能回数
        public ParameterInt ActPoint = new(1);
        // 残り行動回数
        public ParameterInt CurrentActPoint = new(1);

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
    }

    public enum TeamState
    {
        None = 0,
        MoveBattler = 1, // 行動可能
        TurnEnd = 99 // 行動終了
    }    
}
