using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [System.Serializable]
    public class DeckInfo
    {
        public DeckInfo()
        {
            InitUnitInfos();
        }

        public ParameterInt Index = new();
        // 現在位置
        public ParameterInt StageId = new();
        public ParameterInt PositionX = new();
        public ParameterInt PositionY = new();
        public void SetPosition(int x,int y)
        {
            PositionX.SetValue(x);
            PositionY.SetValue(y);
        }

        // ランダムエンカウント値
        public ParameterInt Encount = new();

        // 所持ユニット
        private List<UnitInfo> _unitInfos = new();
        public List<UnitInfo> UnitInfos => _unitInfos;

        private void InitUnitInfos()
        {
            _unitInfos.Clear();
        }

        public void AddUnitInfos(UnitInfo unitInfo)
        {
            _unitInfos.Add(unitInfo);
        }

        public void UpdateBattlerInfo(BattlerInfo battlerInfo)
        {
            var find = _unitInfos.Find(a => a.BattlerInfos.Find(b => b.Index.Value == battlerInfo.Index.Value) != null);
            if (find != null)
            {
                find.UpdateBattlerInfo(battlerInfo);
            }
        }

    }
}
