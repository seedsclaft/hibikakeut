using System;
using System.Collections.Generic;
using System.Numerics;

namespace Ryneus
{
    public partial class TacticsModel : BaseModel
    {
        public List<HexUnitInfo> OnFieldInfos => CurrentStage.OnFieldInfos;

        private HexRoute _hexRoute;
        private List<HexField> _reachAreas = new();
        private List<HexField> _movableAreas = new();
        private List<HexField> _attackAreas = new();
        private string _commandKey = "";
        public string CommandKey =>_commandKey;
        public void SetCommandKey(string key) => _commandKey = key;
        // 移動管理
        public ParameterInt SelectingHexUnitId = new();
        private Vector2 _moveBeforeHexPosition = new Vector2();
        // 出撃管理
        private UnitInfo _departureUnitInfo = null;
        public UnitInfo DepartureUnitInfo => _departureUnitInfo;
        public void SetDepatureUnitInfo(UnitInfo departureUnitInfo) => _departureUnitInfo = departureUnitInfo;
        
        // 行動選択中のチーム
        public bool IsPlayable => CurrentStage.TurnTeamId.Value == (int)TeamIdType.Home;
        public bool CanMoveBattler => IsPlayable && CurrentStage.GetTurnTeamInfo().CurrentActPoint.Value > 0;
        
        public void UseActPoint()
        {
            var term = CurrentStage.GetTurnTeamInfo();
            term.CurrentActPoint.GainValue(-1,0,term.ActPoint.Value);
        }

        public void UnitActEnd()
        {
            SelectingHexUnitId.SetValue(0);
            // 行動ポイントを減らす
            UseActPoint();
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
            CurrentStage.GetTurnTeamInfo().ClearMoveEndUnitIds();
            // ターンチームを変更
            CurrentStage.TurnTeamId.SetValue(nextId);
            // 行動ポイントを初期化
            var nextTeam = CurrentStage.GetTurnTeamInfo();
            if (nextTeam.TeamId.Value == (int)TeamIdType.Home)
            {
                // 拠点数
                var actPoint = CurrentStage.FieldHexList.FindAll(a => a.IsBasementUnit() && a.IsFriend(nextTeam.TeamId.Value));
                nextTeam.ActPoint.SetValue(actPoint.Count);
                nextTeam.CurrentActPoint.SetValue(nextTeam.ActPoint.Value);
            } else
            {
                // 敵部隊数
                var actPoint = nextTeam.UnitInfos.FindAll(a => a.IsUnit && a.IsFriend(nextTeam.TeamId.Value) && !a.IsLostUnit());
                nextTeam.ActPoint.SetValue(actPoint.Count);
                nextTeam.CurrentActPoint.SetValue(nextTeam.ActPoint.Value);
            }
            CurrentStage.TurnCount.GainValue(1);
        }

        public void SetLastSelectHex()
        {
            var thisTeam = CurrentStage.GetTurnTeamInfo();
            thisTeam.SetLastSelectHex(CurrentStage.FieldX.Value,CurrentStage.FieldY.Value);
        }

        public TeamInfo GetTurnTeam()
        {
            return CurrentStage.GetTurnTeamInfo();
        }

