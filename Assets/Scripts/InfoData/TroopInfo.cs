using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class TroopInfo 
    {
        public TroopData TroopMaster => DataSystem.Troops.Find(a => a.TroopId == _troopId);
        private int _troopId = 0;
        private List<BattlerInfo> _battlerInfos = new(); 
        public List<BattlerInfo> BattlerInfos => _battlerInfos;
        public BattlerInfo BossEnemy 
        {
            get 
            {
                var boss = _battlerInfos.Find(a => a.BossFlag == true);
                if (boss != null) return boss;
                if (_battlerInfos.Count > 0)
                {
                    return _battlerInfos[_battlerInfos.Count-1];
                }
                return null;
            }
        }
        private List<GetItemInfo> _getItemInfos = new (); 
        public List<GetItemInfo> GetItemInfos => _getItemInfos;

        // リプレイを保存するか
        public bool NeedReplayData => _troopId != -1;
        public TroopInfo(int troopId)
        {
            _troopId = troopId;
            _battlerInfos.Clear();
            _getItemInfos.Clear();
        }

        public void MakeEnemyTroopDates(int plusLevel)
        {
            // EnemyIndex割り振り
            var enemyIndexKeys = new Dictionary<int,int>();
            foreach (var troopEnemies in TroopMaster.TroopEnemies)
            {
                if (troopEnemies.StageLv <= plusLevel)
                {
                    var enemyData = DataSystem.Enemies.Find(a => a.Id == troopEnemies.EnemyId);
                    var battlerInfo = new BattlerInfo(enemyData,troopEnemies.Lv + plusLevel,_battlerInfos.Count,troopEnemies.Line,troopEnemies.BossFlag);
                    AddEnemy(battlerInfo);
                    if (!enemyIndexKeys.ContainsKey(enemyData.Id))
                    {
                        enemyIndexKeys[enemyData.Id] = 0;
                    }
                    battlerInfo.EnemyIndex.SetValue(enemyIndexKeys[enemyData.Id]);
                    enemyIndexKeys[enemyData.Id]++;
                }
            }
        }

        public void MakeEnemyRandomTroopDates(int baseLv,List<StageEnemyRate> stageEnemyRates)
        {
            var randMax = MathF.Min(3,baseLv / 15);
            var weight = 30;
            while (weight > 0)
            {
                var targetIdRand = UnityEngine.Random.Range(0,stageEnemyRates.Count);
                var enemyData = DataSystem.Enemies.Find(a => a.Id == stageEnemyRates[targetIdRand].EnemyId);
                /*
                var lineRand = UnityEngine.Random.Range(0,1);
                // 遠隔持っていない場合は前列
                if (!enemyData.Kinds.Contains(KindType.Air) && lineRand == 1)
                {
                    lineRand = 0;
                }
                */
                var battlerInfo = new BattlerInfo(enemyData,baseLv,_battlerInfos.Count,LineType.Front,_battlerInfos.Count == 0);
                AddEnemy(battlerInfo);
                weight -= stageEnemyRates[targetIdRand].Weight;
            }
        }

        public void AddEnemy(BattlerInfo battlerInfo)
        {
            _battlerInfos.Add(battlerInfo);
        }
        
        public void AddGetItemInfo(GetItemInfo getItemInfo)
        {
            _getItemInfos.Add(getItemInfo);
        }
    }
}