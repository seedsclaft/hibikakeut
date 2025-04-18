using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    [Serializable]
    public class StageInfo
    {
        public StageData Master => DataSystem.FindStage(StageId.Value);
        public ParameterInt StageId = new();
        public ParameterInt FieldX = new();
        public ParameterInt FieldY = new();
        public ParameterInt TurnCount = new(1);
        public ParameterBool CheckedTurnStart = new();
        
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
        public List<HexUnitInfo> FindUnitInfos(int x,int y) => _hexUnitList.FindAll(a => a.OnField(x,y));
        public List<HexUnitInfo> OnFieldUnitInfos => FindUnitInfos(FieldX.Value,FieldY.Value);
        public List<HexUnitInfo> TurnHexUnitList()
        {
            var findTeam = _teamInfos.Find(a => a.TeamId.Value == TurnTeamId.Value);
            if (findTeam != null)
            {
                return findTeam.GetOnFieldUnitInfos(FieldX.Value,FieldY.Value);
            }
            return null;
        }

        public List<HexUnitInfo> FriendUnitInfos()
        {
            var findTeam = _teamInfos.Find(a => a.TeamId.Value == TurnTeamId.Value);
            if (findTeam != null)
            {
                return findTeam.UnitInfos;
            }
            return null;
        }

        public List<HexUnitInfo> OpponentUnitInfos()
        {
            var findTeam = _teamInfos.Find(a => a.TeamId.Value != TurnTeamId.Value);
            if (findTeam != null)
            {
                return findTeam.UnitInfos;
            }
            return null;
        }

        public void AddHexUnitInfo(HexUnitInfo hexUnit) => _hexUnitList.Add(hexUnit);
        public void AddHexUnitInfos(List<HexUnitInfo> hexUnitList) => _hexUnitList.AddRange(hexUnitList);
        public void SetHexUnitInfos(List<HexUnitInfo> hexUnitList) => _hexUnitList = hexUnitList;
        public void RemoveHexUnitInfo(HexUnitInfo hexUnit) => _hexUnitList.Remove(hexUnit);

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

        private int _loseCount = 0;
        public int LoseCount => _loseCount;
        public void GainLoseCount(){ _loseCount++;}

        public StageInfo(int id)
        {
            StageId.SetValue(id);
            TurnTeamId.SetValue((int)TeamIdType.Home);
            FieldX.SetValue(Master.InitX);
            FieldY.SetValue(Master.InitY);
        }

        public bool CheckVictory()
        {
            var achieveType = Master.AchieveType;
            switch (achieveType)
            {
                case AchieveType.ConquerEnemyBasement:
                    var basement = _hexUnitList.Find(a => a.Id.Value == Master.EnemyBasementId);
                    return basement?.TeamId.Value != (int)TeamIdType.Away;
            }
            return false;
        }    
        
        public bool CheckGameOver()
        {
            // 拠点が0で部隊が0
            var basement = _hexUnitList.Find(a => a.HexUnitType == HexUnitType.Basement && a.TeamId.Value == (int)TeamIdType.Home);
            var fieldUnit = HomeTeamInfo.UnitInfos.Find(a => a.IsUnit);
            if (basement == null)
            {
                return fieldUnit == null;
            }
            // 拠点が1で部隊と出撃可能が0
            var depatureUnit = FriendUnitInfos().FindAll(a => !a.IsLostUnit());
            return (depatureUnit.Count == 0) && fieldUnit == null;
        }
    }
}