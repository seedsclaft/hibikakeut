using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    [Serializable]
    public class StageInfo
    {
        public StageData Master => DataSystem.FindStage(StageId.Value);

        private List<SymbolInfo> _symbolInfos = new();
        public List<SymbolInfo> SymbolInfos => _symbolInfos;
        public void SetSymbolInfos(List<SymbolInfo> symbolInfos) => _symbolInfos = symbolInfos;
        
        private List<HexUnitInfo> _hexUnitList = new();
        public List<HexUnitInfo> HexUnitList => _hexUnitList;
        public void SetHexUnitInfos(List<HexUnitInfo> hexUnitList) => _hexUnitList = hexUnitList;
        
        
        public ParameterInt StageId = new();
        public int EndSeek => 10;//_symbolInfos.Max(a => a.Master.InitX);

        public ParameterInt SeekIndex = new(-1);

        private int _loseCount = 0;
        public int LoseCount => _loseCount;
        public void GainLoseCount(){ _loseCount++;}

        public StageInfo(int id)
        {
            StageId.SetValue(id);
        }
        
        public TroopInfo TestTroops(int troopId,int troopLv)
        {
            var troopDate = DataSystem.Troops.Find(a => a.TroopId == troopId);
            
            var troopInfo = new TroopInfo(troopDate.TroopId);
            for (int i = 0;i < troopDate.TroopEnemies.Count;i++)
            {
                var enemyData = DataSystem.Enemies.Find(a => a.Id == troopDate.TroopEnemies[i].EnemyId);
                bool isBoss = troopDate.TroopEnemies[i].BossFlag;
                var enemy = new BattlerInfo(enemyData,troopDate.TroopEnemies[i].Lv + troopLv - 1,i,troopDate.TroopEnemies[i].Line,isBoss);
                troopInfo.AddEnemy(enemy);
            }
            SeekIndex.SetValue(0);
            return troopInfo;
            
            //_stageSymbolInfos.Add(symbolInfo);
        }


        public int SelectActorIdsClassId(int selectIndex)
        {
            return 0;
        }
    }
}