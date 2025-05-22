using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class StageInfo
    {
        public StageData Master => DataSystem.FindStage(StageId.Value);
        public ParameterInt StageId = new();
        public ParameterInt FieldX = new();
        public ParameterInt FieldY = new();
        public ParameterInt TurnCount = new();
        public ParameterBool CheckedStageStart = new();
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
            list.AddRange(AllUnitInfos().FindAll(a => a.OnField(x,y)));
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

        public List<HexUnitInfo> AllFieldUnitInfos()
        {
            var list = new List<HexUnitInfo>();
            list.AddRange(_fieldHexList);
            list.AddRange(AllUnitInfos());
            return list;
        }

        public List<HexUnitInfo> AllOnFieldUnitInfos()
        {
            var list = new List<HexUnitInfo>();
            list.AddRange(_fieldHexList.FindAll(a => a.OnField(FieldX.Value,FieldY.Value)));
            list.AddRange(AllUnitInfos().FindAll(a => a.OnField(FieldX.Value,FieldY.Value)));
            return list;
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
            foreach (var hexField in hexFields)
            {
                var findAll = _fieldHexList.FindAll(a => a.IsReachUnit && a.HexField.X == hexField.X && a.HexField.Y == hexField.Y);
                for (int i = findAll.Count-1;i >= 0;i--)
                {
                    _fieldHexList.Remove(findAll[i]);
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
            SetFieldPosition(Master.InitX,Master.InitY);
        }

        public void SetFieldPosition(int x,int y)
        {
            FieldX.SetValue(x);
            FieldY.SetValue(y);
        }

        public void SetFieldPosition(HexField hexField)
        {
            SetFieldPosition(hexField.X,hexField.Y);
        }

        public bool CheckVictory()
        {
            var achieveType = Master.AchieveType;
            switch (achieveType)
            {
                case AchieveType.ConquerEnemyBasement:
                    var basement = _fieldHexList.Find(a => a.TeamId.Value == (int)TeamIdType.Home && a.Id.Value == Master.EnemyBasementId && a.IsBasementUnit);
                    return basement != null;
            }
            return false;
        }

        public bool CheckGameOver()
        {
            // 拠点が0で部隊が0
            var basement = _fieldHexList.Find(a => a.IsBasementUnit && a.TeamId.Value == (int)TeamIdType.Home);
            var fieldUnit = HomeTeamInfo.UnitInfos.Find(a => a.IsUnit && !a.IsLostUnit());
            if (basement == null)
            {
                return fieldUnit == null;
            }
            // 拠点が1で部隊と出撃可能が0
            var depatureActorInfos = GameSystem.GameInfo.PartyInfo.ActorInfos.FindAll(a => !a.Lost.Value);
            return (depatureActorInfos.Count == 0) && fieldUnit == null;
        }

        /// <summary>
        /// 制圧してマップを開放
        /// </summary>
        /// <param name="stageId"></param>
        public void AddStageSymbolDates(List<HexUnitInfo> openUnitInfos)
        {
            // マップを開放
            var newFields = new List<HexUnitInfo>();
            // 既存ユニットを保持
            newFields.AddRange(FieldHexList.FindAll(a => a.HexUnitType != HexUnitType.None));
            // 壁を開放
            var walls = FieldHexList.FindAll(a => a.HexUnitType == HexUnitType.None);
            var openFields = openUnitInfos.FindAll(a => a.HexUnitType == HexUnitType.None);
            foreach (var wall in walls)
            {
                if (openFields.Find(a => a.OnField(wall.HexField)) != null)
                {
                    newFields.Add(wall);
                }
            }
            SetHexUnitInfos(newFields);
            // 新規ユニットを追加
            AddHexUnitInfos(openUnitInfos.FindAll(a => a.HexUnitType != HexUnitType.None && !a.IsUnit));
            // 敵ユニットを登録
            var oppnents = openUnitInfos.FindAll(a => a.IsBattlerUnit);
            foreach (var oppnent in oppnents)
            {
                AwayTeamInfo.AddUnitInfos(oppnent);
            }
            // 敵は敵部隊数分1回ずつ行動可能
            var awayActPoint = AwayTeamInfo.UnitInfos.FindAll(a => a.IsUnit && a.IsFriend(AwayTeamInfo.TeamId.Value));
            AwayTeamInfo.SetActPoint(awayActPoint.Count - 1);
        }
    }
}