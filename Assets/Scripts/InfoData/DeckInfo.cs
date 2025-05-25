using System.Collections.Generic;
using System.Linq;
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

        // 編成情報
        private Dictionary<int,int> _actorIdDict = new();
        public Dictionary<int,int> ActorIdDict => _actorIdDict;

        public void SwapBattler(int fromEditIndex,int toActorId)
        {
            var toEditIndex = FindEditIndex(toActorId);
            _actorIdDict[fromEditIndex] = toActorId;
            if (toEditIndex != -1)
            {
                _actorIdDict[toEditIndex] = fromEditIndex;
            }
        }

        public int FindEditIndex(int actorId)
        {
            foreach (var actorIdDict in _actorIdDict)
            {
                if (actorIdDict.Value == actorId)
                {
                    return actorIdDict.Key;
                }
            }
            return -1;
        }

        // 所持ユニット
        //private List<UnitInfo> _unitInfos = new();
        //public List<UnitInfo> UnitInfos => _unitInfos;

        private void InitUnitInfos()
        {
            for (int i = 1;i <= 6;i++)
            {
                _actorIdDict[i] = -1;
            }
            _actorIdDict[1] = 1;
        }

        public void AddUnitInfos(UnitInfo unitInfo)
        {
        }

        public void UpdateBattlerInfo(BattlerInfo battlerInfo)
        {
            /*
            var find = _unitInfos.Find(a => a.BattlerInfos.Find(b => b.Index.Value == battlerInfo.Index.Value) != null);
            if (find != null)
            {
                find.UpdateBattlerInfo(battlerInfo);
            }
            */
        }

    }
}
