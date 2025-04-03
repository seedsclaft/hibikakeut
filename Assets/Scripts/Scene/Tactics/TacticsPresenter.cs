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
            _view.SetBattleMemberList(MakeListData(_model.EditMembers()));
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
                case CommandType.SelectRecord:
                    CommandSelectRecordSeek((SymbolResultInfo)viewEvent.template);
                    break;
                case CommandType.CancelSymbolRecord:
                    if (_alcanaSelectBusy || _shopSelectBusy)
                    {
                        return;
                    }
                    CommandCancelSymbolRecord();
                    break;
                case CommandType.CancelSelectSymbol:
                    CommandCancelSelectSymbol();
                    break;
                case CommandType.CallBattleMemberSelect:
                    CommandBattleMemberSelect();
                    break;
                case CommandType.CallBattleMemberSelectEnd:
                    CommandBattleMemberSelectEnd();
                    break;
                case CommandType.Back:
                    CommandBack();
                    break;
                case CommandType.DecideRecord:
                    CommandDecideRecord();
                    break;
                case CommandType.SelectAlcanaList:
                    CommandSelectAlcanaList((SkillInfo)viewEvent.template);
                    break;
                case CommandType.EndShopSelect:
                    CommandEndShopSelect();
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
                case CommandType.CancelRecordList:
                    CommandCancelRecordList();
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



        private void CommandCallEdit()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.ActivateBattleMemberList();
        }

        private void CommandBattleMemberSelect()
        {
            var actorInfo = _view.SelectBattleMember;
            if (actorInfo == null)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            if (_model.SwapFromActor == null)
            {
                // 選択する
                _model.SetSwapFromActorInfo(actorInfo);
            } else
            {
                // 交換する
                _model.SwapActorInfo(actorInfo);
                _model.SetSwapFromActorInfo(null);
            }

            bool select(ActorInfo a)
            {
                return _model.SwapFromActor != null &&_model.SwapFromActor.BattleIndex.Value == a.BattleIndex.Value;
            }
            _view.RefreshBattleMemberList(MakeListData(_model.EditMembers(),(a) => {return true;},select));
            CommandRefresh();
        }

        private void CommandBattleMemberSelectEnd()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SetSwapFromActorInfo(null);
            _view.RefreshBattleMemberList(MakeListData(_model.EditMembers()));
            CommandRefresh();
            _view.DeactivateBattleMemberList();
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

        private void CommandOnCancelSymbol()
        {
            _view.SetViewBusy(false);
            _view.ActivateCommandList();
        }


        private void CommandSelectRecordSeek(SymbolResultInfo symbolResultInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            //Symbolに対応したシンボルを表示
            _view.SetSymbols(_model.StageResultInfos(symbolResultInfo));
            _view.ShowRecordList();
            _view.ShowSymbolRecord();
            _view.CommandRefresh();
        }

        private void CommandCancelSymbolRecord()
        {
            _view.HideRecordList();
            _view.HideSymbolRecord();
            _view.ChangeBackCommandActive(false);
            _view.CommandRefresh();
            _backCommand = CommandType.None;
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
                    CommandCallDeparture();
                    break;
                case "MoveBattler":
                    CommandCallMoveBattler();
                    break;
                case "Wait":
                    CommandCallWait();
                    break;
                case "Battle":
                    CommandCallBattle();
                    break;
                case "PARADIGM":
                    break;
                case "MENU":
                    CommandStatus();
                    break;
                case "EDIT":
                    CommandCallEdit();
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

        private void CommandCallDeparture()
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

        private void CommandCallMoveBattler()
        {
            _view.EndTacticsCommand();
            _model.MakeMoveBattlerHex();
            _view.RefreshTiles();
            UpdateHexIndex();
        }

        private void CommandCallWait()
        {
            _view.EndTacticsCommand();
        }

        private void CommandCallBattle()
        {
            var battleSceneInfos = _model.BattleSceneInfos();
            _view.BattleMemberSelect(MakeListData(battleSceneInfos));
        }

        private void CommandDecideBattleMemberSelect(BattleSceneInfo battleSceneInfo)
        {
            CommandStartStage(battleSceneInfo);
        }

        private void CommandCancelBattleMemberSelect()
        {
            _view.CancelBattleMemberSelect();
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
            // 行動
            _view.SetTacticsCommand(_model.EndMoveBattlerCommand());
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


        private void CommandCancelSelectSymbol()
        {
            //_model.ResetRecordStage();
            _model.SetFirstBattleActorId();
            CommandRefresh();
            _view.ShowRecordList();
            _view.ShowSymbolRecord();
        }

        private void CancelSelectSymbol()
        {
            CommandRefresh();
            //CommandSelectRecordSeek(_model.CurrentSelectRecord());
            //_model.ResetRecordStage();
        }

        private void CommandDecideRecord()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19300),(a) => UpdatePopupCheckStartRecord((ConfirmCommandType)a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupCheckStartRecord(ConfirmCommandType confirmCommandType)
        {
        }

        private void CommandParallel()
        {
        }

        private void CommandRefreshShop()
        {
            //_view.SetNuminous(_model.Currency - _model.LearningShopMagicCost());
            //var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
            //_view.SetAlcanaSelectInfos(ListData.MakeListData(_model.ShopMagicSkillInfos(getItemInfos)));
        }

        private void CommandSelectAlcanaList(SkillInfo skillInfo)
        {
            /*
            var symbolType = _model.CurrentSelectRecord().SymbolType;
            if (symbolType == SymbolType.Alcana && _alcanaSelectBusy)
            {
                var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(19250,skillInfo.Master.Name),(a) => UpdateSelectAlcana(a),ConfirmType.SkillDetail);
                confirmInfo.SetSkillInfo(new List<SkillInfo>(){skillInfo});
                _view.CommandCallConfirm(confirmInfo);
            } else
            if (symbolType == SymbolType.Shop && _shopSelectBusy)
            {
                if (_model.IsSelectedShopMagic(skillInfo))
                {
                    // 既に選択済み
                    _model.CancelShopCurrency(skillInfo);
                    CommandRefreshShop();
                } else
                if (_model.EnableShopMagic(skillInfo))
                {
                    var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(19260,_model.ShopLearningCost(skillInfo).ToString()) + DataSystem.GetReplaceText(19250,skillInfo.Master.Name),(a) => UpdateShop(a,skillInfo),ConfirmType.SkillDetail);
                    confirmInfo.SetSkillInfo(new List<SkillInfo>(){skillInfo});
                    _view.CommandCallConfirm(confirmInfo);
                } else
                {
                    var confirmInfo = new ConfirmInfo(DataSystem.GetText(19410),(a) => 
                    {
                    });
                    confirmInfo.SetIsNoChoice(true);
                    _view.CommandCallConfirm(confirmInfo);
                }
            }
            */
        }

        private void UpdateSelectAlcana(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                /*
                // アルカナ選択
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                var alcanaSelect = _view.AlcanaSelectSkillInfo();
                getItemInfos = getItemInfos.FindAll(a => a.Param1 == alcanaSelect.Id);
                GotoStrategyScene(getItemInfos,_model.StageMembers());
                _model.MakeSelectRelic(alcanaSelect.Id);
                */
            }
        }

        private void UpdateShop(ConfirmCommandType confirmCommandType,SkillInfo skillInfo)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                // 魔法入手、Nu消費
                _model.PayShopCurrency(skillInfo);
                CommandRefreshShop();
            }
        }

        private void CommandEndShopSelect()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19230),(a) => UpdatePopupEndShopSelect((ConfirmCommandType)a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupEndShopSelect(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var getItemInfos = _model.LearningShopMagics();
                GotoStrategyScene(getItemInfos,_model.StageMembers());
            } else
            {
                _backCommand = CommandType.EndShopSelect;
            }
        }

        private void CheckActorSymbol(GetItemInfo getItemInfo)
        {
            var actorData = DataSystem.FindActor(getItemInfo.Param1);
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(19270,actorData.Name),(a) => UpdatePopupActorSymbol(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupActorSymbol(ConfirmCommandType confirmCommandType)
        {
            /*
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                var actorInfos = _model.PartyInfo.ActorInfos.FindAll(a => a.ActorId == getItemInfos[0].Param1);
                GotoStrategyScene(getItemInfos,actorInfos);
            } else
            {
                CancelSelectSymbol();
            }
            */
        }

        private void CheckSelectActorSymbol()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19210),(a) => UpdatePopupSelectActorSymbol(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupSelectActorSymbol(ConfirmCommandType confirmCommandType)
        {
            /*
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                CommandCallAddActorInfo(_model.CurrentSelectRecord(),true);
            } else
            {
                CancelSelectSymbol();
            }
            */
        }

        private void CheckShopStageSymbol()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19220),(a) => UpdatePopupShopSymbol((ConfirmCommandType)a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupShopSymbol(ConfirmCommandType confirmCommandType)
        {
            /*
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _shopSelectBusy = true;            
                _backCommand = CommandType.EndShopSelect;
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                _view.SetAlcanaSelectInfos(ListData.MakeListData(_model.ShopMagicSkillInfos(getItemInfos)));
            } else
            {
                CancelSelectSymbol();
            }
            */
        }

        private void CheckAlcanaSymbol(List<GetItemInfo> getItemInfos)
        {
            CheckAlcanaSymbol(_model.AlcanaMagicSkillInfos(getItemInfos));
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }
        
        private void CheckAlcanaSymbol(List<SkillInfo> skillInfos)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(19200,""),(a) => UpdatePopupAlcanaSymbol(a),ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(skillInfos);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupAlcanaSymbol(ConfirmCommandType confirmCommandType)
        {
            /*
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _alcanaSelectBusy = true;
                // アルカナ選択
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                _view.SetAlcanaSelectInfos(ListData.MakeListData(_model.AlcanaMagicSkillInfos(getItemInfos)));
            } else
            {
                CancelSelectSymbol();
            }
            */
        }

        private void CheckResourceSymbol(GetItemInfo getItemInfo)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(19280,getItemInfo.Param1.ToString()),(a) => UpdatePopupResourceSymbol(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupResourceSymbol(ConfirmCommandType confirmCommandType)
        {
            /*
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var currentRecord = _model.CurrentSelectRecord();
                GotoStrategyScene(currentRecord.SymbolInfo.GetItemInfos,_model.StageMembers());
            } else
            {
                CancelSelectSymbol();
            }
            */
        }

        private void GotoStrategyScene(List<GetItemInfo> getItemInfos,List<ActorInfo> actorInfos)
        {
            var strategySceneInfo = new StrategySceneInfo
            {
                GetItemInfos = getItemInfos.FindAll(a => !a.GetFlag),
                ActorInfos = actorInfos,
                InBattle = false
            };
            _model.ResetBattlerIndex();
            _view.CommandGotoSceneChange(Scene.Strategy,strategySceneInfo);
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
            _backCommand = CommandType.CancelSymbolRecord;
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