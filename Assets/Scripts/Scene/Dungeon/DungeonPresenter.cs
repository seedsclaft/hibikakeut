using System;
using System.Collections.Generic;
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
            // ダンジョン生成
            _view.CommandChangeDungeon(_model.DungeonPrefabName());
            await PlayTacticsBgm(_model.DungeonBgmTimeStamp());
            _view.SetupDungeon();
            _model.UpdateTraverses();
            _model.SetPlayerPosition();
            _model.UpdateEventObjects();
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
                case CommandType.Heal:
                    CommandHeal();
                    break;
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
            }
        }

        private void CommandMoveEnd()
        {
            var moved = _model.CommandMoveEnd();
            if (moved)
            {
                var hpHeal = _model.CheckHpHeal();
            }

            CommandRefresh();

            // ターン数が0の場合
            if (_model.EndDungeonByTurnCount())
            {
                _model.DungeonBusy(true);
                var confirmInfo = new ConfirmInfo("残りターン数が枯渇したため帰還します!",(a) => 
                {
                    _model.ReturnDungeon();
                    _view.CommandSceneChange(Scene.MainMenu);
                });
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
                return;
            }

            // イベントマスの場合
            if (CheckEventData(() => {_model.DungeonBusy(false);}))
            {
                _model.DungeonBusy(true);
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

        private bool CheckEventData(Action endEvent = null)
        {
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            UnityEngine.Debug.Log(playerPosition);
            var stageEvent = GetStageEventData(EventTiming.Dungeon,playerPosition.x,playerPosition.y);
            if (stageEvent != null)
            {
                switch (stageEvent.Type)
                {
                    case StageEventType.AdvStart:
                        // TimeStampを取得してBgmをフェードアウト
                        var timeStamp = SoundManager.Instance.CurrentTimeStamp();
                        if (CheckAdvEvent(EventTiming.BattleVictory,timeStamp,() => CheckEventData(() => Initialize())))
                        {
                            return true;
                        }
                        return true;
                    case StageEventType.ExitDungeon:
                        CommandReturn();
                        return true;
                    case StageEventType.MoveDungeonFloor:
                        CommandMoveDungeonFloor(stageEvent.Param,stageEvent.Param2,stageEvent.Param3);
                        return true;
                    case StageEventType.GetArtifact:
                        _model.AddEventReadFlag(stageEvent);
                        CommandGetArtifact(stageEvent.Param);
                        return true;
                    case StageEventType.SelectAddActor:
                        // 選択して仲間を加入
                        // 確認後仲間選択
                        var confirmInfo = new ConfirmInfo("加入したい仲間を選択！",(a) =>
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
                    case StageEventType.ForceBattle:
                    case StageEventType.ForceBossBattle:
                        _model.AddEventReadFlag(stageEvent);
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
                                Param1 = _model.CurrentStage.StageId.Value
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
                        SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
                        return true;
                    case StageEventType.AddEventFlag:
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
            }
            endEvent?.Invoke();
            return false;
        }

        private void CommandReturn()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo("帰還しますか？", (a) =>
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
            if (item != null)
            {
                _busy = true;
                var skillId = item.Param1;
                var learnSkillInfo = new LearnSkillInfo(0,0,new SkillInfo(skillId));
                SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);
                var popupInfo = new PopupInfo
                {
                    PopupType = PopupType.LearnSkill,
                    EndEvent = () =>
                    {
                        var confirmInfo = new ConfirmInfo("アーティファクトを入手しますか？",(a) => 
                        {
                            if (a == ConfirmCommandType.Yes)
                            {
                                var itemData = new GetItemData
                                {
                                    Param1 = item.Param1,
                                    Param2 = 1
                                };
                                var getItemInfo = new GetItemInfo(itemData);
                                _model.AddGetItemInfo(getItemInfo);
                            }
                            _busy = false;
                            _model.DungeonBusy(false);
                        });
                        confirmInfo.IsArtifact.SetValue(true);
                        _view.CommandCallConfirm(confirmInfo);
                    },
                    template = learnSkillInfo
                };
                _view.CommandCallPopup(popupInfo);
            }
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
                cautionInfo.SetTitle("回復できませんでした");
                _view.CommandCallCaution(cautionInfo);
            }
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
            _view.CommandRefresh();
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
        }


    }
}