using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Ryneus
{
    public partial class TacticsModel : BaseModel
    {
        public List<HexUnitInfo> OnFieldInfos => CurrentStage.OnFieldInfos;
        // 選択中のユニット
        public HexUnitInfo SelectingUnit => CurrentStage.OnFieldTurnUnitInfos().Find(a => a.IsBattlerUnit);

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
                var actPoint = CurrentStage.FieldHexList.FindAll(a => a.IsBasementUnit && a.IsFriend(nextTeam.TeamId.Value));
                nextTeam.SetActPoint(actPoint.Count);
            } else
            {
                // 敵部隊数
                var actPoint = nextTeam.UnitInfos.FindAll(a => a.IsUnit && a.IsFriend(nextTeam.TeamId.Value) && !a.IsLostUnit());
                nextTeam.SetActPoint(actPoint.Count);
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
            for (int j = 0;j <= stageData.Height;j++)
            {
                for (int i = 0;i <= stageData.Width;i++)
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
            var hexUnits = CurrentStage.AllOnFieldUnitInfos();
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
            var departureHex = hexUnits.Find(a => a.IsBasementUnit);
            var move = _departureUnitInfo.BattlerInfos[0].CurrentMov();
            SelectingHexUnitId.SetValue(_departureUnitInfo.Index.Value);
            _reachAreas = GetHexReach(departureHex.HexField,move,false,true);
            var depaterIndex = 1000;
            var friends = CurrentStage.FriendUnitInfos();
            foreach (var path in _reachAreas)
            {
                // 自陣のユニット配置場所は候補から外す
                if (friends.Find(a => a.HexField.X == path.X && a.HexField.Y == path.Y) != null)
                {
                    var hexFields = CurrentStage.FindFieldInfos(path.X,path.Y);
                    // 自陣の拠点は候補に入れる
                    if (hexFields.Find(a => a.TeamId.Value == departureHex.TeamId.Value && a.IsBasementUnit) == null)
                    {
                        continue;
                    }
                }
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
            var hexUnits = CurrentStage.OnFieldTurnUnitInfos();
            if (hexUnits.Count == 0)
            {
                return;
            }
            var moveBattlerHex = hexUnits.Find(a => a.IsBattlerUnit);
            if (moveBattlerHex == null)
            {
                return;
            }
            SelectingHexUnitId.SetValue(moveBattlerHex.Index.Value);
            _reachAreas = GetHexReach(moveBattlerHex.HexField,moveBattlerHex.GetUnitMov(),false,true);
            var moveBattlerIndex = 1000;
            // 自身の位置も移動候補に入れる
            var selfHex = new HexField
            {
                X = moveBattlerHex.HexField.X,
                Y = moveBattlerHex.HexField.Y
            };
            _reachAreas.Add(selfHex);
            var friends = CurrentStage.FriendUnitInfos();
            foreach (var path in _reachAreas)
            {
                // 自陣のユニット配置場所は候補から外す
                if (friends.Find(a => a.HexField.X == path.X && a.HexField.Y == path.Y) != null)
                {
                    var hexFields = CurrentStage.FindFieldInfos(path.X,path.Y);
                    // 自陣の拠点は候補に入れる
                    if (hexFields.Find(a => a.TeamId.Value == moveBattlerHex.TeamId.Value && a.IsBasementUnit) == null)
                    {
                        continue;
                    }
                }
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
            var moveBattlerHex = hexUnits.Find(a => a.IsBattlerUnit && !a.IsLostUnit());
            if (moveBattlerHex == null)
            {
                return null;
            }
            // 味方の行動範囲は表示しない
            if (moveBattlerHex.IsHomeUnit())
            {
                return moveBattlerHex;
            }
            // 出撃・移動中は表示しない
            if (_commandKey == DepartureCommand.Key || _commandKey == MoveBattlerCommand.Key)
            {
                return moveBattlerHex;
            }
            _movableAreas = GetHexReach(moveBattlerHex.HexField,moveBattlerHex.GetUnitMov(),true,true);
            _attackAreas = GetHexReach(moveBattlerHex.HexField,moveBattlerHex.GetUnitMov()+1,true,true);
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
            var hexUnits = CurrentStage.FieldHexList.FindAll(a => a.IsBattlerUnit);
            var depaterActorIndex = hexUnits.Count + 1;
            
            var unitData = new StageSymbolData
            {
                InitX = CurrentStage.FieldX.Value,
                InitY = CurrentStage.FieldY.Value,
                UnitType = HexUnitType.Battler,
            };
            var depaterUnit = new HexUnitInfo(depaterActorIndex,unitData,(int)TeamIdType.Home);
            depaterUnit.Index.SetValue(SelectingHexUnitId.Value);
            depaterUnit.SetUnitInfo(_departureUnitInfo);
            // Teamに設定
            var teamInfo = CurrentStage.GetTurnTeamInfo();
            teamInfo.AddUnitInfos(depaterUnit);

            // Reachを消去
            ClearReachAreas();
            _departureUnitInfo = null;

            return depaterUnit;
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
                GetFindRoute(moveUnitInfo.HexField,endHexUnit,true,true);
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


        public List<BattleSceneInfo> BattleSceneInfos()
        {
            var list = new List<BattleSceneInfo>();
            var mainParty = SelectingUnit;
            // バトルを行う組み合わせ
            _reachAreas = GetHexReach(mainParty.HexField,1,true);
            // 隣接候補
            var oppnents = CurrentStage.OpponentUnitInfos();
            var friends = CurrentStage.FriendUnitInfos();
            var opponentUnits = oppnents.FindAll(a => _reachAreas.Find(b => a.OnField(b.X,b.Y)) != null);
            if (mainParty != null && opponentUnits.Count > 0)
            {
                if (CurrentStage.GetTurnTeamInfo().TeamId.Value == (int)TeamIdType.Home)
                {
                    // インデックスの割り振り 中央
                    mainParty.SetBattlerIndex(2);
                    foreach (var battlerUnit in opponentUnits)
                    {
                        // インデックスの割り振り 中央
                        battlerUnit.SetBattlerIndex(2);
                        // メインのみ
                        var m1 = new BattleSceneInfo
                        {
                            ActorBattlerInfos = mainParty.UnitInfo.BattlerInfos.FindAll(a => a.ActorInfo != null),
                            EnemyInfos = battlerUnit.UnitInfo.BattlerInfos.FindAll(a => a.EnemyData != null)
                        };
                        list.Add(m1);
                        var enemyReach = GetHexReach(battlerUnit.HexField,1,true);
                        // メインと隣接あり
                        var nearFriends = friends.FindAll(a => a != mainParty && enemyReach.Find(b => a.OnField(b.X,b.Y)) != null && _reachAreas.Find(b => a.OnField(b.X,b.Y)) != null);
                        var battlerIndex = 1;
                        foreach (var nearFriend in nearFriends)
                        {
                            // インデックスの割り振り 左右
                            nearFriend.SetBattlerIndex(battlerIndex);
                            if (nearFriends.Count == 2)
                            {
                                battlerIndex = 3;
                            }
                            var battlerInfos = new List<BattlerInfo>();
                            battlerInfos.AddRange(mainParty.UnitInfo.BattlerInfos);
                            battlerInfos.AddRange(nearFriend.UnitInfo.BattlerInfos);
                            var m2 = new BattleSceneInfo
                            {
                                ActorBattlerInfos = battlerInfos.FindAll(a => a.ActorInfo != null),
                                EnemyInfos = battlerUnit.UnitInfo.BattlerInfos.FindAll(a => a.EnemyData != null),
                            };
                            list.Insert(0,m2);
                        }
                        if (nearFriends.Count == 2)
                        {
                            nearFriends[0].SetBattlerIndex(1);
                            nearFriends[1].SetBattlerIndex(3);
                            var battlerInfos = new List<BattlerInfo>();
                            battlerInfos.AddRange(mainParty.UnitInfo.BattlerInfos);
                            battlerInfos.AddRange(nearFriends[0].UnitInfo.BattlerInfos);
                            battlerInfos.AddRange(nearFriends[1].UnitInfo.BattlerInfos);
                            // 両サイドどちらも
                            var m3 = new BattleSceneInfo
                            {
                                ActorBattlerInfos = battlerInfos.FindAll(a => a.ActorInfo != null),
                                EnemyInfos = battlerUnit.UnitInfo.BattlerInfos.FindAll(a => a.EnemyData != null),
                            };
                            list.Insert(0,m3);
                        }
                    }
                }
                else
                {
                    // インデックスの割り振り 中央
                    mainParty.SetBattlerIndex(2);
                    foreach (var battlerUnit in opponentUnits)
                    {
                        // インデックスの割り振り 中央
                        battlerUnit.SetBattlerIndex(2);
                        // メインのみ
                        var m1 = new BattleSceneInfo
                        {
                            ActorBattlerInfos = battlerUnit.UnitInfo.BattlerInfos.FindAll(a => a.ActorInfo != null),
                            EnemyInfos = mainParty.UnitInfo.BattlerInfos.FindAll(a => a.EnemyData != null)
                        };
                        list.Add(m1);
                        var enemyReach = GetHexReach(battlerUnit.HexField,1,true);
                        // メインと隣接あり
                        var nearFriends = friends.FindAll(a => a != mainParty && enemyReach.Find(b => a.OnField(b.X,b.Y)) != null && _reachAreas.Find(b => a.OnField(b.X,b.Y)) != null);
                        var battlerIndex = 1;
                        foreach (var nearFriend in nearFriends)
                        {
                            // インデックスの割り振り 左右
                            nearFriend.SetBattlerIndex(battlerIndex);
                            if (nearFriends.Count == 2)
                            {
                                battlerIndex = 3;
                            }
                            var battlerInfos = new List<BattlerInfo>();
                            battlerInfos.AddRange(mainParty.UnitInfo.BattlerInfos);
                            battlerInfos.AddRange(nearFriend.UnitInfo.BattlerInfos);
                            var m2 = new BattleSceneInfo
                            {
                                EnemyInfos = battlerInfos.FindAll(a => a.EnemyData != null),
                                ActorBattlerInfos = battlerUnit.UnitInfo.BattlerInfos.FindAll(a => a.ActorInfo != null)
                            };
                            list.Insert(0,m2);
                        }
                        if (nearFriends.Count == 2)
                        {
                            nearFriends[0].SetBattlerIndex(1);
                            nearFriends[1].SetBattlerIndex(3);
                            var battlerInfos = new List<BattlerInfo>();
                            battlerInfos.AddRange(mainParty.UnitInfo.BattlerInfos);
                            battlerInfos.AddRange(nearFriends[0].UnitInfo.BattlerInfos);
                            battlerInfos.AddRange(nearFriends[1].UnitInfo.BattlerInfos);
                            // 両サイドどちらも
                            var m3 = new BattleSceneInfo
                            {
                                EnemyInfos = battlerInfos.FindAll(a => a.EnemyData != null),
                                ActorBattlerInfos = battlerUnit.UnitInfo.BattlerInfos.FindAll(a => a.ActorInfo != null)
                            };
                            list.Insert(0,m3);
                        }
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
            var basement = OnFieldInfos.Find(a => a.IsBasementUnit);
            if (basement != null)
            {
                var term = CurrentStage.GetTurnTeamInfo();
                basement.Conquer(term.TeamId.Value);
                if (basement.Master.Param1 == 1)
                {
                    var openUnitInfos = GetStageHexUnitInfos(basement.Master.Param2,CurrentData.PlayerInfo.ClearCount);
                    CurrentStage.AddStageSymbolDates(openUnitInfos);
                }
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
            var alcana = OnFieldInfos.Find(a => a.IsAlcanaUnit);
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

        public List<GetItemInfo> GetItemOpen()
        {
            UnitActEnd();
            var getItem = OnFieldInfos.Find(a => a.IsGetItemUnit);
            if (getItem != null)
            {
                foreach (var getItemInfo in getItem.GetItemInfos)
                {
                    AddGetItemInfo(getItemInfo);
                }
                // 消失
                CurrentStage.RemoveHexUnitInfo(getItem);
                return getItem.GetItemInfos;
            }
            return null;
        }

        public bool GetSelectActor()
        {
            UnitActEnd();
            var selectActor = OnFieldInfos.Find(a => a.IsSelectActorUnit);
            if (selectActor != null)
            {
                /*
                foreach (var getItemInfo in selectActor.GetItemInfos)
                {
                    AddGetItemInfo(getItemInfo);
                }
                */
                // 消失
                CurrentStage.RemoveHexUnitInfo(selectActor);
                return AddSelectActorInfos().Count > 0;
                //return selectActor.GetItemInfos;
            }
            return false;
        }

        public List<ActorInfo> AddSelectActorInfos()
        {
            // 未加入の仲間
            var actorDates = DataSystem.Actors.Where(a => PartyInfo.ActorInfos.Find(b => a.Value.Id == b.ActorId.Value) == null).ToList();
            var actorInfos = new List<ActorInfo>();
            foreach (var actorDate in actorDates)
            {
                actorInfos.Add(new ActorInfo(actorDate.Value));
            }
            return actorInfos;
        }

        public StrategySceneInfo GachaOpen()
        {
            var getItem = OnFieldInfos.Find(a => a.IsGachaUnit);
            // 消失
            CurrentStage.RemoveHexUnitInfo(getItem);

            var list = new List<GetItemInfo>();
            // 確定で未加入キャラ1名は当選
            var actorDates = DataSystem.Actors.Where(a => PartyInfo.ActorInfos.Find(b => a.Value.Id == b.ActorId.Value) == null).ToList();
            var actorInfos = new List<ActorInfo>();
            while (actorInfos.Count < 1)
            {
                var rand = UnityEngine.Random.Range(0,actorDates.Count() - 1);
                if (actorInfos.Find(a => a.ActorId.Value == actorDates[rand].Value.Id) == null)
                {
                    actorInfos.Add(new ActorInfo(actorDates[rand].Value));
                }
            }
            foreach (var actorInfo in actorInfos)
            {
                var addActorGetItemData = new GetItemData
                {
                    Type = GetItemType.AddActor,
                    Param1 = actorInfo.ActorId.Value
                };
                var addActorGetItem = new GetItemInfo(addActorGetItemData);
                list.Add(addActorGetItem);
            }
            // 残り8枠を抽選
            // キャラ 3% ,魔法 10%, Nu 87%
            while (list.Count < 10)
            {
                var itemRand = UnityEngine.Random.Range(0,100);
                if (itemRand < 3)
                {
                    var rand = UnityEngine.Random.Range(0,actorDates.Count() - 1);
                    if (actorInfos.Find(a => a.ActorId.Value == actorDates[rand].Value.Id) == null)
                    {
                        var actorInfo = new ActorInfo(actorDates[rand].Value);
                        actorInfos.Add(actorInfo);
                        var addActorGetItemData = new GetItemData
                        {
                            Type = GetItemType.AddActor,
                            Param1 = actorInfo.ActorId.Value
                        };
                        var addActorGetItem = new GetItemInfo(addActorGetItemData);
                        list.Add(addActorGetItem);
                    }
                } else
                if (itemRand >= 3 && itemRand < 13)
                {
                    var getItemData = MakeSkillGetItemInfo();
                    if (getItemData != null && list.Find(a => a.GetItemType == GetItemType.Skill && a.Param1 == getItemData.Param1) == null)
                    {
                        list.Add(new GetItemInfo(getItemData));
                    }
                } else
                {
                    var numinosGetItem = MakeEnemyRandomNuminos(10);
                    list.Add(numinosGetItem);
                }
            }
            UseActPoint();
            var strategySceneInfo = new StrategySceneInfo
            {
                ActorInfos = actorInfos,
                GetItemInfos = list
            };
            return strategySceneInfo;
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

        public List<UnitInfo> DepatureEditUnitInfos()
        {
            // 最大5部隊まで
            var list = new List<UnitInfo>();
            var hexUnitInfos = CurrentStage.GetTurnTeamInfo().DepatuerInfos;
            for (int i = 0;i < 5;i++)
            {
                if (hexUnitInfos.Count > i)
                {
                    list.Add(hexUnitInfos[i].UnitInfo);
                } else
                {
                    var temp = new UnitInfo();
                    temp.SetBattlers(new List<BattlerInfo>(){new BattlerInfo(),new BattlerInfo()});
                    temp.Index.SetValue(i+1);
                    list.Add(temp);
                }
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
            var basements = CurrentStage.FieldHexList.FindAll(a => a.IsBasementUnit && a.TeamId.Value == GetTurnTeam().TeamId.Value);
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

        public bool StageClear()
        {
            // チュートリアル以外はマス情報を引き継ぐ
            if (CurrentStage.StageId.Value < 1000)
            {
                var next = DataSystem.Stages.Find(a => a.Id > CurrentStage.StageId.Value && a.Id < 1000);
                if (next != null)
                {
                    MakeStageInfo(next.Id,false);
                }
                return true;
            }
            return false;
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
            var basement = OnFieldInfos.Find(a => a.IsBasementUnit && a.TeamId.Value == GetTurnTeam().TeamId.Value);
            if (basement != null)
            {
                list.Insert(1,DepartureCommand);
                list.Insert(2,UnitEditCommand);
                list.Insert(3,ReturnCommand);
            }
            // 敵と隣接している
            var battler = SelectingUnit;
            var reachAreas = GetHexReach(battler.HexField,1,true);
            var battlerUnits = CurrentStage.OpponentUnitInfos().FindAll(a => reachAreas.Find(b => a.OnField(b.X,b.Y)) != null);
            var enemyInfos = battlerUnits.FindAll(a => a.UnitInfo != null);
            if (enemyInfos.Count > 0)
            {
                list.Insert(0,BattleCommand);
            }
            // 同時に敵拠点がある場合
            var conq = OnFieldInfos.Find(a => a.IsBasementUnit && a.TeamId.Value != GetTurnTeam().TeamId.Value);
            if (conq != null)
            {
                list.Insert(0,ConquerCommand);
            }
            list.Add(WaitCommand);
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
            var battler = CurrentStage.OnFieldTurnUnitInfos().Find(a => a.IsBattlerUnit);
            var hexUnits = OnFieldInfos;
            if (hexUnits.Find(a => a.IsBasementUnit && a.TeamId.Value != battler.TeamId.Value) != null)
            {
                list.Insert(0,ConquerCommand);
            } else
            // 味方拠点の上であれば帰還
            if (hexUnits.Find(a => a.IsBasementUnit && a.TeamId.Value == battler.TeamId.Value) != null)
            {
                list.Insert(0,ReturnCommand);
            }
            // 宝箱の上
            if (hexUnits.Find(a => a.IsGetItemUnit) != null)
            {
                list.Insert(0,GetItemCommand);
            }
            // 召喚の上
            if (hexUnits.Find(a => a.IsSelectActorUnit) != null)
            {
                list.Insert(0,SelectActorCommand);
            }
            // イベントの上
            if (hexUnits.Find(a => a.IsAlcanaUnit) != null)
            {
                list.Insert(0,EventCommand);
            }
            // 奇跡の上
            if (hexUnits.Find(a => a.IsGachaUnit) != null)
            {
                list.Insert(0,GachaCommand);
            }
            bool enable(SystemData.CommandData a)
            {
                if (a == BattleCommand)
                {
                    // 隣接している
                    var battler = SelectingUnit;
                    var reachAreas = GetHexReach(battler.HexField,1,true);
                    var battlerUnits = CurrentStage.OpponentUnitInfos().FindAll(a => reachAreas.Find(b => a.OnField(b.X,b.Y)) != null);
                    var enemyInfos = battlerUnits.FindAll(a => a.UnitInfo != null);
                    return enemyInfos.Count > 0;
                } else
                if (a == WaitCommand)
                {
                    // 味方が占有している
                    if (CurrentStage.OnFieldTurnUnitInfos().Find(a => a.Index.Value != SelectingHexUnitId.Value && a.IsBattlerUnit) != null)
                    {
                        return false;
                    }
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
            // 同時に拠点がある場合
            var basement = OnFieldInfos.Find(a => a.IsBasementUnit && a.TeamId.Value == GetTurnTeam().TeamId.Value);
            if (basement != null)
            {
                list.Insert(0,UnitEditCommand);
            }
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
        public SystemData.CommandData GetItemCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "GetItem");
        public SystemData.CommandData SelectActorCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "SelectActor");
        public SystemData.CommandData GachaCommand => DataSystem.System.TacticsCommandData.Find(a => a.Key == "Gacha");
    }

}
