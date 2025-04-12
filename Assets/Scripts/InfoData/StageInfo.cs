using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    [Serializable]
    public class StageInfo
    {
        public StageData Master => DataSystem.FindStage(StageId.Value);
        public ParameterInt TurnCount = new(1);
        
        private List<TeamInfo> _teamInfos = new();
        public List<TeamInfo> TeamInfos => _teamInfos;
        public void AddTeamInfo(TeamInfo teamInfo)
        {
            _teamInfos.Add(teamInfo);
        }
        public TeamInfo GetTurnTeamInfo()
        {
            return _teamInfos.Find(a => a.TeamId.Value == TurnTeamId.Value);
        }
        public ParameterInt TurnTeamId = new();
        public TeamInfo HomeTeamInfo => _teamInfos.Find(a => a.TeamId.Value == (int)TeamIdType.Home);
        public TeamInfo AwayTeamInfo => _teamInfos.Find(a => a.TeamId.Value == (int)TeamIdType.Away);
        private List<HexUnitInfo> _hexUnitList = new();
        public List<HexUnitInfo> HexUnitList => _hexUnitList;
        public List<HexUnitInfo> TurnHexUnitList()
        {
            var findTeam = _teamInfos.Find(a => a.TeamId.Value == TurnTeamId.Value);
            if (findTeam != null)
            {
                return findTeam.GetUnitInfos();
            }
            return null;
        }

        public List<HexUnitInfo> BattleHexUnitList()
        {
            var findTeam = _teamInfos.Find(a => a.TeamId.Value != TurnTeamId.Value);
            if (findTeam != null)
            {
                return findTeam.GetUnitInfos();
            }
            return null;
        }
        public void AddHexUnitInfo(HexUnitInfo hexUnit) => _hexUnitList.Add(hexUnit);
        public void AddHexUnitInfos(List<HexUnitInfo> hexUnitList) => _hexUnitList.AddRange(hexUnitList);
        public void SetHexUnitInfos(List<HexUnitInfo> hexUnitList) => _hexUnitList = hexUnitList;

        public void RemoveReachUnitInfo(List<HexField> hexFields)
        {
            for (int i= _hexUnitList.Count-1;i >= 0;i--)
            {
                if (_hexUnitList[i].HexUnitType == HexUnitType.Reach || _hexUnitList[i].HexUnitType == HexUnitType.ReachAttack)
                {
                    if (hexFields.Find(a => a.X == _hexUnitList[i].HexField.X && a.Y == _hexUnitList[i].HexField.Y) != null)
                    {
                        _hexUnitList.Remove(_hexUnitList[i]);
                    }
                }
            }
        }

        public void LostUnitInfos(List<HexUnitInfo> unitInfos)
        {
            for (int i= _hexUnitList.Count-1;i >= 0;i--)
            {
                if (unitInfos.Contains(_hexUnitList[i]))
                {
                    _hexUnitList.Remove(_hexUnitList[i]);
                }
            }
            foreach (var unitInfo in unitInfos)
            {
                foreach (var teamInfo in _teamInfos)
                {
                    teamInfo.RemoveUnitInfos(unitInfo);
                }
            }
        }
        
        public ParameterInt StageId = new();


        private int _loseCount = 0;
        public int LoseCount => _loseCount;
        public void GainLoseCount(){ _loseCount++;}

        public StageInfo(int id)
        {
            StageId.SetValue(id);
            TurnTeamId.SetValue((int)TeamIdType.Home);
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
            return troopInfo;
            
            //_stageSymbolInfos.Add(symbolInfo);
        }


        public int SelectActorIdsClassId(int selectIndex)
        {
            return 0;
        }
    }
}