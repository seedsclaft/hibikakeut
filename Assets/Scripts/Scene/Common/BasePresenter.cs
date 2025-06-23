using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Ryneus
{
    public class BasePresenter
    {
        private BaseView _view = null;
        public void SetView(BaseView view)
        {
            _view = view;
        }

        private BaseModel _model = null;
        public void SetModel(BaseModel model)
        {
            _model = model;
        }

        public List<ListData> MakeListData<T>(List<T> dataList)
        {
            return ListData.MakeListData(dataList);
        }

        public List<ListData> MakeListData<T>(List<T> dataList,int selectIndex)
        {
            return ListData.MakeListData(dataList,selectIndex);
        }

        public List<ListData> MakeListDataFunc<T>(List<T> dataList,int selectIndex,Func<T,bool> enableFunc)
        {
            return ListData.MakeListData(dataList,selectIndex,enableFunc);
        }

        public List<ListData> MakeListData<T>(List<T> dataList,T selected)
        {
            return ListData.MakeListData(dataList,selected);
        }

        public List<ListData> MakeListData<T>(List<T> dataList,List<T> selected)
        {
            return ListData.MakeListData(dataList,selected);
        }

        public List<ListData> MakeListData<T>(List<T> dataList,Func<T,bool> enable,Func<T,bool> selectFunc)
        {
            return ListData.MakeListData(dataList,enable,selectFunc);
        }

        public StageEventData GetStageEventData(EventTiming eventTiming)
        {
            var timingEvents = _model.StageEvents(eventTiming);
            if (timingEvents.Count > 0)
            {
                return timingEvents.First();
            }
            return null;
        }

        public StageEventData GetStageEventData(EventTiming eventTiming,int positionX,int positionY)
        {
            var timingEvents = _model.StageEvents(eventTiming,positionX,positionY);
            if (timingEvents.Count > 0)
            {
                return timingEvents.First();
            }
            return null;
        }

        public bool CheckAdvEvent(EventTiming eventTiming,float timeStamp = 0,Action endEvent = null)
        {
            if (CheckEvent(eventTiming,(a) => CheckAdvEvent(eventTiming,timeStamp,endEvent)))
            {
                return true;
            } else
            {
                endEvent?.Invoke();
            }
            return false;
        }

        private bool CheckEvent(EventTiming eventTiming,Action<bool> callEvent = null)
        {
            var advInfo = CheckAdvStageEvent(eventTiming);
            if (advInfo != null)
            {
                BeforeStageAdv();
                _view.WaitFrame(10,() =>
                {
                    advInfo.SetCallEvent(() =>
                    {
                        callEvent?.Invoke(true);
                    });
                    _view.CommandCallAdv(advInfo);
                });
                return true;
            }
            return false;
        }

        private AdvCallInfo CheckAdvStageEvent(EventTiming eventTiming,int selectActorId = 0)
        {
            var stageEvents = _model.StageEvents(eventTiming);
            var find = stageEvents.Find(a => a.Type == StageEventType.AdvStart);
            if (find != null)
            {
                var advId = find.Param;
                _model.AddEventReadFlag(find);
                var advInfo = new AdvCallInfo();
                advInfo.Label.SetValue(_model.GetAdvFile(advId));
                return advInfo;
            }
            return null;
        }

        public int CheckForceBattleEvent(EventTiming eventTiming)
        {
            var seekIndex = -1;
            var stageEvents = _model.StageEvents(eventTiming);
            var forceBattle = stageEvents.Find(a => a.Type == StageEventType.ForceBattle);
            if (forceBattle != null)
            {
                seekIndex = forceBattle.Param;
            }
            return seekIndex;
        }

        public void BeforeStageAdv()
        {
            _view.CallSystemCommand(Base.CommandType.SceneHideUI);
            // BGMとBGSのフェードアウト
            //SoundManager.Instance.FadeOutBgm();
            //SoundManager.Instance.FadeOutBgs();
        }

        public async UniTask PlayTacticsBgm(float timeStamp = 0)
        {
            var bgmData = _model.TacticsBgmData();
            if (bgmData != null)
            {
                var bgm = await _model.GetBgmData(bgmData.Key);
                SoundManager.Instance.PlayBgm(bgm,bgmData.Volume,bgmData.Loop,timeStamp);
            } else
            {
                SoundManager.Instance.FadeOutBgm();
            }
        }

        public async void PlayBattleBgm()
        {
            var bgmData = DataSystem.BGM.Find(a => a.Key == "Battle1");
            if (bgmData != null)
            {
                var bgm = await _model.GetBgmData("Battle1");
                SoundManager.Instance.PlayBgm(bgm,bgmData.Volume,bgmData.Loop);
            }
        }

        public async void PlayBossBgm()
        {
            var bgmData = DataSystem.BGM.Find(a => a.Id == _model.CurrentStage.Master.BossBGMId);
            var bgm = await _model.GetBgmData(bgmData.Key);
            SoundManager.Instance.PlayBgm(bgm,bgmData.Volume);
        }

        public void CommandSave(bool isReturnScene)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var sceneParam = new FileListSceneInfo
            {
                IsLoad = false
            };
            var popupInfo = new PopupInfo()
            {
                PopupType = PopupType.FileList,
                EndEvent = () =>
                {
                    //SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                },
                template = sceneParam
            };
            _view.CommandCallPopup(popupInfo);
            /*
#if UNITY_ANDROID
            var savePopupTitle = _model.SavePopupTitle();
            var saveNeedAds = _model.NeedAdsSave();
            var popupInfo = new ConfirmInfo(savePopupTitle,(a) => UpdatePopupSaveCommand((ConfirmCommandType)a,isReturnScene));
            
            popupInfo.SetSelectIndex(1);
            if (saveNeedAds)
            {
                //popupInfo.SetDisableIds(new List<int>(){1});
                popupInfo.SetCommandTextIds(_model.SaveAdsCommandTextIds());
            } else
            {
            }
            _view.CommandCallConfirm(popupInfo);
            _view.ChangeUIActive(false);
#else
            SuccessSave(isReturnScene);
#endif
*/
        }

        private void SuccessSave(bool isReturnScene)
        {
            // ロード非表示
            _view.CallSystemCommand(Base.CommandType.CloseLoading);
            _model.GainSaveCount();
            _model.SavePlayerStageData(true,GameSystem.SceneStackManager.Current);
            // 成功表示
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19500),(a) =>
            {
                if (isReturnScene)
                {
                    _view.CommandGotoSceneChange(Scene.Tactics);
                } else
                {
                    _view.ChangeUIActive(true);
                }
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        /// <summary>
        /// ステータス詳細を表示
        /// </summary>
        /// <param name="actorInfos"></param>
        public void CommandStatusInfo(List<ActorInfo> actorInfos,bool inBattle,bool backButton = true,bool levelUpObj = true,bool addActor = false,int startIndex = -1,Action closeEvent = null,bool isRanking = false,bool characterList = false)
        {
            var statusViewInfo = new StatusViewInfo(() =>
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _view.CallSystemCommand(Base.CommandType.CloseStatus);
                _view.ChangeUIActive(true);
                closeEvent?.Invoke();
            });
            statusViewInfo.SetActorInfos(actorInfos,inBattle);
            if (startIndex > -1)
            {
                statusViewInfo.StartIndex.SetValue(startIndex);
            }
            statusViewInfo.DisplayDecideButton.SetValue(addActor);
            statusViewInfo.DisplayCharacterList.SetValue(characterList);
            statusViewInfo.DisplayLvUpInfo.SetValue(levelUpObj);
            statusViewInfo.DisplayBackButton.SetValue(backButton);
            statusViewInfo.IsRanking.SetValue(isRanking);
            _view.CallSystemCommand(Base.CommandType.CallStatusView,statusViewInfo);
            _view.ChangeUIActive(false);
        }

        /// <summary>
        /// 仲間加入ステータス詳細を表示
        /// </summary>
        /// <param name="actorInfos"></param>
        public void CommandAddActorStatusInfo(List<ActorInfo> actorInfos,Action closeEvent = null)
        {
            var statusViewInfo = new StatusViewInfo(() =>
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _view.CallSystemCommand(Base.CommandType.CloseStatus);
                _view.ChangeUIActive(true);
                closeEvent?.Invoke();
            });
            statusViewInfo.SetActorInfos(actorInfos,false);
            statusViewInfo.DisplayDecideButton.SetValue(true);
            statusViewInfo.DisplayCharacterList.SetValue(true);
            statusViewInfo.DisplayLvUpInfo.SetValue(false);
            statusViewInfo.DisplayBackButton.SetValue(false);
            statusViewInfo.IsRanking.SetValue(false);
            _view.CallSystemCommand(Base.CommandType.CallStatusView,statusViewInfo);
            _view.ChangeUIActive(false);
        }

        /// <summary>
        /// 敵詳細を表示
        /// </summary>
        /// <param name="battlerInfos"></param>
        public void CommandEnemyInfo(List<BattlerInfo> battlerInfos,bool inBattle,System.Action closeEvent = null)
        {
            var enemyViewInfo = new StatusViewInfo(() => 
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _view.CallSystemCommand(Base.CommandType.CloseStatus);
                _view.ChangeUIActive(true);
                closeEvent?.Invoke();
            });
            enemyViewInfo.SetEnemyInfos(battlerInfos,inBattle);
            _view.CallSystemCommand(Base.CommandType.CallEnemyInfoView,enemyViewInfo);
            _view.ChangeUIActive(false);
        }

        public void CommandCallSideMenu(List<ListData> sideMenuCommands,System.Action closeEvent = null)
        {
            var sideMenuViewInfo = new SideMenuViewInfo
            {
                EndEvent = () =>
                {
                    closeEvent?.Invoke();
                },
                CommandLists = sideMenuCommands
            };
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.SideMenu,
                template = sideMenuViewInfo,
                EndEvent = () =>
                {
                    closeEvent?.Invoke();
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        public void CloseConfirm()
        {
            _view.CallSystemCommand(Base.CommandType.CloseConfirm);
        }

        public void CommandCautionInfo(string title,int from = -1,int to = -1)
        {
            var cautionInfo = new CautionInfo();
            cautionInfo.SetTitle(title);
            if (from != -1 && to != -1)
            {
                cautionInfo.SetLevelUp(from,to);
            }
            _view.CommandCallCaution(cautionInfo);
        }

        public void CommandActorLevelUp(ActorInfo actorInfo,Action endEvent = null)
        {
            if (_model.EnableActorLevelUp(actorInfo))
            {
                SoundManager.Instance.PlayStaticSe(SEType.LevelUp);
                // 新規魔法取得があるか
                var skills = actorInfo.LearningSkills(1);
                var from = actorInfo.Evaluate();
                _model.ActorLevelUp(actorInfo);
                var to = actorInfo.Evaluate();
                if (skills.Count > 0)
                {
                    //_busy = true;
                    _view.SetBusy(true);
                    var learnSkillInfo = new LearnSkillInfo(from,to,skills[0]);
                    SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);

                    var popupInfo = new PopupInfo
                    {
                        PopupType = PopupType.LearnSkill,
                        EndEvent = () =>
                        {
                            endEvent?.Invoke();
                            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                        },
                        template = learnSkillInfo
                    };
                    _view.CommandCallPopup(popupInfo);
                } else
                {
                    CommandCautionInfo("",from,to);
                    endEvent?.Invoke();
                    SoundManager.Instance.PlayStaticSe(SEType.CountUp);
                }
            } else
            {
                var textId = _model.ActorLevelLinked(actorInfo) ? 19420 : 19410;
                CommandCautionInfo(DataSystem.GetText(textId));
                endEvent?.Invoke();
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
            }
        }

        public void CommandLevelUp(ActorInfo actorInfo,Action endEvent = null)
        {
            CommandActorLevelUp(actorInfo,() =>
            {
                endEvent?.Invoke();
            });
        }

        public void CommandLearnMagic(ActorInfo actorInfo,SkillInfo skillInfo,Action endEvent = null)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(19510,skillInfo.LearningCost.Value.ToString()) + DataSystem.GetReplaceText(19520,skillInfo.Master.Name),(a) => UpdatePopupLearnSkill(a,actorInfo,skillInfo,endEvent));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupLearnSkill(ConfirmCommandType confirmCommandType, ActorInfo actorInfo,SkillInfo skillInfo,System.Action endEvent = null)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var from = actorInfo.Evaluate();
                _model.ActorLearnMagic(actorInfo,skillInfo.Id.Value);
                var to = actorInfo.Evaluate();

                var learnSkillInfo = new LearnSkillInfo(from,to,skillInfo);
                SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);

                var popupInfo = new PopupInfo
                {
                    PopupType = PopupType.LearnSkill,
                    EndEvent = () =>
                    {
                        endEvent?.Invoke();
                        SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    },
                    template = learnSkillInfo
                };
                _view.CommandCallPopup(popupInfo);
            }
        }

        public bool CheckAchievements(bool checkMissionRank = false,Action endEvent = null)
        {
            // 現在のRank
            var currentRank = _model.PartyInfo.MissionRank.Value;
            var achievements = _model.CheckAchievements(checkMissionRank);
            if (achievements.Count > 0)
            {
                var missionClearInfo = new MissionClearInfo();
                var text = DataSystem.GetText(36010) + "\n";
                foreach (var achievement in achievements)
                {
                    text += achievement.GetTitleData();
                    if (achievement != achievements[achievements.Count-1])
                    {
                        text += " ";
                    }
                }
                text += DataSystem.GetText(36011);
                missionClearInfo.SetTitle(text);
                _view.CommandCallMissionClear(missionClearInfo);
            }
            var afterRank = _model.PartyInfo.MissionRank.Value;
            if (checkMissionRank && afterRank > currentRank)
            {
                _model.PartyInfo.SetAchievementRank(DataSystem.Achievements);
                var rankupInfo = new RankupInfo(currentRank,afterRank);
                SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);

                var popupInfo = new PopupInfo
                {
                    PopupType = PopupType.Rankup,
                    EndEvent = () =>
                    {
                        SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                        endEvent?.Invoke();
                    },
                    template = rankupInfo
                };
                _view.CommandCallPopup(popupInfo);
            }
            return checkMissionRank && afterRank > currentRank;
        }
    }
}