using System.Collections.Generic;
using System;
using Ryneus.Strategy;

namespace Ryneus
{
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
                        if (CheckAdvEvent(EventTiming.BattleVictory, timeStamp, () => CheckEventData(() => Initialize())))
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
            if (_model.BackGround() != null)
            {
                _view.SetBackGround(_model.BackGround());
            }
            if (_model.StageEnd())
            {
                SoundManager.Instance.FadeOutBgm();
            }
            _view.SetEvent((type) => UpdateCommand(type));

            _view.ChangeUIActive(true);
            CommandStartStrategy();
            CheckAchievements();
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
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.EndAnimation:
                    CommandEndAnimation();
                    break;
                case CommandType.CallEnemyInfo:
                    CommandCallEnemyInfo();
                    break;
                case CommandType.PopupSkillInfo:
                    CommandPopupSkillInfo((GetItemInfo)viewEvent.Template);
                    break;
                case CommandType.ResultClose:
                    CommandResultClose((SystemData.CommandData)viewEvent.Template);
                    break;
                case CommandType.EndLvUpAnimation:
                    NextSeekResult();
                    break;
                case CommandType.LvUpNext:
                    CommandLvUpNext();
                    break;
                case CommandType.SelectLearnSkillList:
                    CommandSelectLearnSkillList((SkillInfo)viewEvent.Template);
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
                    //checkFlag = _model.InBattleResult && _model.BattleResultVictory == false && _model.CurrentStage.LoseCount == 2;
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
            _view.SetTitle(_model.TitleText());
            // キャラ表示アニメーションを開始
            var displayActorInfos = _model.DisplayActorInfos;
            if (displayActorInfos.Count > 0)
            {
                _view.StartResultAnimation(MakeListData(displayActorInfos));
            } else
            {
                NextSeekResult();
            }
        }

        private void CheckTacticsActors()
        {
            var tacticsActors = _model.DisplayActorInfos;
            if (tacticsActors != null && tacticsActors.Count > 0)
            {
                var bonusList = new List<bool>();
                foreach (var item in tacticsActors)
                {
                    bonusList.Add(_model.IsBonusTactics(item.ActorId.Value));
                }
                _view.SetTitle(DataSystem.GetText(20040));
                //_view.SetResultActorList(_model.MakeListData(tacticsActors));
            } else
            {
                EndStrategy();
            }
        }

        private void CommandEndAnimation()
        {
            NextSeekResult();
        }

        private void CommandLvUpNext()
        {
            var learnSkillInfo = _model.LearnSkillInfo.Count > 0 ? _model.LearnSkillInfo[0] : null;
            if (learnSkillInfo != null && learnSkillInfo.SkillInfo != null)
            {
                learnSkillInfo.SetToValue(_model.LevelUpActorInfos[0].Evaluate());
                SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);

                CallLearnSkillPopupView(() =>
                {
                    _model.RemoveLevelUpData();
                    NextSeekResult();
                }, learnSkillInfo);
            } else
            {
                _model.RemoveLevelUpData();
                NextSeekResult();
            }
        }

        private void NextSeekResult()
        {
            if (_model.DisplayLevelUpInfos.Count > 0)
            {
                _view.StartGetExpAnimation(_model.DisplayLevelUpInfos);
                _model.ClearExpDict();
                return;
            }
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
                _view.SetLearnSkillInfos(MakeListData(_model.SelectLearnSkills));
                return;
            }
            ShowResultList();
        }

        private void ShowResultList()
        {
            _view.ShowResultList(MakeListData(_model.ResultViewInfos), _model.SceneParam.BattleScore);
        }

        private void CommandResultClose(SystemData.CommandData commandData)
        {
            var battledMembers = _model.DisplayActorInfos;
            if (battledMembers != null && battledMembers.Count > 0)
            {
                //_model.ClearSceneParam();
            }
            if (_model.ReleifScene())
            {
                CallConfirmNoChoiceView(DataSystem.GetText(11012), (a) =>
                {
                    // 仲間選択確認
                    List<ActorInfo> actorInfos = _model.AddSelectActorInfos();
                    CommandAddActorStatusInfo(actorInfos, () =>
                    {
                    });
                });
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            EndStrategy();
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
            _model.EndStrategy();
            // 敗北して戻る
            if (_model.InBattleResult && !_model.BattleResultVictory)
            {
                _view.CommandGotoSceneChange(Scene.Title);
                /*
                _model.ReturnDungeon();
                _view.CallSystemCommand(Base.CommandType.MapClear);
                _view.CommandGotoSceneChange(Scene.MainMenu);
                */
                return;
            }

            if (_model.ReturnScene != Scene.None)
            {
                if (_model.SceneParam.ReturnMainMenuSceneParam != null)
                {
                    _view.CommandGotoSceneChange(_model.ReturnScene, _model.SceneParam.ReturnMainMenuSceneParam);
                } else
                {
                    _view.CommandGotoSceneChange(_model.ReturnScene);
                }
            } else
            if (_model.InBattleResult && _model.BattleResultVictory)
            {
                var dungeonSceneInfo = new DungeonSceneInfo();
                dungeonSceneInfo.BattleEnd = true;
                _view.CommandSceneChange(Scene.Dungeon, dungeonSceneInfo);
            } else
            {
                if (_model.SceneParam.ReturnMainMenuSceneParam != null)
                {
                    _view.CommandGotoSceneChange(Scene.MainMenu, _model.SceneParam.ReturnMainMenuSceneParam);
                } else
                {
                    _view.CommandGotoSceneChange(Scene.MainMenu);
                }
            }
        }
    }
}