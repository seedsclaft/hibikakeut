using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    abstract public class BaseView : MonoBehaviour
    {
        private bool _testMode = false;
        public bool TestMode => _testMode;
        private bool _testBattleMode = false;
        public bool TestBattleMode => _testBattleMode;
        private InputSystem _inputSystem;
        private InputSystemModel _inputSystemModel = null;

        private bool _busy = false;
        public bool Busy => _busy;
        public void SetBusy(bool isBusy)
        {
            _busy = isBusy;
        }

        public List<Action<ViewEvent>> _commandData = new ();
        public void SetEvent(Action<ViewEvent> commandData)
        {
            _commandData.Add(commandData);
        }
    
        [SerializeField] private Button _backCommand = null;
        [SerializeField] private SpriteRenderer _backGround = null;
        private Action _backEvent = null;
        public Action BackEvent => _backEvent;
        [SerializeField] private GameObject uiRoot = null;
        public GameObject UiRoot => uiRoot;
        [SerializeField] private OnOffButton sideMenuButton = null;
        public OnOffButton SideMenuButton => sideMenuButton;
        private BaseAnimation baseAnimation = null;
        public void SetBaseAnimation(BaseAnimation animation) => baseAnimation = animation;
        public bool AnimationBusy => baseAnimation != null && baseAnimation.Busy;
        private int _wait = 0;
        public Action _waitEndEvent = null;
        private List<BaseList> _viewActives = new ();
        public void AddViewActives(BaseList baseList) => _viewActives.Add(baseList);
        public void SetActivate(BaseList baseView)
        {
            var find = _viewActives.Find(a => a == baseView);
            foreach (var viewActives in _viewActives)
            {
                if (viewActives == find)
                {
                    find.Activate();
                } else
                {
                    viewActives.Deactivate();
                }
            }
        }
        public BaseList ActivateView => _viewActives.Find(a => a.Active);

        private ViewCommandSceneType _viewCommandSceneType = ViewCommandSceneType.None;
        public void SetViewCommandSceneType(ViewCommandSceneType viewCommandSceneType) => _viewCommandSceneType = viewCommandSceneType;
        public void CallViewEvent(object template,object sendData = null)
        {
            if (_viewCommandSceneType == ViewCommandSceneType.None)
            {
                return;
            }
            var commandType = new ViewCommandType(_viewCommandSceneType,template);
            var eventData = new ViewEvent(commandType)
            {
                template = sendData
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

        public void SetBackGround(string fileName)
        {
            _backGround.sprite = ResourceSystem.LoadBackGround(fileName);
        }

        public void SetInputFrame(int frame)
        {
            _inputSystemModel.SetInputFrame(frame);
        }

        public virtual void Initialize()
        {
            _inputSystemModel = new InputSystemModel();
            InitializeInput();
            SetInputHandler(gameObject);
        }

        public void InitializeInput()
        {    
            _inputSystem = new InputSystem();
        }

        public void SetHelpWindow(HelpWindow helpWindow)
        {
            _helpWindow = helpWindow;
        }

        public void SetInputHandler(IInputHandlerEvent handler)
        {
            _inputSystemModel.AddInputHandler(handler);
        }

        public void SetInputHandler(GameObject gameObject)
        {
            var handler = gameObject.GetComponent<IInputHandlerEvent>();
            if (handler != null)
            {
                SetInputHandler(handler);
            }
        }

        public void LateUpdate() 
        {
            if (_inputSystem != null && _busy == false)
            {
                _inputSystemModel.UpdateInputKeyType(_inputSystem.Update());
            }
            UpdateWait();
        }

        public void Update()
        {
        }

        private void UpdateWait()
        {
            if (_wait <= 0) return;
            _busy = true;
            _wait--;
            if (_wait <= 0)
            {
                _busy = false;
                _waitEndEvent?.Invoke();
            }
        }

        public void CommandOpenSideMenu()
        {
            _helpWindow.SetInputInfo("SIDEMENU");
            _helpWindow.SetHelpText(DataSystem.GetHelp(19700));
        }

        public void CallSystemCommand(object template,object sendData = null)
        {
            var commandType = new ViewCommandType(ViewCommandSceneType.System,template);
            var eventData = new ViewEvent(commandType)
            {
                template = sendData
            };
            foreach (var commandData in _commandData)
            {
                commandData(eventData);
            }
        }

        public void CommandSceneChange(Scene scene,object sceneParam = null,SceneChangeType sceneChangeType = SceneChangeType.Push)
        {
            var sceneInfo = new SceneInfo()
            {
                ToScene = scene,
                SceneChangeType = sceneChangeType,
                SceneParam = sceneParam
            };
            CallSystemCommand(Base.CommandType.SceneChange,sceneInfo);
        }

        public void CommandMapChange(MapType mapType)
        {
            CallSystemCommand(Base.CommandType.MapChange,mapType);
        }

        public void CommandCreateMapObject(GameObject mapObject)
        {
            CallSystemCommand(Base.CommandType.CreateMapObject,mapObject);
        }

        public void CommandPopSceneChange(object sceneParam = null)
        {
            var sceneInfo = new SceneInfo()
            {
                SceneChangeType = SceneChangeType.Pop,
                SceneParam = sceneParam
            };
            CallSystemCommand(Base.CommandType.SceneChange,sceneInfo);
        }

        public void CommandGotoSceneChange(Scene scene,object sceneParam = null)
        {
            var sceneInfo = new SceneInfo()
            {
                ToScene = scene,
                SceneChangeType = SceneChangeType.Goto,
                SceneParam = sceneParam
            };
            CallSystemCommand(Base.CommandType.SceneChange,sceneInfo);
        }

        public void CommandCallConfirm(ConfirmInfo popupInfo)
        {
            CallSystemCommand(Base.CommandType.CallConfirmView,popupInfo);
        }

        public void CommandCallSkillDetail(ConfirmInfo popupInfo)
        {
            CallSystemCommand(Base.CommandType.CallSkillDetailView,popupInfo);
        }

        public void CommandCallCaution(CautionInfo popupInfo)
        {
            CallSystemCommand(Base.CommandType.CallCautionView,popupInfo);
        }

        public void CommandCallPopup(PopupInfo popupInfo)
        {
            CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        public void CommandCallOption(Action endEvent)
        {
            CallSystemCommand(Base.CommandType.CallOptionView,endEvent);
        }

        public void CommandCallSkillTrigger(SkillTriggerViewInfo skillTriggerViewInfo)
        {
            CallSystemCommand(Base.CommandType.CallSkillTriggerView,skillTriggerViewInfo);
        }

        public void CommandCallAdv(AdvCallInfo advCallInfo)
        {
            CallSystemCommand(Base.CommandType.CallAdvScene,advCallInfo);
        }

        public void CommandChangeViewToTransition(Action<string> endEvent)
        {
            CallSystemCommand(Base.CommandType.ChangeViewToTransition,endEvent);
        }

        public void CommandStartTransition(Action endEvent)
        {
            CallSystemCommand(Base.CommandType.StartTransition,endEvent);
        }

        public void CommandCheckTutorialState(TutorialViewInfo tutorialViewInfo)
        {
            CallSystemCommand(Base.CommandType.CheckTutorialState,tutorialViewInfo);
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

        public void SetBackCommand(Action callEvent)
        {
            if (_backCommand != null)
            {
                _backCommand.onClick.RemoveAllListeners();
                _backCommand.onClick.AddListener(() => 
                {
                    if (!_backCommand.gameObject.activeSelf) return;
                    callEvent();
                });
            }
            _backEvent = callEvent;
        }

        public void SetBackEvent(Action backEvent)
        {
            SetBackCommand(() => 
            {
                backEvent?.Invoke();
            });
            ChangeBackCommandActive(true);
        }
        
        public void ChangeBackCommandActive(bool IsActive)
        {
            _backCommand?.gameObject.SetActive(IsActive);
        }

        public void ChangeUIActive(bool IsActive)
        {
            uiRoot.SetActive(IsActive);
        }

        public void SetTestMode(bool isTest)
        {
            _testMode = isTest;
        }

        public void SetBattleTestMode(bool isTest)
        {
            _testBattleMode = isTest;
        }

        public void MouseCancelHandler()
        {

        }

        public void MouseMoveHandler(Vector3 position)
        {

        }

        public void MouseWheelHandler(Vector2 position)
        {

        }

        public void WaitFrame(int frame,System.Action waitEndEvent)
        {
            _wait = frame;
            _waitEndEvent = waitEndEvent;
        }

        private void OnDestroy() 
        {
            var listViews = GetComponentsInChildren<ListWindow>();
            for (int i = listViews.Length-1;i >= 0;i--)
            {
                listViews[i].Release();
            }
        }

    }

    namespace Base
    {
        public enum CommandType
        {
            None = 0,
            SceneChange,
            MapChange,
            MapClear,
            CreateMapObject,
            CallConfirmView,
            CallSkillDetailView,
            CallCautionView,
            CallPopupView,
            ClosePopup,
            ClosePopupAll,
            CloseConfirm,
            CallOptionView,
            CallSideMenu,
            CallRankingView,
            CallHelpView,
            CallSlotSaveView,
            CallStatusView,
            CloseStatus,
            CallAdvScene,
            CallEnemyInfoView,
            CallTacticsStatusView,
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
            SceneShowUI,
            SceneHideUI,
        }
    }
}