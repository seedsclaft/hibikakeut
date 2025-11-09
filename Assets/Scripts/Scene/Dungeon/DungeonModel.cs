using System.Linq;
using System.Collections.Generic;
using Ariadne;
using UnityEngine;

namespace Ryneus
{
    public class DungeonModel : BaseModel
    {
        private MoveController _moveController;
        public ParameterInt SelectIndex = new(-1);
        public DungeonModel(MoveController moveController)
        {
            _moveController = moveController;
        }

        public Material DungeonSkyboxMaterial()
        {
            var skyboxName = DungeonSkyboxName();
            return skyboxName != "" ? ResourceSystem.LoadSkyboxMaterial(skyboxName) : null;
        }

        public int SelectedCharacterIndex()
        {
            var index = 0;
            var idx = 0;
            foreach (var battlerInfo in PartyUnit())
            {
                if (SelectIndex.Value == idx && battlerInfo.ActorInfo != null)
                {
                    index = battlerInfo.Index.Value;
                }
                idx++;
            }
            return index;
        }

        public void SwapSelectIndex(int selectIndex)
        {
            var toActorId = -1;
            var idx = 0;
            foreach (var battlerInfo in PartyUnit())
            {
                if (selectIndex == idx && battlerInfo.ActorInfo != null)
                {
                    toActorId = battlerInfo.ActorInfo.ActorId.Value;
                }
                idx++;
            }
            //if (toActorId > -1)
            {
                CurrentDeckInfo.SwapBattler(SelectIndex.Value + 1, toActorId, selectIndex + 1);
            }
            SelectIndex.SetValue(-1);
        }

        // 開示マスを復帰
        public void UpdateTraverses()
        {
            if (CurrentStage == null)
            {
                return;
            }

            var traversDates = PartyInfo.GetDungeonTraverse(CurrentStage.StageId.Value);
            if (traversDates != null)
            {
                TraverseManager.Instance.UpdateTraverses(CurrentStage.StageId.Value,traversDates);
            }
        }

        public void TraverseRegeon(int regeonNo)
        {
            var dungeonFloor = DataSystem.FindDungeonFloor(CurrentStage.StageId.Value);
            var playerDungeonId = PlayerPosition.Instance.currentDungeonId;
            var traverses = TraverseManager.Instance.GetDungeonTraverseData(playerDungeonId);
            foreach (var mapInfo in dungeonFloor.mapInfo)
            {
                if (mapInfo.regeonNo == regeonNo)
                {
                    var X = mapInfo.eventId % dungeonFloor.floorSizeVertical;
                    var Y = mapInfo.eventId / dungeonFloor.floorSizeVertical;
                    string key = dungeonFloor.floorId.ToString() + "-" + X.ToString() + "-" + Y.ToString();
                    LogOutput.Log(key);
                    traverses.traverseDict[key] = true;
                }
            }
            AddDungeonTraverse();
            UpdateTraverses();
        }

        public void SetPlayerPosition()
        {
            // 位置保存情報を復帰
            _moveController.SetPlayerPosition(CurrentDeckInfo.PositionX.Value,CurrentDeckInfo.PositionY.Value,CurrentDeckInfo.Direction.Value);
        }

        public void UpdateEventObjects()
        {
            // フラグ情報でイベント表示を制御
            var endStageEvents = EndStageEvents();
            foreach (var endStageEvent in endStageEvents)
            {
                if (endStageEvent.Type == StageEventType.GetItem || endStageEvent.Type == StageEventType.GetArtifact || endStageEvent.Type == StageEventType.GetSkill || endStageEvent.Type == StageEventType.SelectAddActor)
                {
                    _moveController.SetEventEndDeactiveEventObj(endStageEvent.PositionX, endStageEvent.PositionY);
                } else
                if (endStageEvent.Type == (StageEventType)0)
                {
                    _moveController.SetDeactiveParentObj(endStageEvent.PositionX, endStageEvent.PositionY);
                }else
                {
                    _moveController.SetDeactiveChildObj(endStageEvent.PositionX, endStageEvent.PositionY);
                }
            }
        }

