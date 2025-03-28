using System;
using System.Collections.Generic;
using System.Diagnostics;
using BattleParty;

namespace Ryneus
{
    using BattleParty;
    public class BattlePartyPresenter :BasePresenter
    {
        BattlePartyModel _model = null;
        BattlePartyView _view = null;

        private bool _busy = true;
        public BattlePartyPresenter(BattlePartyView view)
        {
            _view = view;
            _model = new BattlePartyModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            CommandRefresh();
            //_view.SetEnemyMembers(MakeListData(_model.EnemyInfos()));
            //_view.SetAttributeList(MakeListData(_model.AttributeTabList()));
            //_view.SetStatusButtonEvent(() => CommandStatusInfo());
            _view.SetTacticsMembers(MakeListData(_model.BattlePartyMembers()));
            _view.SetCommandList(MakeListData(_model.BattlePartyCommand()));
            //_view.SetBattleReplayEnable(_model.IsEnableBattleReplay());
            _view.OpenAnimation();
            _busy = false;
            CheckTutorialState();
        }

        private void CheckTutorialState(CommandType commandType = CommandType.None)
        {
            return;
            /*
            if (commandType == CommandType.SelectTacticsMember)
            {
                return;
            }
            Func<TutorialData,bool> enable = (tutorialData) => 
            {
                var checkFlag = true;
                if (tutorialData.Param1 == 100)
                {
                    // バトルメンバーがいる
                    checkFlag = _model.BattleMembers().Count > 0;
                }
                if (tutorialData.Param1 == 400)
                {
                    // Nuが1以上,Actor1のLvが2
                    checkFlag = _model.Currency > 0 && _model.StageMembers()[0].Level == 2;
                }
                if (tutorialData.Param1 == 500)
                {
                    // Actor1のLvが3以上,ウルフソウルを未習得
                    checkFlag = _model.StageMembers()[0].Level > 2 && !_model.StageMembers()[0].IsLearnedSkill(11010);
                }
                return checkFlag;
            };
            Func<TutorialData,bool> checkEnd = (tutorialData) => 
            {
                var checkFlag = true;
                if (tutorialData.Param3 == 410)
                {
                    // トレジャーのマスを初めて開く
                    checkFlag = _model.StageMembers()[0].Level > 2;
                }
                return checkFlag;
            };
            var tutorialViewInfo = new TutorialViewInfo
            {
                SceneType = (int)PopupType.BattleParty + 100,
                CheckEndMethod = checkEnd,
                CheckMethod = enable,
                EndEvent = () => 
                {
                    _busy = false;
                    CheckTutorialState(commandType);
                }
            };
            _view.CommandCheckTutorialState(tutorialViewInfo);
            */
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            LogOutput.Log(viewEvent.ViewCommandType.CommandType);
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.CallCommandList:
                    CommandCallCommandList((SystemData.CommandData)viewEvent.template);
                    break;
                case CommandType.CommandEndEdit:
                    CommandEndEdit();
                    break;
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
                case CommandType.DecideTacticsMember:
                    CommandDecideTacticsMember((ActorInfo)viewEvent.template);
                    break;
                case CommandType.SelectTacticsMember:
                    CommandSelectTacticsMember((ActorInfo)viewEvent.template);
                    break;
                case CommandType.SelectAttribute:
                    CommandSelectAttribute((AttributeType)viewEvent.template);
                    break;
                case CommandType.LeftAttribute:
                    CommandLeftAttribute();
                    break;
                case CommandType.RightAttribute:
                    CommandRightAttribute();
                    break;
                case CommandType.EnemyInfo:
                    CommandEnemyInfo();
                    break;
                case CommandType.BattleReplay:
                    CommandBattleReplay();
                    break;
                case CommandType.BattleStart:
                    CommandBattleStart();
                    break;
                case CommandType.CommandHelp:
                    CommandGuide();
                    break;
                case CommandType.ChangeLineIndex:
                    CommandChangeLineIndex((ActorInfo)viewEvent.template);
                    break;
            }
            CheckTutorialState((CommandType)viewEvent.ViewCommandType.CommandType);
        }

