using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

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
        private List<HexUnitInfo> _fieldHexList = new();
        public List<HexUnitInfo> FieldHexList => _fieldHexList;
        public List<HexUnitInfo> FindFieldInfos(int x,int y) => _fieldHexList.FindAll(a => a.OnField(x,y));
        public List<HexUnitInfo> FindFieldUnitInfos(int x,int y)
        {
            var list = new List<HexUnitInfo>();
            list.AddRange(_fieldHexList.FindAll(a => a.OnField(x,y)));
            list.AddRange(AllUnitInfos(true));
            return list;
        }

        public List<HexUnitInfo> OnFieldInfos => FindFieldInfos(FieldX.Value,FieldY.Value);
        public List<HexUnitInfo> OnFieldTurnUnitInfos()
        {
            var findTeam = _teamInfos.Find(a => a.TeamId.Value == TurnTeamId.Value);
            if (findTeam != null)
            {
                return findTeam.GetOnFieldUnitInfos(FieldX.Value,FieldY.Value);
            }
            return null;
        }

        public List<HexUnitInfo> AllUnitInfos(bool onFieldOnly = false)
        {
            var list = new List<HexUnitInfo>();
            foreach (var teamInfos in _teamInfos)
            {
                if (onFieldOnly)
                {
                    list.AddRange(teamInfos.GetOnFieldUnitInfos(FieldX.Value,FieldY.Value));
                } else
                {
                    list.AddRange(teamInfos.UnitInfos);
                }
            }
            return list;
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

        public void AddHexUnitInfo(HexUnitInfo hexUnit) => _fieldHexList.Add(hexUnit);
        public void AddHexUnitInfos(List<HexUnitInfo> hexUnitList) => _fieldHexList.AddRange(hexUnitList);
        public void SetHexUnitInfos(List<HexUnitInfo> hexUnitList) => _fieldHexList = hexUnitList;
        public void RemoveHexUnitInfo(HexUnitInfo hexUnit) => _fieldHexList.Remove(hexUnit);

        public void RemoveReachUnitInfo(List<HexField> hexFields)
        {
            for (int i= _fieldHexList.Count-1;i >= 0;i--)
            {
                if (_fieldHexList[i].HexUnitType == HexUnitType.Reach || _fieldHexList[i].HexUnitType == HexUnitType.ReachAttack)
                {
                    if (hexFields.Find(a => a.X == _fieldHexList[i].HexField.X && a.Y == _fieldHexList[i].HexField.Y) != null)
                    {
                        _fieldHexList.Remove(_fieldHexList[i]);
                    }
                }
            }
        }

        public List<HexUnitInfo> LostUnitInfos()
        {
            var list = new List<HexUnitInfo>();
            foreach (var teamInfo in _teamInfos)
            {
                list.AddRange(teamInfo.LostUnitInfos());
            }
            return list;
        }

        public void RemoveLostUnitInfos(List<HexUnitInfo> unitInfos)
        {
            foreach (var unitInfo in unitInfos)
            {
                foreach (var teamInfo in _teamInfos)
                {
                    teamInfo.RemoveUnitInfos(unitInfo);
                }
            }
        }

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
                    var basement = _fieldHexList.Find(a => a.Id.Value == Master.EnemyBasementId);
                    return basement?.TeamId.Value != (int)TeamIdType.Away;
            }
            return false;
        }    
        
        public bool CheckGameOver()
        {
            // 拠点が0で部隊が0
            var basement = _fieldHexList.Find(a => a.HexUnitType == HexUnitType.Basement && a.TeamId.Value == (int)TeamIdType.Home);
            var fieldUnit = HomeTeamInfo.UnitInfos.Find(a => a.IsUnit && !a.IsLostUnit());
            if (basement == null)
            {
                return fieldUnit == null;
            }
            // 拠点が1で部隊と出撃可能が0
            var depatureActorInfos = GameSystem.GameInfo.PartyInfo.ActorInfos.FindAll(a => !a.Lost.Value);
            return (depatureActorInfos.Count == 0) && fieldUnit == null;
        }
    }
}