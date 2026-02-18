using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class UnitInfo
    {
        public ParameterInt Index = new();
        [UnityEngine.SerializeField] private List<BattlerInfo> _battlerInfos = new();
        public List<BattlerInfo> BattlerInfos => _battlerInfos;
        public List<BattlerInfo> AliveBattlerInfos => _battlerInfos.FindAll(a => a != null && a.IsAlive());

        public UnitInfo()
        {
        }

        public void SetBattlers(List<BattlerInfo> battlerInfos)
        {
            _battlerInfos = battlerInfos;
            SortBattlerInfos();
        }

        public void AddBattlerInfo(BattlerInfo battlerInfos)
        {
            var findIndex = _battlerInfos.FindIndex(a => a.ActorInfo.ActorId.Value == battlerInfos.ActorInfo.ActorId.Value);
            if (findIndex == -1)
            {
                _battlerInfos.Add(battlerInfos);
            }
            SortBattlerInfos();
        }

        public void RemoveBattlerInfo(BattlerInfo battlerInfos)
        {
            var findIndex = _battlerInfos.FindIndex(a => a.ActorInfo.ActorId.Value == battlerInfos.ActorInfo.ActorId.Value);
            if (findIndex > -1)
            {
                _battlerInfos.RemoveAt(findIndex);
            }
        }

        public void SortBattlerInfos()
        {
            _battlerInfos.Sort((a, b) => a.Index.Value - b.Index.Value > 0 ? 1 : -1);
        }

        public void UpdateBattlerInfo(BattlerInfo battlerInfo)
        {
            var findIndex = _battlerInfos.FindIndex(a => a.ActorInfo.ActorId.Value == battlerInfo.ActorInfo.ActorId.Value);
            if (findIndex > -1)
            {
                _battlerInfos[findIndex] = battlerInfo;
            }
        }

        public List<BattlerInfo> FrontBattlers()
        {
            // 最前列は
            if (IsFrontAlive())
            {
                return _battlerInfos.FindAll(a => a.LineIndex == LineType.Front);
            }
            return _battlerInfos;
        }

        private bool IsFrontAlive()
        {
            // 最前列は
            return AliveBattlerInfos.Find(a => a.LineIndex == LineType.Front) != null;
        }

        public int TotalEvaluate()
        {
            var evaluate = 0;
            foreach (var battlerInfo in _battlerInfos)
            {
                evaluate += battlerInfo.Evaluate();
            }
            return evaluate;
        }

        // 戦略用
        public BattlerInfo FrontBattlerInfo()
        {
            if (_battlerInfos.Count <= 0)
            {
                return null;
            }
            if (_battlerInfos[0].Index.Value == 0)
            {
                return null;
            }
            return _battlerInfos[0];
        }

        public BattlerInfo BackBattlerInfo()
        {
            if (_battlerInfos.Count <= 1)
            {
                return null;
            }
            if (_battlerInfos[1].Index.Value == 0)
            {
                return null;
            }
            return _battlerInfos[1];
        }

        public bool DeadWithoutSelf(BattlerInfo battlerInfo)
        {
            if (_battlerInfos.Find(a => a.Index.Value == battlerInfo.Index.Value) == null)
            {
                return false;
            }
            if (_battlerInfos.Count == 1)
            {
                return false;
            }
            return AliveBattlerInfos.Count == 1;
        }

        public List<BattlerInfo> CoverableBattlerInfo(BattlerInfo target)
        {
            var coverableBattlerInfos = AliveBattlerInfos.FindAll(a => a != target);
            return coverableBattlerInfos;
        }
    }
}