        public void AddEventNotFlag()
        {
            var displayEvents = NotEndStageEvents().FindAll(a => a.Type == StageEventType.AddEventNotFlag);
            foreach (var displayEvent in displayEvents)
            {
                var position = StageEventDates.Find(a => a.Param == displayEvent.Param && a != displayEvent);
                if (position != null)
                {
                    _moveController.SetDeactiveParentObj(position.PositionX, position.PositionY);
                }
            }
        }

        public void DisplayAddEventNotFlag(StageEventData stageEvent)
        {
            _moveController.SetActiveEventObj(stageEvent.PositionX, stageEvent.PositionY);
        }

        public void DungeonBusy(bool busy)
        {
            _moveController.isInDungeon = !busy;
        }

        public List<BattlerInfo> PartyUnit()
        {
            return PartyInfo.DeckEditBattlerInfos();
        }

        public bool CommandMoveEnd()
        {
            if (CurrentDeckInfo == null)
            {
                return false;
            }
            var playerDungeonId = PlayerPosition.Instance.currentDungeonId;
            var playerPosition = PlayerPosition.Instance.playerPos;
            var playerDirection = PlayerPosition.Instance.direction;
            var lastPositionX = CurrentDeckInfo.PositionX;
            var lastPositionY = CurrentDeckInfo.PositionY;
            if (lastPositionX.Value != playerPosition.x || lastPositionY.Value != playerPosition.y)
            {
                CurrentDeckInfo.SetPosition(playerDungeonId, playerPosition.x, playerPosition.y, (int)playerDirection);
                PartyInfo.UpdateDungeonResumeInfo(CurrentStage.Master.StageNo, playerDungeonId, playerPosition.x, playerPosition.y, (int)playerDirection);
                if (IsActiveDungeon() && !CurrentStage.Cleared.Value)
                {
                    // ランダムエンカウントフラグ加算
                    int flag = Random.Range(CurrentStage.Master.EncountMin, CurrentStage.Master.EncountMax);
                    var encountValue = (int)(flag * CurrentDeckInfo.EncountRate.Value);
                    CurrentDeckInfo.Encount.GainValue(encountValue, 0, 100);
                    CurrentDeckInfo.EncountRateTurn.GainValue(-1, 0);
                    if (CurrentDeckInfo.EncountRateTurn.Value == 0)
                    {
                        CurrentDeckInfo.EncountRate.SetValue(1);
                    }

                    // 歩数を加算
                    CurrentDeckInfo.TurnCount.GainValue(1);
                }
                //SaveAutoFile();
                return true;
            }
            return false;
        }

        public int CheckHpHeal()
        {
            var hpHeal = PartyInfo.MoveHpHealValue();
            foreach (var actorInfo in PartyInfo.CurrentDeckActorInfos())
            {
                if (actorInfo == null)
                {
                    continue;
                }
                actorInfo.ChangeHp(actorInfo.CurrentHp.Value + hpHeal);
            }
            return hpHeal;
        }

        public Vector2Int GetForwardPosition()
        {
            return _moveController.GetForwardPosition();
        }

        public Vector2Int GetCurrentPosition()
        {
            return PlayerPosition.Instance.playerPos;
        }

        public bool CheckDirectionEvent()
        {
            var position = GetForwardPosition();
            var stageEvent = StageEvents(EventTiming.Dungeon, position.x, position.y);
            return stageEvent.Count > 0 && (stageEvent[0].Type == StageEventType.GetItem || stageEvent[0].Type == StageEventType.GetArtifact || stageEvent[0].Type == StageEventType.GetSkill);
        }

        public bool CheckCurrentPositionEvent()
        {
            var position = GetCurrentPosition();
            var stageEvent = StageEvents(EventTiming.Dungeon, position.x, position.y);
            return stageEvent.Count > 0 && (stageEvent[0].Type == StageEventType.ExitDungeon || stageEvent[0].Type == StageEventType.MoveDungeonFloor || stageEvent[0].Type == StageEventType.AdvStart);
        }

