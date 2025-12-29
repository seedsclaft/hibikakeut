using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ryneus.Dungeon;
using UnityEngine;

namespace Ryneus
{
    public class DungeonPresenter : BasePresenter
    {
        DungeonModel _model = null;
        DungeonView _view = null;

        private bool _busy = true;
        private bool _battleBusy = false;
        private bool _routeMode = true;
        private int _routeMoveFailCount = 0;
        private bool _checkTurnOver = false;

        private List<StageEventData> _thisTurnStageEvents = new();
        public DungeonPresenter(DungeonView view)
        {
            _view = view;
            SetView(_view);
            _model = new DungeonModel(view.MoveController);
            SetModel(_model);

            Initialize();
        }

        private async Task Initialize(float timeStamp = 0)
        {
            //_view.SetHelpWindow();
            _view.SetEvent((type) => UpdateCommand(type));

            var ev = CheckEnterDungeonEvents();
            if (!ev && _model.BattleVictory.Value)
            {
                ev = CheckBattleVictoryEvents();
            }

            _view.SetPartyUnitList(MakeListData(_model.PartyUnit(), -1));
            _view.SetActiveStageInfo(_model.IsActiveDungeon());
            // ダンジョン生成
            _view.CommandChangeDungeon(_model.DungeonPrefabName());
            _view.ChangeSkybox(_model.DungeonSkyboxMaterial());
            _view.SetupDungeon();
            _model.UpdateTraverses();
            _model.SetPlayerPosition();
            _model.UpdateEventObjects();
            CommandRefresh();
            // 未読の非表示マスを管理
            _model.AddEventNotFlag();
            //_model.SaveAutoFile();
            // 移動したあとのイベント
            _view.CallSystemCommand(Base.CommandType.CloseLoading);
            if (ev)
            {
                return;
            }
            await PlayDungeonBgm(_model.DungeonBgmTimeStamp());
            // 戦場ステージでバトルイベントが0になった時
            if (_model.BattleFieldEncountZero())
            {
                CommandEncountZero();
                return;
            }
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            LogOutput.Log(viewEvent.ViewCommandType.CommandType);
            if (_busy || _view.AnimationBusy)
            {
                switch (viewEvent.ViewCommandType.CommandType)
                {
                    case CommandType.MoveEnd:
                        CommandRouteMoveEnd();
                        break;
                    case CommandType.MoveEndFinish:
                        CommandRouteMoveEndFinish();
                        break;
                    case CommandType.RouteModeEnd:
                        CommandRouteModeEnd();
                        break;
                    case CommandType.UseItemHeal:
                        CommandUseItemHeal((int)viewEvent.Template);
                        break;
                    case CommandType.MoveDirection:
                        CommandMoveDirection((int)viewEvent.Template);
                        break;
                }
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Dungeon)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.MoveEnd:
                    CommandMoveEnd();
                    break;
                case CommandType.DecideDirectEvent:
                    CommandDecideDirectEvent();
                    break;
                case CommandType.CheckRemainTurn:
                    CommandCheckRemainTurn();
                    break;
                case CommandType.Heal:
                    CommandHeal();
                    break;
                case CommandType.DungeonMap:
                    CommandDungeonMap();
                    break;
                case CommandType.UseItem:
                    CommandUseItem();
                    break;
                case CommandType.SelectCharacter:
                    CommandSelectCharacter((int)viewEvent.Template);
                    break;
                case CommandType.EndFormation:
                    CommandEndFormation();
                    break;
                case CommandType.Aritifact:
                    CommandAritifact();
                    break;
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
            }
        }

        private void CommandMoveEnd()
        {
            _busy = false;
            // 移動したか
            var moved = _model.CommandMoveEnd();
            if (moved)
            {
                var hpHeal = _model.CheckHpHeal();
                if (hpHeal > 0 && !_model.PartyInfo.Cursed.Value)
                {
                    _view.StartHeal(hpHeal);
                    _view.SetPartyUnitList(MakeListData(_model.PartyUnit(), -1));
                }
            }

            _model.AddDungeonTraverse();
            CommandRefresh();
            // 未読の非表示マスを管理
            _model.AddEventNotFlag();

            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            var stageEvent = GetStageEventData(EventTiming.Dungeon, playerPosition.x, playerPosition.y);

            _thisTurnStageEvents.Clear();
            // イベントがある場合
            if (stageEvent != null)
            {
                CheckStageEvent(moved);
                return;
            }

            // ターン数確認
            CommandTurnOverEvent(moved);
        }

