using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ryneus
{
    using Strategy;
    using Utage;

    public class StrategyPresenter : BasePresenter
    {
        StrategyModel _model = null;
        StrategyView _view = null;

        private bool _busy = true;


        public StrategyPresenter(StrategyView view)
        {
            _view = view;
            SetView(_view);
            _model = new StrategyModel();
            SetModel(_model);

            // イベント取得
            if (_model.BattleResultVictory && CheckEventData())
            {
                return;
            }
            Initialize();
        }

        private bool CheckEventData(Action endEvent = null)
        {
            var stageEvent = GetStageEventData(EventTiming.BattleVictory);
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
                }
            }
            endEvent?.Invoke();
            return false;
        }

        private void Initialize()
        {
            _busy = true;
            _view.SetHelpWindow();

            _view.InitResultList(MakeListData(_model.ResultCommand()));
            _view.SetBackGround(_model.CurrentStage.Master.BackGround);
            if (_model.StageEnd())
            {
                SoundManager.Instance.FadeOutBgm();
            }
            _view.SetEvent((type) => UpdateCommand(type));

            _view.ChangeUIActive(true);
            CommandStartStrategy();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Strategy)
            {
                return;
            }
            Debug.Log(viewEvent.commandType);
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.StartStrategy:
                    CommandStartStrategy();
                    break;
                case CommandType.EndAnimation:
                    CommandEndAnimation();
                    break;
                case CommandType.CallEnemyInfo:
                    CommandCallEnemyInfo();
                    break;
                case CommandType.PopupSkillInfo:
                    CommandPopupSkillInfo((GetItemInfo)viewEvent.template);
                    break;
                case CommandType.ResultClose:
                    CommandResultClose((SystemData.CommandData)viewEvent.template);
                    break;
                case CommandType.EndLvUpAnimation:
                    NextSeekResult();
                    break;
                case CommandType.LvUpNext:
                    CommandLvUpNext();
                    break;
                case CommandType.SelectLearnSkillList:
                    CommandSelectLearnSkillList((SkillInfo)viewEvent.template);
                    break;
            }
            // チュートリアル確認
            //CheckTutorialState(viewEvent.commandType);
        }

        private void CheckTutorialState(CommandType commandType = CommandType.None)
        {
            Func<TutorialData,bool> enable = (tutorialData) => 
            {
                var checkFlag = true;
                if (tutorialData.Param1 == 100)
                {
                    // Lvアップ後にいリザルトを初めて開く
                    checkFlag = _model.InBattleResult && _model.BattleResultVictory && _view.StrategyResultListActive;
                }
                if (tutorialData.Param1 == 700)
                {
                    // 初めて敗北する
                    checkFlag = _model.InBattleResult && _model.BattleResultVictory == false;
                }
                if (tutorialData.Param1 == 800)
                {
                    // 2回目の敗北する
                    checkFlag = _model.InBattleResult && _model.BattleResultVictory == false && _model.CurrentStage.LoseCount == 2;
                }
                return checkFlag;
            };
            var tutorialViewInfo = new TutorialViewInfo
            {
                SceneType = (int)Scene.Strategy,
                CheckEndMethod = null,
                CheckMethod = enable,
                EndEvent = () => 
                {
                    _busy = false;
                    CheckTutorialState(commandType);
                }
            };
            _view.CommandCheckTutorialState(tutorialViewInfo);
        }

        private void CommandStartStrategy()
        {
            if (_model.InBattleResult)
            {
                _view.SetTitle(DataSystem.GetText(20010));
            }
            NextSeekResult();
            /*
            if (_model.InBattleResult)
            {
                var battledResultActors = _model.BattleResultActors();
                _view.SetTitle(DataSystem.GetText(20010));
                _view.SetResultActorList(_model.MakeListData(battledResultActors));
                // 勝利時
                if (_model.BattleResultVictory)
                {
                    if (_model.LevelUpActorInfos.Count > 0)
                    {
                        _view.StartLvUpAnimation();
                        _view.HideResultList();
                    } else
                    {
                        NextSeekResult();
                    }
                }
            } else
            { 
                CheckTacticsActors();
            }
            */
        }

        private void CheckTacticsActors()
        {
            var tacticsActors = _model.TacticsActors();
            if (tacticsActors != null && tacticsActors.Count > 0)
            {
                var bonusList = new List<bool>();
                foreach (var item in tacticsActors)
                {
                    bonusList.Add(_model.IsBonusTactics(item.ActorId.Value));
                }
                _view.SetTitle(DataSystem.GetText(20040));
                _view.SetResultActorList(_model.MakeListData(tacticsActors));
            } else
            {
                EndStrategy();
            }
        }

        private void CommandEndAnimation()
        {
            if (_model.InBattleResult)
            {
                if (_model.BattleResultVictory)
                {
                    if (_model.LevelUpActorInfos.Count == 0)
                    {
                        ShowResultList();
                    }
                } else
                {
                    ShowResultList();
                }
            } else
            {
                NextSeekResult();
            }
        }

        private void CommandEndLvUpAnimation()
        {
        }

        private void CommandLvUpNext()
        {
            var learnSkillInfo = _model.LearnSkillInfo.Count > 0 ? _model.LearnSkillInfo[0] : null;
            if (learnSkillInfo != null && learnSkillInfo.SkillInfo != null)
            {
                learnSkillInfo.SetToValue(_model.LevelUpActorInfos[0].Evaluate());
                SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);

                var popupInfo = new PopupInfo
                {
                    PopupType = PopupType.LearnSkill,
                    EndEvent = () =>
                    {
                        _model.RemoveLevelUpData();
                        NextSeekResult();
                    },
                    template = learnSkillInfo
                };
                _view.CommandCallPopup(popupInfo);
            } else
            {
                _model.RemoveLevelUpData();
                NextSeekResult();
            }
        }

        private void NextSeekResult()
        {
            // Lvアップ演出スタート
            if (_model.BeforeLevelUpAnimation)
            {
                _view.HideResultList();
                _model.SetBeforeLevelUpAnimation(false);
                _view.StartLvUpAnimation();
                return;
            }
            if (_model.LevelUpActorInfos.Count > 0)
            {
                _view.ShowLvUpActor(_model.LevelUpActorInfos[0],_model.LevelUpActorStatus());
                return;
            }
            if (_model.SelectLearnSkills.Count > 0)
            {
                _view.SetLearnSkillInfos(ListData.MakeListData(_model.SelectLearnSkills));
                return;
            }
            ShowResultList();
        }

        private void ShowResultList()
        {
            var battledResultActors = _model.BattleResultActors();
            _view.SetResultActorList(_model.MakeListData(battledResultActors));
            _view.ShowResultList(MakeListData(_model.ResultViewInfos),
                null,
                _model.BattleResultTurn(),
                _model.BattleResultScore(),
                _model.BattleResultMaxDamage(),
                _model.BattleResultRemainHpPercent(),
                _model.BattleResultDefeatedCount());
        }

        private void CommandResultClose(SystemData.CommandData commandData)
        {
            if (commandData.Key == "Yes")
            {
                if (_model.InBattleResult)
                {
                    var battledMembers = _model.BattleResultActors();
                    if (battledMembers != null && battledMembers.Count > 0)
                    {
                        _model.ClearBattleData(battledMembers);
                        _model.ClearSceneParam();
                    }
                } else
                {
                    var tacticsActors = _model.TacticsActors();
                    if (tacticsActors != null && tacticsActors.Count > 0)
                    {
                        _model.ClearBattleData(tacticsActors);
                        _model.ClearSceneParam();
                    }
                }
                CheckTacticsActors();
            } else
            {
                if (_model.InBattleResult && _model.BattleResultVictory == false)
                {
                    _model.ReturnTempBattleMembers(); 
                    _view.CommandChangeViewToTransition(null);  
                    PlayStartBattleBgm();
                    var battleSceneInfo = new BattleSceneInfo
                    {
                        ActorInfos = _model.BattleMembers(),
                        //EnemyInfos = _model.CurrentSymbolInfo().TroopInfo.BattlerInfos,
                        //GetItemInfos = _model.CurrentSymbolInfo()?.GetItemInfos,
                        BossBattle = false,//_model.CurrentSymbolInfo().SymbolType == SymbolType.Boss,
                    };
                    SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
                    _view.CommandGotoSceneChange(Scene.Battle,battleSceneInfo);
                } else
                {
                    ShowStatus();
                }
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private async void PlayStartBattleBgm()
        {
            //var currentSymbol = _model.CurrentSymbolInfo();
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
                    await PlayTacticsBgm();
                }
            }
            SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
        }

        private void CommandSelectLearnSkillList(SkillInfo skillInfo)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19200),(a) => UpdateSelectAlcana(a),ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(new List<SkillInfo>(){skillInfo});
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdateSelectAlcana(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                // アルカナ選択
                var selectLearnSkill = _view.LearnSelectSkillInfo();
                _model.MakeSelectLearnSkill(selectLearnSkill.Id.Value);
                _view.HideLearnSkillList();
                NextSeekResult();
            }
        }
    
        private void ShowStatus()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandStatusInfo(_model.StageMembers(),false,true,false,false,-1,() => 
            {
                SetHelpInputSkipEnable();
                _view.SetHelpText(DataSystem.GetText(20020));
                _view.SetHelpInputInfo("STRATEGY");
            });
        }

        private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
        {
            var confirmInfo = new ConfirmInfo("",(a) => {});
            confirmInfo.SetSkillInfo(_model.BasicSkillInfos(getItemInfo));
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallSkillDetail(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandCallEnemyInfo()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);  
            /*
            var enemyIndex = _model.CurrentStage.CurrentSeekIndex;
            var enemyInfos = _model.TacticsSymbols()[enemyIndex].SymbolInfo.BattlerInfos();
            _busy = true;
            CommandEnemyInfo(enemyInfos,false,() => 
            { 
                SetHelpInputSkipEnable();
                _view.SetHelpText(DataSystem.GetText(20020));
                _busy = false;
            });  
            */
        }

        private void SetHelpInputSkipEnable()
        {
        }

        private void EndStrategy()
        {
            _view.EndShinyEffect();
            if (_model.InBattleResult && _model.BattleResultVictory == false)
            {
                // 敗北して戻る
                _model.ReturnTempBattleMembers();
                var tacticsSceneInfo = new TacticsSceneInfo
                {
                    ReturnBeforeBattle = true,
                    SeekIndex = _model.CurrentStage.SeekIndex.Value
                };
                _model.EndStrategy();
                _view.CommandGotoSceneChange(Scene.Tactics,tacticsSceneInfo);
            } else
            {
                _model.EndStrategy();
                _model.SeekStage();
                var tacticsSceneInfo = new TacticsSceneInfo
                {
                    ReturnNextBattle = true,
                    SeekIndex = _model.CurrentStage.SeekIndex.Value
                };
                _view.CommandGotoSceneChange(Scene.Tactics,tacticsSceneInfo);
            }
        }
    }
}