        private void UpdateStatusCommand(ViewEvent statusViewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            switch (statusViewEvent.ViewCommandType.CommandType)
            {
                case Status.CommandType.LevelUp:
                    CommandLevelUp();
                    return;
                case Status.CommandType.ShowLearnMagic:
                    CommandShowLearnMagic();
                    return;
                case Status.CommandType.LearnMagic:
                    CommandLearnMagic((SkillInfo)statusViewEvent.template);
                    return;
                case Status.CommandType.HideLearnMagic:
                    CommandHideLearnMagic();
                    return;
                case Status.CommandType.SelectCommandList:
                    CommandSelectSkillTrigger();
                    return;
            }
            CheckTutorialState();
        }

        private void CommandCallCommandList(SystemData.CommandData commandData)
        {
            switch (commandData.Key)
            {
                case "Edit":
                    CommandEdit();
                    return;
                case "EnemyInfo":
                    CommandEnemyInfo();
                    return;
                case "Replay":
                    CommandBattleReplay();
                    return;
                case "Battle":
                    CommandBattleStart();
                    return;
            }
        }

        private void CommandEdit()
        {
            _view.SetEditMode(true);
            _view.CommandRefresh();
        }

        private void CommandEndEdit()
        {
            _view.SetEditMode(false);
            _view.CommandRefresh();
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            CommandCallSideMenu(_model.SideMenu(),() => 
            {
                _busy = false;
                _view.CommandRefresh();
            });
        }
        
        private void CommandDecideTacticsMember(ActorInfo actorInfo)
        {
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
            
            //_model.SetInBattle();
            _view.RefreshTacticsMembers(MakeListData(_model.BattlePartyMembers(),_model.SwapFromActor));
            CommandRefresh();
        }

        private void CommandSelectTacticsMember(ActorInfo actorInfo)
        {
            _model.SetCurrentActorInfo(actorInfo);
            CommandRefresh();
        }

        private void CommandSelectAttribute(AttributeType attributeType)
        {
            var lastSelectSkillId = -1;
            var lastSelectSkill = _view.SelectMagic;
            if (lastSelectSkill != null)
            {
                lastSelectSkillId = lastSelectSkill.Id.Value;
            }
            _view.RefreshLeaningList(_model.SelectActorLearningMagicList((int)attributeType,lastSelectSkillId));
            _view.CommandRefresh();
        }

        private void CommandLeftAttribute()
        {
            var current = _view.AttributeType;
            var list = _model.AttributeTabList();
            var index = list.FindIndex(a => a == current);
            var selectIndex = index - 1;
            if (selectIndex <= -1)
            {
                selectIndex = list.Count-1;
            }
            //CommandSelectAttribute(list[selectIndex]);
            _view.SelectAttribute(selectIndex);
        }

        private void CommandRightAttribute()
        {
            var current = _view.AttributeType;
            var list = _model.AttributeTabList();
            var index = list.FindIndex(a => a == current);
            var selectIndex = index + 1;
            if (selectIndex > list.Count-1)
            {
                selectIndex = 0;
            }
            //CommandSelectAttribute(list[selectIndex]);
            _view.SelectAttribute(selectIndex);
        }

