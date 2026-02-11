using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

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

        public void CallPopupView(PopupType popupType, Action endEvent = null, object templateData = null)
        {
            var popupInfo = new PopupInfo
            {
                PopupType = popupType,
                template = templateData,
                EndEvent = endEvent,
            };
            _view.CommandCallPopup(popupInfo);
        }

        public void CallLearnSkillPopupView(object templateData, Action endEvent = null)
        {
            SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.LearnSkill,
                template = templateData,
                EndEvent = endEvent,
            };
            _view.CommandCallPopup(popupInfo);
        }

        public void CallConfirmView(string title, Action<ConfirmCommandType> returnEvent)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(title, returnEvent);
            _view.CommandCallConfirm(confirmInfo);
        }

        public void CallConfirmNoChoiceView(string title, Action<ConfirmCommandType> returnEvent)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(title, returnEvent);
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        public void CallConfirmSkillDetailView(string title, List<SkillInfo> skillInfos, Action<ConfirmCommandType> returnEvent)
        {
            var confirmInfo = new ConfirmInfo(title, returnEvent, ConfirmType.SkillDetail);
            confirmInfo.SetIsNoChoice(true);
            if (skillInfos != null)
            {
                confirmInfo.SetSkillInfo(skillInfos);
            }
            _view.CommandCallConfirm(confirmInfo);
        }

        public void CallConfirmItemDetailView(string title, List<ItemInfo> itemInfos, Action<ConfirmCommandType> returnEvent)
        {
            var confirmInfo = new ConfirmInfo(title, returnEvent, ConfirmType.ItemDetail);
            confirmInfo.SetIsNoChoice(true);
            if (itemInfos != null)
            {
                confirmInfo.SetItemInfo(itemInfos);
            }
            _view.CommandCallConfirm(confirmInfo);
        }

        public void CallConfirmStageDetailView(string title, StageInfo stageInfos, Action<ConfirmCommandType> returnEvent)
        {
            var confirmInfo = new ConfirmInfo(title, returnEvent, ConfirmType.StageConfirm);
            if (stageInfos != null)
            {
                confirmInfo.SetStageInfo(stageInfos);
            }
            _view.CommandCallConfirm(confirmInfo);
        }

        public void CallCommandOther(ViewCommandSceneType sceneType, object commandType, object sendData = null)
        {
            var otherViewEvent = new OtherViewEvent
            {
                ViewCommandSceneType = sceneType,
                CommandType = commandType,
                Templete = sendData
            };
            _view.CallSystemCommand(Base.CommandType.CommandOther, otherViewEvent);
        }

        public List<ListData> MakeListData<T>(List<T> dataList)
        {
            return ListData.MakeListData(dataList);
        }

        public List<ListData> MakeListData<T>(List<T> dataList, int selectIndex)
        {
            return ListData.MakeListData(dataList, selectIndex);
        }

        public List<ListData> MakeListDataFunc<T>(List<T> dataList, int selectIndex, Func<T, bool> enableFunc)
        {
            return ListData.MakeListData(dataList, selectIndex, enableFunc);
        }

        public List<ListData> MakeListData<T>(List<T> dataList, T selected)
        {
            return ListData.MakeListData(dataList, selected);
        }

        public List<ListData> MakeListData<T>(List<T> dataList, List<T> selected)
        {
            return ListData.MakeListData(dataList, selected);
        }

        public List<ListData> MakeListData<T>(List<T> dataList, Func<T, bool> enable, Func<T, bool> selectFunc)
        {
            return ListData.MakeListData(dataList, enable, selectFunc);
        }

        public List<ListData> MakeListData<T>(List<T> dataList, Func<T, bool> enable, Func<T, bool> selectFunc, Func<T, bool> batchFunc, int selectIndex)
        {
            return ListData.MakeListData(dataList, enable, selectFunc, batchFunc, selectIndex);
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

        private void CallAdvScene(int advId, Action<bool> callEvent = null)
        {
            var advInfo = CheckAdvStageEvent(advId);
            if (advInfo != null)
            {
                BeforeStageAdv();
                _view.WaitFrame(10, () =>
                {
                    advInfo.SetCallEvent(() =>
                    {
                        callEvent?.Invoke(true);
                    });
                    _view.CommandCallAdv(advInfo);
                });
            }
        }

        public AdvCallInfo CheckAdvStageEvent(int advId)
        {
            var advInfo = new AdvCallInfo();
            advInfo.Label.SetValue(_model.GetAdvFile(advId));
            return advInfo;
        }

        public void CallAdvEvent(AdvData advData, float timeStamp = 0, Action endEvent = null)
        {
            _model.AddEventReadFlag(advData.EventKey);
            CallAdvScene(advData.Id, (a) => endEvent?.Invoke());
        }

        public void CallAdvEvent(int advId, float timeStamp = 0, Action endEvent = null)
        {
            CallAdvScene(advId, (a) => endEvent?.Invoke());
        }

        public bool CheckAdvEvent(EventTiming eventTiming, float timeStamp = 0, Action endEvent = null)
        {
            if (CheckEvent(eventTiming, (a) => CheckAdvEvent(eventTiming, timeStamp, endEvent)))
            {
                return true;
            }
            else
            {
                endEvent?.Invoke();
            }
            return false;
        }

        private bool CheckEvent(EventTiming eventTiming, Action<bool> callEvent = null)
        {
            var advInfo = CheckAdvStageEvent(eventTiming);
            if (advInfo != null)
            {
                BeforeStageAdv();
                _view.WaitFrame(10, () =>
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

        private AdvCallInfo CheckAdvStageEvent(EventTiming eventTiming, int selectActorId = 0)
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

        public async UniTask PlayDungeonBgm(float timeStamp = 0)
        {
            var bgmData = _model.DungeonBgmData();
            if (bgmData != null)
            {
                var bgm = await _model.GetBgmData(bgmData.Key);
                SoundManager.Instance.PlayBgm(bgm, bgmData.Volume, true, timeStamp);
            }
            else
            {
                SoundManager.Instance.FadeOutBgm();
            }
        }

        public async void PlayBattleBgm()
        {
            if (_model.CurrentStage == null)
            {
                var bgm = await _model.GetBgmData("Battle1");
                SoundManager.Instance.PlayBgm(bgm, 1);
                return;
            }
            var bgmData = DataSystem.BGM.Find(a => a.Id == _model.CurrentStage.Master.BattleBGMId);
            if (bgmData != null)
            {
                var bgm = await _model.GetBgmData(bgmData.Key);
                SoundManager.Instance.PlayBgm(bgm, bgmData.Volume);
            }
        }

        public async void PlayBossBgm()
        {
            var bgmData = DataSystem.BGM.Find(a => a.Id == _model.CurrentStage.Master.BossBGMId);
            var bgm = await _model.GetBgmData(bgmData.Key);
            SoundManager.Instance.PlayBgm(bgm, bgmData.Volume);
        }

        public void CommandSave(bool isReturnScene)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var sceneParam = new FileListSceneInfo
            {
                IsLoad = false
            };
            CallPopupView(PopupType.FileList, () =>
            {

            }, sceneParam);
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
            _view.CommandCloseLoading();
            _model.GainSaveCount();
            _model.SavePlayerStageData(GameSystem.SceneStackManager.Current);
            // 成功表示
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19500), (a) =>
            {
                if (isReturnScene)
                {
                    _view.CommandGotoSceneChange(Scene.Tactics);
                }
                else
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
        public void CommandActorStatusInfo(List<ActorInfo> actorInfos, bool inBattle, int startIndex = -1, Action closeEvent = null)
        {
            CommandStatusInfo(actorInfos, inBattle, true, false, false, startIndex, closeEvent);
        }

        /// <summary>
        /// 仲間加入ステータス詳細を表示
        /// </summary>
        /// <param name="actorInfos"></param>
        public void CommandAddActorStatusInfo(List<ActorInfo> actorInfos, Action closeEvent = null)
        {
            CommandStatusInfo(actorInfos, false, false, false, true, 0, closeEvent);
        }

        /// <summary>
        /// ステータス詳細を表示
        /// </summary>
        /// <param name="actorInfos"></param>
        public void CommandStatusInfo(List<ActorInfo> actorInfos, bool inBattle, bool backButton = true, bool levelUpObj = true, bool addActor = false, int startIndex = -1, Action closeEvent = null, bool isRanking = false, bool characterList = false)
        {
            var statusViewInfo = new StatusViewInfo(() =>
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _view.CallSystemCommand(Base.CommandType.CloseStatus);
                _view.ChangeUIActive(true);
                closeEvent?.Invoke();
            });
            statusViewInfo.SetActorInfos(actorInfos, inBattle);
            if (startIndex > -1)
            {
                statusViewInfo.StartIndex.SetValue(startIndex);
            }
            statusViewInfo.AddActor.SetValue(addActor);
            statusViewInfo.DisplayCharacterList.SetValue(characterList);
            statusViewInfo.DisplayLvUpInfo.SetValue(levelUpObj);
            statusViewInfo.DisplayBackButton.SetValue(backButton);
            statusViewInfo.IsRanking.SetValue(isRanking);
            _view.CallSystemCommand(Base.CommandType.CallStatusView, statusViewInfo);
            _view.ChangeUIActive(false);
        }

        /// <summary>
        /// 敵詳細を表示
        /// </summary>
        /// <param name="battlerInfos"></param>
        public void CommandEnemyInfo(List<BattlerInfo> battlerInfos, bool inBattle, Action closeEvent = null)
        {
            var enemyViewInfo = new StatusViewInfo(() =>
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _view.CallSystemCommand(Base.CommandType.CloseStatus);
                _view.ChangeUIActive(true);
                closeEvent?.Invoke();
            });
            enemyViewInfo.SetEnemyInfos(battlerInfos, inBattle);
            _view.CallSystemCommand(Base.CommandType.CallEnemyInfoView, enemyViewInfo);
            _view.ChangeUIActive(false);
        }

        public void CommandCallSideMenu(List<ListData> sideMenuCommands, Action closeEvent = null)
        {
            //sideMenuCommands[0].SetSelected(true);
            var sideMenuViewInfo = new SideMenuViewInfo
            {
                CommandLists = sideMenuCommands
            };
            CallPopupView(PopupType.SideMenu, () =>
            {
                closeEvent?.Invoke();
            }, sideMenuViewInfo);
        }

        public void CommandCautionInfo(string title, int from = -1, int to = -1)
        {
            var cautionInfo = new CautionInfo();
            cautionInfo.Title.SetValue(title);
            if (from != -1 && to != -1)
            {
                cautionInfo.SetLevelUp(from, to);
            }
            _view.CommandCallCaution(cautionInfo);
        }

        public void CommandExpUp(ActorInfo actorInfo, int getExp, Action endEvent = null)
        {
            if (getExp > 0)
            {
                //_model.PartyInfo.Currency.GainValue(-1);
                SoundManager.Instance.PlayStaticSe(SEType.LevelUp);
                var levelUpViewInfo = _model.MakeLevelUpViewInfo(actorInfo, getExp);
                if (levelUpViewInfo.StrategyStrengthInfos.Count > 0 || levelUpViewInfo.SkillInfo != null)
                {
                    CallPopupView(PopupType.LevelUp, () =>
                    {
                        endEvent?.Invoke();
                        SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    }, levelUpViewInfo);
                    return;
                }
                CommandCautionInfo("", levelUpViewInfo.From.Value, levelUpViewInfo.To.Value);
                endEvent?.Invoke();
                SoundManager.Instance.PlayStaticSe(SEType.CountUp);
            }
            endEvent?.Invoke();
        }


        public void CommandAttributeUp(ActorInfo actorInfo, AttributeType attributeType, Action endEvent = null)
        {
            if (attributeType == AttributeType.None)
            {
                endEvent?.Invoke();
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.LevelUp);
            actorInfo.AddAttributeUpper(attributeType);
            endEvent?.Invoke();
        }

        public void CommandStatusUp(ActorInfo actorInfo, StatusParamType statusType, int upper, Action endEvent = null)
        {
            SoundManager.Instance.PlayStaticSe(SEType.LevelUp);
            actorInfo.AddStatusUpper(statusType, upper);
            endEvent?.Invoke();
        }

        public void CommandLearnMagic(LearnSkillInfo learnSkillInfo, Action endEvent = null)
        {
            SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);
            CallLearnSkillPopupView(learnSkillInfo, () =>
            {
                endEvent?.Invoke();
            });
        }

        public bool CheckAchievements(bool checkMissionRank = false, Action endEvent = null)
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
                    if (achievement != achievements[^1])
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
                // 達成済みを破棄する
                _model.PartyInfo.RemoveAchievedAchirvements();
                var rankupInfo = new RankupInfo(currentRank, afterRank);
                SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);

                CallPopupView(PopupType.Rankup, () =>
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    endEvent?.Invoke();
                }, rankupInfo);
            }
            return checkMissionRank && afterRank > currentRank;
        }
    }
}