        private void CommandRouteMoveEnd()
        {
            // 移動したか
            var moved = _model.CommandMoveEnd();
            if (moved)
            {
                _model.ClearRoute();
                _routeMoveFailCount = 0;
                var hpHeal = _model.CheckHpHeal();
                if (hpHeal > 0 && !_model.PartyInfo.Cursed.Value)
                {
                    _view.StartHeal(hpHeal);
                    _view.SetPartyUnitList(MakeListData(_model.PartyUnit(), -1));
                }
            } else
            {
                if (_routeMode)
                {
                    _routeMoveFailCount++;
                    if (_routeMoveFailCount > 1)
                    {
                        _routeMoveFailCount = 0;
                        _model.ClearRouteAll();
                    }
                }
            }

            _model.AddDungeonTraverse();
            CommandRefresh();
            // 未読の非表示マスを管理
            _model.AddEventNotFlag();

            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            var stageEvent = GetStageEventData(EventTiming.Dungeon, playerPosition.x, playerPosition.y);

            _thisTurnStageEvents.Clear();
            // イベントがある場合
            if (stageEvent != null)
            {
                CheckStageEvent(moved);
                return;
            }

            // ターン数確認
            CommandTurnOverEvent(moved);
        }

        private void CommandRouteMoveEndFinish()
        {
            CheckRouteMode();
        }

        private void CommandRouteModeEnd()
        {
            if (_routeMode)
            {
                _routeMoveFailCount = 0;
                _model.ClearRouteAll();
                CheckRouteMode();
            }
        }