        private void CommandStatusInfo()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandStatusInfo(_model.BattlePartyMembers(),false,true,true,false,_model.CurrentActor.ActorId.Value,() => 
            {
                _view.CommandRefresh();
            });
        }

        private void CommandEnemyInfo()
        {
            _busy = true;
            var enemyInfos = _model.EnemyInfos();
            CommandEnemyInfo(enemyInfos,false,() => 
            {
                _busy = false;
                _view.CommandRefresh();
            });
        }

        private void CommandBattleReplay()
        {
            if (_model.IsEnableBattleReplay())
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                _busy = true;
                var popupInfo = new PopupInfo
                {
                    PopupType = PopupType.ClearParty,
                    EndEvent = () =>
                    {
                        _busy = false;
                        _view.CommandRefresh();
                        SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    }
                };
                _view.CommandCallPopup(popupInfo);
            } else
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(30040));
                _view.CommandCallCaution(cautionInfo);
            }
        }

        private void CommandBattleStart()
        {
            var battleMembers = _model.BattleMembers();
            if (battleMembers.Count > 0)
            {
                var stageMembers = _model.StageMembers();
                // バトル人数が最大でないのでチェック
                if (battleMembers.Count < 5 && battleMembers.Count < stageMembers.Count)
                {
                    CheckBattleLessMember();
                } else
                {
                    BattleStart(); 
                }
            } else
            {
                CheckBattleMember();
            }
        }

        private void CheckBattleMember()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Deny);
            CommandCautionInfo(DataSystem.GetText(19400));
        }

        private void CheckBattleLessMember()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Deny);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19401),(a) => 
            {
                if (a == ConfirmCommandType.Yes)
                {
                    BattleStart();
                }
            });
            _view.CommandCallConfirm(confirmInfo);
        }

        private void BattleStart()
        {
            _model.SaveTempBattleMembers();
            _view.CommandChangeViewToTransition(null);
            _view.ChangeUIActive(false);
            // ボス戦なら
            if (_model.SceneParam.IsBoss)
            {
                PlayBossBgm();
            } else
            {
                var bgmData = _model.TacticsBgmData();
                if (bgmData.CrossFade != "" && SoundManager.Instance.CrossFadeMode)
                {
                    SoundManager.Instance.ChangeCrossFade();
                } else
                {
                    PlayTacticsBgm();
                }
            }
            _model.SetPartyBattlerIdList();
            SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
            var battleSceneInfo = new BattleSceneInfo
            {
                ActorInfos = _model.BattleMembers(),
                EnemyInfos = _model.EnemyInfos(),
                GetItemInfos = _model.CurrentSymbolInfo()?.GetItemInfos,
                BossBattle = _model.CurrentSymbolInfo().SymbolType == SymbolType.Boss,
            };
            _view.CommandSceneChange(Scene.Battle,battleSceneInfo);
        }

        private void ShowCharacterDetail()
        {
            _view.ShowCharacterDetail(_model.CurrentActor,_model.BattlePartyMembers(),_model.SkillActionListData(_model.CurrentActor));  
        }

        private void CommandChangeLineIndex(ActorInfo actorInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            if (actorInfo.LineIndex == LineType.Front)
            {
                actorInfo.SetLineIndex(LineType.Back);
            } else
            {
                actorInfo.SetLineIndex(LineType.Front);
            }
            _view.RefreshTacticsMembers(MakeListData(_model.BattlePartyMembers()));
            CommandRefresh();
        }

        private void CommandGuide()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Guide,
                template = "Battle",
                EndEvent = () =>
                {
                    _busy = false;
                    _view.CommandRefresh();
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
        }
        
        private void CommandLevelUp()
        {
            _busy = true;
            _view.SetBusy(true);
            CommandLevelUp(_model.CurrentActor,() => 
            {
                _busy = false;
                _view.SetBusy(false);
                CommandRefresh();
                _view.RefreshTacticsMembers(MakeListData(_model.BattlePartyMembers()));
                CheckTutorialState();
            });
        }

        private void CommandShowLearnMagic()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            CommandSelectAttribute(AttributeType.None);
            var lastSelectSkillId = -1;
            var lastSelectSkill = _view.SelectMagic;
            if (lastSelectSkill != null)
            {
                lastSelectSkillId = lastSelectSkill.Id.Value;
            }
            _view.ShowLeaningList(_model.SelectActorLearningMagicList(-1,lastSelectSkillId));
            _view.SetLearnMagicButtonActive(true);
            _view.CommandRefresh();
        }

        private void CommandLearnMagic(SkillInfo skillInfo)
        {
            CommandLearnMagic(_model.CurrentActor,skillInfo,() => 
            {
                _view.SetNuminous(_model.Currency);
                //_view.CommandRefresh();
                CommandSelectAttribute(_view.AttributeType);
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            });
        }

        private void CommandHideLearnMagic()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _view.SetLearnMagicButtonActive(false);
            CommandRefresh();
        }

        private void CommandSelectSkillTrigger()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            var skillTriggerViewInfo = new SkillTriggerViewInfo(_model.CurrentActor.ActorId.Value,() => 
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _busy = false;
                CommandRefresh();
            });
            _view.CommandCallSkillTrigger(skillTriggerViewInfo);
        }

        private void CommandRefresh()
        {
            ShowCharacterDetail();
            //_view.SetBattleMembers(MakeListData(_model.BattleMembers()));
            _view.SetNuminous(_model.Currency);
            _view.CommandRefresh();
            //CheckTutorialState();
        }
    }

}