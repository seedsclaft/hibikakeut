﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utage;

namespace Ryneus
{
    public class GameSystem : SingletonMonoBehaviour<GameSystem>
    {
        [SerializeField] private bool testMode = false;
        [SerializeField] private SceneAssign sceneAssign = null;
        [SerializeField] private MapAssign mapAssign = null;
        [SerializeField] private PopupAssign popupAssign = null;
        [SerializeField] private StatusAssign statusAssign = null;
        [SerializeField] private ConfirmAssign confirmAssign = null;

        [SerializeField] private Canvas uiCanvas = null;
        public static Canvas UiCanvas;
        [SerializeField] private GameObject transitionRoot = null;
        [SerializeField] private Fade transitionFade = null;
        [SerializeField] private LoadingView loadingView = null;
        [SerializeField] private TutorialView tutorialView = null;
        [SerializeField] private AdvEngine advEngine = null;
        [SerializeField] private AdvController advController = null;

        [SerializeField] private DebugBattleData debugBattleData = null;
        [SerializeField] private HelpWindow helpWindow = null;
        [SerializeField] private HelpWindow advHelpWindow = null;
        
        private BaseView _currentScene = null;

        private BaseModel _model = null;
        
        public static SaveInfo CurrentData = null;
        public static SaveGameInfo GameInfo = null;
        public static SaveOptionInfo OptionData = null;
        public static TempInfo TempData = null;
        private static TutorialData _lastTutorialData = null;

        private bool _busy = false;
        public bool Busy => _busy;

        public static string Version;
        public static DebugBattleData DebugBattleData;

        private static SceneStackManager _sceneStackManager = new ();
        public static SceneStackManager SceneStackManager => _sceneStackManager;


        private void Awake() 
        {
    #if UNITY_WEBGL || UNITY_ANDROID || UNITY_STANDALONE_WIN// && !UNITY_EDITOR
            //FirebaseController.Instance.Initialize();
    #endif
            Application.targetFrameRate = 60;
            advController.Initialize();
            advController.SetHelpWindow(advHelpWindow);
            transitionRoot.SetActive(false);
            loadingView.Initialize();
            loadingView.gameObject.SetActive(false);
            transitionFade.Init();
            tutorialView.Initialize();
            statusAssign.CloseStatus();
            InputSystem.Initialize();
            UiCanvas = uiCanvas;
            TempData = new TempInfo();
            _model = new BaseModel();
            Version = Application.version;
    #if UNITY_EDITOR
            DebugBattleData = debugBattleData;
    #endif 
    #if UNITY_ANDROID
            AdMobController.Instance.Initialize(() => {CommandSceneChange(Scene.Boot);});
    #else
            CommandSceneChange(new SceneInfo(){ToScene = Scene.Boot});
    #endif
        }