        private void CommandUseItemHeal(int heal)
        {
            // 回復できない
            if (_model.PartyInfo.Cursed.Value)
            {
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(10131));
                _view.CommandCallCaution(cautionInfo);
                return;
            }
            _view.StartHeal(heal);
            _view.SetPartyUnitList(MakeListData(_model.PartyUnit(), -1));
            CommandRefresh();
        }

        private void CheckStageEvent(bool moved)
        {
            _model.DungeonBusy(true);
            // イベントマスの場合
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            CheckEventData(moved, playerPosition, () =>
            {
                _model.DungeonBusy(false);
                // ターン数確認
                CommandTurnOverEvent(moved);
                if (_routeMode)
                {
                    CheckRouteMode();
                }
            });
        }

        private void CheckEncount()
        {
            if (!_model.IsEncountEnemy())
            {
                return;
            }
            _busy = true;
            _battleBusy = true;
            _model.DungeonBusy(true);
            _model.ResetEncountValue();
            // ダンジョンの再開時間を記憶
            _model.SaveBgmTiming();
            _model.EncountEnemy();
            var battleSceneInfo = new BattleSceneInfo
            {
                ActorInfos = _model.PartyInfo.CurrentDeckActorInfos(),
                EnemyInfos = _model.RandumTroopInfos(),
            };
            PlayBattleBgm();
            _view.CommandChangeViewToTransition(null);
            //_view.ChangeUIActive(false);
            _view.CommandSceneChange(Scene.Battle, battleSceneInfo);
            SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
        }

        private void CommandEncountZero()
        {
            SoundManager.Instance.PlayStaticSe(SEType.PlayStart);
            _model.DungeonBusy(true);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10190), (a) =>
            {
                ReturnDungeon();
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void StartEventProcesses(List<StageEventData> enterDungeonEvents, int count, int index, bool moved)
        {
            var stageEventCount = count;
            var stageEventIndex = index;
            CommandStageEvent(enterDungeonEvents[stageEventIndex], moved, () =>
            {
                stageEventCount--;
                stageEventIndex++;
                if (stageEventCount > 0)
                {
                    StartEventProcesses(enterDungeonEvents, stageEventCount, stageEventIndex, moved);
                } else
                {
                    _view.CallSystemCommand(Base.CommandType.SceneShowUI);
                    _busy = false;
                    _model.DungeonBusy(false);
                    CommandRefresh();
                    // ターン数確認
                    CommandTurnOverEvent(false);
                }
            });
        }

        private bool CheckEnterDungeonEvents()
        {
            var enterDungeonEvents = _model.CheckEnterDungeonEvents();
            if (enterDungeonEvents.Count > 0)
            {
                _view.CallSystemCommand(Base.CommandType.SceneHideUI);
                StartEventProcesses(enterDungeonEvents, enterDungeonEvents.Count, 0, false);
            }
            return enterDungeonEvents.Count > 0;
        }

        private bool CheckBattleVictoryEvents()
        {
            var battleVictoryEvents = _model.CheckBattleVictoryEvents();
            if (battleVictoryEvents.Count > 0)
            {
                _view.CallSystemCommand(Base.CommandType.SceneHideUI);
                StartEventProcesses(battleVictoryEvents, battleVictoryEvents.Count, 0, false);
            }
            return battleVictoryEvents.Count > 0;
        }

        private void CommandTurnOverEvent(bool moved)
        {
            // ターン数が10の場合
            if (_model.EndDungeonByTurnCountValue(10))
            {
                CommandTurnOverBeforeTen(moved);
            }

            // ターン数が0の場合
            if (_model.EndDungeonByTurnCount())
            {
                CommandTurnOver(moved);
            }

            if (moved)
            {
                CheckEncount();
            }
        }

        private void CommandTurnOverBeforeTen(bool moved)
        {
            // 評価値が減少まであと10ターン
            var cautionInfo = new CautionInfo();
            cautionInfo.SetTitle(DataSystem.GetText(10181));
            _view.CommandCallCaution(cautionInfo);
        }

        private void CommandTurnOver(bool moved)
        {
            if (!moved)
            {
                return;
            }
            // 評価値が減少
            SoundManager.Instance.PlayStaticSe(SEType.Deny);
            _model.TurnOver();
            _view.SeekTweens();
            _view.MinusVictoryBonus(-0.2f);
            _view.MinusEvaluate(-1);
            _model.PartyInfo.EvaluationValue.GainValue(-1, 0);
            var cautionInfo = new CautionInfo();
            cautionInfo.SetTitle(DataSystem.GetText(10180));
            _view.CommandCallCaution(cautionInfo);
            // 強制帰還
            /*
            if (_model.PartyInfo.Cursed.Value)
            {
                if (moved)
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Deny);
                    var cautionInfo = new CautionInfo();
                    cautionInfo.SetTitle(DataSystem.GetText(10132));
                    _view.CommandCallCaution(cautionInfo);
                    _model.PartyInfo.BattleScore.GainValue(-50, 0);
                }
                _model.DungeonBusy(false);
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.PlayStart);
            _model.DungeonBusy(true);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10110), (a) =>
            {
                RetunrDungeon();
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
            */
        }

        private void ReturnDungeon()
        {
            var checkNotSeekPeriod = _model.CheckNotSeekPeriod();
            if (checkNotSeekPeriod != null)
            {
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetReplaceText(10200, checkNotSeekPeriod.Master.Name));
                _view.CommandCallCaution(cautionInfo);
            }
            _model.ReturnDungeon();
            _view.CallSystemCommand(Base.CommandType.MapClear);
            var mainMenuSceneInfo = new MainMenuSceneInfo
            {
                PeriodAnimation = true
            };
            var periodItemInfos = _model.PeriodGetItemInfos();
            if (periodItemInfos.Count > 0)
            {
                var strategySceneInfo = new StrategySceneInfo
                {
                    ActorInfos = _model.PartyInfo.CurrentDeckActorInfos(),
                    InBattle = false,
                    GetItemInfos = periodItemInfos,
                    ReturnScene = Scene.MainMenu,
                    ReturnMainMenuSceneParam = mainMenuSceneInfo
                };
                _view.CommandSceneChange(Scene.Strategy, strategySceneInfo);
                return;
            }
            _view.CommandSceneChange(Scene.MainMenu, mainMenuSceneInfo);
        }

        private void CommandDecideDirectEvent()
        {
            // 正面にイベントがあるか
            var directionEvent = _model.CheckDirectionEvent();
            if (!directionEvent)
            {
                _thisTurnStageEvents.Clear();
                var currentPositionEvent = _model.CheckCurrentPositionEvent();
                if (currentPositionEvent)
                {
                    var position = _model.GetCurrentPosition();
                    CheckEventData(true, position, () =>
                    {
                    });
                }
                return;
            }
            var playerPosition = _model.GetForwardPosition();
            CheckEventData(true, playerPosition, () =>
            {
            });
        }

        public StageEventData GetStageEventData(EventTiming eventTiming, int positionX, int positionY)
        {
            var timingEvents = _model.StageEvents(eventTiming, positionX, positionY);
            timingEvents = timingEvents.FindAll(a => !_thisTurnStageEvents.Contains(a));
            if (timingEvents.Count > 0)
            {
                return timingEvents.First();
            }
            return null;
        }

        public List<StageEventData> GetStageEventsData(EventTiming eventTiming, int positionX, int positionY)
        {
            var timingEvents = _model.StageEvents(eventTiming, positionX, positionY);
            timingEvents = timingEvents.FindAll(a => !_thisTurnStageEvents.Contains(a));
            return timingEvents;
        }

        private void CheckEventData(bool moved, Vector2Int position, Action endEvent = null)
        {
            var stageEvent = GetStageEventsData(EventTiming.Dungeon, position.x, position.y);
            if (stageEvent.Count > 0)
            {
                StartEventProcesses(stageEvent, stageEvent.Count, 0, moved);
            }
        }

        private void CommandStageEvent(StageEventData stageEvent, bool moved, Action endEvent)
        {
            _thisTurnStageEvents.Add(stageEvent);
            _model.AddEventReadFlag(stageEvent);
            switch (stageEvent.Type)
            {
                case StageEventType.AdvStart:
                    StageEventAdvEvent(moved, stageEvent.Param, endEvent);
                    return;
                case StageEventType.ExitDungeon:
                    StageEventExitDungeon(moved, endEvent);
                    return;
                case StageEventType.MoveDungeonFloor:
                    StageEventMoveDungeonFloor(moved, stageEvent, endEvent);
                    return;
                case StageEventType.MoveDungeonFloorForce:
                    StageEventMoveDungeonFloorForce(moved, stageEvent, endEvent);
                    return;
                case StageEventType.DungeonClear:
                    StageEventDungeonClear(moved, stageEvent, endEvent);
                    return;
                case StageEventType.GetArtifact:
                    StageEventGetArtifact(stageEvent);
                    return;
                case StageEventType.GetItem:
                    StageEventGetItem(stageEvent);
                    return;
                case StageEventType.GetSkill:
                    StageEventGetSkill(stageEvent);
                    return;
                case StageEventType.AddActor:
                    StageEventAddActor(moved, stageEvent, endEvent);
                    return;
                case StageEventType.RemoveActor:
                    StageEventRemoveActor(moved, stageEvent, endEvent);
                    return;
                case StageEventType.SelectAddActor:
                    StageEventSelectAddActor(stageEvent);
                    return;
                case StageEventType.ForceBattle:
                case StageEventType.ForceBossBattle:
                    StageEventForceBattle(stageEvent);
                    return;
                case StageEventType.AddEventFlag:
                    StageEventAddEventFlag(moved, stageEvent, endEvent);
                    return;
                case StageEventType.AddEventNotFlag:
                    StageEventAddEventNotFlag(stageEvent, endEvent);
                    return;
                case StageEventType.AddEventFlagEndForceBattle:
                    StageEventAddEventFlagEndForceBattle(moved, stageEvent, endEvent);
                    return;
                case StageEventType.DamageFloor:
                    StageEventDamageFloor(stageEvent, endEvent);
                    return;
                case StageEventType.CurseFloor:
                    StageEventCurseFloor(stageEvent, endEvent);
                    return;
                case StageEventType.EndCurseFloor:
                    StageEventEndCurseFloor(stageEvent, endEvent);
                    return;
                case StageEventType.TraverseRegeon:
                    StageEventTraverseRegeon(stageEvent, endEvent);
                    return;
                case StageEventType.None:
                case StageEventType.EventEnd:
                    _view.CallSystemCommand(Base.CommandType.SceneShowUI);
                    _busy = false;
                    _model.DungeonBusy(false);
                    CommandRefresh();
                    // ターン数確認
                    CommandTurnOverEvent(false);
                    return;
            }
        }

        private void StageEventAdvEvent(bool moved, int advId, Action endEvent)
        {
            // TimeStampを取得してBgmをフェードアウト
            _model.UpdateEventObjects();
            var timeStamp = SoundManager.Instance.CurrentTimeStamp();
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            CallAdvEvent(advId, timeStamp, () =>
            {
                _view.CallSystemCommand(Base.CommandType.SceneShowUI);
                endEvent?.Invoke();
            });
        }

        private void StageEventExitDungeon(bool moved, Action endEvent)
        {
            if (moved)
            {
                CommandReturn();
            } else
            {
                endEvent?.Invoke();
            }
        }

        private void StageEventMoveDungeonFloor(bool moved, StageEventData stageEvent, Action endEvent)
        {
            if (moved)
            {
                CommandMoveDungeonFloor(stageEvent.Param, stageEvent.Param2, stageEvent.Param3);
            } else
            {
                endEvent?.Invoke();
            }
        }

        private void StageEventMoveDungeonFloorForce(bool moved, StageEventData stageEvent, Action endEvent)
        {
            endEvent?.Invoke();
            CommandMoveDungeonFloor(stageEvent.Param, stageEvent.Param2, stageEvent.Param3);
        }

        private void StageEventDungeonClear(bool moved, StageEventData stageEvent, Action endEvent)
        {
            _model.CurrentStage.Cleared.SetValue(true);
            var stages = DataSystem.Stages.FindAll(a => a.StageNo == DataSystem.FindStage(_model.CurrentDeckInfo.StageNo.Value).StageNo);
            foreach (var stage in stages)
            {
                _model.PartyInfo.ClearStage(stage.Id);
            }
            _model.CurrentDeckInfo.DungeonBgmTimeStamp.SetValue(0);
            CheckStageEvent(moved);
        }

        private void StageEventGetArtifact(StageEventData stageEvent)
        {
            _model.UpdateEventObjects();
            CommandGetArtifact(stageEvent.Param);
        }

        private void StageEventGetItem(StageEventData stageEvent)
        {
            _model.UpdateEventObjects();
            CommandGetItem(stageEvent.Param);
        }

        private void StageEventGetSkill(StageEventData stageEvent)
        {
            _model.UpdateEventObjects();
            CommandGetSkill(stageEvent.Param);
        }

        private void StageEventAddActor(bool moved, StageEventData stageEvent, Action endEvent)
        {
            var getItemData = new GetItemData
            {
                Type = GetItemType.AddActor,
                Param1 = stageEvent.Param,
                Param2 = stageEvent.Param3
            };
            var getItemInfo = new GetItemInfo(getItemData);
            _model.AddGetItemInfo(getItemInfo);
            if (stageEvent.Param2 > 0)
            {
                _model.CurrentDeckInfo.ActorIdDict[stageEvent.Param2] = stageEvent.Param;
                _view.SetPartyUnitList(MakeListData(_model.PartyUnit(), -1));
            }
            endEvent?.Invoke();
        }

        private void StageEventRemoveActor(bool moved, StageEventData stageEvent, Action endEvent)
        {
            var getItemData = new GetItemData
            {
                Type = GetItemType.AddActor,
                Param1 = stageEvent.Param
            };
            var getItemInfo = new GetItemInfo(getItemData);
            _model.RemoveGetItemInfo(getItemInfo);
            _view.SetPartyUnitList(MakeListData(_model.PartyUnit(), -1));
            endEvent?.Invoke();
        }

        private void StageEventSelectAddActor(StageEventData stageEvent)
        {
            // 選択して仲間を加入
            // 確認後仲間選択
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10120), (a) =>
            {
                CommandCallAddActorInfo(new List<int>(){stageEvent.Param});
            });
            confirmInfo.SetIsNoChoice(true);
            confirmInfo.SetBackEvent(() => {});
            _view.CommandCallConfirm(confirmInfo);
            _model.UpdateEventObjects();
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void StageEventForceBattle(StageEventData stageEvent)
        {
            _model.UpdateEventObjects();
            var battleSceneInfo = new BattleSceneInfo
            {
                ActorInfos = _model.PartyInfo.CurrentDeckActorInfos(),
                EnemyInfos = _model.ForceBattleTroopInfos(stageEvent.Param, stageEvent.Param3),
                GetItemInfos = new(),
                IsEnableDefeat = stageEvent.Param3 == 1,
            };
            if (stageEvent.Type == StageEventType.ForceBattle)
            {
                PlayBattleBgm();
            } else
            if (stageEvent.Type == StageEventType.ForceBossBattle)
            {
                // バトルの報酬にステージクリアを足す
                var clearStageGetItemInfo = _model.MakeGetItemInfo(GetItemType.ClearStage, _model.CurrentStage.Master.StageNo);
                battleSceneInfo.GetItemInfos.Add(clearStageGetItemInfo);
                PlayBossBgm();
            }
            // 報酬設定があれば入れる
            if (stageEvent.Param2 > 0)
            {
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == stageEvent.Param2);
                if (prizeSets != null)
                {
                    foreach (var prizeSet in prizeSets)
                    {
                        var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                        battleSceneInfo.GetItemInfos.Add(getItemInfo);
                    }
                }
            }
            _view.CommandChangeViewToTransition(null);
            //_view.ChangeUIActive(false);
            _view.CommandSceneChange(Scene.Battle, battleSceneInfo);
            _model.ResetEncountValue();
            SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
        }

        private void StageEventAddEventFlag(bool moved, StageEventData stageEvent, Action endEvent)
        {
            var findAll = _model.StageEvents(EventTiming.Dungeon).FindAll(a => a.Param == stageEvent.Param);
            // 同じParam値のイベントを既読にする
            foreach (var item in findAll)
            {
                _model.AddEventReadFlagForce(item);
            }
            _model.UpdateEventObjects();
            CheckStageEvent(moved);
        }

        private void StageEventAddEventNotFlag(StageEventData stageEvent, Action endEvent)
        {
            var findAll = _model.StageEvents(EventTiming.Dungeon).FindAll(a => a.Param == stageEvent.Param);
            // 同じParam値のイベントを既読にする
            foreach (var item in findAll)
            {
                _model.DisplayAddEventNotFlag(item);
                //_model.AddEventReadFlag(item);
            }
            _model.UpdateEventObjects();
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            CheckEventData(false, playerPosition, endEvent);
        }

        private void StageEventAddEventFlagEndForceBattle(bool moved, StageEventData stageEvent, Action endEvent)
        {
            var findAll = _model.StageEventDates.FindAll(a => a.Type == StageEventType.ForceBattle);
            var open = true;
            // 強制戦闘が終わっているか
            foreach (var item in findAll)
            {
                if (!_model.CurrentGameInfo.ReadEventKeys.Contains(item.EventKey))
                {
                    open = false;
                }
            }
            if (open)
            {
                _model.AddEventReadFlag(stageEvent);
                StageEventAddEventFlag(false, stageEvent, null);
                _model.UpdateEventObjects();
                var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
                CheckEventData(moved, playerPosition, endEvent);
            }
            endEvent?.Invoke();
        }

        private void StageEventDamageFloor(StageEventData stageEvent, Action endEvent)
        {
            var endStageEvents = _model.EndStageEvents();
            var findAll = _model.StageEvents(EventTiming.Dungeon, stageEvent.PositionX, stageEvent.PositionY);
            var closed = false;
            foreach (var item in findAll)
            {
                if (endStageEvents.Find(a => a.Type == 0 && item.PositionX == a.PositionX && item.PositionY == a.PositionY) != null)
                {
                    closed = true;
                }
            }
            if (closed)
            {
                endEvent?.Invoke();
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Damage);
            _model.DamageFloor(stageEvent.Param);
            _view.StartDamage(stageEvent.Param);
            _view.SetPartyUnitList(MakeListData(_model.PartyUnit(), -1));
            if (_model.CheckGameover())
            {
                var confirmInfo2 = new ConfirmInfo(DataSystem.GetText(10150), (a) =>
                {
                    _view.CallSystemCommand(Base.CommandType.MapClear);
                    _view.CommandGotoSceneChange(Scene.Title);
                });
                confirmInfo2.SetIsNoChoice(true);
                confirmInfo2.SetBackEvent(() => {});
                _view.CommandCallConfirm(confirmInfo2);
            } else
            {
                endEvent?.Invoke();
            }
        }

        private void StageEventCurseFloor(StageEventData stageEvent, Action endEvent)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10160), (a) =>
            {
                _model.CursedParty();
                var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
                CheckEventData(false, playerPosition, endEvent);
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void StageEventEndCurseFloor(StageEventData stageEvent, Action endEvent)
        {
            _model.EndCursedParty();
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            CheckEventData(false, playerPosition, endEvent);
        }

        private void StageEventTraverseRegeon(StageEventData stageEvent, Action endEvent)
        {
            _model.TraverseRegeon(stageEvent.Param);
            CommandRefresh();
            // 未読の非表示マスを管理
            _model.AddEventNotFlag();
            _model.DungeonBusy(false);
            // ターン数確認
            CommandTurnOverEvent(false);
        }

        private void CommandCheckRemainTurn()
        {
            if (CheckRemainTurn())
            {
                return;
            }
            _busy = true;
            _view.UpdateMoveKeys(new List<InputKeyType>(){InputKeyType.Up});
        }

        private bool CheckRemainTurn()
        {
            // 移動したら評価値が減る場合に確認
            if (_model.EndDungeonByTurnCountValue(0) && !_checkTurnOver)
            {
                _model.DungeonBusy(true);
                _busy = true;
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(10182), (a) =>
                {
                    if (a == ConfirmCommandType.Yes)
                    {
                        _model.DungeonBusy(false);
                        _busy = false;
                        _checkTurnOver = true;
                    } else
                    {
                        // 帰還できない
                        if (_model.PartyInfo.Cursed.Value)
                        {
                            _model.DungeonBusy(false);
                            _busy = false;
                            _checkTurnOver = true;
                            var cautionInfo = new CautionInfo();
                            cautionInfo.SetTitle(DataSystem.GetText(10131));
                            _view.CommandCallCaution(cautionInfo);
                            return;
                        }
                        _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
                        ReturnDungeon();
                    }
                });
                _view.CommandCallConfirm(confirmInfo);
                return true;
            }
            return false;
        }

        private void CommandReturn()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var textId = _model.CurrentStage.Master.OnlyOnce ? 10133 : 10130;
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(textId), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
                    ReturnDungeon();
                } else
                {
                    _model.DungeonBusy(false);
                    _busy = false;
                }
            });
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandMoveDungeonFloor(int floorId, int x, int y)
        {
            _model.MakeStageInfo(floorId, false);
            _model.CurrentDeckInfo.SetPosition(floorId, x, y, _model.CurrentDeckInfo.Direction.Value);
            _view.CommandGotoSceneChange(Scene.Dungeon);
        }

        private async Task CommandGetArtifact(int itemId)
        {
            var item = DataSystem.Items.Find(a => a.Id == itemId);
            if (item == null)
            {
                return;
            }
            _model.SaveBgmTiming();
            SoundManager.Instance.FadeOutBgm();
            SoundManager.Instance.PlayStaticSe(SEType.Artifact);
            _busy = true;
            var skillId = item.Param1;
            var learnSkillInfo = new LearnSkillInfo(0, 0, new SkillInfo(skillId));
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.LearnSkill,
                EndEvent = () =>
                {
                    PresentArtifact(item.Id);
                },
                template = learnSkillInfo
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void PresentArtifact(int itemId)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Alert);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10140), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _busy = false;
                    _model.DungeonBusy(false);
                } else
                {
                    GetArtifact(itemId);
                }
                //var timeStamp = SoundManager.Instance.CurrentTimeStamp();
                PlayDungeonBgm(_model.DungeonBgmTimeStamp());
            });
            confirmInfo.IsArtifact.SetValue(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void GetArtifact(int itemId)
        {
            var getItemInfo = _model.MakeGetItemInfo(GetItemType.Item, itemId, 1);
            _model.AddGetItemInfo(getItemInfo);
            _view.MinusEvaluate(-10);
            _model.PartyInfo.EvaluationValue.GainValue(-10, 0);
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(10141, 10.ToString()),(a) =>
            {
                _busy = false;
                _model.DungeonBusy(false);
                CommandRefresh();
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandGetItem(int itemId)
        {
            var item = DataSystem.Items.Find(a => a.Id == itemId);
            if (item == null)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10170), (a) =>
            {
                var getItemInfo = _model.MakeGetItemInfo(GetItemType.Item, item.Id, 1);
                _model.AddGetItemInfo(getItemInfo);
                _busy = false;
                CommandRefresh();
                _model.DungeonBusy(false);
            }, ConfirmType.ItemDetail);
            var itemInfo = new ItemInfo(item.Id, 1);
            confirmInfo.SetItemInfo(new List<ItemInfo>(){itemInfo});
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandGetSkill(int skillId)
        {
            var skill = DataSystem.FindSkill(skillId);
            if (skill == null)
            {
                return;
            }
            var learnSkillInfo = new LearnSkillInfo(0, 0, new SkillInfo(skillId));
            SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.LearnSkill,
                EndEvent = () =>
                {
                    var getItemInfo = _model.MakeGetItemInfo(GetItemType.Skill, skill.Id);
                    _model.AddGetItemInfo(getItemInfo);
                    _busy = false;
                    CommandRefresh();
                    _model.DungeonBusy(false);
                },
                template = learnSkillInfo
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandCallAddActorInfo(List<int> limitRanks)
        {
            List<ActorInfo> actorInfos = _model.AddSelectActorInfos(limitRanks);
            // 加入する用
            CommandAddActorStatusInfo(actorInfos,() =>
            {
                CommandRefresh();
            });
        }

        private void CommandHeal()
        {
            if (!_model.IsActiveDungeon())
            {
                return;
            }
            // 回復できない
            if (_model.PartyInfo.Cursed.Value)
            {
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(10131));
                _view.CommandCallCaution(cautionInfo);
                return;
            }
            if (_model.CanUseRecoveryHeal())
            {
                SoundManager.Instance.PlayStaticSe(SEType.Heal);
                _model.UseRecoveryHeal();
                _view.SeekTweens();
                _view.StartHeal(_model.PartyInfo.HpHealValue());
                //_view.MinusVictoryBonus(-0.2f);
                _view.SetPartyUnitList(MakeListData(_model.PartyUnit(), -1));
                CommandRefresh();
            } else
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                var textId = (_model.CurrentDeckInfo.RecoveryCount.Value <= 0) ? 10100 : 10101;
                cautionInfo.SetTitle(DataSystem.GetText(textId));
                _view.CommandCallCaution(cautionInfo);
            }
        }

        private void CommandDungeonMap()
        {
            if (!_model.IsActiveDungeon())
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.DungeonBusy(true);
            //_view.StartFormation();
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.DungeonMap,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    _model.DungeonBusy(false);
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    CommandRefresh();
                    // 自動移動モード切替
                    CheckRouteMode();
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView, popupInfo);
        }

        private void CommandUseItem()
        {
            if (!_model.IsActiveDungeon())
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            _model.DungeonBusy(true);
            var useItemSceneInfo = new UseItemSceneInfo();
            useItemSceneInfo.UsableItemTypes = new()
            {
                UseItemType.Heal,
                UseItemType.DungeonTurn,
                UseItemType.EncountRate
            };
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.UseItem,
                template = useItemSceneInfo,
                EndEvent = () =>
                {
                    _busy = false;
                    _model.DungeonBusy(false);
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    CommandRefresh();
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView, popupInfo);
        }

        private void CommandMoveDirection(int direction)
        {
            _view.UpdateMoveKeys(new List<InputKeyType>(){(InputKeyType)direction});
        }

        private void CommandSelectCharacter(int selectIndex)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            if (_model.SelectIndex.Value > -1)
            {
                _model.SwapSelectIndex(selectIndex);
                _view.UpdatePartyUnitList(MakeListData(_model.PartyUnit()));
                _view.StartFormation();
                _view.UpdateSelectCursor(new List<int>(){});
                return;
            }
            _model.SelectIndex.SetValue(selectIndex);
            _view.UpdateSelectCursor(new List<int>(){_model.SelectedCharacterIndex()});
        }

        private void CommandEndFormation()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.UpdateSelectCursor(new List<int>(){});
            _view.EndFormation();
            _view.SetPartyUnitList(MakeListData(_model.PartyUnit(), -1));
            _model.SelectIndex.SetValue(-1);
            _model.DungeonBusy(false);
        }

        private void CommandAritifact()
        {
            if (_model.PartyInfo.AritifactSkills().Count == 0)
            {
                return;
            }
            _busy = true;
            _model.DungeonBusy(true);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.ArtifactList,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    _model.DungeonBusy(false);
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView, popupInfo);
        }

        private void CommandSelectSideMenu()
        {
            if (!_model.IsActiveDungeon())
            {
                return;
            }
            _busy = true;
            _model.DungeonBusy(true);
            CommandCallSideMenu(MakeListData(_model.SideMenu(), 0), () =>
            {
                _model.DungeonBusy(false);
                _busy = false;
            });
        }

        private void CommandRefresh()
        {
            var directionEvent = _model.CheckDirectionEvent();
            _view.SetActiveDisplayEventKey(directionEvent);
            if (!directionEvent)
            {
                // その場にイベントがある
                var currentPositionEvent = _model.CheckCurrentPositionEvent();
                _view.SetActiveDisplayEventKey(currentPositionEvent);
            }
            _view.CommandRefresh();
            if (!_model.IsActiveDungeon())
            {
                _view.SetActiveFormationButton(false);
                _view.SetActiveHealButton(false);
                _view.SetActiveUseItemButton(false);
                _view.SetActiveDisplayEventKey(false);
                _view.SideMenuButton.gameObject.SetActive(false);
            }
        }

        private void CheckRouteMode()
        {
            if (_battleBusy)
            {
                return;
            }
            if (_model.IsRouteMode())
            {
                if (CheckRemainTurn())
                {
                    return;
                }
                _busy = true;
                //_model.DungeonBusy(true);
                // 方向を見て進む。MoveEndまで待つ
                _routeMode = true;
                _view.UpdateMoveKeys(new List<InputKeyType>(){ _model.RouteModeInputKeyType()});
            } else
            {
                _busy = false;
                //_model.DungeonBusy(false);
                _routeMode = false;
            }
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
        }


    }
}