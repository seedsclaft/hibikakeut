using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    using Tactics;
    using UnityEngine.U2D;

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
            
            _view.SetStageInfo(_model.CurrentStage);
            //_view.SetTacticsCommand(MakeListData(_model.TacticsCommand()));
            //_view.SetSymbols(ListData.MakeListData(_model.TacticsSymbols()));
            _view.SetUIButton();
            _view.SetBackGround(_model.CurrentStage.Master.BackGround);
            //_view.SetBattleMemberList(MakeListData(_model.EditMembers()));
            _view.SetHexTileList(MakeListData(_model.HexFields()));
            //_view.SetNuminous(_model.Currency);
            CommandRefresh();
            await PlayTacticsBgm(timeStamp);
            _view.ChangeUIActive(true);
            // チュートリアル確認
            //CheckTutorialState();
        }

        private void CheckTutorialState(object commandType = null)
        {
            Func<TutorialData,bool> enable = (tutorialData) => 
            {
                var checkFlag = false;
                /*
                if (tutorialData.Param1 == 100)
                {
                    // マス一覧を初めて開く
                    checkFlag = _view.SymbolRecordListActive;
                }
                if (tutorialData.Param1 == 200)
                {
                    // 編成を初めて開く
                    checkFlag = commandType == CommandType.SelectSymbol;
                }
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
            if (_model.SceneParam != null && _model.SceneParam.ReturnBeforeBattle)
            {
                /*
                var currentRecord = _model.CurrentSelectRecord();
                if (currentRecord != null)
                {
                    CommandSelectTacticsCommand(TacticsCommandType.Paradigm);
                    CommandStageSymbol();
                    CommandSelectRecordSeek(currentRecord);
                    CommandSelectRecord(currentRecord);
                }
                */
            } else
            if (_model.SceneParam != null && _model.SceneParam.ReturnNextBattle)
            {
                /*
                var currentRecord = _model.CurrentSelectRecord();
                if (currentRecord != null)
                {
                    CommandSelectTacticsCommand(TacticsCommandType.Paradigm);
                }
                */
            }
            CheckTutorialState();
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
                case CommandType.CallEnemyInfo:
                    //if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandCallEnemyInfo((SymbolResultInfo)viewEvent.template);
                    break;
                case CommandType.CallAddActorInfo:
                    CommandCallAddActorInfo((SymbolResultInfo)viewEvent.template,false);
                    break;
                case CommandType.PopupSkillInfo:
                    CommandPopupSkillInfo((List<GetItemInfo>)viewEvent.template);
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
                    CommandSelectCharaLayer((int)viewEvent.template);
                    break;
                case CommandType.MoveHexMap:
                    CommandMoveHexMap((InputKeyType)viewEvent.template);
                    break;
                case CommandType.EndMoveBattler:
                    CommandEndMoveBattler();
                    break;
                case CommandType.DecideBattleMemberSelect:
                    CommandDecideBattleMemberSelect((BattleSceneInfo)viewEvent.template);
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
            _model.PartyInfo.StartStage.SetValue(true);
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
                case "SAVE":
                    CommandSave();
                    break;
            }
        }

        private void CommandCancellTacticsCommand()
        {
            _view.EndTacticsCommand();
        }

        private void CommandDeparture()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var characterListInfo = new CharacterListInfo((int actorId) => 
            {
                _model.SetDepatureActorId(actorId);
                _view.EndTacticsCommand();
                _view.CallSystemCommand(Base.CommandType.ClosePopup);
                CommandDepartureHex();
                CommandRefresh();
                _busy = false;
            },
            () => 
            {
                CommandRefresh();
                _busy = false;
            });
            characterListInfo.SetActorInfos(_model.StageMembers());
            
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.CharacterList,
                template = characterListInfo,
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
            _view.RefreshTiles();
            UpdateHexIndex();
        }

        private void CommandMoveBattler()
        {
            _view.EndTacticsCommand();
            _model.MakeMoveBattlerHex();
            _view.RefreshTiles();
            UpdateHexIndex();
            // 自動選択
            if (_model.AutoMode.Value)
            {
                // 移動先を設定して選択動作
                _model.DecideAutoMoveBattlerField();
                var (actions,moveBattler) = _model.SelectMoveBattler();
                _view.SelectMoveBattler(actions,moveBattler);
            }
        }

        private void CommandWait()
        {
            _view.EndTacticsCommand();
        }

        private void CommandBattle()
        {
            var battleSceneInfos = _model.BattleSceneInfos();
            _view.BattleMemberSelect(MakeListData(battleSceneInfos));
        }

        private void CommandUnitActEnd()
        {
            _view.EndTacticsCommand();
            _model.UnitActEnd();
            // 操作不可プレイヤー・オート動作なら操作を委託
            if (_model.IsPlayable == false)
            {
                _model.AutoMode.SetValue(true);
                CommandAutoMode();
            }
        }

        private void CommandTurnEnd()
        {
            _view.EndTacticsCommand();
            _model.TurnEnd();
            // 操作不可プレイヤー・オート動作なら操作を委託
            if (_model.IsPlayable == false)
            {
                _model.AutoMode.SetValue(true);
                CommandAutoMode();
            }
        }

        private void CommandDecideBattleMemberSelect(BattleSceneInfo battleSceneInfo)
        {
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
            var hexUnit = _model.HexUnit();
            if (hexUnit == null)
            {
                return;
            }
            switch (hexUnit.HexUnitType)
            {
                case HexUnitType.Battler:
                    CommandSelectBattler();
                    break;
                case HexUnitType.Basement:
                    CommandSelectBasement();
                    break;
                case HexUnitType.Reach:
                    CommandSelectReach();
                    break;
            }
        }

        private void CommandSelectBattler()
        {
            _view.SetTacticsCommand(_model.BattlerCommand());
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

        private void CommandSelectReach()
        {
            switch (_model.CommandKey)
            {
                case "Departure":
                    _model.SelectDeparture();
                    _view.RefreshTiles();
                    UpdateHexIndex();
                    break;
                case "MoveBattler":
                    var (actions,moveBattler) = _model.SelectMoveBattler();
                    _view.SelectMoveBattler(actions,moveBattler);
                    break;
            }
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
            _view.SetStageInfo(_model.CurrentStage);
            //_view.SetAlcanaInfo(_model.AlcanaSkillInfos());
            _view.SetTacticsCharaLayer(_model.StageMembers());
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

        private void CommandMoveHexMap(InputKeyType inputKeyType)
        {
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
            UpdateHexIndex();
        }

        private void UpdateHexIndex()
        {
            _view.UpdateHexIndex(_model.FieldX.Value,_model.FieldY.Value);
        }
    }
}