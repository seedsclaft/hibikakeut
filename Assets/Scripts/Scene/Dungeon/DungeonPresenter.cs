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

            _view.SetPartyUnitList(MakeListData(_model.PartyUnit(),-1));
            _view.SetActiveStageInfo(_model.IsActiveDungeon());
            await PlayDungeonBgm(_model.DungeonBgmTimeStamp());
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
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            LogOutput.Log(viewEvent.ViewCommandType.CommandType);
            if (_busy || _view.AnimationBusy)
            {
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
                case CommandType.Heal:
                    CommandHeal();
                    break;
                case CommandType.Formation:
                    CommandFormation();
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
            // 移動したか
            var moved = _model.CommandMoveEnd();
            if (moved)
            {
                var hpHeal = _model.CheckHpHeal();
                if (hpHeal > 0)
                {
                    _view.SetPartyUnitList(MakeListData(_model.PartyUnit(),-1));
                }
            }


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

            // ターン数が0の場合
            if (_model.EndDungeonByTurnCount())
            {
                CommandTurnOver(moved);
                return;
            }

            // エンカウントした場合
            CheckEncount();
        }

        private void CheckStageEvent(bool moved)
        {
            _model.DungeonBusy(true);
            // イベントマスの場合
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            CheckEventData(moved,playerPosition,() =>
            {
                _model.DungeonBusy(false);
                // ターン数が0の場合
                if (_model.EndDungeonByTurnCount())
                {
                    CommandTurnOver(moved);
                }
            });
        }

        private void CheckEncount()
        {
            if (_model.EncountEnemy())
            {
                _model.DungeonBusy(true);
                _model.ResetEncountValue();
                // ダンジョンの再開時間を記憶
                _model.SaveBgmTiming();
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
        }

        private void CommandTurnOver(bool moved)
        {
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
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10110),(a) =>
            {
                _model.ReturnDungeon();
                var periodItemInfos = _model.PeriodGetItemInfos();
                if (periodItemInfos.Count > 0)
                {
                    var strategySceneInfo = new StrategySceneInfo
                    {
                        ActorInfos = _model.PartyInfo.ActorInfos,
                        InBattle = false,
                        GetItemInfos = periodItemInfos,
                        ReturnScene = Scene.MainMenu,
                    };
                    _view.CommandSceneChange(Scene.Strategy, strategySceneInfo);
                } else
                {
                    _view.CommandSceneChange(Scene.MainMenu);
                }
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandDecideDirectEvent()
        {
            // 正面にイベントがあるか
            var directionEvent = _model.CheckDirectionEvent();
            if (!directionEvent)
            {
                return;
            }
            var playerPosition = _model.GetForwardPosition();
            CheckEventData(true,playerPosition,() =>
            {
            });
        }

        public StageEventData GetStageEventData(EventTiming eventTiming,int positionX,int positionY)
        {
            var timingEvents = _model.StageEvents(eventTiming,positionX,positionY);
            timingEvents = timingEvents.FindAll(a => !_thisTurnStageEvents.Contains(a));
            if (timingEvents.Count > 0)
            {
                return timingEvents.First();
            }
            return null;
        }

        private void CheckEventData(bool moved, Vector2Int position, Action endEvent = null)
        {
            var stageEvent = GetStageEventData(EventTiming.Dungeon, position.x, position.y);
            if (stageEvent != null)
            {
                _thisTurnStageEvents.Add(stageEvent);
                switch (stageEvent.Type)
                {
                    case StageEventType.AdvStart:
                        _model.AddEventReadFlag(stageEvent);
                        StageEventAdvEvent(moved, stageEvent.Param, endEvent);
                        return;
                    case StageEventType.ExitDungeon:
                        StageEventExitDungeon(moved, endEvent);
                        return;
                    case StageEventType.MoveDungeonFloor:
                        StageEventMoveDungeonFloor(moved,stageEvent,endEvent);
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
                    case StageEventType.None:
                    case StageEventType.EventEnd:
                        _model.DungeonBusy(false);
                        // ターン数が0の場合
                        if (_model.EndDungeonByTurnCount())
                        {
                            CommandTurnOver(false);
                        }
                        return;
                }
            }
        }

        private void StageEventAdvEvent(bool moved, int advId, Action endEvent)
        {
            // TimeStampを取得してBgmをフェードアウト
            _model.UpdateEventObjects();
            var timeStamp = SoundManager.Instance.CurrentTimeStamp();
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            CheckStageAdvEvent(advId, timeStamp, () =>
            {
                _view.CallSystemCommand(Base.CommandType.SceneShowUI);
                CheckStageEvent(moved);
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

        private void StageEventGetArtifact(StageEventData stageEvent)
        {
            _model.AddEventReadFlag(stageEvent);
            _model.UpdateEventObjects();
            CommandGetArtifact(stageEvent.Param);
        }

        private void StageEventGetItem(StageEventData stageEvent)
        {
            _model.AddEventReadFlag(stageEvent);
            _model.UpdateEventObjects();
            CommandGetItem(stageEvent.Param);
        }

        private void StageEventGetSkill(StageEventData stageEvent)
        {
            _model.AddEventReadFlag(stageEvent);
            _model.UpdateEventObjects();
            CommandGetSkill(stageEvent.Param);
        }

        private void StageEventSelectAddActor(StageEventData stageEvent)
        {
            // 選択して仲間を加入
            // 確認後仲間選択
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10120),(a) =>
            {
                CommandCallAddActorInfo(true,true);
            });
            confirmInfo.SetIsNoChoice(true);
            confirmInfo.SetBackEvent(() => {});
            _view.CommandCallConfirm(confirmInfo);
            _model.AddEventReadFlag(stageEvent);
            _model.UpdateEventObjects();
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void StageEventForceBattle(StageEventData stageEvent)
        {
            _model.AddEventReadFlag(stageEvent);
            _model.UpdateEventObjects();
            var battleSceneInfo = new BattleSceneInfo
            {
                ActorInfos = _model.PartyInfo.CurrentDeckActorInfos(),
                EnemyInfos = _model.ForceBattleTroopInfos(stageEvent.Param),
                GetItemInfos = new()
            };
            if (stageEvent.Type == StageEventType.ForceBattle)
            {
                PlayBattleBgm();
            } else
            if (stageEvent.Type == StageEventType.ForceBossBattle)
            {
                // バトルの報酬にステージクリアを足す
                var clearStageItem = new GetItemData
                {
                    Type = GetItemType.ClearStage,
                    Param1 = _model.CurrentStage.Master.StageNo
                };
                var clearStageGetItemInfo = new GetItemInfo(clearStageItem);
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
            _model.AddEventReadFlag(stageEvent);
            var findAll = _model.StageEvents(EventTiming.Dungeon).FindAll(a => a.Param == stageEvent.Param);
            // 同じParam値のイベントを既読にする
            foreach (var item in findAll)
            {
                _model.AddEventReadFlag(item);
            }
            _model.UpdateEventObjects();
            CheckStageEvent(moved);
        }

        private void StageEventAddEventNotFlag(StageEventData stageEvent, Action endEvent)
        {
            _model.AddEventReadFlag(stageEvent);
            var findAll = _model.StageEvents(EventTiming.Dungeon).FindAll(a => a.Param == stageEvent.Param);
            // 同じParam値のイベントを既読にする
            foreach (var item in findAll)
            {
                _model.DisplayAddEventNotFlag(item);
                //_model.AddEventReadFlag(item);
            }
            _model.UpdateEventObjects();
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            CheckEventData(false,playerPosition,endEvent);
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
                return;
            }
            CheckStageEvent(moved);
        }

        private void StageEventDamageFloor(StageEventData stageEvent, Action endEvent)
        {
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
            _model.AddEventReadFlag(stageEvent);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10160),(a) =>
            {
                _model.CursedParty();
                var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
                CheckEventData(false,playerPosition,endEvent);
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void StageEventEndCurseFloor(StageEventData stageEvent, Action endEvent)
        {
            _model.AddEventReadFlag(stageEvent);
            _model.EndCursedParty();
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            CheckEventData(false,playerPosition,endEvent);
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
                    _model.ReturnDungeon();
                    var periodItemInfos = _model.PeriodGetItemInfos();
                    if (periodItemInfos.Count > 0)
                    {
                        var strategySceneInfo = new StrategySceneInfo
                        {
                            ActorInfos = _model.PartyInfo.ActorInfos,
                            InBattle = false,
                            GetItemInfos = periodItemInfos,
                            ReturnScene = Scene.MainMenu,
                        };
                        _view.CommandSceneChange(Scene.Strategy, strategySceneInfo);
                    } else
                    {
                        _view.CommandSceneChange(Scene.MainMenu);
                    }
                } else
                {
                    _model.DungeonBusy(false);
                    _busy = false;
                }
            });
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandMoveDungeonFloor(int floorId,int x,int y)
        {
            _model.MakeStageInfo(floorId,false);
            _model.CurrentDeckInfo.SetPosition(floorId,x,y,_model.CurrentDeckInfo.Direction.Value);
            _view.CommandGotoSceneChange(Scene.Dungeon);
        }

        private void CommandGetArtifact(int itemId)
        {
            var item = DataSystem.Items.Find(a => a.Id == itemId);
            if (item == null)
            {
                return;
            }
            _busy = true;
            var skillId = item.Param1;
            var learnSkillInfo = new LearnSkillInfo(0,0,new SkillInfo(skillId));
            SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);
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
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10140),(a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _busy = false;
                    _model.DungeonBusy(false);
                } else
                {
                    GetArtifact(itemId);
                }
            });
            confirmInfo.IsArtifact.SetValue(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void GetArtifact(int itemId)
        {
            var itemData = new GetItemData
            {
                Param1 = itemId,
                Param2 = 1,
                Type = GetItemType.Item
            };
            var getItemInfo = new GetItemInfo(itemData);
            _model.AddGetItemInfo(getItemInfo);
            _view.MinusEvaluate(-10);
            _model.PartyInfo.EvaluationValue.GainValue(-10, 0);
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(10141,10.ToString()),(a) =>
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
            var confirmInfo = new ConfirmInfo(item.Name + "を入手！",(a) =>
            {
                var itemData = new GetItemData
                {
                    Param1 = item.Id,
                    Param2 = 1,
                    Type = GetItemType.Item
                };
                var getItemInfo = new GetItemInfo(itemData);
                _model.AddGetItemInfo(getItemInfo);
                _busy = false;
                CommandRefresh();
                _model.DungeonBusy(false);
            });
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
            var learnSkillInfo = new LearnSkillInfo(0,0,new SkillInfo(skillId));
            SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.LearnSkill,
                EndEvent = () =>
                {
                    var skillGetItemData = new GetItemData
                    {
                        Param1 = skill.Id,
                        Type = GetItemType.Skill
                    };
                    var getItemInfo = new GetItemInfo(skillGetItemData);
                    _model.AddGetItemInfo(getItemInfo);
                    _busy = false;
                    CommandRefresh();
                    _model.DungeonBusy(false);
                },
                template = learnSkillInfo
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandCallAddActorInfo(bool freeSelect,bool addCommand)
        {
            List<ActorInfo> actorInfos = null;
            if (freeSelect)
            {
                actorInfos = _model.AddSelectActorInfos();
            } else
            {
                //actorInfos = _model.AddSelectActorGetItemInfos(symbolResultInfo.SymbolInfo.GetItemInfos);
            }
            if (addCommand)
            {
                // 加入する用
                CommandAddActorStatusInfo(actorInfos,() =>
                {
                    CommandRefresh();
                });
            } else
            {
                /*
                var selectActorId = -1;
                var getItemInfo = _view.SymbolGetItemInfo;
                if (getItemInfo != null)
                {
                    selectActorId = getItemInfo.Param1;
                }
                // 確認する用
                CommandStatusInfo(actorInfos,false,true,false,false,selectActorId,() => 
                {

                });
                */
            }
        }

        private void CommandHeal()
        {
            if (!_model.IsActiveDungeon())
            {
                return;
            }
            if (_model.CanUseCurrencyHeal())
            {
                SoundManager.Instance.PlayStaticSe(SEType.Heal);
                _model.UseCurrencyHeal();
                _view.StartHeal(10);
                _view.MinusVictoryBonus(-0.5f);
                _view.SetPartyUnitList(MakeListData(_model.PartyUnit(),-1));
                CommandRefresh();
            } else
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                var textId = (_model.PartyInfo.Currency.Value <= 0) ? 10100 : 10101;
                cautionInfo.SetTitle(DataSystem.GetText(textId));
                _view.CommandCallCaution(cautionInfo);
            }
        }

        private void CommandFormation()
        {
            if (!_model.IsActiveDungeon())
            {
                return;
            }
            _model.DungeonBusy(true);
            _view.StartFormation();
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
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandSelectSideMenu()
        {
            if (!_model.IsActiveDungeon())
            {
                return;
            }
            _busy = true;
            _model.DungeonBusy(true);
            CommandCallSideMenu(MakeListData(_model.SideMenu()), () =>
            {
                _model.DungeonBusy(false);
                _busy = false;
            });
        }

        private void CommandRefresh()
        {
            var directionEvent = _model.CheckDirectionEvent();
            _view.SetActiveDisplayEventKey(directionEvent);
            _view.CommandRefresh();
            if (!_model.IsActiveDungeon())
            {
                _view.SetActiveFormationButton(false);
                _view.SetActiveHealButton(false);
            }
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
        }


    }
}