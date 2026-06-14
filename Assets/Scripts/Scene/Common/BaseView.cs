using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public abstract class BaseView : MonoBehaviour
    {
        private bool _isIntialized = false;
        public bool IsInitilized => _isIntialized;
        private bool _testMode = false;
        public bool TestMode => _testMode;
        private bool _testBattleMode = false;
        public bool TestBattleMode => _testBattleMode;
        private InputSystem _inputSystem;
        private InputSystemModel _inputSystemModel = null;

        public ParameterBool Busy = new();
        public void SetBusy(bool isBusy)
        {
            Debug.LogError(this.name + " isBusy = " + isBusy);
            Busy.SetValue(isBusy);
        }

        public List<Action<ViewEvent>> _commandData = new();
        public void SetEvent(Action<ViewEvent> commandData)
        {
            if (_commandData.Contains(commandData))
            {
                return;
            }
            _commandData.Add(commandData);
        }
        public void SetEvent(List<Action<ViewEvent>> commandDates)
        {
            foreach (var commandData in commandDates)
            {
                SetEvent(commandData);
            }
        }

        [SerializeField] private Button _backCommand = null;
        [SerializeField] private SpriteRenderer _backGround = null;
        public SpriteRenderer BackGround => _backGround;
        private Action _backEvent = null;
        public Action BackEvent => _backEvent;
        [SerializeField] private GameObject uiRoot = null;
        public GameObject UiRoot => uiRoot;
        [SerializeField] private OnOffButton sideMenuButton = null;
        public OnOffButton SideMenuButton => sideMenuButton;
        private BaseAnimation baseAnimation = null;
        public void SetBaseAnimation(BaseAnimation animation) => baseAnimation = animation;
        public bool AnimationBusy => baseAnimation != null && baseAnimation.Busy.Value;
        private int _wait = 0;
        public Action _waitEndEvent = null;
        private List<BaseList> _viewActives = new();
        private BaseList _lastActiveBaseList = null;
        public void AddViewActives(BaseList baseList)
        {
            SetInputHandler(baseList.gameObject);
            _viewActives.Add(baseList);
        }

        private void SetInputHandler(IInputHandlerEvent handler)
        {
            _inputSystemModel.AddInputHandler(handler);
        }

        public void SetInputHandler(GameObject gameObject)
        {
            var handler = gameObject.GetComponent<IInputHandlerEvent>();
            if (handler == null)
            {
                return;
            }
            SetInputHandler(handler);
        }

        public void SetActivate(BaseList baseView)
        {
            var find = _viewActives.Find(a => a == baseView);
            foreach (var viewActives in _viewActives)
            {
                if (viewActives == find)
                {
                    find.Activate();
                    _lastActiveBaseList = find;
                }
                else
                {
                    viewActives.Deactivate();
                }
            }
        }

        public void SetLastActivate()
        {
            if (_lastActiveBaseList == null)
            {
                return;
            }
            SetDeactivate();
            _lastActiveBaseList.Activate();
        }

        public void SetDeactivate()
        {
            foreach (var viewActives in _viewActives)
            {
                viewActives.Deactivate();
            }
        }

        private ViewCommandSceneType _viewCommandSceneType = ViewCommandSceneType.None;
        public ViewCommandSceneType ViewCommandSceneType => _viewCommandSceneType;
        public void SetViewCommandSceneType(ViewCommandSceneType viewCommandSceneType) => _viewCommandSceneType = viewCommandSceneType;
        public void CallViewEvent(object template, object sendData = null, bool throwBusy = false)
        {
            if (_viewCommandSceneType == ViewCommandSceneType.None)
            {
                return;
            }
            var commandType = new ViewCommandType(_viewCommandSceneType, template);
            var eventData = new ViewEvent(commandType)
            {
                Template = sendData,
                ThrowBusy = throwBusy
            };
            foreach (var commandData in _commandData)
            {
                commandData(eventData);
            }
        }

        private HelpWindow _helpWindow = null;
        public HelpWindow HelpWindow => _helpWindow;
        public void SetHelpInputInfo(string key)
        {
            _helpWindow?.SetInputInfo(key);
        }

        public void SetHelpText(string text)
        {
            _helpWindow?.SetHelpText(text);
        }

        public async Task SetBackGround(string fileName)
        {
            _backGround.sprite = await ResourceSystem.LoadBackGround(fileName);
        }

        public void SetInputFrame(int frame)
        {
            _inputSystemModel.SetInputFrame(frame);
        }

        public virtual void Initialize()
        {
            if (_isIntialized)
            {
                return;
            }
            _inputSystemModel = new InputSystemModel();
            InitializeInput();
            SetInputHandler(gameObject);
            _isIntialized = true;
        }

        public void InitializeInput()
        {
            _inputSystem = new InputSystem();
        }

        public void SetHelpWindow(HelpWindow helpWindow)
        {
            _helpWindow = helpWindow;
        }

        public void LateUpdate()
        {
            if (_inputSystem == null)
            {
                return;
            }
            if (InputSystem._inputDates.Count == 0)
            {
                return;
            }
            if (GameSystem.Instance?.LaseInputableBaseView == null)
            {
                return;
            }
            if (this != GameSystem.Instance?.LaseInputableBaseView)
            {
                return;
            }
            UpdateWait();
            if (Busy.Value)
            {
                return;
            }
            //Debug.LogError(name + "Inputable");
            _inputSystemModel.UpdateInputKeyType(_inputSystem.Update());
        }

        private void UpdateWait()
        {
            if (_wait <= 0)
            {
                return;
            }
            Busy.SetValue(true);
            _wait--;
            if (_wait <= 0)
            {
                Busy.SetValue(false);
                _waitEndEvent?.Invoke();
            }
        }

        public void CallSystemCommand(object template, object sendData = null, bool throwBusy = false)
        {
            var commandType = new ViewCommandType(ViewCommandSceneType.System, template);
            var eventData = new ViewEvent(commandType)
            {
                Template = sendData
            };
            eventData.ThrowBusy = throwBusy;
            foreach (var commandData in _commandData)
            {
                commandData(eventData);
            }
        }

        public void CommandSceneChange(Scene scene, object sceneParam = null, SceneChangeType sceneChangeType = SceneChangeType.Push)
        {
            var sceneInfo = new SceneInfo()
            {
                ToScene = scene,
                SceneChangeType = sceneChangeType,
                SceneParam = sceneParam
            };
            CallSystemCommand(Base.CommandType.SceneChange, sceneInfo);
        }

        public void CommandChangeDungeon(string mapName)
        {
            CallSystemCommand(Base.CommandType.ChangeDungeon, mapName);
        }

        public void CommandPopSceneChange(object sceneParam = null)
        {
            var sceneInfo = new SceneInfo()
            {
                SceneChangeType = SceneChangeType.Pop,
                SceneParam = sceneParam
            };
            CallSystemCommand(Base.CommandType.SceneChange, sceneInfo);
        }

        public void CommandGotoSceneChange(Scene scene, object sceneParam = null)
        {
            var sceneInfo = new SceneInfo()
            {
                ToScene = scene,
                SceneChangeType = SceneChangeType.Goto,
                SceneParam = sceneParam
            };
            CallSystemCommand(Base.CommandType.SceneChange, sceneInfo);
        }

        public void CommandCallConfirm(ConfirmInfo confirmInfo)
        {
            CallSystemCommand(Base.CommandType.CallConfirmView, confirmInfo);
        }

        public void CommandCallSkillDetail(ConfirmInfo popupInfo)
        {
            CallSystemCommand(Base.CommandType.CallSkillDetailView, popupInfo);
        }

        public void CommandCallCaution(CautionInfo popupInfo)
        {
            CallSystemCommand(Base.CommandType.CallCautionView, popupInfo);
        }

        public void CommandCallMissionClear(MissionClearInfo popupInfo)
        {
            CallSystemCommand(Base.CommandType.CallMissionClearView, popupInfo);
        }

        public void CommandCallPopup(PopupInfo popupInfo)
        {
            CallSystemCommand(Base.CommandType.CallPopupView, popupInfo);
        }

        public void CommandCallSkillTrigger(SkillTriggerViewInfo skillTriggerViewInfo)
        {
            CallSystemCommand(Base.CommandType.CallSkillTriggerView, skillTriggerViewInfo);
        }

        public void CommandCallAdv(AdvCallInfo advCallInfo)
        {
            CallSystemCommand(Base.CommandType.CallAdvScene, advCallInfo);
        }

        public void CommandChangeViewToTransition(Action<string> endEvent)
        {
            CallSystemCommand(Base.CommandType.ChangeViewToTransition, endEvent);
        }

        public void CommandStartTransition(Action endEvent)
        {
            CallSystemCommand(Base.CommandType.StartTransition, endEvent);
        }

        public void CommandCheckTutorialState(TutorialViewInfo tutorialViewInfo)
        {
            CallSystemCommand(Base.CommandType.CheckTutorialState, tutorialViewInfo);
        }

        public void CommandCloseTutorialFocus()
        {
            CallSystemCommand(Base.CommandType.CloseTutorialFocus);
        }

        public void CommandSceneShowUI()
        {
            CallSystemCommand(Base.CommandType.SceneShowUI);
        }

        public void CommandSceneHideUI()
        {
            CallSystemCommand(Base.CommandType.SceneHideUI);
        }

        public void CommandCallLoading()
        {
            CallSystemCommand(Base.CommandType.CallLoading);
        }

        public void CommandCloseLoading()
        {
            CallSystemCommand(Base.CommandType.CloseLoading);
        }

        public void SetBackCommand(Action callEvent)
        {
            if (_backCommand != null)
            {
                _backCommand.onClick.RemoveAllListeners();
                _backCommand.onClick.AddListener(() =>
                {
                    if (!_backCommand.gameObject.activeSelf)
                    {
                        return;
                    }
                    callEvent();
                });
            }
            _backEvent = callEvent;
        }

        public void SetPopupCloseBackEvent()
        {
            // 既に登録済みであれば上書きしない
            if (_backEvent != null)
            {
                return;
            }
            SetBackCommand(PopupClose);
        }

        public void PopupClose()
        {
            var endPopupInfo = GameSystem.SceneStackManager.LastPopupInfo;
            CallSystemCommand(Base.CommandType.ClosePopup);
            if (endPopupInfo != null && !Busy.Value)
            {
                endPopupInfo.EndEvent?.Invoke();
                GameSystem.SceneStackManager.RemovePopupInfo(endPopupInfo);
            }
        }

        public void SetBackEvent(Action backEvent)
        {
            SetBackCommand(() =>
            {
                if (uiRoot.activeSelf)
                {
                    backEvent?.Invoke();
                }
            });
            ChangeBackCommandActive(true);
        }

        public void ChangeBackCommandActive(bool IsActive)
        {
            if (_backCommand == null)
            {
                return;
            }
            UIComponent.SetActive(_backCommand.gameObject, IsActive);
        }

        public void ChangeUIActive(bool IsActive)
        {
            UIComponent.SetActive(uiRoot, IsActive);
        }

        public void SetTestMode(bool isTest)
        {
            _testMode = isTest;
        }

        public void SetBattleTestMode(bool isTest)
        {
            _testBattleMode = isTest;
        }

        public virtual void MouseCancelHandler()
        {

        }

        public void MouseMoveHandler(Vector3 position)
        {

        }

        public void MouseWheelHandler(Vector2 position)
        {

        }

        public void WaitFrame(int frame, Action waitEndEvent)
        {
            _wait = frame;
            _waitEndEvent = waitEndEvent;
        }

        private void OnDestroy()
        {
            var listViews = GetComponentsInChildren<ListWindow>();
            for (int i = listViews.Length - 1; i >= 0; i--)
            {
                listViews[i].Release();
            }
        }

        public void Dispose()
        {
            //_commandData.Clear();
        }

    }

    namespace Base
    {
        public enum CommandType
        {
            None = 0,
            SceneChange,
            ChangeDungeon,
            MapClear,
            CallConfirmView,
            CallSkillDetailView,
            CallCautionView,
            CallMissionClearView,
            CallPopupView,
            ClosePopup,
            ClosePopupAll,
            CloseConfirm,
            CallRankingView,
            CallHelpView,
            CallStatusView,
            CloseStatus,
            CallAdvScene,
            CommandOther,
            CallEnemyInfoView,
            CallSkillTriggerView,
            CallSkillLogView,
            DecidePlayerName,
            CallLoading,
            CloseLoading,
            SetRouteSelect,
            ChangeViewToTransition,
            StartTransition,
            CallTutorialFocus,
            CloseTutorialFocus,
            CheckTutorialState,
            ShowMap,
            HideMap,
            ResumeDungeonBgm,
            SceneShowUI,
            SceneHideUI,
            PlayEffect,
            FlashEffect,
        }
    }
}