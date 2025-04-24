using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    using Tactics;

    public partial class TacticsPresenter : BasePresenter
    {
        TacticsModel _model = null;
        TacticsView _view = null;

        private bool _busy = true;
        private bool _alcanaSelectBusy = false;
        private bool _shopSelectBusy = false;

        private CommandType _backCommand = CommandType.None;
        public TacticsPresenter(TacticsView view)
        {
            _view = view;
            SetView(_view);
            _model = new TacticsModel();
            SetModel(_model);

            // イベント取得
            if (CheckEventData())
            {
                return;
            }
            InitializeView();
        }

        private bool CheckEventData(Action endEvent = null)
        {
            var stageEvent = GetStageEventData(EventTiming.BeforeTactics);
            if (stageEvent != null)
            {
                switch (stageEvent.Type)
                {
                    case StageEventType.AdvStart:
                        // TimeStampを取得してBgmをフェードアウト
                        var timeStamp = SoundManager.Instance.CurrentTimeStamp();
                        if (CheckAdvEvent(EventTiming.BeforeTactics,timeStamp,() => CheckEventData(() => InitializeView(timeStamp))))
                        {
                            return true;
                        }
                        return true;
                    case StageEventType.ForceBattle:
                        return true;
                }
            }
            endEvent?.Invoke();
            return false;
        }

        private async void InitializeView(float timeStamp = 0)
        {
            _busy = false;
            _view.ChangeUIActive(false);
            //_model.AssignBattlerIndex();
            _view.SetHelpWindow();
            _view.ChangeBackCommandActive(false);
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetBackGround(_model.CurrentStage?.Master?.BackGround);
            
            _view.UpdateStageInfo(_model.CurrentStage);
            //_view.SetTacticsCommand(MakeListData(_model.TacticsCommand()));
            //_view.SetSymbols(ListData.MakeListData(_model.TacticsSymbols()));
            _view.SetUIButton();
            _view.SetBackGround(_model.CurrentStage.Master.BackGround);
            //_view.SetBattleMemberList(MakeListData(_model.EditMembers()));
            _view.SetHexTileList(MakeListData(_model.HexFields()),_model.CurrentStage.Master.Width);
            //_view.SetNuminous(_model.Currency);
            CommandRefresh();
            await PlayTacticsBgm(timeStamp);
            _view.ChangeUIActive(true);
            CommandReturnStrategy();
            // チュートリアル確認
            CheckTutorialState();
            // ステージ開始時
            if (_model.CurrentStage.TurnCount.Value == 1 && _model.GetTurnTeam().ActPoint.Value == _model.GetTurnTeam().CurrentActPoint.Value)
            {
                TurnStartAnimation();
            }
        }

        private void CheckTutorialState(object commandType = null)
        {
            Func<TutorialData,bool> enable = (tutorialData) => 
            {
                var checkFlag = false;
                if (tutorialData.Param1 == 0)
                {
                    // ゲームの手引きを表示
                    checkFlag = true;
                }
                if (tutorialData.Param1 == 100 && commandType != null)
                {
                    // 出撃コマンドを選択前
                    checkFlag = (CommandType)commandType == CommandType.SelectHexUnit && _view.TacticsCommandData.Key == _model.DepartureCommand.Key;
                }
                if (tutorialData.Param1 == 300)
                {
                    // 出撃前
                    checkFlag = _model.DepartureUnitInfo != null;
                }
                if (tutorialData.Param1 == 400)
                {
                    // 出撃を終える
                    checkFlag = _model.GetTurnTeamState() == TeamState.TurnEnd;
                }
                if (tutorialData.Param1 == 500 && commandType != null)
                {
                    // 2ターン目開始
                    checkFlag = _model.CurrentStage.TurnCount.Value == 3 && (CommandType)commandType == CommandType.CallTacticsCommand;
                }
                if (tutorialData.Param1 == 600 && commandType != null)
                {
                    // 出撃コマンドを選択前
                    checkFlag = (CommandType)commandType == CommandType.SelectHexUnit && _model.CommandKey == _model.MoveBattlerCommand.Key;
                }
                /*
                if (tutorialData.Param1 == 300)
                {
                    // トレジャーのマスを初めて開く
                    checkFlag = _model.CurrentStage.CurrentSeek == 2;
                }
                if (tutorialData.Param1 == 400)
                {
                    // Seek３の編成を初めて開く
                    checkFlag = _model.CurrentStage.CurrentSeek == 3 && commandType == CommandType.SelectSymbol;
                }
                if (tutorialData.Param1 == 900)
                {
                    // 仲間加入のマスを初めて開く
                    checkFlag = _view.SymbolRecordListActive && _model.CurrentStage.CurrentSeek == 7;
                }
                if (tutorialData.Param1 == 1100)
                {
                    // ステージ2の最初
                    checkFlag = _model.CurrentStage.Id == 2;
                }
                if (tutorialData.Param1 == 1200)
                {
                    // Activeの魔法を初めて入手するかステージ3の最初
                    checkFlag = _model.StageMembers().Find(a => a.LearnSkillIds().FindAll(b => DataSystem.FindSkill(b).SkillType == SkillType.Active).Count > 0) != null || _model.CurrentStage.Id == 3;
                }
                */
                if (checkFlag)
                {
                    var stageEvent = GetStageEventData(EventTiming.StartTutorial);
                    if (stageEvent != null)
                    {
                        if (stageEvent.Param == tutorialData.Param1)
                        {
                            switch (stageEvent.Type)
                            {
                                case StageEventType.TurnEndCommandEnable:
                                    _model.SetTurnEndCommandEnable(true);
                                    break;
                                case StageEventType.TurnEndCommandDisable:
                                    _model.SetTurnEndCommandEnable(false);
                                    break;
                            }
                            _model.AddEventReadFlag(stageEvent);
                        }
                        
                    }
                    _model.SetTutorial(tutorialData);
                }
                return checkFlag;
            };
            Func<TutorialData,bool> checkEnd = (tutorialData) => 
            {
                return true;
            };
            var tutorialViewInfo = new TutorialViewInfo
            {
                SceneType = (int)Scene.Tactics,
                CheckEndMethod = checkEnd,
                CheckMethod = enable,
                EndEvent = () => 
                {
                    _busy = false;
                    CheckTutorialState(commandType);
                }
            };
            _view.CommandCheckTutorialState(tutorialViewInfo);
        }

        public void CommandReturnStrategy()
        {
            // マップ表示初期位置を設定
            _view.RefreshTiles(_model.CurrentStage.FieldX.Value,_model.CurrentStage.FieldY.Value);
            // Hp0のユニットを消滅する
            var lostUnitInfos = _model.LostUnitInfos();
            if (lostUnitInfos.Count > 0)
            {
                _view.LostBattlerUnit(lostUnitInfos);
            }
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Tactics)
            {
                return;
            }
            Debug.Log(viewEvent.ViewCommandType.CommandType);
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.CallTacticsCommand:
                    CommandCallTacticsCommand();
                    break;
                case CommandType.CancellTacticsCommand:
                    CommandCancellTacticsCommand();
                    break;
                case CommandType.SymbolDetailInfo:
                    break;
                case CommandType.CallStatus:
                    CommandStatus();
                    break;
                case CommandType.SelectHexUnit:
                    CommandSelectHexUnit();
                    break;
                case CommandType.CancelHexUnit:
                    CommandCancelHexUnit();
                    break;
                case CommandType.CallEnemyInfo:
                    //if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandCallEnemyInfo((SymbolResultInfo)viewEvent.Template);
                    break;
                case CommandType.CallAddActorInfo:
                    CommandCallAddActorInfo((SymbolResultInfo)viewEvent.Template,false);
                    break;
                case CommandType.PopupSkillInfo:
                    CommandPopupSkillInfo((List<GetItemInfo>)viewEvent.Template);
                    break;
                case CommandType.Back:
                    CommandBack();
                    break;
                case CommandType.HideAlcanaList:
                    CommandHideAlcanaList();
                    break;
                case CommandType.ScorePrize:
                    CommandScorePrize();
                    break;
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
                case CommandType.StageHelp:
                    CommandStageHelp();
                    break;
                case CommandType.AlcanaCheck:
                    CommandAlcanaCheck();
                    break;
                case CommandType.SelectCharaLayer:
                    CommandSelectCharaLayer((int)viewEvent.Template);
                    break;
                case CommandType.SelectHexMap:
                    CommandSelectHexMap((HexField)viewEvent.Template);
                    break;
                case CommandType.MoveHexMap:
                    CommandMoveHexMap((InputKeyType)viewEvent.Template);
                    break;
                case CommandType.EndMoveBattler:
                    CommandEndMoveBattler();
                    break;
                case CommandType.EndLostBattler:
                    CommandEndLostBattler();
                    break;
                case CommandType.EndHealUnits:
                    CommandEndAnimation();
                    break;
                case CommandType.DecideBattleMemberSelect:
                    CommandDecideBattleMemberSelect((BattleSceneInfo)viewEvent.Template);
                    break;
                case CommandType.CancelBattleMemberSelect:
                    CommandCancelBattleMemberSelect();
                    break;
                    
                    
            }
            // チュートリアル確認
            CheckTutorialState(viewEvent.ViewCommandType.CommandType);
        }

        private void CommandStartStage(BattleSceneInfo battleSceneInfo)
        {
            // 演出
            _busy = true;
            SoundManager.Instance.StopBgm();
            PlayStartBattleBgm();
            var animation = _model.StartStageAnimation();
            _view.StartStageAnimation(animation);
            _view.WaitFrame(120,() => 
            {
                BattleStart(battleSceneInfo);
            });
        }

        private void BattleStart(BattleSceneInfo battleSceneInfo)
        {
            _model.SaveTempBattleMembers();
            _view.CommandChangeViewToTransition(null);
            _view.ChangeUIActive(false);
            _view.CommandSceneChange(Scene.Battle,battleSceneInfo);
        }

        private async void PlayStartBattleBgm()
        {
            //var currentSymbol = _model.SelectedSymbol();
            // ボス戦なら
            /*
            if (currentSymbol.Master.SymbolType == SymbolType.Boss)
            {
                PlayBossBgm();
            } else
            */
            {
                var bgmData = _model.TacticsBgmData();
                if (bgmData.CrossFade != "" && SoundManager.Instance.CrossFadeMode)
                {
                    SoundManager.Instance.ChangeCrossFade();
                } else
                {
                    PlayBattleBgm();
                }
            }
            SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
        }

        private void CommandSave()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            var sceneParam = new FileListSceneInfo
            {
                IsLoad = false
            };
            var popupInfo = new PopupInfo()
            {
                PopupType = PopupType.FileList,
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                },
                template = sceneParam
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandBack()
        {
            if (_alcanaSelectBusy)
            {
                return;
            }
            if (_backCommand != CommandType.None)
            {
                //CallEvent(_backCommand,_model.TacticsCommandType);
                //UpdateCommand(eventData);
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _backCommand = CommandType.None;
            }
        }

        private void CheckHealUnit()
        {
            // 拠点回復確認
            if (_model.CurrentStage.CheckedTurnStart.Value == false)
            {
                _model.CurrentStage.CheckedTurnStart.SetValue(true);
                var healHexUnits = _model.CheckHealUnits();
                _view.HealUnits(healHexUnits.Item1,healHexUnits.Item2);
                return;
            }
            CommandEndAnimation();
        }

        private void CommandEndAnimation()
        {
            // 操作不可プレイヤー・オート動作なら操作を委託
            if (!_model.IsPlayable)
            {
                _model.AutoMode.SetValue(true);
                CommandAutoMode();
            } else
            {
                _view.EndTacticsCommand();
                // 座標をチームの最終選択に戻す
                var lastHexX = _model.CurrentStage.GetTurnTeamInfo().LastSelectHexX.Value;
                var lastHexY = _model.CurrentStage.GetTurnTeamInfo().LastSelectHexY.Value;
                if (lastHexX != 0 && lastHexY != 0)
                {
                    _view.RefreshTiles(lastHexX,lastHexY);
                }
                _model.AutoMode.SetValue(false);
            }
        }

        private void CommandCallTacticsCommand()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var tacticsCommandData = _view.TacticsCommandData;
            _model.SetCommandKey(tacticsCommandData.Key);
            switch (tacticsCommandData.Key)
            {
                case "Departure":
                    CommandDeparture();
                    break;
                case "MoveBattler":
                    CommandMoveBattler();
                    break;
                case "Wait":
                    CommandWait();
                    break;
                case "Battle":
                    CommandBattle();
                    break;
                case "UnitActEnd":
                    CommandUnitActEnd();
                    break;
                case "TurnEnd":
                    CommandTurnEnd();
                    break;
                case "Units":
                    CommandUnits();
                    break;
                case "Save":
                    CommandSave();
                    break;
                case "Conquer":
                    CommandConquer();
                    break;
                case "UnitEdit":
                    CommandUnitEdit();
                    break;
                case "Return":
                    CommandReturn();
                    break;
                case "Event":
                    CommandEvent();
                    break;
            }
        }

        private void CommandCancellTacticsCommand()
        {
            switch (_model.CommandKey)
            {
                //case "Departure":
                case "MoveBattler":
                case "Battle":
                    //_model.SetCommandKey("");
                    //_model.ClearReachAreas();
                    // 移動前に戻す
                    _model.SetCommandKey("");
                    _model.BeforeMoveBattler();
                    _view.UpdateTileItems();
                    _model.SelectingHexUnitId.SetValue(0);
                    break;
            }
            _view.EndTacticsCommand();
        }

        private void CommandDeparture()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var unitInfoListInfo = new UnitInfoListInfo((unitInfo) => 
            {
                _model.SetDepatureUnitInfo(unitInfo);
                _view.CallSystemCommand(Base.CommandType.ClosePopup);
                CommandRefresh();
                CommandDepartureHex();
                _view.EndTacticsCommand();
                _busy = false;
            },
            () => 
            {
                CommandRefresh();
                _busy = false;
            });
            unitInfoListInfo.SetUnitInfos(_model.DepatureUnitInfos());
            unitInfoListInfo.IsUnitEdit.SetValue(false);
            
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.DepatureList,
                template = unitInfoListInfo,
                EndEvent = () =>
                {
                    CommandRefresh();
                    _busy = false;
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandDepartureHex()
        {
            _model.MakeDepartureHex();
            _view.RefreshTiles(_model.CurrentStage.FieldX.Value,_model.CurrentStage.FieldY.Value);
        }

        private void CommandMoveBattler()
        {
            _view.EndTacticsCommand();
            _model.ClearMoveReachAreas();
            _model.MakeMoveBattlerHex();
            _view.RefreshTiles(_model.CurrentStage.FieldX.Value,_model.CurrentStage.FieldY.Value);
            // 自動選択
            if (_model.AutoMode.Value)
            {
                // 移動先を設定して選択動作
                _view.DeActivateHexTiles();
                _model.DecideAutoMoveBattlerField();
                var (actions,moveBattler) = _model.SelectMoveBattler(_model.CurrentStage.FieldX.Value,_model.CurrentStage.FieldY.Value);
                _view.UpdateTileItems();
                _view.SelectMoveBattler(actions,moveBattler);
            }
        }

        private void CommandWait()
        {
            _view.EndTacticsCommand();
            _model.UnitActEnd();
            CommandRefresh();
            _model.SetLastSelectHex();
        }

        private void CommandBattle()
        {
            var battleSceneInfos = _model.BattleSceneInfos();
            _view.BattleMemberSelect(MakeListData(battleSceneInfos));
        }

        private void CommandConquer()
        {
            _model.ConquerBasement();
            CommandRefresh();
            _view.UpdateTileItems();
            _model.SetLastSelectHex();
            _model.SetCommandKey("");
            var victory = CheckVictory();
            if (victory)
            {
                return;
            }
            var gameOver = CheckGameOver();
            if (gameOver)
            {
                return;
            }
            if (_model.GetTurnTeam().CurrentActPoint.Value == 0)
            {
                _model.TurnEnd();
                CommandRefresh();
                TurnStartAnimation();
            }
        }

        private void CommandUnitEdit()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var unitInfoListInfo = new UnitInfoListInfo((unitInfo) => 
            {
                _view.CallSystemCommand(Base.CommandType.ClosePopup);
                CommandRefresh();
                _view.EndTacticsCommand();
                _busy = false;
            },
            () => 
            {
                CommandRefresh();
                _busy = false;
            });
            unitInfoListInfo.SetUnitInfos(_model.DepatureUnitInfos());
            unitInfoListInfo.IsUnitEdit.SetValue(true);
            
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.UnitInfoList,
                template = unitInfoListInfo,
                EndEvent = () =>
                {
                    CommandRefresh();
                    _busy = false;
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandReturn()
        {
            _view.EndTacticsCommand();
            _model.ReturnBasement();
            CommandRefresh();
            _view.UpdateTileItems();
            _model.SetLastSelectHex();
            _model.SetCommandKey("");
        }

        private void CommandEvent()
        {
            var results = _model.AlcanaOpen();
            _view.EndTacticsCommand();
            if (results != null)
            {
                CommandPopupSkillInfo(results);
                return;
            }
            CommandRefresh();
            _view.UpdateTileItems();
            _model.SetLastSelectHex();
            _model.SetCommandKey("");
        }

        private void CommandUnitActEnd()
        {
            _view.EndTacticsCommand();
            _model.UnitActEnd();
            CommandRefresh();
            _model.SetLastSelectHex();
            _model.SetCommandKey("");
            if (_model.GetTurnTeam().CurrentActPoint.Value == 0)
            {
                _model.TurnEnd();
                CommandRefresh();
                TurnStartAnimation();
            }
        }

        private void CommandTurnEnd()
        {
            _view.EndTacticsCommand();
            _model.TurnEnd();
            CommandRefresh();
            TurnStartAnimation();
        }

        private void TurnStartAnimation()
        {
            _model.CurrentStage.CheckedTurnStart.SetValue(false);
            var text = _model.GetTurnTeam().TeamId.Value == (int)TeamIdType.Home ? "Player Turn" : "Enemy Turn";
            _view.DeActivateHexTiles();
            _view.StartAnimation(text,() => 
            {
                _busy = false;
                CheckHealUnit();
            });
            _busy = true;
        }

        private void CommandUnits()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var unitInfoListInfo = new UnitInfoListInfo((unitInfo) => 
            {
                _view.CallSystemCommand(Base.CommandType.ClosePopup);
                CommandRefresh();
                _view.EndTacticsCommand();
                _busy = false;
            },
            () => 
            {
                CommandRefresh();
                _busy = false;
            });
            unitInfoListInfo.SetUnitInfos(_model.FieldUnitInfos());
            
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.UnitInfoList,
                template = unitInfoListInfo,
                EndEvent = () =>
                {
                    CommandRefresh();
                    _busy = false;
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        public bool CheckVictory()
        {
            if (_model.CurrentStage.CheckVictory())
            {
                _busy = true;
                _view.StartAnimation("Victory!",() => 
                {
                    _busy = false;
                    _model.StageClear();
                    _view.CommandSceneChange(Scene.Title);
                });
                return true;
            }
            return false;
        }

        public bool CheckGameOver()
        {
            if (_model.CurrentStage.CheckGameOver())
            {
                _busy = true;
                _view.StartAnimation("Failed...",() => 
                {
                    _busy = false;
                    //_model.StageClear();
                    _view.CommandSceneChange(Scene.Title);
                });
                return true;
            }
            return false;
        }

        private void CommandDecideBattleMemberSelect(BattleSceneInfo battleSceneInfo)
        {
            _model.UseActPoint();
            CommandStartStage(battleSceneInfo);
        }

        private void CommandCancelBattleMemberSelect()
        {
            _view.CancelBattleMemberSelect();
        }

        private void CommandAutoMode()
        {
            // チームの状態から行動を選択
            var teamState = _model.GetTurnTeamState();
            switch (teamState)
            {
                case TeamState.MoveBattler:
                    // 選択マスを設定して移動
                    var isSelectable = _model.SelectAutoMoveBattler();
                    if (isSelectable)
                    {
                        CommandMoveBattler();
                    } else
                    {
                        CommandUnitActEnd();
                    }
                    return;
                case TeamState.TurnEnd:
                    CommandTurnEnd();
                    return;
            }
        }

        private void CommandStatus(int startIndex = -1)
        {
            int actorId = -1;
            if (startIndex != -1)
            {
                // actorIdに変換
                var actor = _model.TacticsActor();
                if (actor != null)
                {
                    actorId = actor.ActorId.Value;
                }
            }

            CommandStatusInfo(_model.PastActorInfos(),false,true,true,false,actorId,() => 
            {
                //_view.SetNuminous(_model.Currency);
                CommandRefresh();
            });
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandSelectHexUnit()
        {
            var tile = _view.SelectHexField;
            _model.SetFieldXY(tile.X,tile.Y);
            var hexUnit = _model.SortedHexUnit();
            if (_model.CommandKey == _model.DepartureCommand.Key || _model.CommandKey == _model.MoveBattlerCommand.Key)
            {
                if (hexUnit == null || hexUnit.HexUnitType != HexUnitType.Reach)
                {
                    return;
                }
            }
            if (hexUnit == null)
            {
                CommandSelectDefault();
                return;
            }
            switch (hexUnit.HexUnitType)
            {
                case HexUnitType.Battler:
                    if (_model.CanMoveBattler)
                    {
                        CommandSelectBattler();
                    } else
                    {
                        CommandSelectDefault();
                    }
                    break;
                case HexUnitType.Basement:
                    CommandSelectBasement();
                    break;
                case HexUnitType.Reach:
                    CommandSelectReach(hexUnit.HexField.X,hexUnit.HexField.Y);
                    break;
                default:
                    CommandSelectDefault();
                    break;
            }
        }

        private void CommandCancelHexUnit()
        {
            switch (_model.CommandKey)
            {
                case "Departure":
                case "MoveBattler":
                case "Event":
                    _model.SetCommandKey("");
                    _model.ClearReachAreas();
                    _view.UpdateTileItems();
                    break;
            }
        }

        private void CommandSelectBattler()
        {
            _view.SetTacticsCommand(_model.BattlerCommand());
            //_model.SetCommandKey(_model.MoveBattlerCommand.Key);
            //_view.EndTacticsCommand();
            //CommandMoveBattler();
        }

        private void CommandSelectBasement()
        {
            _view.SetTacticsCommand(_model.BasementCommand());
        }

        private void CommandEndMoveBattler()
        {
            // 自動
            if (_model.IsPlayable == false)
            {
                var commandKey =  _model.DecideAutoMoveBattlerEnd();
                switch (commandKey)
                {
                    case "Battle":
                        CommandBattle();
                        break;
                    case "UnitActEnd":
                        CommandUnitActEnd();
                        break;
                }
            } else
            {
                // 手動行動選択
                _view.SetTacticsCommand(_model.EndMoveBattlerCommand());
            }
        }

        private void CommandEndLostBattler()
        {
            _model.EndLostActions();
            _view.RefreshTiles(_model.CurrentStage.FieldX.Value,_model.CurrentStage.FieldY.Value);
            
            var gameOver = CheckGameOver();
            if (gameOver)
            {
                return;
            }
        }

        private void CommandSelectReach(int x,int y)
        {
            switch (_model.CommandKey)
            {
                case "Departure":
                    var selectDeparture = _model.SelectDeparture();
                    if (selectDeparture != null)
                    {
                        _view.RefreshTiles(selectDeparture.HexField.X,selectDeparture.HexField.Y);
                        _model.SetLastSelectHex();
                        _model.SetCommandKey("");
                    }
                    break;
                case "MoveBattler":
                    var (actions,moveBattler) = _model.SelectMoveBattler(x,y);
                    if (moveBattler != null)
                    {
                        _view.DeActivateHexTiles();
                        _view.SelectMoveBattler(actions,moveBattler);
                    }
                    break;
            }
            CommandRefresh();
        }

        private void CommandSelectDefault()
        {
            _view.SetTacticsCommand(_model.DefaultCommand());
        }

        private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
        {
        }

        private void CommandPopupSkillInfo(List<GetItemInfo> getItemInfos)
        {
            if (getItemInfos.Count == 1)
            {
                CallPopupSkillDetail("",_model.BasicSkillInfos(getItemInfos[0]));
            } else
            {
                CallPopupSkillDetail(DataSystem.GetText(19200),_model.BasicSkillGetItemInfos(getItemInfos));
            }
        }

        private void CommandRefresh()
        {
            //_view.SetSaveScore(_model.TotalScore);
            _view.UpdateStageInfo(_model.CurrentStage);
            //_view.SetAlcanaInfo(_model.AlcanaSkillInfos());
            //_view.SetTacticsCharaLayer(_model.StageMembers());
            _view.CommandRefresh();
        }

        private void CommandCallEnemyInfo(SymbolResultInfo symbolResultInfo)
        {
            /*
            switch (symbolResultInfo.SymbolType)
            {
                case SymbolType.Battle:
                case SymbolType.Boss:
                    var enemyInfos = symbolResultInfo.SymbolInfo.BattlerInfos();
                    _busy = true;
                    CommandEnemyInfo(enemyInfos,false,() => 
                    {
                        _busy = false;
                        _view.CommandRefresh();
                    });
                    break;
                case SymbolType.Alcana:
                    CallPopupSkillDetail(DataSystem.GetText(19200),_model.BasicSkillGetItemInfos(symbolResultInfo.SymbolInfo.GetItemInfos));
                    break;
                case SymbolType.Actor:
                    CommandStatusInfo(_model.AddActorInfos(symbolResultInfo.SymbolInfo.GetItemInfos[0].Param1),false,true,false,false,-1,() => 
                    {
                        _view.CommandRefresh();
                    });
                    break;
                case SymbolType.SelectActor:
                    CommandCallAddActorInfo(symbolResultInfo,false);
                    break;
                case SymbolType.Shop:
                    CallPopupSkillDetail(DataSystem.GetText(19240),_model.BasicSkillGetItemInfos(symbolResultInfo.SymbolInfo.GetItemInfos));
                    break;
            }
            */
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandCallAddActorInfo(SymbolResultInfo symbolResultInfo,bool addCommand)
        {
            List<ActorInfo> actorInfos;
            /*
            if (symbolResultInfo.StageSymbolData.Param2 == 0 && symbolResultInfo.SymbolInfo.GetItemInfos.Find(a => a.GetItemType == GetItemType.AddActor) == null)
            {
                actorInfos = _model.AddSelectActorInfos();
            } else
            {
                actorInfos = _model.AddSelectActorGetItemInfos(symbolResultInfo.SymbolInfo.GetItemInfos);
            }
            if (addCommand)
            {
                // 加入する用
                CommandStatusInfo(actorInfos,false,false,false,true,-1,() => 
                {

                });
            } else
            {
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
            }
            */
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            CommandCallSideMenu(_model.SideMenu(),() => 
            {
                _busy = false;
            });
        }

        private void CallPopupSkillDetail(string title,List<SkillInfo> skillInfos)
        {
            var confirmInfo = new ConfirmInfo(title,(a) => 
            {
                CloseConfirm();
                _view.CommandRefresh();
                CommandRefresh();
                _view.UpdateTileItems();
                _model.SetLastSelectHex();
                _model.SetCommandKey("");
            },ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(skillInfos);
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallSkillDetail(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandStageHelp()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Guide,
                template = "Tactics",
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandCancelRecordList()
        {
            //_model.ResetRecordStage();
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.HideRecordList();
            _view.CommandRefresh();
        }

        private void CommandScorePrize()
        {
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.ScorePrize,
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandAlcanaCheck()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            //_view.SetAlcanaSelectInfos(_model.MakeListData(_model.AlcanaSkillInfos(),0));
            _backCommand = CommandType.HideAlcanaList;
        }

        private void CommandHideAlcanaList()
        {
            _view.HideAlcanaList();
            _view.ChangeBackCommandActive(false);
            _backCommand = CommandType.None;
        }

        private void CommandSelectCharaLayer(int actorId)
        {
            _busy = true;
            _view.CommandSelectCharaLayer(actorId);
            CommandTacticsStatusInfo(_model.StageMembers(),false,true,true,false,actorId,() => 
            {
                _view.WaitFrame(12,() => 
                {
                    _busy = false;
                });
                _view.ActivateCommandList();
                _view.SetHelpText(DataSystem.GetText(20020));
                //_view.SetNuminous(_model.Currency);
                CommandRefresh();
            },(a) => 
            {
                _view.CommandSelectCharaLayer(a);
            });
        }

        private void CommandSelectHexMap(HexField hexField)
        {
            if (hexField == null)
            {
                return;
            }
            var lastX = _model.CurrentStage.FieldX.Value;
            var lastY = _model.CurrentStage.FieldY.Value;
            if (hexField.X != lastX || hexField.Y != lastY)
            {
                _model.ClearMoveReachAreas();
                _model.SetFieldXY(hexField.X,hexField.Y);
                if (_model.OnFieldInfos.Count > 0)
                {
                    // 移動と攻撃範囲を表示
                    var battlerUnit = _model.MakeBattlerActHex();
                    _view.ShowUnitStatus(battlerUnit);
                } else
                {
                    _view.ShowUnitStatus(null);
                }
                var fieldUnit = _model.OnFieldInfos.Find(a => !a.IsUnit);
                _view.ShowFieldStatus(fieldUnit);
            }
        }

        private void CommandMoveHexMap(InputKeyType inputKeyType)
        {
            if (_view.Busy)
            {
                return;
            }
            switch (inputKeyType)
            {
                case InputKeyType.Up:
                    _model.MoveFieldXY(0,-1);
                    break;
                case InputKeyType.Down:
                    _model.MoveFieldXY(0,1);
                    break;
                case InputKeyType.Right:
                    _model.MoveFieldXY(1,0);
                    break;
                case InputKeyType.Left:
                    _model.MoveFieldXY(-1,0);
                    break;
            }
            _view.RefreshTiles(_model.CurrentStage.FieldX.Value,_model.CurrentStage.FieldY.Value);
        }

    }
}