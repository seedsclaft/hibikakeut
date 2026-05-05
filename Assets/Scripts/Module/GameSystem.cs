using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Effekseer;
using UnityEngine;
using UnityEngine.UI;
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

        [SerializeField] private GameObject transitionRoot = null;
        [SerializeField] private Fade transitionFade = null;
        [SerializeField] private RenderTexToPNG renderTexToPNG = null;
        [SerializeField] private EffekseerEmitter effectmitter = null;
        [SerializeField] private UnityEngine.UI.Image flashImage = null;
        [SerializeField] private LoadingView loadingView = null;
        [SerializeField] private TutorialView tutorialView = null;
        [SerializeField] private AdvEngine advEngine = null;
        [SerializeField] private AdvController advController = null;

        [SerializeField] private DebugBattleData debugBattleData = null;
        [SerializeField] private HelpWindow helpWindow = null;
        [SerializeField] private HelpWindow advHelpWindow = null;
        [SerializeField] private GameObject dungeonObjects = null;
        [SerializeField] private ColorSettings colorSettings = null;


        private BaseView _currentScene = null;
        public BaseView CurrentScene => _currentScene;

        private BaseModel _model = null;

        public static SaveInfo CurrentData = null;
        public static SaveGameInfo GameInfo = null;
        public static SaveOptionInfo OptionData = null;
        public static TempInfo TempData = null;
        private static TutorialData _lastTutorialData = null;
        private bool _busy = false;
        public bool TutorialBusy => tutorialView != null ? tutorialView.gameObject.activeSelf : false;

        public static string Version;
        public static DebugBattleData DebugBattleData;

        private static SceneStackManager _sceneStackManager = new();
        public static SceneStackManager SceneStackManager => _sceneStackManager;

        private List<BaseView> _inputableBaseViews = new();
        public BaseView LaseInputableBaseView => _inputableBaseViews.Count > 0 ? _inputableBaseViews[^1] : null;

        private void Awake()
        {
            if (Instance == null)
            {

            }
#if UNITY_WEBGL || UNITY_ANDROID || UNITY_STANDALONE_WIN// && !UNITY_EDITOR
            //FirebaseController.Instance.Initialize();
#endif
            Application.targetFrameRate = 60;
            ResourceSystem.Initialize();
            advController.Initialize();
            advController.SetHelpWindow(advHelpWindow);
            UIComponent.SetActive(transitionRoot, false);
            loadingView.Initialize();
            UIComponent.SetActive(loadingView?.gameObject, false);
            transitionFade.Init();
            statusAssign.CloseStatus();
            InputSystem.Initialize();
            sceneAssign.Initilize();
            popupAssign.Initilize();
            statusAssign.Initilize();
            TempData = new TempInfo();
            if (colorSettings != null)
            {
                TempData.ColorSettings = colorSettings;
            }
            _model = new BaseModel();
            _lastTutorialData = null;
            Version = Application.version;
            _sceneStackManager.ClearPopupInfo();
#if UNITY_EDITOR
            DebugBattleData = debugBattleData;
#endif 
#if UNITY_ANDROID
            AdMobController.Instance.Initialize(() => {CommandSceneChange(Scene.Boot);});
#else
            CommandSceneChange(new SceneInfo() { ToScene = Scene.Boot });
#endif
        }

        private BaseView CreateStatus(StatusType statusType, StatusViewInfo statusViewInfo)
        {
            _sceneStackManager.PushStatusViewInfo(statusViewInfo);
            var prefab = statusAssign.CreatePopup(statusType, helpWindow);
            var baseView = prefab.GetComponent<BaseView>();
            baseView.SetEvent(async (type) => await UpdateCommand(type));
            baseView.Initialize();
            _inputableBaseViews.Add(baseView);
            return baseView;
        }

        private async Task UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy && !viewEvent.ThrowBusy)
            {
                return;
            }
            if (viewEvent == null || viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.System)
            {
                return;
            }
            LogOutput.Log(viewEvent.ViewCommandType.CommandType);
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case Base.CommandType.SceneChange:
                    var sceneInfo = (SceneInfo)viewEvent.Template;
                    if (testMode && sceneInfo.ToScene == Scene.Battle)
                    {
                        if (debugBattleData.AdvName != "")
                        {
                            StartCoroutine(JumpScenarioAsync(debugBattleData.AdvName, null));
                        }
                        else
                        {
                            debugBattleData.MakeBattleActor();
                            CommandSceneChange(sceneInfo);
                        }
                    }
                    else
                    {
                        CommandSceneChange(sceneInfo);
                    }
                    break;
                case Base.CommandType.ChangeDungeon:
                    var mapType = (string)viewEvent.Template;
                    CommandMapChange(mapType);
                    break;
                case Base.CommandType.MapClear:
                    CommandMapClear();
                    break;
                case Base.CommandType.CallConfirmView:
                case Base.CommandType.CallSkillDetailView:
                    CommandConfirmView((ConfirmInfo)viewEvent.Template);
                    break;
                case Base.CommandType.CallCautionView:
                    CommandCautionView((CautionInfo)viewEvent.Template);
                    break;
                case Base.CommandType.CallMissionClearView:
                    CommandMissionClearView((MissionClearInfo)viewEvent.Template);
                    break;
                case Base.CommandType.ClosePopup:
                    _inputableBaseViews.Remove(popupAssign.LastPopupView);
                    popupAssign.ClosePopup();
                    break;
                case Base.CommandType.ClosePopupAll:
                    _sceneStackManager.ClearPopupInfo();
                    popupAssign.ClosePopupAll();
                    _inputableBaseViews.Clear();
                    break;
                case Base.CommandType.CloseConfirm:
                    _inputableBaseViews.Remove(confirmAssign.LastPopupPrefab.GetComponent<ConfirmView>());
                    confirmAssign.CloseConfirm();
                    break;
                case Base.CommandType.CallPopupView:
                    await CommandPopupView((PopupInfo)viewEvent.Template);
                    break;
                case Base.CommandType.CallRankingView:
                    CommandRankingView((RankingViewInfo)viewEvent.Template);
                    break;
                case Base.CommandType.CallHelpView:
                    CommandHelpView((List<ListData>)viewEvent.Template);
                    break;
                case Base.CommandType.CallSkillTriggerView:
                    CommandSkillTriggerView((SkillTriggerViewInfo)viewEvent.Template);
                    break;
                case Base.CommandType.CallSkillLogView:
                    CommandCallSkillLogView((SkillLogViewInfo)viewEvent.Template);
                    break;
                case Base.CommandType.CallStatusView:
                    var statusViewInfo = (StatusViewInfo)viewEvent.Template;
                    var statusView = CreateStatus(StatusType.Status, statusViewInfo) as StatusView;
                    break;
                case Base.CommandType.CloseStatus:
                    _inputableBaseViews.Remove(statusAssign.StatusView);
                    statusAssign.CloseStatus();
                    break;
                case Base.CommandType.CallEnemyInfoView:
                    var enemyStatusInfo = (StatusViewInfo)viewEvent.Template;
                    var enemyInfoView = CreateStatus(StatusType.EnemyInfo, enemyStatusInfo) as EnemyInfoView;
                    enemyInfoView.SetBackEvent(enemyStatusInfo.BackEvent);
                    break;
                case Base.CommandType.CallAdvScene:
                    UIComponent.SetActive(Instance.gameObject, true);
                    var advCallInfo = (AdvCallInfo)viewEvent.Template;
                    StartCoroutine(JumpScenarioAsync(advCallInfo.Label.Value, advCallInfo.CallEvent.Invoke));
                    break;
                case Base.CommandType.DecidePlayerName:
                    string playerName = (string)advEngine.Param.GetParameter("PlayerName");
                    var name = (string)viewEvent.Template;
                    if (name != null)
                    {
                        advEngine.Param.SetParameterString("PlayerName", (string)viewEvent.Template);
                    }
                    break;
                case Base.CommandType.CommandOther:
                    var otherViewEvent = (OtherViewEvent)viewEvent.Template;
                    var sceneBaseView = _currentScene.gameObject.GetComponent<BaseView>();
                    if (sceneBaseView.ViewCommandSceneType == otherViewEvent.ViewCommandSceneType)
                    {
                        sceneBaseView.CallViewEvent(otherViewEvent.CommandType, otherViewEvent.Templete);
                    }
                    else
                    {
                        var popupBaseView = popupAssign.LastPopupView;
                        if (popupBaseView.ViewCommandSceneType == otherViewEvent.ViewCommandSceneType)
                        {
                            popupBaseView.CallViewEvent(otherViewEvent.CommandType, otherViewEvent.Templete);
                        }
                    }
                    break;
                case Base.CommandType.CallLoading:
                    UIComponent.SetActive(loadingView?.gameObject, true);
                    break;
                case Base.CommandType.CloseLoading:
                    UIComponent.SetActive(loadingView?.gameObject, false);
                    break;
                case Base.CommandType.SetRouteSelect:
                    //int routeSelect = (int)advEngine.Param.GetParameter("RouteSelect");
                    advEngine.Param.SetParameterInt("RouteSelect", (int)viewEvent.Template);
                    break;
                case Base.CommandType.ChangeViewToTransition:
                    renderTexToPNG.SaveRenderTextureToPNG();
                    UIComponent.SetActive(transitionRoot, true);
                    _currentScene.gameObject.transform.SetParent(transitionRoot.transform, false);
                    _currentScene = null;
                    break;
                case Base.CommandType.StartTransition:
                    transitionFade.FadeIn(0.8f, () =>
                    {
                        foreach (Transform child in transitionRoot.transform)
                        {
                            if (child.gameObject.name == "ScreenShot")
                            {
                                continue;
                            }
                            var endEvent = (Action)viewEvent.Template;
                            if ((Action)viewEvent.Template != null) endEvent();
                            Destroy(child.gameObject);
                            transitionFade.FadeOut(0);
                            UIComponent.SetActive(transitionRoot, false);
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
                    CheckTutorialState((TutorialViewInfo)viewEvent.Template);
                    break;
                case Base.CommandType.ShowMap:
                    UIComponent.SetActive(mapAssign?.gameObject, true);
                    break;
                case Base.CommandType.HideMap:
                    UIComponent.SetActive(mapAssign?.gameObject, false);
                    break;
                case Base.CommandType.ResumeDungeonBgm:
                    await ResumeDungeonBgm();
                    break;
                case Base.CommandType.SceneHideUI:
                    SceneHideUI();
                    break;
                case Base.CommandType.SceneShowUI:
                    SceneShowUI();
                    break;
                case Base.CommandType.PlayEffect:
                    PlayEffect();
                    break;
                case Base.CommandType.FlashEffect:
                    FlashEffect();
                    break;
            }
        }

        private void CommandConfirmView(ConfirmInfo confirmInfo)
        {
            var first = confirmAssign.CreateConfirm(confirmInfo.ConfirmType, helpWindow);
            var prefab = confirmAssign.LastPopupPrefab;
            var confirmView = prefab.GetComponent<ConfirmView>();
            if (first)
            {
                confirmView.SetEvent((type) => UpdateCommand(type));
            }
            confirmView.Initialize();
            confirmView.SetViewInfo(confirmInfo);
            confirmView.SetBackEvent(() =>
            {
                _inputableBaseViews.Remove(confirmView);
                confirmView.CallSystemCommand(Base.CommandType.CloseConfirm);
                confirmView.CallCancelEvent();
            });
            _inputableBaseViews.Add(confirmView);
        }

        private void CommandCautionView(CautionInfo confirmInfo)
        {
            var first = confirmAssign.CreateConfirm(ConfirmType.Caution, helpWindow);
            var prefab = confirmAssign.LastPopupPrefab;
            var confirmView = prefab.GetComponent<CautionView>();
            if (first)
            {
                confirmView.SetEvent((type) => UpdateCommand(type));
            }
            confirmView.Initialize();
            if (confirmInfo.Title != null)
            {
                confirmView.SetTitle(confirmInfo.Title.Value);
            }
            if (confirmInfo.From.Value > 0 && confirmInfo.To.Value > 0)
            {
                confirmView.SetLevelup(confirmInfo.From.Value, confirmInfo.To.Value);
            }
        }

        private void CommandMissionClearView(MissionClearInfo confirmInfo)
        {
            var first = confirmAssign.CreateConfirm(ConfirmType.MissionClear, helpWindow);
            var prefab = confirmAssign.LastPopupPrefab;
            var missionClearView = prefab.GetComponent<MissionClearView>();
            if (first)
            {
                missionClearView.SetEvent((type) => UpdateCommand(type));
            }
            missionClearView.Initialize();
            missionClearView.SetTitle(confirmInfo.Title);
        }

        private async Task CommandPopupView(PopupInfo popupInfo)
        {
            _sceneStackManager.PushPopupInfo(popupInfo);
            var first = popupAssign.CreatePopup(popupInfo.PopupType, helpWindow);
            var prefab = popupAssign.LastPopupPrefab;
            var baseView = prefab.GetComponent<BaseView>();
            if (first)
            {
                var popupAnimation = prefab.GetComponent<PopupAnimation>();
                popupAnimation?.Initialize(baseView.UiRoot.transform);
                await UniTask.DelayFrame(1);
                baseView.SetEvent((type) => UpdateCommand(type));
            }
            baseView.Initialize();
            if (first)
            {
                baseView.SetPopupCloseBackEvent();
            }
            if (popupInfo.PopupType == PopupType.LearnSkill)
            {
                var learnSkill = prefab.GetComponent<LearnSkillView>();
                learnSkill.SetLearnSkillInfo((LearnSkillInfo)popupInfo.template);
            }
            else
            if (popupInfo.PopupType == PopupType.Guide)
            {
                var guide = prefab.GetComponent<GuideView>();
                guide.SetGuide((string)popupInfo.template);
            }
            else
            if (popupInfo.PopupType == PopupType.Rankup)
            {
                var rankup = prefab.GetComponent<RankupView>();
                rankup.SetRankupInfo((RankupInfo)popupInfo.template);
            }
            else
            if (popupInfo.PopupType == PopupType.BattleScoreCurrency)
            {
                var battleScoreCurrencyView = prefab.GetComponent<BattleScoreCurrencyView>();
                battleScoreCurrencyView.SetBattleScoreCurrencyInfo((BattleScoreCurrencyInfo)popupInfo.template);
            }
            else
            if (popupInfo.PopupType == PopupType.ClassChange)
            {
                var classChange = prefab.GetComponent<ClassChangeView>();
                classChange.SetClassChangeInfo((ClassChangeInfo)popupInfo.template);
            }
            else
            if (popupInfo.PopupType == PopupType.Tutorial)
            {
                var tutorial = prefab.GetComponent<TutorialView>();
                tutorial.SetTutorialData((TutorialData)popupInfo.template);
            }
            _inputableBaseViews.Add(baseView);
        }

        private void CommandSkillTriggerView(SkillTriggerViewInfo skillTriggerViewInfo)
        {
            var first = popupAssign.CreatePopup(PopupType.SkillTrigger, helpWindow);
            var prefab = popupAssign.LastPopupPrefab;
            var skillTriggerView = prefab.GetComponent<SkillTriggerView>();
            skillTriggerView.SetSkillTriggerViewInfo(skillTriggerViewInfo);
            skillTriggerView.SetEvent((type) => UpdateCommand(type));
            skillTriggerView.Initialize();
            skillTriggerView.SetBackEvent(() =>
            {
                skillTriggerView.CallSystemCommand(Base.CommandType.ClosePopup);
                skillTriggerViewInfo.EndEvent?.Invoke();
            });
        }

        private void CommandCallSkillLogView(SkillLogViewInfo skillLogViewInfo)
        {
            var first = popupAssign.CreatePopup(PopupType.SkillLog, helpWindow);
            var prefab = popupAssign.LastPopupPrefab;
            var skillLogView = prefab.GetComponent<SkillLogView>();
            skillLogView.SetEvent((type) => UpdateCommand(type));
            skillLogView.Initialize();
            skillLogView.SetSkillLogViewInfo(skillLogViewInfo.SkillLogListInfos);
            skillLogView.SetBackEvent(() =>
            {
                skillLogView.CallSystemCommand(Base.CommandType.ClosePopup);
                skillLogViewInfo.EndEvent?.Invoke();
            });
        }

        private void CommandRankingView(RankingViewInfo rankingViewInfo)
        {
            var first = popupAssign.CreatePopup(PopupType.Ranking, helpWindow);
            var prefab = popupAssign.LastPopupPrefab;
            var rankingView = prefab.GetComponent<RankingView>();
            rankingView.SetEvent((type) => UpdateCommand(type));
            rankingView.Initialize();
            rankingView.SetRankingViewInfo(rankingViewInfo);
            rankingView.SetBackEvent(() =>
            {
                rankingView.CallSystemCommand(Base.CommandType.ClosePopup);
                rankingViewInfo.EndEvent?.Invoke();
            });
        }

        private void CommandHelpView(List<ListData> helpTextList)
        {
            var first = popupAssign.CreatePopup(PopupType.Help, helpWindow);
            var prefab = popupAssign.LastPopupPrefab;
            var helpView = prefab.GetComponent<HelpView>();
            helpView.SetEvent((type) => UpdateCommand(type));
            helpView.Initialize();
            helpView.SetHelp(helpTextList);
            helpView.SetBackEvent(() =>
            {
                helpView.CallSystemCommand(Base.CommandType.ClosePopup);
            });
        }

        private IEnumerator JumpScenarioAsync(string label, Action onComplete)
        {
            _busy = true;
            //advHelpWindow.SetInputInfo("ADV_READING");
            while (advEngine.IsWaitBootLoading)
            {
                yield return null;
            }

            while (advEngine.IsLoading)
            {
                yield return null;
            }
            advEngine.JumpScenario(label);
            advEngine.Config.IsSkip = false;//OptionData.EventTextSkipIndex;
            advController.StartAdv();
            _inputableBaseViews.Add(advController);
            while (!advEngine.IsEndOrPauseScenario)
            {
                yield return null;
            }
            advController.EndAdv();
            _inputableBaseViews.Remove(advController);

            _busy = false;
            onComplete?.Invoke();
        }

        public void CommandSceneChange(SceneInfo sceneInfo)
        {
            if (_currentScene != null)
            {
                Destroy(_currentScene.gameObject);
                //ResourceSystem.ReleaseAssets();
                //ResourceSystem.ReleaseScene();
                Resources.UnloadUnusedAssets();
            }
            if (sceneInfo.SceneChangeType == SceneChangeType.Pop)
            {
                sceneInfo.FromScene = _sceneStackManager.LastScene;
                sceneInfo.ToScene = _sceneStackManager.LastScene;
            }
            else
            {
                sceneInfo.FromScene = _sceneStackManager.Current;
            }
            UIComponent.SetActive(mapAssign?.gameObject, sceneInfo.ToScene == Scene.Dungeon);
            var prefab = sceneAssign.CreateScene(sceneInfo.ToScene, helpWindow);
            _currentScene = prefab.GetComponent<BaseView>();
            _currentScene.SetTestMode(testMode);
            _currentScene.SetBattleTestMode(debugBattleData.TestBattle);
            _currentScene.SetEvent((type) => UpdateCommand(type));
            _sceneStackManager.PushSceneInfo(sceneInfo);
            _currentScene.Initialize();
            _inputableBaseViews.Clear();
            _inputableBaseViews.Add(_currentScene);
            //tutorialView.HideFocusImage();
        }

        public void CommandMapChange(string mapName)
        {
            if (mapAssign.LastMapName == mapName)
            {
                return;
            }
            mapAssign.SetLastMapName(mapName);

            mapAssign.ClearMap();
            var prefab = Instantiate(dungeonObjects);
            prefab.transform.SetParent(mapAssign.transform, false);
            var dungeonSettings = prefab.GetComponentInChildren<Ariadne.DungeonSettings>();
            UIComponent.SetActive(mapAssign?.gameObject, true);
            var dungeonCamera = prefab.GetComponentInChildren<Camera>();
            if (dungeonCamera != null)
            {
                renderTexToPNG.targetCamera = dungeonCamera;
            }
            var dungeonData = ResourceSystem.LoadDungeonMaster(mapName);
            dungeonSettings.OnSetDungeon(dungeonData);
            _model.PartyInfo.SetupDungeonTraverse(Ariadne.PlayerPosition.Instance.currentDungeonId);
        }

        private void CommandMapClear()
        {
            mapAssign.SetLastMapName("");
            mapAssign.ClearMap();
        }

        private async Task ResumeDungeonBgm()
        {
            var bgmData = _model.DungeonBgmData();
            if (bgmData != null)
            {
                var bgm = await _model.GetBgmData(bgmData.Key);
                SoundManager.Instance.PlayBgm(bgm, bgmData.Volume, true, _model.DungeonBgmTimeStamp());
            }
        }

        private void SceneShowUI()
        {
            _currentScene?.ChangeUIActive(true);
        }

        private void SceneHideUI()
        {
            _currentScene?.ChangeUIActive(false);
        }

        private void PlayEffect()
        {
            if (effectmitter == null)
            {
                return;
            }
            UIComponent.SetActive(effectmitter.gameObject, true);
            effectmitter.speed = 2f;
            effectmitter.Play();
        }

        private void FlashEffect()
        {
            if (flashImage == null)
            {
                return;
            }
            UIComponent.SetActive(flashImage.gameObject, true);
            AnimationUtility.AlphaToTransform(flashImage, 0, 255, 0.2f, 0, () => 
            {
                AnimationUtility.AlphaToTransform(flashImage, 255, 0, 0.2f);
            });
        }

        private void CheckTutorialState(TutorialViewInfo tutorialViewInfo)
        {
            if (!OptionData.TutorialCheck)
            {
                return;
            }
            var tutorialDates = _model.SceneTutorialDates(tutorialViewInfo.SceneType);
            var tutorialData = tutorialDates.Count > 0 ? tutorialDates[0] : null;
            if (tutorialData == null)
            {
                return;
            }
            var checkEndFlag = _lastTutorialData != null && tutorialViewInfo.CheckEndMethod != null ? tutorialViewInfo.CheckEndMethod(_lastTutorialData) : false;
            if (checkEndFlag)
            {
                //tutorialView.gameObject.SetActive(false);
            }
            var checkFlag = tutorialViewInfo.CheckMethod(tutorialData);
            if (!checkFlag)
            {
                return;
            }
            if (_lastTutorialData?.Id == tutorialData.Id)
            {
                return;
            }
            tutorialViewInfo.CheckTrueAction?.Invoke(tutorialData);
            _lastTutorialData = tutorialData;
/*
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Tutorial
            };
            _sceneStackManager.PushPopupInfo(popupInfo);
            var first = popupAssign.CreatePopup(popupInfo.PopupType, helpWindow);
            var prefab = popupAssign.LastPopupPrefab;
            var baseView = prefab.GetComponent<TutorialView>();
            baseView.gameObject.SetActive(true);
            baseView.SetTutorialData(tutorialData);
            baseView.SetBackEvent(() =>
            {
                baseView.gameObject.SetActive(false);
                tutorialViewInfo.EndEvent?.Invoke();
            });
            if (first)
            {
                baseView.SetEvent((type) => UpdateCommand(type));
            }
            baseView.Initialize();
            if (first)
            {
                baseView.SetPopupCloseBackEvent();
            }
            _inputableBaseViews.Add(baseView);
*/
            _model.ReadTutorialData(tutorialData);
        }

    }
}