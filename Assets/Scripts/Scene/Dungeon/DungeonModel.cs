using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class DungeonModel : BaseModel
    {
        public DungeonModel()
        {
        }

        public List<UnitInfo> PartyUnit()
        {
            return CurrentDeckInfo.UnitInfos;
        }

        public void CommandMoveEnd(UnityEngine.Vector2Int position)
        {
            if (CurrentDeckInfo == null)
            {
                return;
            }
            var lastPosition = CurrentDeckInfo.Position;
            if (lastPosition != position)
            {
                CurrentDeckInfo.SetPosition(position);
                // ランダムエンカウントフラグ加算
                int flag = UnityEngine.Random.Range(5, 15);
                CurrentDeckInfo.Encount.GainValue(flag,0,100);

                // 残りターン数を減算
                CurrentStage.TurnCount.GainValue(-1);
            }
        }

        public bool EncountEnemy()
        {
            if (CurrentDeckInfo == null)
            {
                return false;
            }
            return CurrentDeckInfo.Encount.Value >= 100;
        }

        public void ResetEncountValue()
        {
            if (CurrentDeckInfo == null)
            {
                return;
            }
            CurrentDeckInfo.Encount.SetValue(0);
        }

        public List<UnitInfo> RandumTroopInfos()
        {
            var list = new List<UnitInfo>();
            var troopInfo = new TroopInfo(-1);
            troopInfo.MakeEnemyRandomTroopDates(1,CurrentStage.Master.RandomTroopEnemyRates);
            
            for (int i = 101;i <= 103;i++)
            {
                var unitInfo = new UnitInfo();
                unitInfo.Index.SetValue(i-100);
                list.Add(unitInfo);
            }
            
            for (int i = 101;i <= 103;i++)
            {
                var unitInfo = list.Find(a => a.Index.Value == i - 100);
                var front = troopInfo.BattlerInfos.Find(a => a != null && a.Index.Value == i);
                var back = troopInfo.BattlerInfos.Find(a => a != null && a.Index.Value == (i + 3));
                var battlerInfos = new List<BattlerInfo>(){front,back};
                unitInfo.SetBattlers(battlerInfos.FindAll(a => a != null));
            }
            return list.FindAll(a => a.BattlerInfos.Count > 0);
        }

        public TroopInfo RandumTroopInfo()
        {
            var troopInfo = new TroopInfo(-1);
            troopInfo.MakeEnemyRandomTroopDates(1,CurrentStage.Master.RandomTroopEnemyRates);
            return troopInfo;
        }

        public List<SystemData.CommandData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var status = new SystemData.CommandData
            {
                Id = 1,
                Name = "メンバー確認",
                Key = "Status"
            };
            list.Add(status);
            var @return = new SystemData.CommandData
            {
                Id = 1,
                Name = "帰還する",
                Key = "Return"
            };
            list.Add(@return);
            var option = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(13410),
                Key = "Option"
            };
            list.Add(option);
            var menuCommand = new SystemData.CommandData
            {
                Id = 3,
                Name = DataSystem.GetText(19700),
                Key = "Help"
            };
            list.Add(menuCommand);
            var dictionaryCommand = new SystemData.CommandData
            {
                Id = 11,
                Name = DataSystem.GetText(19730),
                Key = "Dictionary"
            };
            list.Add(dictionaryCommand);
            var saveCommand = new SystemData.CommandData
            {
                Id = 4,
                Name = DataSystem.GetText(19710),
                Key = "Save"
            };
            list.Add(saveCommand);
            var titleCommand = new SystemData.CommandData
            {
                Id = 5,
                Name = DataSystem.GetText(19720),
                Key = "Title"
            };
            list.Add(titleCommand);
            return list;
        }
    }
}