        public bool BattleFieldEncountZero()
        {
            if (CurrentStage == null)
            {
                return false;
            }
            if (CurrentStage.Master.Category != StageCategory.BattleField)
            {
                return false;
            }
            var stageEvents = StageEvents(EventTiming.Dungeon);
            return stageEvents.Find(a => a.Type == StageEventType.ForceBattle || a.Type == StageEventType.ForceBossBattle) == null;
        }

        public bool EndDungeonByTurnCountValue(int value)
        {
            return false;
            /* ターン制御しない
            if (CurrentStage == null)
            {
                return false;
            }
            return PartyInfo.CurrentDeckInfo.TurnCount.Value == value;
            */
        }

        public bool EndDungeonByTurnCount()
        {
            return false;
            /* ターン制御しない
            if (CurrentStage == null)
            {
                return false;
            }
            return PartyInfo.CurrentDeckInfo.TurnCount.Value < 0;
            */
        }

        public bool EncountEnemy()
        {
            return CurrentDeckInfo == null ? false : CurrentDeckInfo.Encount.Value >= 100;
        }

        public void ResetEncountValue()
        {
            if (CurrentDeckInfo == null)
            {
                return;
            }
            CurrentDeckInfo.Encount.SetValue(0);
        }

        public List<BattlerInfo> RandumTroopInfos(int plusLv = 0)
        {
            var troopInfo = new TroopInfo(-1);
            troopInfo.MakeEnemyRandomTroopDates(CurrentStage.Master.StageLv + plusLv, CurrentStage.Master.RandomTroopEnemyRates);
            return troopInfo.BattlerInfos;
        }

        public List<BattlerInfo> ForceBattleTroopInfos(int troopId, int plusLv = 0)
        {
            if (troopId != -1)
            {
                var troopInfo = new TroopInfo(troopId);
                troopInfo.MakeEnemyTroopDates(CurrentStage.Master.StageLv + plusLv);
                return troopInfo.BattlerInfos;
            }
            return RandumTroopInfos(plusLv);
        }

        public List<ActorInfo> AddSelectActorInfos(List<int> limitRanks)
        {
            // 未加入の仲間
            var actorDates = DataSystem.Actors.Where(a => PartyInfo.ActorInfos.Find(b => a.Value.Id == b.ActorId.Value) == null).ToList();
            var actorInfos = new List<ActorInfo>();
            foreach (var actorDate in actorDates)
            {
                if (limitRanks.Count > 0 && !limitRanks.Contains(actorDate.Value.Rank))
                {
                    continue;
                }
                actorInfos.Add(new ActorInfo(actorDate.Value));
            }
            return actorInfos;
        }

        public bool CanUseRecoveryHeal()
        {
            var notLimited = PartyInfo.CurrentDeckActorInfos().FindAll(a => a.CurrentHp.Value < a.MaxHp);
            return notLimited.Count > 0 && CurrentDeckInfo.RecoveryCount.Value > 0;
        }

        public void UseRecoveryHeal()
        {
            if (CurrentDeckInfo.RecoveryCount.Value <= 0)
            {
                return;
            }
            PartyInfo.UseRecoveryHeal();
            CurrentDeckInfo.RecoveryCount.GainValue(-1, 0);
        }

        public void TurnOver()
        {
            PartyInfo.PartyStatInfo.BattleScore.GainValue(-20, 0);
        }

        public void SaveBgmTiming()
        {
            var timeStamp = SoundManager.Instance.CurrentTimeStamp();
            CurrentDeckInfo.DungeonBgmTimeStamp.SetValue(timeStamp);
        }

        public float DungeonBgmTimeStamp()
        {
            if (CurrentDeckInfo != null && CurrentDeckInfo.DungeonBgmTimeStamp != null)
            {
                return CurrentDeckInfo.DungeonBgmTimeStamp.Value;
            }
            return 0;
        }

        public void DamageFloor(int damage)
        {
            PartyInfo.DamageFloor(damage);
        }