        private BaseView CreateStatus(StatusType statusType,StatusViewInfo statusViewInfo)
        {
            _sceneStackManager.PushStatusViewInfo(statusViewInfo);
            var prefab = statusAssign.CreatePopup(statusType,helpWindow);
            var baseView = prefab.GetComponent<BaseView>();
            baseView.SetEvent((type) => UpdateCommand(type));
            baseView.Initialize();
            return baseView;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy)
            {
                return;
            }
            if (viewEvent == null || viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.System)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case Base.CommandType.SceneChange:
                    var sceneInfo = (SceneInfo)viewEvent.template; 
                    if (testMode && sceneInfo.ToScene == Scene.Battle)
                    {
                        if (debugBattleData.AdvName != "")
                        {
                            StartCoroutine(JumpScenarioAsync(debugBattleData.AdvName,null));
                        } else
                        {
                            debugBattleData.MakeBattleActor();
                            CommandSceneChange(sceneInfo);
                        }
                    } else
                    {
                        CommandSceneChange(sceneInfo);
                    }
                    break;
                case Base.CommandType.MapChange:
                    var mapType = (MapType)viewEvent.template; 
                    CommandMapChange(mapType);
                    break;
                case Base.CommandType.MapClear:
                    CommandMapClear();
                    break;
                case Base.CommandType.CreateMapObject:
                    var mapObject = (GameObject)viewEvent.template; 
                    CommandCreateMapObject(mapObject);
                    break;
                case Base.CommandType.CallConfirmView:
                case Base.CommandType.CallSkillDetailView:
                    CommandConfirmView((ConfirmInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallCautionView:
                    CommandCautionView((CautionInfo)viewEvent.template);
                    break;
                case Base.CommandType.ClosePopup:
                    popupAssign.ClosePopup();
                    SetIsNotBusyMainAndStatus();
                    break;
                case Base.CommandType.ClosePopupAll:
                    popupAssign.ClosePopupAll();
                    SetIsNotBusyMainAndStatus();
                    break;
                case Base.CommandType.CloseConfirm:
                    confirmAssign.CloseConfirm();
                    SetIsNotBusyMainAndStatus();
                    break;
                case Base.CommandType.CallPopupView:
                    CommandPopupView((PopupInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallOptionView:
                    CommandOptionView((System.Action)viewEvent.template);
                    break;
                case Base.CommandType.CallSideMenu:
                    CommandSideMenu((SideMenuViewInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallRankingView:
                    CommandRankingView((RankingViewInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallCharacterListView:
                    CommandCharacterListView((CharacterListInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallHelpView:
                    CommandHelpView((List<ListData>)viewEvent.template);
                    break;
                case Base.CommandType.CallSlotSaveView:
                    break;
                case Base.CommandType.CallSkillTriggerView:
                    CommandSkillTriggerView((SkillTriggerViewInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallSkillLogView:
                    CommandCallSkillLogView((SkillLogViewInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallStatusView:
                    var statusViewInfo = (StatusViewInfo)viewEvent.template;
                    var statusView = CreateStatus(StatusType.Status,statusViewInfo) as StatusView;
                    statusView.SetViewInfo(statusViewInfo);
                    _currentScene.SetBusy(true);
                    break;
                case Base.CommandType.CloseStatus:
                    statusAssign.CloseStatus();
                    _currentScene.SetBusy(false);
                    break;
                case Base.CommandType.CallEnemyInfoView:
                    var enemyStatusInfo = (StatusViewInfo)viewEvent.template;
                    var enemyInfoView = CreateStatus(StatusType.EnemyInfo,enemyStatusInfo) as EnemyInfoView;
                    enemyInfoView.SetBackEvent(enemyStatusInfo.BackEvent);
                    _currentScene.SetBusy(true);
                    break;
                case Base.CommandType.CallTacticsStatusView:
                    var tacticsStatusInfo = (StatusViewInfo)viewEvent.template;
                    var tacticsStatusInfoView = CreateStatus(StatusType.TacticsStatus,tacticsStatusInfo) as TacticsStatusView;
                    tacticsStatusInfoView.SetViewInfo(tacticsStatusInfo);
                    tacticsStatusInfoView.SetBackEvent(tacticsStatusInfo.BackEvent);
                    _currentScene.SetBusy(true);
                    break;
                case Base.CommandType.CallAdvScene:
                    SetIsBusyMainAndStatus();
                    var advCallInfo = (AdvCallInfo)viewEvent.template;
                    StartCoroutine(JumpScenarioAsync(advCallInfo.Label.Value,advCallInfo.CallEvent));
                    break;
                case Base.CommandType.DecidePlayerName:
                    string playerName = (string)advEngine.Param.GetParameter("PlayerName");
                    advEngine.Param.SetParameterString("PlayerName",(string)viewEvent.template);
                    break;
                case Base.CommandType.CallLoading:
                    loadingView.gameObject.SetActive(true);
                    SetIsBusyMainAndStatus();
                    break;
                case Base.CommandType.CloseLoading:
                    loadingView.gameObject.SetActive(false);
                    SetIsNotBusyMainAndStatus();
                    break;
                case Base.CommandType.SetRouteSelect:
                    int routeSelect = (int)advEngine.Param.GetParameter("RouteSelect");
                    break;
                case Base.CommandType.ChangeViewToTransition:
                    transitionRoot.SetActive(true);
                    _currentScene.gameObject.transform.SetParent(transitionRoot.transform, false);
                    _currentScene = null;
                    break;
                case Base.CommandType.StartTransition:
                    transitionFade.FadeIn(0.8f,() => {
                        foreach(Transform child in transitionRoot.transform){
                            var endEvent = (Action)viewEvent.template;
                            if ((Action)viewEvent.template != null) endEvent();
                            Destroy(child.gameObject);
                            transitionFade.FadeOut(0);
                            transitionRoot.SetActive(false);
                        }
                    });
                    break;
                case Base.CommandType.CallTutorialFocus:
                    break;
                case Base.CommandType.CloseTutorialFocus:
                    if (popupAssign.StackPopupView != null)
                    {
                        if (popupAssign.StackPopupView.Find(a => a.GetType() == typeof(TutorialView)) != null) 
                        {                
                            popupAssign.CloseTutorialPopup();
                        }
                    }
                    break;
                case Base.CommandType.CheckTutorialState:
                    CheckTutorialState((TutorialViewInfo)viewEvent.template);
                    break;
                case Base.CommandType.SceneHideUI:
                    SceneHideUI();
                    break;
                case Base.CommandType.SceneShowUI:
                    SceneShowUI();
                    break;
            }
        }

        private void CommandConfirmView(ConfirmInfo confirmInfo)
        {
            var prefab = confirmAssign.CreateConfirm(confirmInfo.ConfirmType,helpWindow);
            var confirmView = prefab.GetComponent<ConfirmView>();
            confirmView.SetEvent((type) => UpdateCommand(type));
            confirmView.Initialize();
            confirmView.SetViewInfo(confirmInfo);
            confirmView.SetBackEvent(() => 
            {
                confirmView.CallSystemCommand(Base.CommandType.CloseConfirm);
                confirmInfo.BackEvent?.Invoke();
            });
            SetIsBusyMainAndStatus();
        }

        private void CommandCautionView(CautionInfo confirmInfo)
        {
            var prefab = confirmAssign.CreateConfirm(ConfirmType.Caution,helpWindow);
            var confirmView = prefab.GetComponent<CautionView>();
            confirmView.SetEvent((type) => UpdateCommand(type));
            confirmView.Initialize();
            if (confirmInfo.Title != null)
            {
                confirmView.SetTitle(confirmInfo.Title);
            }
            if (confirmInfo.From > 0 && confirmInfo.To > 0 )
            {
                confirmView.SetLevelup(confirmInfo.From,confirmInfo.To);
            }
            //SetIsBusyMainAndStatus();
        }

        private void CommandPopupView(PopupInfo popupInfo)
        {
            _sceneStackManager.PushPopupInfo(popupInfo);
            var prefab = popupAssign.CreatePopup(popupInfo.PopupType,helpWindow);
            var baseView = prefab.GetComponent<BaseView>();
            baseView.SetEvent((type) => UpdateCommand(type));
            baseView.Initialize();
            baseView.SetBackEvent(() => 
            {
                baseView.CallSystemCommand(Base.CommandType.ClosePopup);
                popupInfo.EndEvent?.Invoke();
            });
            if (popupInfo.PopupType == PopupType.LearnSkill)
            {
                var learnSkill = prefab.GetComponent<LearnSkillView>();
                learnSkill.SetLearnSkillInfo((LearnSkillInfo)popupInfo.template);
            } else
            if (popupInfo.PopupType == PopupType.Guide)
            {
                var guide = prefab.GetComponent<GuideView>();
                guide.SetGuide((string)popupInfo.template);
            }
            SetIsBusyMainAndStatus();
        }
        
        private void CommandOptionView(Action endEvent)
        {
            var prefab = popupAssign.CreatePopup(PopupType.Option,helpWindow);
            var optionView = prefab.GetComponent<OptionView>();
            optionView.SetEvent((type) => UpdateCommand(type));
            optionView.Initialize();
            optionView.SetBackEvent(() => 
            {
                OptionData.UpdateSoundParameter(
                    SoundManager.Instance.BgmVolume,
                    SoundManager.Instance.BGMMute,
                    SoundManager.Instance.SeVolume,
                    SoundManager.Instance.SeMute
                );
                SaveSystem.SaveOptionStart(OptionData);
                optionView.CallSystemCommand(Base.CommandType.ClosePopup);
                endEvent?.Invoke();
            });
            optionView.SetEvent((type) => UpdateCommand(type));
            SetIsBusyMainAndStatus();
        }

        private void CommandSkillTriggerView(SkillTriggerViewInfo skillTriggerViewInfo)
        {
            var prefab = popupAssign.CreatePopup(PopupType.SkillTrigger,helpWindow);
            var skillTriggerView = prefab.GetComponent<SkillTriggerView>();
            skillTriggerView.SetSkillTriggerViewInfo(skillTriggerViewInfo);
            skillTriggerView.SetEvent((type) => UpdateCommand(type));
            skillTriggerView.Initialize();
            skillTriggerView.SetBackEvent(() => 
            {
                skillTriggerView.CallSystemCommand(Base.CommandType.ClosePopup);
                skillTriggerViewInfo.EndEvent?.Invoke();
            });
            SetIsBusyMainAndStatus();
        }

        private void CommandCallSkillLogView(SkillLogViewInfo skillLogViewInfo)
        {
            var prefab = popupAssign.CreatePopup(PopupType.SkillLog,helpWindow);
            var skillLogView = prefab.GetComponent<SkillLogView>();
            skillLogView.SetEvent((type) => UpdateCommand(type));
            skillLogView.Initialize();
            skillLogView.SetSkillLogViewInfo(skillLogViewInfo.SkillLogListInfos);
            skillLogView.SetBackEvent(() => 
            {
                skillLogView.CallSystemCommand(Base.CommandType.ClosePopup);
                skillLogViewInfo.EndEvent?.Invoke();
            });
            SetIsBusyMainAndStatus();
        }

        private void CommandSideMenu(SideMenuViewInfo sideMenuViewInfo)
        {
            var prefab = popupAssign.CreatePopup(PopupType.SideMenu,helpWindow);
            var sideMenuView = prefab.GetComponent<SideMenuView>();
            sideMenuView.SetEvent((type) => UpdateCommand(type));
            sideMenuView.Initialize();
            sideMenuView.SetBackEvent(() => 
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                sideMenuView.CallSystemCommand(Base.CommandType.ClosePopup);
                sideMenuViewInfo.EndEvent?.Invoke();
            });
            sideMenuView.SetSideMenuViewInfo(sideMenuViewInfo);
        }
        
        private void CommandRankingView(RankingViewInfo rankingViewInfo)
        {
            var prefab = popupAssign.CreatePopup(PopupType.Ranking,helpWindow);
            var rankingView = prefab.GetComponent<RankingView>();
            rankingView.SetEvent((type) => UpdateCommand(type));
            rankingView.Initialize();
            rankingView.SetRankingViewInfo(rankingViewInfo);
            rankingView.SetBackEvent(() => 
            {
                rankingView.CallSystemCommand(Base.CommandType.ClosePopup);
                rankingViewInfo.EndEvent?.Invoke();
            });
            SetIsBusyMainAndStatus();
        }

        private void CommandCharacterListView(CharacterListInfo characterListInfo)
        {
            var prefab = popupAssign.CreatePopup(PopupType.CharacterList,helpWindow);
            var characterListView = prefab.GetComponent<CharacterListView>();
            characterListView.SetEvent((type) => UpdateCommand(type));
            characterListView.Initialize(characterListInfo.ActorInfos);
            characterListView.SetViewInfo(characterListInfo);
            characterListView.SetBackEvent(() => 
            {
                characterListInfo.BackEvent?.Invoke();
                characterListView.CallSystemCommand(Base.CommandType.ClosePopup);
            });
            SetIsBusyMainAndStatus();
        }

        private void CommandHelpView(List<ListData> helpTextList)
        {
            var prefab = popupAssign.CreatePopup(PopupType.Help,helpWindow);
            var helpView = prefab.GetComponent<HelpView>();
            helpView.SetEvent((type) => UpdateCommand(type));
            helpView.Initialize();
            helpView.SetHelp(helpTextList);
            helpView.SetBackEvent(() => 
            {
                helpView.CallSystemCommand(Base.CommandType.ClosePopup);
            });
            SetIsBusyMainAndStatus();
        }

        IEnumerator JumpScenarioAsync(string label, Action onComplete)
        {
            _busy = true;
            advHelpWindow.SetInputInfo("ADV_READING");
            if (!OptionData.EventSkipIndex)
            {
                while (advEngine.IsWaitBootLoading) yield return null;
                while (advEngine.IsLoading) yield return null;
                advEngine.Param.SetParameterBoolean("SelectionParam_0",false);
                advEngine.Param.SetParameterBoolean("SelectionParam_1",false);
                advEngine.JumpScenario(label);
                advEngine.Config.IsSkip = OptionData.EventTextSkipIndex;
                advController.StartAdv();
                while (!advEngine.IsEndOrPauseScenario)
                {
                    yield return null;
                }
            }
            SetIsNotBusyMainAndStatus();
            advController.EndAdv();
            advHelpWindow.SetInputInfo("");
            
            _busy = false;
            onComplete?.Invoke();
        }

        public void CommandSceneChange(SceneInfo sceneInfo)
        {
            if (_currentScene != null)
            { 
                Destroy(_currentScene.gameObject);
                ResourceSystem.ReleaseAssets();
                ResourceSystem.ReleaseScene();
                Resources.UnloadUnusedAssets();
            }
            if (sceneInfo.SceneChangeType == SceneChangeType.Pop)
            {
                sceneInfo.FromScene = _sceneStackManager.LastScene;
                sceneInfo.ToScene = _sceneStackManager.LastScene;
            } else
            {
                sceneInfo.FromScene = _sceneStackManager.Current;
            }
            var prefab = sceneAssign.CreateScene(sceneInfo.ToScene,helpWindow);
            _currentScene = prefab.GetComponent<BaseView>();
            _currentScene.SetTestMode(testMode);
            _currentScene.SetBattleTestMode(debugBattleData.TestBattle);
            _currentScene.SetEvent((type) => UpdateCommand(type));
            _sceneStackManager.PushSceneInfo(sceneInfo);
            _currentScene.Initialize();
            //tutorialView.HideFocusImage();
        }

        public void CommandMapChange(MapType mapType)
        {
            var prefab = mapAssign.CreateMap(mapType);
            /*
            _currentScene = prefab.GetComponent<BaseView>();
            _currentScene.SetTestMode(testMode);
            _currentScene.SetBattleTestMode(debugBattleData.TestBattle);
            _currentScene.SetEvent((type) => UpdateCommand(type));
            _sceneStackManager.PushSceneInfo(mapType);
            _currentScene.Initialize();
            */
            //tutorialView.HideFocusImage();
        }

        private void CommandMapClear()
        {
            mapAssign.ClearMap();
        }

        private void CommandCreateMapObject(GameObject mapObject)
        {
            mapAssign.CreateMapObject(mapObject);
        }


        private void SetIsBusyMainAndStatus()
        {
            _currentScene.SetBusy(true);
            statusAssign.SetBusy(true);
        }

        private void SetIsNotBusyMainAndStatus()
        {
            if (!statusAssign.StatusRoot.gameObject.activeSelf) _currentScene.SetBusy(false);
            statusAssign.SetBusy(false);
        }

        private void SceneShowUI()
        {
            _currentScene?.ChangeUIActive(true);
        }

        private void SceneHideUI()
        {
            _currentScene?.ChangeUIActive(false);
        }

        private void CheckTutorialState(TutorialViewInfo tutorialViewInfo)
        {
            if (OptionData.TutorialCheck == false)
            {
                return;
            }
            var TutorialDates = _model.SceneTutorialDates(tutorialViewInfo.SceneType);
            var tutorialData = TutorialDates.Count > 0 ? TutorialDates[0] : null;
            var checkEndFlag = _lastTutorialData != null && tutorialViewInfo.CheckEndMethod != null ? tutorialViewInfo.CheckEndMethod(_lastTutorialData) : false;
            if (checkEndFlag)
            {
                tutorialView.gameObject.SetActive(false);
            }
            if (tutorialData != null)
            {
                var checkFlag = tutorialViewInfo.CheckMethod(tutorialData);
                if (!checkFlag)
                {
                    return;
                }
                if (_lastTutorialData?.Id == tutorialData.Id)
                {
                    return;
                }
            }
            if (tutorialData != null)
            {
                tutorialViewInfo.CheckTrueAction?.Invoke();
                _lastTutorialData = tutorialData;
                tutorialView.gameObject.SetActive(true);
                tutorialView.SetTutorialData(tutorialData);
                tutorialView.SetBackEvent(() => 
                {
                    tutorialView.OnClickBack();
                    tutorialView.gameObject.SetActive(false);
                    tutorialViewInfo.EndEvent?.Invoke();
                });
                _model.ReadTutorialData(tutorialData);
            }
        }

        private void Update() 
        {
            TempData.AddPlayingTime(Time.deltaTime);
        }
    }


    public class AdvCallInfo
    {
        public ParameterString Label = new();
        private Action _callEvent;
        public Action CallEvent => _callEvent;
        public AdvCallInfo()
        {
        }
        public void SetCallEvent(Action callEvent)
        {
            _callEvent = callEvent;
        }
    }
}