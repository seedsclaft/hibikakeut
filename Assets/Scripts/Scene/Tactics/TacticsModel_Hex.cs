using System;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class TacticsModel : BaseModel
    {
        public ParameterInt FieldX = new();
        public ParameterInt FieldY = new();
        public List<HexUnitInfo> SelectableUnitInfos => CurrentStage.TurnHexUnitList()?.FindAll(a => a.HexField.X == FieldX.Value && a.HexField.Y == FieldY.Value);
        public List<HexUnitInfo> HexUnitInfos => CurrentStage.HexUnitList?.FindAll(a => a.HexField.X == FieldX.Value && a.HexField.Y == FieldY.Value);
        private HexRoute _hexRoute;
        private List<HexField> _reachAreas = new();
        private List<HexField> _movableAreas = new();
        private List<HexField> _attackAreas = new();
        private string _commandKey = "";
        public string CommandKey =>_commandKey;
        public void SetCommandKey(string key) => _commandKey = key;
        private ParameterInt _selectingHexUnitId = new();
        private int _departureActorId = -1;
        public void SetDepatureActorId(int departureActorId) => _departureActorId = departureActorId;
        
        // 行動選択中のチーム
        public bool IsPlayable => CurrentStage.TurnTeamId.Value == (int)TeamIdType.Home;
        public void UnitActEnd()
        {
            _selectingHexUnitId.SetValue(0);
            // 行動ポイントを減らす
            var term = CurrentStage.TeamInfos.Find(a => a.TeamId.Value == CurrentStage.TurnTeamId.Value);
            term.CurrentActPoint.GainValue(-1,0,term.ActPoint.Value);
            // 行動ポイントがなければ次へ
            if (term.CurrentActPoint.Value == 0)
            {
                TurnEnd();
            }
        }

        public void TurnEnd()
        {
            var nextId = CurrentStage.TurnTeamId.Value;
            if (nextId == (int)TeamIdType.None)
            {
                nextId = (int)TeamIdType.Home;
            } else
            if (nextId == (int)TeamIdType.Home)
            {
                nextId = (int)TeamIdType.Away;
            } else
            if (nextId == (int)TeamIdType.Away)
            {
                nextId = (int)TeamIdType.Home;
            }
            // 行動ポイントを初期化
            var term = CurrentStage.TeamInfos.Find(a => a.TeamId.Value == CurrentStage.TurnTeamId.Value);
            term.CurrentActPoint.SetValue(term.ActPoint.Value);
            CurrentStage.TurnTeamId.SetValue(nextId);
        }

        public TeamState GetTurnTeamState()
        {
            var teamInfo = CurrentStage.GetTurnTeamInfo();
            return teamInfo.GetTeamState();
        }

        /// <summary>
        /// 自動行動するユニットを選択
        /// </summary>
        /// <returns></returns>
        public bool SelectAutoMoveBattler()
        {
            var teamInfo = CurrentStage.GetTurnTeamInfo();
            var moveBattler = teamInfo.GetMoveBattlerUnit();
            if (moveBattler != null)
            {
                FieldX.SetValue(moveBattler.HexField.X);
                FieldY.SetValue(moveBattler.HexField.Y);
                return true;
            }
            return false;
        }

        public ParameterBool AutoMode = new();

        /// <summary>
        /// マップの基礎Fieldデータ
        /// </summary>
        /// <returns></returns>
        public List<HexField> HexFields()
        {
            var stageData = PartyInfo.StageMaster;
            var list = new List<HexField>();
            for (int j = 0;j < stageData.Height;j++)
            {
                for (int i = 0;i < stageData.Width;i++)
                {
                    var field = new HexField
                    {
                        X = i,
                        Y = j
                    };
                    list.Add(field);
                }
            }
            return list;
        }

        public void SetFieldXY(int x,int y)
        {
            if (_reachAreas.Count > 0)
            {
                var nextX = x;
                var nextY = y;
                if (_reachAreas.Find(a => a.X == nextX && a.Y == nextY) == null)
                {
                    return;
                }
            }
            var stageData = PartyInfo.StageMaster;
            FieldX.SetValue(x,0,stageData.Width-1);
            FieldY.SetValue(y,0,stageData.Height-1);
        }

        public void MoveFieldXY(int x,int y)
        {
            if (_reachAreas.Count > 0)
            {
                var nextX = FieldX.Value + x;
                var nextY = FieldY.Value + y;
                if (_reachAreas.Find(a => a.X == nextX && a.Y == nextY) == null)
                {
                    return;
                }
            }
            var stageData = PartyInfo.StageMaster;
            FieldX.GainValue(x,0,stageData.Width-1);
            FieldY.GainValue(y,0,stageData.Height-1);
        }

        
        public HexUnitInfo HexUnit()
        {
            var hexUnit = HexUnits();
            return hexUnit.Count > 0 ? hexUnit[0] : null;
        }

        public List<HexUnitInfo> HexUnits()
        {
            var hexUnits = SelectableUnitInfos;
            if (hexUnits.Count == 0)
            {
                // フィールド
                hexUnits = HexUnitInfos;
            }
            if (hexUnits.Count > 1)
            {
                hexUnits.Sort((a,b) => a.HexUnitType > b.HexUnitType ? -1 : 1);
            }
            return hexUnits;
        }

        public void MakeDepartureHex()
        {
            var hexUnits = HexUnits();
            if (hexUnits.Count == 0)
            {
                return;
            }
            var departureHex = hexUnits.Find(a => a.HexUnitType == HexUnitType.Basement);
            _reachAreas = _hexRoute.GetReachableArea(MoveType.Normal,departureHex.HexField,1,false);
            var depaterIndex = 1000;
            foreach (var path in _reachAreas)
            {
                var unitData = new StageSymbolData
                {
                    InitX = path.X,
                    InitY = path.Y,
                    UnitType = HexUnitType.Reach
                };
                var depaterUnit = new HexUnitInfo(depaterIndex,unitData);
                CurrentStage.AddHexUnitInfo(depaterUnit);
                depaterIndex++;
            }
        }

        public void MakeMoveBattlerHex()
        {
            var hexUnits = HexUnitInfos;
            if (hexUnits.Count == 0)
            {
                return;
            }
            var moveBattlerHex = hexUnits.Find(a => a.HexUnitType == HexUnitType.Battler);
            if (moveBattlerHex == null)
            {
                return;
            }
            _selectingHexUnitId.SetValue(moveBattlerHex.Index.Value);
            _reachAreas = _hexRoute.GetReachableArea(MoveType.Normal,moveBattlerHex.HexField,2,false);
            var moveBattlerIndex = 1000;
            foreach (var path in _reachAreas)
            {
                var unitData = new StageSymbolData
                {
                    InitX = path.X,
                    InitY = path.Y,
                    UnitType = HexUnitType.Reach
                };
                var moveBattlerUnit = new HexUnitInfo(moveBattlerIndex,unitData);
                CurrentStage.AddHexUnitInfo(moveBattlerUnit);
                moveBattlerIndex++;
            }
        }

        /// <summary>
        /// 移動と攻撃範囲を表示
        /// </summary>
        public HexUnitInfo MakeBattlerActHex()
        {
            if (_selectingHexUnitId.Value != 0)
            {
                return null;
            }
            var hexUnits = HexUnitInfos;
            if (hexUnits.Count == 0)
            {
                return null;
            }
            var moveBattlerHex = hexUnits.Find(a => a.HexUnitType == HexUnitType.Battler);
            if (moveBattlerHex == null)
            {
                return null;
            }
            _movableAreas = _hexRoute.GetReachableArea(MoveType.Normal,moveBattlerHex.HexField,2,false);
            _attackAreas = _hexRoute.GetReachableArea(MoveType.Normal,moveBattlerHex.HexField,3,false);
            for (int i = _attackAreas.Count-1;i >= 0;i--)
            {
                if (_movableAreas.Find(a => a.X == _attackAreas[i].X && a.Y== _attackAreas[i].Y) != null)
                {
                    _attackAreas.RemoveAt(i);
                }
            }
            var movableIndex = 1000;
            foreach (var path in _movableAreas)
            {
                var unitData = new StageSymbolData
                {
                    InitX = path.X,
                    InitY = path.Y,
                    UnitType = HexUnitType.Reach
                };
                var movableUnit = new HexUnitInfo(movableIndex,unitData);
                CurrentStage.AddHexUnitInfo(movableUnit);
                movableIndex++;
            }
            var attackIndex = 1000;
            foreach (var path in _attackAreas)
            {
                var unitData = new StageSymbolData
                {
                    InitX = path.X,
                    InitY = path.Y,
                    UnitType = HexUnitType.ReachAttack
                };
                var attackableUnit = new HexUnitInfo(attackIndex,unitData);
                CurrentStage.AddHexUnitInfo(attackableUnit);
                movableIndex++;
            }
            return moveBattlerHex;
        }

        public void ClearReachAreas()
        {
            CurrentStage.RemoveReachUnitInfo(_reachAreas);
            _reachAreas.Clear();
        }

        public void ClearMoveReachAreas()
        {
            CurrentStage.RemoveReachUnitInfo(_attackAreas);
            _attackAreas.Clear();
            CurrentStage.RemoveReachUnitInfo(_movableAreas);
            _movableAreas.Clear();
        }

        public HexUnitInfo SelectDeparture()
        {
            if (_departureActorId == -1)
            {
                return null;
            }
            // 出撃する
            var hexUnits = HexUnits();
            if (hexUnits.Count == 0)
            {
                return null;
            }
            hexUnits = hexUnits.FindAll(a => a.HexUnitType == HexUnitType.Battler);
            var depaterActorIndex = hexUnits.Count + 1;
            
            var unitData = new StageSymbolData
            {
                InitX = FieldX.Value,
                InitY = FieldY.Value,
                UnitType = HexUnitType.Battler,
            };
            var depaterActor = new HexUnitInfo(depaterActorIndex,unitData,(int)TeamIdType.Home);
            var unitInfo = new UnitInfo();
            var actorInfo = StageMembers().Find(a => a.ActorId.Value == _departureActorId);
            var battlerInfo = new BattlerInfo(actorInfo,1);
            unitInfo.SetBattlers(new List<BattlerInfo>(){battlerInfo});
            depaterActor.SetUnitInfo(unitInfo);
            CurrentStage.AddHexUnitInfo(depaterActor);
            // Teamに設定
            var teamInfo = CurrentStage.GetTurnTeamInfo();
            teamInfo.AddUnitInfos(depaterActor);

            // Reachを消去
            CurrentStage.RemoveReachUnitInfo(_reachAreas);
            _reachAreas.Clear();
            _departureActorId = -1;
            return depaterActor;
        }
        
        /// <summary>
        /// 自動選択ユニットの移動先を決定
        /// </summary>
        public void DecideAutoMoveBattlerField()
        {
            var moveBattler = CurrentStage.TurnHexUnitList()?.Find(a => a.Index.Value == _selectingHexUnitId.Value && a.IsUnit);
            if (moveBattler != null)
            {
                // AIは単体の思考ルーチンに従う
                if (CurrentStage.TurnTeamId.Value != (int)TeamIdType.Home)
                {                
                    switch (moveBattler.HexMoveType)
                    {
                        // 相手の本拠地に向かう、先に敵がいたら戦闘
                        case HexMoveType.MoveBasement:
                            AutoMoveBasement(moveBattler);
                            return;
                        // 何もしない
                        case HexMoveType.None:
                            
                            return;
                    }
                }
            }
        }

        /// <summary>
        ///  相手の本拠地に向かう、先に敵がいたら戦闘
        /// </summary>
        private void AutoMoveBasement(HexUnitInfo moveBattler)
        {
            var basement = CurrentStage.HexUnitList.Find(a => a.HexUnitType == HexUnitType.Basement && a.TeamId.Value != CurrentStage.TurnTeamId.Value);
            if (basement != null)
            {
                var decide = false;
                var baseMentCost = 0;
                var moveBattlerCost = 0;
                var moveBattlerMax = 2;
                var isBaseMent = true;
                // 移動圏内にいる場合
                var baseMentReaches = _hexRoute.GetReachableArea(MoveType.Normal,basement.HexField,baseMentCost,false);
                var moveBattlerReaches = _hexRoute.GetReachableArea(MoveType.Normal,moveBattler.HexField,moveBattlerMax,false);
                // 重なりを検知
                var findReach = baseMentReaches.Find(a => moveBattlerReaches.Find(b => a.X == b.X && a.Y == b.Y) != null);
                if (findReach != null)
                {
                    decide = true;
                    FieldX.SetValue(findReach.X);
                    FieldY.SetValue(findReach.Y);
                    return;
                }
                
                // 移動圏外にいる場合
                while (decide == false)
                {
                    baseMentReaches = _hexRoute.GetReachableArea(MoveType.Normal,basement.HexField,baseMentCost,false);
                    moveBattlerReaches = _hexRoute.GetReachableArea(MoveType.Normal,moveBattler.HexField,moveBattlerCost,false);
                    moveBattlerReaches.Reverse();
                    // 重なりを検知
                    findReach = baseMentReaches.Find(a => moveBattlerReaches.Find(b => a.X == b.X && a.Y == b.Y) != null);
                    if (findReach != null)
                    {
                        decide = true;
                        FieldX.SetValue(findReach.X);
                        FieldY.SetValue(findReach.Y);
                    } else
                    {
                        isBaseMent = !isBaseMent;
                        if (isBaseMent)
                        {
                            baseMentCost++;
                        } else
                        {
                            if (moveBattlerCost < moveBattlerMax)
                            {
                                moveBattlerCost++;
                            } else
                            {
                                baseMentCost++;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  相手の本拠地に向かう、先に敵がいたら戦闘
        /// </summary>
        private string EndAutoMoveBasement(int x,int y)
        {
            var moveBattler = CurrentStage.TurnHexUnitList()?.Find(a => a.Index.Value == _selectingHexUnitId.Value && a.IsUnit);
            if (moveBattler != null)
            {
                // 攻撃範囲
                var reach = 1;
                for (int i = 0;i < reach;i++)
                {
                    var reachPathes = _hexRoute.GetReachableArea(MoveType.Normal,moveBattler.HexField,reach,true);
                    foreach (var reachPath in reachPathes)
                    {
                        var findBattler = CurrentStage.HexUnitList.Find(a => a.HexField.X == reachPath.X && a.HexField.Y == reachPath.Y && a.TeamId.Value != CurrentStage.TurnTeamId.Value && a.HexUnitType == HexUnitType.Battler);
                        if (findBattler != null)
                        {
                            return "Battle";
                        }
                    }
                }
            }
            return "UnitActEnd";
        }

        public (List<Action>,HexUnitInfo) SelectMoveBattler()
        {
            var moveActions = new List<Action>();
            var pathes = new List<HexPath>();
            // 移動する
            var moveBattler = CurrentStage.TurnHexUnitList()?.Find(a => a.Index.Value == _selectingHexUnitId.Value && a.IsUnit);
            if (moveBattler != null)
            {
                var endHexUnit = new HexField
                {
                    X = FieldX.Value,
                    Y = FieldY.Value
                };
                // 移動ルート作成
                _hexRoute.FindRoute(MoveType.Normal,moveBattler.HexField,endHexUnit);
                pathes = _hexRoute.Pathlist;
                pathes.Reverse();
                foreach (var path in pathes)
                {
                    void action()
                    {
                        moveBattler.HexField.X = path.X;
                        moveBattler.HexField.Y = path.Y;
                    }
                    moveActions.Add(action);
                }
            }
            // Reachを消去
            CurrentStage.RemoveReachUnitInfo(_reachAreas);
            _reachAreas.Clear();
            return (moveActions,moveBattler);
        }

        public string DecideAutoMoveBattlerEnd()
        {
            var moveBattler = CurrentStage.TurnHexUnitList()?.Find(a => a.Index.Value == _selectingHexUnitId.Value && a.IsUnit);
            if (moveBattler != null)
            {
                // AIは単体の思考ルーチンに従う
                if (CurrentStage.TurnTeamId.Value != (int)TeamIdType.Home)
                {                
                    switch (moveBattler.HexMoveType)
                    {
                        // 敵がいたら戦闘
                        case HexMoveType.MoveBasement:
                            return EndAutoMoveBasement(moveBattler.HexField.X,moveBattler.HexField.Y);
                        // 何もしない
                        case HexMoveType.None:
                            
                            return "UnitActEnd";
                    }
                }
            }
            return "UnitActEnd";
        }

        public List<BattleSceneInfo> BattleSceneInfos()
        {
            var list = new List<BattleSceneInfo>();
            // バトルを行う組み合わせ
            var hexUnits = HexUnits();
            if (hexUnits.Count == 0)
            {
                return list;
            }
            var mainParty = hexUnits.Find(a => a.HexUnitType == HexUnitType.Battler);
            _reachAreas = _hexRoute.GetReachableArea(MoveType.Normal,mainParty.HexField,1,true);
            // 隣接候補
            var battlerUnits = CurrentStage.BattleHexUnitList()?.FindAll(a => a.HexUnitType == HexUnitType.Battler && _reachAreas.Find(b => b.X == a.HexField.X && b.Y == a.HexField.Y) != null);
            var enemyInfos = battlerUnits.FindAll(a => a.UnitInfo != null);
            // メインのみ
            if (mainParty != null && enemyInfos.Count > 0)
            {
                var m1 = new BattleSceneInfo
                {
                    ActorBattlerInfos = mainParty.UnitInfo.BattlerInfos,
                    EnemyInfos = new List<BattlerInfo>() { enemyInfos[0].UnitInfo.BattlerInfos[0] }
                };
                list.Add(m1);
            }
            
            return list;
        }

        public List<ListData> BattlerCommand()
        {
            var list = new List<SystemData.CommandData>();
            var move = new SystemData.CommandData
            {
                Id = 1,
                Name = "移動",
                Key = "MoveBattler"
            };
            list.Add(move);
            Func<SystemData.CommandData,bool> enable = (a) => 
            {
                return true;
            };
            return MakeListData(list,enable);
        }

        public List<ListData> BasementCommand()
        {
            var list = new List<SystemData.CommandData>();
            var departure = new SystemData.CommandData
            {
                Id = 1,
                Name = "出撃",
                Key = "Departure"
            };
            list.Add(departure);
            var turnEnd = new SystemData.CommandData
            {
                Id = 2,
                Name = "ターン終了",
                Key = "TurnEnd"
            };
            list.Add(turnEnd);
            Func<SystemData.CommandData,bool> enable = (a) => 
            {
                return true;
            };
            return MakeListData(list,enable);
        }        
        
        public List<ListData> EndMoveBattlerCommand()
        {
            var list = new List<SystemData.CommandData>();
            var battle = new SystemData.CommandData
            {
                Id = 1,
                Name = "戦闘",
                Key = "Battle"
            };
            list.Add(battle);
            var wait = new SystemData.CommandData
            {
                Id = 2,
                Name = "待機",
                Key = "Wait"
            };
            list.Add(wait);
            Func<SystemData.CommandData,bool> enable = (a) => 
            {
                return true;
            };
            return MakeListData(list,enable);
        }

        public List<ListData> DefaultCommand()
        {
            var list = new List<SystemData.CommandData>();
            var unit = new SystemData.CommandData
            {
                Id = 1,
                Name = "部隊",
                Key = "Unit"
            };
            list.Add(unit);
            var turnEnd = new SystemData.CommandData
            {
                Id = 2,
                Name = "ターン終了",
                Key = "TurnEnd"
            };
            list.Add(turnEnd);
            Func<SystemData.CommandData,bool> enable = (a) => 
            {
                return true;
            };
            return MakeListData(list,enable);
        }
    }
}
