using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    [System.Serializable]
    public class UnitInfo
    {
        public ParameterInt Index = new();
        [UnityEngine.SerializeField] private List<BattlerInfo> _battlerInfos = new();
        public List<BattlerInfo> BattlerInfos => _battlerInfos;
        public List<BattlerInfo> AliveBattlerInfos => _battlerInfos.FindAll(a => a != null && a.IsAlive());

        public void SetBattlers(List<BattlerInfo> battlerInfos)
        {
            _battlerInfos = battlerInfos;
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
            if (_battlerInfos.Count == 0)
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
            if (_battlerInfos.Count == 1)
            {
                return null;
            }
            if (_battlerInfos[1].Index.Value == 0)
            {
                return null;
            }
            return _battlerInfos[1];
        }

        //LineIndexを整列する
        public void SetLineIndexes(int battleIndex)
        {
            if (_battlerInfos.Count > 0 && _battlerInfos[0].Index.Value > 0)
            {
                _battlerInfos[0].Index.SetValue(battleIndex);
                _battlerInfos[0].SetLineIndex(LineType.Front);
            }
            if (_battlerInfos.Count > 1 && _battlerInfos[1].Index.Value > 0)
            {
                _battlerInfos[1].Index.SetValue(battleIndex+3);
                _battlerInfos[1].SetLineIndex(LineType.Back);
            }
        }

        /// バトル終了時の状態で整列する
        public void BattleEndSetLineIndexes()
        {
            if (_battlerInfos == null)
            {
                return;
            }

            if (_battlerInfos.Count > 1)
            {
                var frontBattler = _battlerInfos.Find(a => a.LineIndex == LineType.Front);
                var backBattler = _battlerInfos.Find(a => a.LineIndex == LineType.Back);
                _battlerInfos.Clear();
                _battlerInfos.Add(frontBattler);
                _battlerInfos.Add(backBattler);
            }
        }

        public int UnitMp()
        {
            return _battlerInfos.FindAll(a => a != null).Sum(a => a.MaxMp);
        }

        public int CurrentMov()
        {
            if (_battlerInfos.Count > 0 && _battlerInfos[0].Index.Value != 0)
            {
                return _battlerInfos[0].CurrentMov();
            }
            return 0;
        }

        public UnitInfo CopyData()
        {
            var copyData = new UnitInfo();
            copyData.Index.SetValue(Index.Value);
            var battlerInfos = new List<BattlerInfo>();
            var idx = 0;
            foreach (var battlerInfo in _battlerInfos)
            {
                if (battlerInfo.ActorInfo != null)
                {
                    var copyBattlerInfo = new BattlerInfo(battlerInfo.ActorInfo,idx+1);
                    battlerInfos.Add(copyBattlerInfo);
                } else
                if (battlerInfo.EnemyData != null)
                {
                    var copyBattlerInfo = new BattlerInfo(battlerInfo.EnemyData,battlerInfo.Level.Value,idx,battlerInfo.LineIndex,battlerInfo.BossFlag);
                    battlerInfos.Add(copyBattlerInfo);
                } else
                {
                    battlerInfos.Add(battlerInfo);
                }
                idx++;
            }
            copyData.SetBattlers(battlerInfos);
            return copyData;
        }
    }
}