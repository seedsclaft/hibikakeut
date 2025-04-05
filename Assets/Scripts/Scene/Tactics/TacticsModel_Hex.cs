using System;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class TacticsModel : BaseModel
    {
        public ParameterInt FieldX = new();
        public ParameterInt FieldY = new();
        public List<HexUnitInfo> SelectUnitInfos => CurrentStage.TurnHexUnitList()?.FindAll(a => a.HexField.X == FieldX.Value && a.HexField.Y == FieldY.Value);
        public List<HexUnitInfo> SelectFieldInfos => CurrentStage.HexUnitList?.FindAll(a => a.HexField.X == FieldX.Value && a.HexField.Y == FieldY.Value);
        private HexRoute _hexRoute;
        private List<HexField> _reachAreas = new();
        private string _commandKey = "";
        public string CommandKey =>_commandKey;
        public void SetCommandKey(string key) => _commandKey = key;
        private ParameterInt SelectingHexUnitId = new();
        private int _departureActorId = -1;
        public void SetDepatureActorId(int departureActorId) => _departureActorId = departureActorId;
        
        // 行動選択中のチーム
        public bool IsPlayable => CurrentStage.TurnTeamId.Value == (int)TeamIdType.Home;
        public void UnitActEnd()
        {
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
            var hexUnits = SelectUnitInfos;
            if (hexUnits.Count == 0)
            {
                // フィールド
                hexUnits = SelectFieldInfos;
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
            var hexUnits = HexUnits();
            if (hexUnits.Count == 0)
            {
                return;
            }
            var moveBattlerHex = hexUnits.Find(a => a.HexUnitType == HexUnitType.Battler);
            SelectingHexUnitId.SetValue(moveBattlerHex.Index.Value);
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

        public void SelectDeparture()
        {
            // 出撃する
            var hexUnits = HexUnits();
            if (hexUnits.Count == 0)
            {
                return;
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
            depaterActor.SetActorInfos(new List<ActorInfo>(){StageMembers().Find(a => a.ActorId.Value == _departureActorId)});
            CurrentStage.AddHexUnitInfo(depaterActor);

            // Reachを消去
            CurrentStage.RemoveReachUnitInfo();
            _reachAreas.Clear();
            _selectActorId = -1;
        }
        
        /// <summary>
        /// 自動選択ユニットの移動先を決定
        /// </summary>
        public void DecideAutoMoveBattlerField()
        {
            var moveBattler = CurrentStage.TurnHexUnitList()?.Find(a => a.Index.Value == SelectingHexUnitId.Value && a.IsUnit);
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
            var moveBattler = CurrentStage.TurnHexUnitList()?.Find(a => a.Index.Value == SelectingHexUnitId.Value && a.IsUnit);
            if (moveBattler != null)
            {
                // 攻撃範囲
                var reach = 1;
                for (int i = 1;i <= reach;i++)
                {
                    var reachPathes = _hexRoute.GetReachableArea(MoveType.Normal,moveBattler.HexField,reach,true);
                    foreach (var reachPath in reachPathes)
                    {
                        var findBattler = CurrentStage.HexUnitList.Find(a => a.TeamId.Value != CurrentStage.TurnTeamId.Value && a.HexUnitType == HexUnitType.Battler);
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
            var moveBattler = CurrentStage.TurnHexUnitList()?.Find(a => a.Index.Value == SelectingHexUnitId.Value && a.IsUnit);
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
            CurrentStage.RemoveReachUnitInfo();
            _reachAreas.Clear();
            return (moveActions,moveBattler);
        }

        public string DecideAutoMoveBattlerEnd()
        {
            var moveBattler = CurrentStage.TurnHexUnitList()?.Find(a => a.Index.Value == SelectingHexUnitId.Value && a.IsUnit);
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
            var battlerUnits = CurrentStage.TurnHexUnitList()?.FindAll(a => _reachAreas.Find(b => b.X == a.HexField.X && b.Y == a.HexField.Y) != null);
            var enemyInfos = battlerUnits.FindAll(a => a.TroopInfo != null);
            // メインのみ
            if (mainParty != null && enemyInfos.Count > 0)
            {
                var m1 = new BattleSceneInfo();
                m1.ActorInfos = mainParty.ActorInfos;
                m1.EnemyInfos = new List<BattlerInfo>(){enemyInfos[0].TroopInfo.BattlerInfos[0]};
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
    }
}