        public void CursedParty()
        {
            PartyInfo.CursedParty(true);
        }

        public void EndCursedParty()
        {
            PartyInfo.CursedParty(false);
        }

        public bool CheckGameover()
        {
            return PartyUnit().Find(a => a.Hp.Value > 0) == null;
        }

        public bool IsRouteMode()
        {
            return CurrentDeckInfo.RoutePaths.Count > 0;
        }

        public void ClearRoute()
        {
            if (IsRouteMode())
            {
                CurrentDeckInfo.RoutePaths.RemoveAt(CurrentDeckInfo.RoutePaths.Count - 1);
            }
        }

        public void ClearRouteAll()
        {
            if (IsRouteMode())
            {
                CurrentDeckInfo.RoutePaths.Clear();
            }
        }

        public InputKeyType RouteModeInputKeyType()
        {
            if (IsRouteMode())
            {
                var position = GetCurrentPosition();
                var direction = PlayerPosition.Instance.direction;
                var route = CurrentDeckInfo.RoutePaths[^1];
                switch (direction)
                {
                    case DungeonDir.North:
                        if (position.y + 1 == route.Y)
                        {
                            return InputKeyType.Up;
                        }
                        if (position.y - 1 == route.Y)
                        {
                            return InputKeyType.Down;
                        }
                        if (position.x + 1 == route.X)
                        {
                            return InputKeyType.Right;
                        }
                        if (position.x - 1 == route.X)
                        {
                            return InputKeyType.Left;
                        }
                        break;
                    case DungeonDir.East:
                        if (position.y + 1 == route.Y)
                        {
                            return InputKeyType.Left;
                        }
                        if (position.y - 1 == route.Y)
                        {
                            return InputKeyType.Right;
                        }
                        if (position.x + 1 == route.X)
                        {
                            return InputKeyType.Up;
                        }
                        if (position.x - 1 == route.X)
                        {
                            return InputKeyType.Down;
                        }
                        break;
                    case DungeonDir.South:
                        if (position.y - 1 == route.Y)
                        {
                            return InputKeyType.Up;
                        }
                        if (position.y + 1 == route.Y)
                        {
                            return InputKeyType.Down;
                        }
                        if (position.x - 1 == route.X)
                        {
                            return InputKeyType.Right;
                        }
                        if (position.x + 1 == route.X)
                        {
                            return InputKeyType.Left;
                        }
                        break;
                    case DungeonDir.West:
                        if (position.y + 1 == route.Y)
                        {
                            return InputKeyType.Right;
                        }
                        if (position.y - 1 == route.Y)
                        {
                            return InputKeyType.Left;
                        }
                        if (position.x + 1 == route.X)
                        {
                            return InputKeyType.Down;
                        }
                        if (position.x - 1 == route.X)
                        {
                            return InputKeyType.Up;
                        }
                        break;
                }
            }
            return InputKeyType.None;
        }

        public List<SystemData.CommandData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var status = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(19006),
                Key = "Status"
            };
            list.Add(status);
            var artifact = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(37000),
                Key = "Artifact"
            };
            list.Add(artifact);
            var @return = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(19100),
                Key = "Return"
            };
            list.Add(@return);
            var option = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(19101),
                Key = "Option"
            };
            list.Add(option);
            var menuCommand = new SystemData.CommandData
            {
                Id = 3,
                Name = DataSystem.GetText(19102),
                Key = "Help"
            };
            list.Add(menuCommand);
            var dictionaryCommand = new SystemData.CommandData
            {
                Id = 11,
                Name = DataSystem.GetText(19103),
                Key = "Dictionary"
            };
            list.Add(dictionaryCommand);
            var saveCommand = new SystemData.CommandData
            {
                Id = 4,
                Name = DataSystem.GetText(19104),
                Key = "Save"
            };
            list.Add(saveCommand);
            var titleCommand = new SystemData.CommandData
            {
                Id = 5,
                Name = DataSystem.GetText(19106),
                Key = "Title"
            };
            list.Add(titleCommand);
            return list;
        }
    }
}