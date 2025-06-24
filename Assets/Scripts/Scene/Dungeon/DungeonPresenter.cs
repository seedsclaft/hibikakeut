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
            _view.SetupDungeon();
            _model.UpdateTraverses();
            _model.SetPlayerPosition();
            _model.UpdateEventObjects();
            CommandRefresh();
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

            _thisTurnStageEvents.Clear();
            // イベントマスの場合
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            if (CheckEventData(moved,playerPosition,() => {_model.DungeonBusy(false);}))
            {
                _model.DungeonBusy(true);
                // ターン数が0の場合
                if (_model.EndDungeonByTurnCount())
                {
                    CommandTurnOver();
                }
                return;
            }

            // ターン数が0の場合
            if (_model.EndDungeonByTurnCount())
            {
                CommandTurnOver();
                return;
            }

            // エンカウントした場合
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

        private void CommandTurnOver()
        {
            SoundManager.Instance.PlayStaticSe(SEType.PlayStart);
            _model.DungeonBusy(true);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10110),(a) =>
            {
                _model.ReturnDungeon();
                _view.CommandSceneChange(Scene.MainMenu);
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
            if (CheckEventData(true,playerPosition,() =>
            {
                _model.DungeonBusy(false);
            }))
            {
                _model.DungeonBusy(true);
                return;
            }
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

        private bool CheckEventData(bool moved, Vector2Int position, Action endEvent = null)
        {
            var stageEvent = GetStageEventData(EventTiming.Dungeon, position.x, position.y);
            if (stageEvent != null)
            {
                _thisTurnStageEvents.Add(stageEvent);
                switch (stageEvent.Type)
                {
                    case StageEventType.AdvStart:
                        _model.AddEventReadFlag(stageEvent);
                        return StageEventAdvEvent(moved,stageEvent.Param,endEvent);
                    case StageEventType.ExitDungeon:
                        return StageEventExitDungeon(moved);
                    case StageEventType.MoveDungeonFloor:
                        return StageEventMoveDungeonFloor(moved,stageEvent);
                    case StageEventType.GetArtifact:
                        return StageEventGetArtifact(stageEvent);
                    case StageEventType.GetItem:
                        return StageEventGetItem(stageEvent);
                    case StageEventType.GetSkill:
                        return StageEventGetSkill(stageEvent);
                    case StageEventType.SelectAddActor:
                        return StageEventSelectAddActor(stageEvent);
                    case StageEventType.ForceBattle:
                    case StageEventType.ForceBossBattle:
                        return StageEventForceBattle(stageEvent);
                    case StageEventType.AddEventFlag:
                        return StageEventAddEventFlag(stageEvent, endEvent);
                    case StageEventType.AddEventNotFlag:
                        return StageEventAddEventNotFlag(stageEvent, endEvent);
                    case StageEventType.AddEventFlagEndForceBattle:
                        return StageEventAddEventFlagEndForceBattle(stageEvent, endEvent);
                    case StageEventType.DamageFloor:
                        return StageEventDamageFloor(stageEvent, endEvent);
                    case StageEventType.CurseFloor:
                        return StageEventCurseFloor(stageEvent, endEvent);
                    case StageEventType.EndCurseFloor:
                        return StageEventEndCurseFloor(stageEvent, endEvent);
                        
                }
            }
            endEvent?.Invoke();
            return false;
        }

        private bool StageEventAdvEvent(bool moved, int advId, Action endEvent)
        {
            // TimeStampを取得してBgmをフェードアウト
            var timeStamp = SoundManager.Instance.CurrentTimeStamp();
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            if (CheckStageAdvEvent(advId,timeStamp,() => CheckEventData(moved,playerPosition,() =>
            {
                _view.CallSystemCommand(Base.CommandType.SceneShowUI);
                endEvent?.Invoke();
            })))
            {
                return true;
            }
            return true;
        }

        private bool StageEventExitDungeon(bool moved)
        {
            if (moved)
            {
                CommandReturn();
                return true;
            }
            return false;
        }

        private bool StageEventMoveDungeonFloor(bool moved, StageEventData stageEvent)
        {
            if (moved)
            {
                CommandMoveDungeonFloor(stageEvent.Param, stageEvent.Param2, stageEvent.Param3);
                return true;
            }
            return false;
        }

        private bool StageEventGetArtifact(StageEventData stageEvent)
        {
            _model.AddEventReadFlag(stageEvent);
            _model.UpdateEventObjects();
            CommandGetArtifact(stageEvent.Param);
            return true;
        }

        private bool StageEventGetItem(StageEventData stageEvent)
        {
            _model.AddEventReadFlag(stageEvent);
            _model.UpdateEventObjects();
            CommandGetItem(stageEvent.Param);
            return true;
        }

        private bool StageEventGetSkill(StageEventData stageEvent)
        {
            _model.AddEventReadFlag(stageEvent);
            _model.UpdateEventObjects();
            CommandGetSkill(stageEvent.Param);
            return true;
        }

        private bool StageEventSelectAddActor(StageEventData stageEvent)
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
            return true;
        }

        private bool StageEventForceBattle(StageEventData stageEvent)
        {
            _model.AddEventReadFlag(stageEvent);
            _model.UpdateEventObjects();
            var battleSceneInfo = new BattleSceneInfo
            {
                ActorInfos = _model.PartyInfo.CurrentDeckActorInfos(),
                EnemyInfos = _model.ForceBattleTroopInfos(stageEvent.Param),
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
                battleSceneInfo.GetItemInfos = new()
                {
                    clearStageGetItemInfo
                };
                PlayBossBgm();
            }
            _view.CommandChangeViewToTransition(null);
            //_view.ChangeUIActive(false);
            _view.CommandSceneChange(Scene.Battle, battleSceneInfo);
            _model.ResetEncountValue();
            SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
            return true;
        }

        private bool StageEventAddEventFlag(StageEventData stageEvent, Action endEvent)
        {
            _model.AddEventReadFlag(stageEvent);
            var findAll = _model.StageEvents(EventTiming.Dungeon).FindAll(a => a.Param == stageEvent.Param);
            // 同じParam値のイベントを既読にする
            foreach (var item in findAll)
            {
                _model.AddEventReadFlag(item);
            }
            _model.UpdateEventObjects();
            endEvent?.Invoke();
            return false;
        }

        private bool StageEventAddEventNotFlag(StageEventData stageEvent, Action endEvent)
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
            return true;
        }

        private bool StageEventAddEventFlagEndForceBattle(StageEventData stageEvent, Action endEvent)
        {
            _model.AddEventReadFlag(stageEvent);
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
                StageEventAddEventFlag(stageEvent,null);
                _model.UpdateEventObjects();
            }
            endEvent?.Invoke();
            return false;
        }

        private bool StageEventDamageFloor(StageEventData stageEvent, Action endEvent)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Damage);
            _model.DamageFloor(stageEvent.Param);
            _view.SetPartyUnitList(MakeListData(_model.PartyUnit(),-1));
            if (_model.CheckGameover())
            {
                var confirmInfo2 = new ConfirmInfo(DataSystem.GetText(10150),(a) =>
                {
                    _view.CallSystemCommand(Base.CommandType.MapClear);
                    _view.CommandGotoSceneChange(Scene.Title);
                });
                confirmInfo2.SetIsNoChoice(true);
                confirmInfo2.SetBackEvent(() => {});
                _view.CommandCallConfirm(confirmInfo2);
                return false;
            }
            endEvent?.Invoke();
            return false;
        }

        private bool StageEventCurseFloor(StageEventData stageEvent, Action endEvent)
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
            return true;
        }

        private bool StageEventEndCurseFloor(StageEventData stageEvent, Action endEvent)
        {
            _model.AddEventReadFlag(stageEvent);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10161),(a) =>
            {
                var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
                CheckEventData(false,playerPosition,endEvent);
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
            return true;
        }

        private void CommandReturn()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(10130), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
                    _model.ReturnDungeon();
                    _view.CommandSceneChange(Scene.MainMenu);
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
            _model.PartyInfo.EvaluationValue.GainValue(-10,0);
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
            if (_model.CanUseCurrencyHeal())
            {
                SoundManager.Instance.PlayStaticSe(SEType.Heal);
                _model.UseCurrencyHeal();
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

        private void CommandSelectSideMenu()
        {
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
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
        }


    }
}