using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class StageInfo
    {
        public StageData _master = null;
        public StageData Master => DataSystem.FindStage(StageId.Value);
        public ParameterInt StageId = new();
        public ParameterBool Cleared = new();
        public ParameterBool Alarted = new();
        public StageInfo(int id, bool cleared = false, bool alarted = false)
        {
            StageId.SetValue(id);
            _master = Master;
            Cleared.SetValue(cleared);
            Alarted.SetValue(alarted);
        }

        public EnemyData BossEnemyData()
        {
            var enemyId = 0;
            var troop = DataSystem.FindTroop(Master.BossTroopId);
            if (troop != null)
            {
                var boss = troop.TroopEnemies.Find(a => a.BossFlag);
                if (boss != null)
                {
                    enemyId = boss.EnemyId;
                }

                if (enemyId == 0 && troop.TroopEnemies.Count > 0)
                {
                    enemyId = troop.TroopEnemies[troop.TroopEnemies.Count - 1].EnemyId;
                }
            }
            return DataSystem.FindEnemy(enemyId);
        }

        public string BossImage()
        {
            var bossEnemy = BossEnemyData();
            if (bossEnemy != null)
            {
                return bossEnemy.ImagePath;
            }
            return "";
        }

        public int BossLv()
        {
            var troop = DataSystem.FindTroop(Master.BossTroopId);
            var boss = troop.TroopEnemies.Find(a => a.BossFlag);
            if (boss != null)
            {
                return boss.Lv;
            }

            if (troop.TroopEnemies.Count > 0)
            {
                return troop.TroopEnemies[troop.TroopEnemies.Count - 1].Lv;
            }
            return 1;
        }

        public bool CheckVictory()
        {
            return false;
        }

        public bool CheckGameOver()
        {
            return false;
        }

        public List<StageEventData> GetStageEvents()
        {
            var dungeon = DataSystem.FindDungeonFloor(Master.Id);
            return dungeon.stageEvents;
        }

        public int DungeonEnemySymbolNum(List<string> readEventKeys)
        {
            return GetStageEvents().FindAll(a => !readEventKeys.Contains(a.EventKey) && (a.Type == StageEventType.ForceBattle || a.Type == StageEventType.ForceBossBattle)).Count;
        }
    }
}