        public TeamState GetTurnTeamState()
        {
            return GetTurnTeam().GetTeamState();
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
                CurrentStage.FieldX.SetValue(moveBattler.HexField.X);
                CurrentStage.FieldY.SetValue(moveBattler.HexField.Y);
                UnityEngine.Debug.Log(CurrentStage.FieldX.Value+":"+CurrentStage.FieldY.Value);
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
                    //return;
                }
            }
            var stageData = PartyInfo.StageMaster;
            CurrentStage.FieldX.SetValue(x,0,stageData.Width-1);
            CurrentStage.FieldY.SetValue(y,0,stageData.Height-1);
            UnityEngine.Debug.Log(CurrentStage.FieldX.Value+":"+CurrentStage.FieldY.Value);
        }

        public void MoveFieldXY(int x,int y)
        {
            if (_reachAreas.Count > 0)
            {
                var nextX = CurrentStage.FieldX.Value + x;
                var nextY = CurrentStage.FieldY.Value + y;
                if (_reachAreas.Find(a => a.X == nextX && a.Y == nextY) == null)
                {
                    return;
                }
            }
            var stageData = PartyInfo.StageMaster;
            CurrentStage.FieldX.GainValue(x,0,stageData.Width-1);
            CurrentStage.FieldY.GainValue(y,0,stageData.Height-1);
        }

        
        public HexUnitInfo SortedHexUnit()
        {
            var hexUnits = CurrentStage.AllFieldUnitInfos();
            if (hexUnits.Count > 1)
            {
                hexUnits.Sort((a,b) => a.HexUnitType > b.HexUnitType ? -1 : 1);
            }
            return hexUnits.Count > 0 ? hexUnits[0] : null;
        }

        public void MakeDepartureHex()
        {
            var hexUnits = OnFieldInfos;
            if (hexUnits.Count == 0)
            {
                return;
            }
            var departureHex = hexUnits.Find(a => a.IsBasementUnit());
            var move = _departureUnitInfo.BattlerInfos[0].CurrentMov();
            _reachAreas = _hexRoute.GetReachableArea(MoveType.Normal,departureHex.HexField,move,false);
            var depaterIndex = 1000;
            foreach (var path in _reachAreas)
            {
                var unitData = new StageSymbolData
                {
                    InitX = path.X,
                    InitY = path.Y,
                    UnitType = HexUnitType.Reach
                };
                var depatureReach = new HexUnitInfo(depaterIndex,unitData);
                CurrentStage.AddHexUnitInfo(depatureReach);
                depaterIndex++;
            }
        }

        public void MakeMoveBattlerHex()
        {
            var hexUnits = CurrentStage.FriendUnitInfos();
            if (hexUnits.Count == 0)
            {
                return;
            }
            var moveBattlerHex = hexUnits.Find(a => a.IsBattlerUnit());
            if (moveBattlerHex == null)
            {
                return;
            }
            SelectingHexUnitId.SetValue(moveBattlerHex.Index.Value);
            _reachAreas = _hexRoute.GetReachableArea(MoveType.Normal,moveBattlerHex.HexField,moveBattlerHex.UnitInfo.BattlerInfos[0].CurrentMov(),false);
            var moveBattlerIndex = 1000;
            foreach (var path in _reachAreas)
            {
                var unitData = new StageSymbolData
                {
                    InitX = path.X,
                    InitY = path.Y,
                    UnitType = HexUnitType.Reach
                };
                var moveBattlerReach = new HexUnitInfo(moveBattlerIndex,unitData);
                CurrentStage.AddHexUnitInfo(moveBattlerReach);
                moveBattlerIndex++;
            }
        }

        /// <summary>
        /// 移動と攻撃範囲を表示
        /// </summary>
        public HexUnitInfo MakeBattlerActHex()
        {
            if (SelectingHexUnitId.Value != 0)
            {
                return null;
            }
            var hexUnits = CurrentStage.AllUnitInfos(true);
            if (hexUnits.Count == 0)
            {
                return null;
            }
            var moveBattlerHex = hexUnits.Find(a => a.IsBattlerUnit() && !a.IsLostUnit());
            if (moveBattlerHex == null)
            {
                return null;
            }
            // 味方の行動範囲は表示しない
            if (moveBattlerHex.IsPlayableUnit())
            {
                return moveBattlerHex;
            }
            // 出撃・移動中は表示しない
            if (_commandKey == DepartureCommand.Key || _commandKey == MoveBattlerCommand.Key)
            {
                return moveBattlerHex;
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
                var movableReach = new HexUnitInfo(movableIndex,unitData);
                CurrentStage.AddHexUnitInfo(movableReach);
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
                var attackAreaReach = new HexUnitInfo(attackIndex,unitData);
                CurrentStage.AddHexUnitInfo(attackAreaReach);
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
            if (_departureUnitInfo == null)
            {
                return null;
            }
            // 出撃する
            var hexUnits = CurrentStage.FieldHexList.FindAll(a => a.IsBattlerUnit());
            var depaterActorIndex = hexUnits.Count + 1;
            
            var unitData = new StageSymbolData
            {
                InitX = CurrentStage.FieldX.Value,
                InitY = CurrentStage.FieldY.Value,
                UnitType = HexUnitType.Battler,
            };
            var depaterUnit = new HexUnitInfo(depaterActorIndex,unitData,(int)TeamIdType.Home);
            depaterUnit.SetUnitInfo(_departureUnitInfo);
            // Teamに設定
            var teamInfo = CurrentStage.GetTurnTeamInfo();
            teamInfo.AddUnitInfos(depaterUnit);

            // Reachを消去
            CurrentStage.RemoveReachUnitInfo(_reachAreas);
            _reachAreas.Clear();
            _departureUnitInfo = null;

            // 行動ポイントを減らす
            UseActPoint();
            return depaterUnit;
        }
        
        /// <summary>
        /// 自動選択ユニットの移動先を決定
        /// </summary>
        public void DecideAutoMoveBattlerField()
        {
            var moveBattler = CurrentStage.OnFieldTurnUnitInfos()?.Find(a => a.Index.Value == SelectingHexUnitId.Value && a.IsUnit);
            if (moveBattler != null)
            {
                CurrentStage.GetTurnTeamInfo().AddMoveEndUnitId(moveBattler.Id.Value);
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
            var basement = OnFieldInfos.Find(a => a.IsBasementUnit() && a.TeamId.Value != CurrentStage.TurnTeamId.Value);
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
                    CurrentStage.FieldX.SetValue(findReach.X);
                    CurrentStage.FieldY.SetValue(findReach.Y);
                    UnityEngine.Debug.Log(CurrentStage.FieldX.Value+":"+CurrentStage.FieldY.Value);
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
                        CurrentStage.FieldX.SetValue(findReach.X);
                        CurrentStage.FieldY.SetValue(findReach.Y);
                        UnityEngine.Debug.Log(CurrentStage.FieldX.Value+":"+CurrentStage.FieldY.Value);
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
            var moveBattler = CurrentStage.OnFieldTurnUnitInfos()?.Find(a => a.Index.Value == SelectingHexUnitId.Value && a.IsUnit);
            if (moveBattler != null)
            {
                // 攻撃範囲
                var reach = 1;
                for (int i = 0;i < reach;i++)
                {
                    var reachPathes = _hexRoute.GetReachableArea(MoveType.Normal,moveBattler.HexField,reach,true);
                    foreach (var reachPath in reachPathes)
                    {
                        var findBattler = OnFieldInfos.Find(a => a.HexField.X == reachPath.X && a.HexField.Y == reachPath.Y && a.TeamId.Value != CurrentStage.TurnTeamId.Value && a.IsBattlerUnit());
                        if (findBattler != null)
                        {
                            return BattleCommand.Key;
                        }
                    }
                }
            }
            return UnitActEndCommand.Key;
        }

        public (List<Action>,HexUnitInfo) SelectMoveBattler(int x,int y)
        {
            var moveActions = new List<Action>();
            var pathes = new List<HexPath>();
            // 移動する
            var moveUnitInfo = CurrentStage.FriendUnitInfos().Find(a => a.Index.Value == SelectingHexUnitId.Value && a.IsUnit);
            if (moveUnitInfo != null)
            {
                _moveBeforeHexPosition = new Vector2(moveUnitInfo.HexField.X,moveUnitInfo.HexField.Y);
                var endHexUnit = new HexField
                {
                    X = x,
                    Y = y
                };
                // 待機
                if (endHexUnit.X == moveUnitInfo.HexField.X && endHexUnit.Y == moveUnitInfo.HexField.Y)
                {
                    // Reachを消去
                    ClearReachAreas();
                    return (moveActions,moveUnitInfo);
                }
                // 移動ルート作成
                _hexRoute.FindRoute(MoveType.Normal,moveUnitInfo.HexField,endHexUnit);
                pathes = _hexRoute.Pathlist;
                pathes.Reverse();
                foreach (var path in pathes)
                {
                    void action()
                    {
                        moveUnitInfo.HexField.X = path.X;
                        moveUnitInfo.HexField.Y = path.Y;
                    }
                    moveActions.Add(action);
                }
            }
            // Reachを消去
            ClearReachAreas();
            return (moveActions,moveUnitInfo);
        }

        public void BeforeMoveBattler()
        {
            var moveBattler = CurrentStage.FriendUnitInfos()?.Find(a => a.Index.Value == SelectingHexUnitId.Value && a.IsUnit);
            moveBattler?.SetPosition((int)_moveBeforeHexPosition.X,(int)_moveBeforeHexPosition.Y);
        }

        public string DecideAutoMoveBattlerEnd()
        {
            var moveBattler = CurrentStage.FriendUnitInfos()?.Find(a => a.Index.Value == SelectingHexUnitId.Value && a.IsUnit);
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
                            
                            return UnitActEndCommand.Key;
                    }
                }
            }
            return UnitActEndCommand.Key;
        }

        public List<BattleSceneInfo> BattleSceneInfos()
        {
            var list = new List<BattleSceneInfo>();
            var battlerUnits = CurrentStage.AllUnitInfos().FindAll(a => a.IsBattlerUnit() && !a.IsLostUnit());
            if (battlerUnits.Count == 0)
            {
                return list;
            }
            // バトルを行う組み合わせ
            var mainParty = CurrentStage.OnFieldTurnUnitInfos().Find(a => a.IsBattlerUnit() && !a.IsLostUnit());
            _reachAreas = _hexRoute.GetReachableArea(MoveType.Normal,mainParty.HexField,1,true);
            // 隣接候補
            var opponentUnits = battlerUnits.FindAll(a => a.UnitInfo != null && !a.IsFriend(mainParty.TeamId.Value) && _reachAreas.Find(b => a.OnField(b.X,b.Y)) != null);
            if (mainParty != null && opponentUnits.Count > 0)
            {
                // インデックスの割り振り 中央
                mainParty.SetBattlerIndex(1);
                foreach (var battlerUnit in opponentUnits)
                {
                    // インデックスの割り振り 中央
                    battlerUnit.SetBattlerIndex(1);
                    // メインのみ
                    var m1 = new BattleSceneInfo
                    {
                        ActorBattlerInfos = mainParty.UnitInfo.BattlerInfos,
                        EnemyInfos = battlerUnit.UnitInfo.BattlerInfos
                    };
                    list.Add(m1);
                    var enemyReach = _hexRoute.GetReachableArea(MoveType.Normal,battlerUnit.HexField,1,true);
                    // メインと隣接あり
                    var nearFriends = battlerUnits.FindAll(a => a != mainParty && a.IsFriend(mainParty.TeamId.Value) && enemyReach.Find(b => a.OnField(b.X,b.Y)) != null && _reachAreas.Find(b => a.OnField(b.X,b.Y)) != null);
                    foreach (var nearFriend in nearFriends)
                    {
                        // インデックスの割り振り 左右
                        nearFriend.SetBattlerIndex(2);
                        var battlerInfos = new List<BattlerInfo>();
                        battlerInfos.AddRange(mainParty.UnitInfo.BattlerInfos);
                        battlerInfos.AddRange(nearFriend.UnitInfo.BattlerInfos);
                        var m2 = new BattleSceneInfo
                        {
                            ActorBattlerInfos = battlerInfos,
                            EnemyInfos = battlerUnit.UnitInfo.BattlerInfos
                        };
                        list.Insert(0,m2);
                    }
                }
            }
            
            return list;
        }

        public List<HexUnitInfo> LostUnitInfos()
        {
            var list = CurrentStage.LostUnitInfos();
            return list;
        }

        public void EndLostActions()
        {
            var list = CurrentStage.LostUnitInfos();
            CurrentStage.RemoveLostUnitInfos(list);
        }

        public void ConquerBasement()
        {
            var basement = OnFieldInfos.Find(a => a.IsBasementUnit());
            if (basement != null)
            {
                var term = CurrentStage.GetTurnTeamInfo();
                basement.Conquer(term.TeamId.Value);
                // 行動ポイントを減らす
                UseActPoint();
            }
        }

        public void ReturnBasement()
        {
            var battlerUnit = CurrentStage.OnFieldTurnUnitInfos().Find(a => a.IsUnit);
            if (battlerUnit != null)
            {
                // Teamから削除
                var teamInfo = CurrentStage.GetTurnTeamInfo();
                teamInfo.RemoveUnitInfos(battlerUnit);
                // 行動ポイントを減らす
                UseActPoint();
            }
        }

        public List<GetItemInfo> AlcanaOpen()
        {
            UnitActEnd();
            var alcana = OnFieldInfos.Find(a => a.IsAlcanaUnit());
            if (alcana != null)
            {
                foreach (var getItemInfo in alcana.GetItemInfos)
                {
                    AddGetItemInfo(getItemInfo);
                }
                // 消失
                CurrentStage.RemoveHexUnitInfo(alcana);
                return alcana.GetItemInfos;
            }
            return null;
        }

        public List<UnitInfo> DepatureUnitInfos()
        {
            var list = new List<UnitInfo>();
            var hexUnitInfos = CurrentStage.GetTurnTeamInfo().DepatuerInfos;
            foreach (var hexUnitInfo in hexUnitInfos)
            {
                list.Add(hexUnitInfo.UnitInfo);
            }
            return list;
        }

        public List<UnitInfo> FieldUnitInfos()
        {
            var list = new List<UnitInfo>();
            var hexUnitInfos = CurrentStage.FriendUnitInfos().FindAll(a => a.IsUnit);
            foreach (var hexUnitInfo in hexUnitInfos)
            {
                list.Add(hexUnitInfo.UnitInfo);
            }
            return list;
        }

        public (List<HexUnitInfo>,List<List<int>>) CheckHealUnits()
        {
            var hpHealList = new List<List<int>>();
            var hpHeals = new List<int>();
            var list = new List<HexUnitInfo>();
            // 拠点回復確認
            var basements = CurrentStage.FieldHexList.FindAll(a => a.IsBasementUnit() && a.TeamId.Value == GetTurnTeam().TeamId.Value);
            var battlerUnitInfos = CurrentStage.FriendUnitInfos();
            foreach (var battlerUnitInfo in battlerUnitInfos)
            {
                if (basements.Find(a => a.HexField.X == battlerUnitInfo.HexField.X && a.HexField.Y == battlerUnitInfo.HexField.Y) != null)
                {
                    foreach (var battlerInfo in battlerUnitInfo.UnitInfo.BattlerInfos)
                    {
                        if (battlerInfo.Index.Value == 0)
                        {
                            continue;
                        }
                        var hpHeal = 0;
                        var heal = (int)(battlerInfo.MaxHp * 0.1f);
                        if (heal > hpHeal)
                        {
                            hpHeal = heal;
                            var damage = battlerInfo.MaxHp - battlerInfo.Hp.Value;
                            if (hpHeal > damage)
                            {
                                hpHeal = damage;
                            }
                        }
                        hpHeals.Add(hpHeal);
                    }
                    list.Add(battlerUnitInfo);
                    hpHealList.Add(hpHeals);
                }
            }
            return (list,hpHealList);
        }

        public void StageClear()
        {
            
        }

        private bool _turnEndCommandEnable = true;
        public void SetTurnEndCommandEnable(bool isEnable) => _turnEndCommandEnable = isEnable;
        public List<ListData> BattlerCommand()
        {
            var list = new List<SystemData.CommandData>
            {
                MoveBattlerCommand,
                UnitsCommand,
            };
            // 同時に拠点がある場合
            var basement = OnFieldInfos.Find(a => a.IsBasementUnit() && a.TeamId.Value == GetTurnTeam().TeamId.Value);
            if (basement != null)
            {
                var BasementCommand = new List<SystemData.CommandData>
                {
                    DepartureCommand,
                    UnitEditCommand,
                };
                list.AddRange(BasementCommand);
            }
            // 同時に敵拠点がある場合
            var conq = OnFieldInfos.Find(a => a.IsBasementUnit() && a.TeamId.Value != GetTurnTeam().TeamId.Value);
            if (conq != null)
            {
                list.Insert(0,ConquerCommand);
            }
            list.Add(SaveCommand);
            list.Add(TurnEndCommand);
            bool enable(SystemData.CommandData a)
            {
                if (a.Key == MoveBattlerCommand.Key)
                {
                    return CurrentStage.GetTurnTeamInfo().CurrentActPoint.Value > 0;
                }
                if (a.Key == DepartureCommand.Key)
                {
                    return CurrentStage.GetTurnTeamInfo().CurrentActPoint.Value > 0;
                }
                if (a.Key == TurnEndCommand.Key)
                {
                    return _turnEndCommandEnable;
                }
                return true;
            }
            return MakeListData(list, enable);
        }

        public List<ListData> BasementCommand()
        {
            var list = new List<SystemData.CommandData>
            {
                DepartureCommand,
                UnitEditCommand,
                SaveCommand,
                TurnEndCommand
            };
            bool enable(SystemData.CommandData a)
            {
                if (a.Key == DepartureCommand.Key)
                {
                    return CurrentStage.GetTurnTeamInfo().CurrentActPoint.Value > 0;
                }
                if (a.Key == TurnEndCommand.Key)
                {
                    return _turnEndCommandEnable;
                }
                return true;
            }
            return MakeListData(list, enable);
        }

        public List<ListData> EndMoveBattlerCommand()
        {
            var list = new List<SystemData.CommandData>
            {
                BattleCommand,
                WaitCommand
            };
            // 敵拠点の上にいる場合は制圧
            var battler = CurrentStage.OnFieldTurnUnitInfos().Find(a => a.IsBattlerUnit());
            var hexUnits = OnFieldInfos;
            if (hexUnits.Find(a => a.IsBasementUnit() && a.TeamId.Value != battler.TeamId.Value) != null)
            {
                list.Insert(0,ConquerCommand);
            } else
            // 味方拠点の上であれば帰還
            if (hexUnits.Find(a => a.IsBasementUnit() && a.TeamId.Value == battler.TeamId.Value) != null)
            {
                list.Insert(0,ReturnCommand);
            }
            // イベントの上
            if (hexUnits.Find(a => a.IsAlcanaUnit()) != null)
            {
                list.Insert(0,EventCommand);
            }
            bool enable(SystemData.CommandData a)
            {
                if (a == BattleCommand)
                {
                    // 隣接している
                    var hexUnits = CurrentStage.AllUnitInfos();
                    var battler = hexUnits.Find(a => a.IsBattlerUnit());
                    var reachAreas = _hexRoute.GetReachableArea(MoveType.Normal,battler.HexField,1,true);
                    var battlerUnits = CurrentStage.OpponentUnitInfos()?.FindAll(a => a.IsBattlerUnit() && reachAreas.Find(b => a.OnField(b.X,b.Y)) != null);
                    var enemyInfos = battlerUnits.FindAll(a => a.UnitInfo != null);
                    return enemyInfos.Count > 0;
                }
                return true;
            }
            return MakeListData(list, enable);
        }

        public List<ListData> DefaultCommand()
        {
            var list = new List<SystemData.CommandData>
            {
                UnitsCommand,
                SaveCommand,
                TurnEndCommand
            };
            bool enable(SystemData.CommandData a)
            {
                if (a.Key == TurnEndCommand.Key)
                {
                    return _turnEndCommandEnable;
                }
                return true;
            }
            return MakeListData(list, enable);
        }
    
        public SystemData.CommandData DepartureCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "Departure");
        public SystemData.CommandData TurnEndCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "TurnEnd");
        public SystemData.CommandData MoveBattlerCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "MoveBattler");
        public SystemData.CommandData WaitCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "Wait");
        public SystemData.CommandData UnitActEndCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "UnitActEnd");
        public SystemData.CommandData BattleCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "Battle");
        public SystemData.CommandData UnitsCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "Units");
        public SystemData.CommandData SaveCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "Save");
        public SystemData.CommandData ConquerCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "Conquer");
        public SystemData.CommandData UnitEditCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "UnitEdit");
        public SystemData.CommandData ReturnCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "Return");
        public SystemData.CommandData EventCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "Event");
    }

}
