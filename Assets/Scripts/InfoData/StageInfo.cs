using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class StageInfo
    {
        public StageData Master => DataSystem.FindStage(StageId.Value);
        public ParameterInt StageId = new();
        public ParameterBool Cleared = new();
        public ParameterBool Alarted = new();
        public StageInfo(int id, bool cleared = false, bool alarted = false)
        {
            StageId.SetValue(id);
            Cleared.SetValue(cleared);
            Alarted.SetValue(alarted);
        }

        public EnemyData BossEnemyData()
        {
            var enemyId = 0;
            var troop = DataSystem.Troops.Find(a => a.TroopId == Master.BossTroopId);
            if (troop != null)
            {
                var boss = troop.TroopEnemies.Find(a => a.BossFlag);
                if (boss != null)
                {
                    enemyId = boss.EnemyId;
                }

                if (troop.TroopEnemies.Count > 0)
                {
                    enemyId = troop.TroopEnemies[troop.TroopEnemies.Count - 1].EnemyId;
                }
            }
            return DataSystem.Enemies.Find(a => a.Id == enemyId);
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
            var troop = DataSystem.Troops.Find(a => a.TroopId == Master.BossTroopId);
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
    }
}