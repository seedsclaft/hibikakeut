using System.Collections;
using System.Collections.Generic;
using Ryneus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Ariadne
{
    /// <Summary>
    /// Controller class of player movement.
    /// </Summary>
    public class MoveController : AriadneSystemBase, IEnterDungeon, IEventProcessor, IPostMoveProcessNotify
    {
        FloorMapMasterData floorMapData;
        GameObject gameController;
        DungeonMasterData dungeonData;
        DungeonBasePartsData dungeonBasePartsData;
        List<DungeonPartsData> dungeonPartsDataList;
        List<MapAttributeData> mapAttributeDataList;
        GameObject postMoveEventObj;

        GameObject player;
        bool canMove = false;
        public float moveWait = 0.5f;
        public float startmoveWait = 0.1f;

        // Button ID
        const int TurnLeft = 0;
        const int TurnRight = 1;
        const int TurnBack = 2;
        const int MoveFront = 3;

        bool isPressedTurnLeft;
        bool isPressedTurnRight;
        bool isPressedTurnBack;
        bool isPressedMoveFront;

        [SerializeField]
        bool useUGUIButton = true;
        [SerializeField]
        GameObject okButtonParent;
        [SerializeField]
        GameObject arrowButtonParent;

        [SerializeField]
        Image screenMaskImage;
        float screenFadeTime = 0.5f;

        [SerializeField]
        Image mapMaskImage;
        [SerializeField]
        float mapFadeTime = 0.5f;

        [SerializeField]
        GameObject keyWaitWindow;
        [SerializeField]
        float keyWaitFadeTime = 0.1f;
        Image keyWaitBg;
        Text keyWaitText;

        Vector3 unitSize;

        [SerializeField]
        List<GameObject> mapParts;
        bool isExecutingEvent = false;
        bool isEventReady = false;

        public bool isInDungeon = false;
        DungeonMasterData moveDestDungeon;

        bool hasPostCheck = false;
        bool didChangeFloor = false;

        Queue<Vector2Int> enterFloorEventPosQueue = new Queue<Vector2Int>();

        [SerializeField]
        GameObject dungeonUI = null;

        void Start()
        {
            
        }

        /// <Summary>
        /// Initialize state of dungeon.
        /// </Summary>
        public virtual void SetUpMoveController()
        {
            // Get settings from dungeon manager.
            GetSettings();

            // Check if DungeonPartsDataList is not null.
            if (dungeonPartsDataList == null)
            {
                return;
            }
            
            // Set references of components.
            SetRef();

            // Set unit size of dungeon.
            SetUnitSize();

            // Set player init position.
            SetInitPos();

            // Set traverse data of init position.
            SetTraverse();

            // Set reference of map objects list.
            SetMapObjList();

            // Set state of UGUI buttons.
            SetUGUIButtons();

            // Check event to execute on entering the floor.
            CheckEventsOnEnteringFloor();

            // Show UGUI map.
            FadeInMap();

            // Refrect traversed data of init position to map.
            SendSetDirtyMsg();
            startmoveWait = moveWait;
        }

        /// <Summary>
        /// Set the object to notify the post move event. 
        /// </Summary>
        /// <param name="obj">Target position.</param>
        public virtual void SetPostMoveEventObject(GameObject obj)
        {
            postMoveEventObj = obj;
        }

        /// <Summary>
        /// Get settings from DungeonSettings component.
        /// </Summary>
        protected virtual void GetSettings()
        {
            gameController = GameObject.FindGameObjectWithTag(AriadneSceneObjectTag.GameController);
            DungeonSettings ds = gameController.GetComponent<DungeonSettings>();
            dungeonData = ds.dungeonData;
            floorMapData = ds.GetCurrentFloorData();

            dungeonBasePartsData = floorMapData.dungeonBasePartsData;
            dungeonPartsDataList = ds.GetDungeonPartsDataList();
            if (dungeonPartsDataList == null)
            {
                Debug.LogError("DungeonPartsData is missing. Check the DungeonPartsData setting in your FloorData.");
            }

            mapAttributeDataList = ds.GetMapAttributeList();

            player = GameObject.FindGameObjectWithTag(AriadneSceneObjectTag.Player);
        }

        /// <Summary>
        /// Set object references.
        /// </Summary>
        protected virtual void SetRef()
        {
            CheckEventProcessorReference();
            CheckEventDataHolderReference();
            CheckKeyWaitReferece();
        }

        /// <Summary>
        /// Check a references for key wait objects.
        /// </Summary>
        protected virtual void CheckKeyWaitReferece()
        {
            // Key wait text
            if (keyWaitBg == null)
            {
                keyWaitBg = keyWaitWindow.GetComponent<Image>();
            }

            if (keyWaitText == null)
            {
                keyWaitText = keyWaitWindow.GetComponentInChildren<Text>();
            }
        }

        /// <Summary>
        /// Get unit size of each grid in the dungeon.
        /// </Summary>
        protected virtual void SetUnitSize()
        {
            GameObject sizePrefab = dungeonBasePartsData.sizeBaseObj;
            unitSize = new Vector3(sizePrefab.transform.localScale.x, sizePrefab.transform.localScale.y, sizePrefab.transform.localScale.z);
        }

        /// <Summary>
        /// Set the initial position of the player.
        /// </Summary>
        protected virtual void SetInitPos()
        {
            Vector2Int initPos = Vector2Int.zero;
            DungeonDir initDir = DungeonDir.North;

            if (floorMapData != null)
            {
                initPos = floorMapData.entrancePos;
                initDir = floorMapData.enteringDir;
            }

            PlayerPosition.Instance.playerPos = initPos;
            PlayerPosition.Instance.direction = initDir;
            PlayerPosition.Instance.currentFloorId = floorMapData.floorId;
            PlayerPosition.Instance.currentDungeonId = dungeonData.dungeonId;

            float targetAngle = CurrentDirAngle();
            player.transform.eulerAngles = new Vector3(0, targetAngle, 0);
            SetCameraPos();
        }

        /// <Summary>
        /// Set traverse data of the floor.
        /// </Summary>
        protected virtual void SetTraverse()
        {
            TraverseManager.Instance.AddDungeonTraverseData(dungeonData.dungeonId, floorMapData.floorId, floorMapData);
            PlayerPosition.Instance.SetTraverseData();
        }

        /// <Summary>
        /// Set the position of the player camera.
        /// </Summary>
        protected virtual void SetCameraPos()
        {
            Vector3 currentPos = Vector3.zero;
            currentPos.x += PlayerPosition.Instance.playerPos.x * unitSize.x;
            currentPos.y = player.transform.position.y;
            currentPos.z += PlayerPosition.Instance.playerPos.y * unitSize.z;
            Vector3 targetPos = currentPos;
            player.transform.position = targetPos;
        }

        /// <Summary>
        /// Set the list of map objects to send SetDirty message.
        /// </Summary>
        protected virtual void SetMapObjList()
        {
            /*
            GameObject mapBackground = GameObject.Find(AriadneMapPartsName.MapBackground);
            GameObject mapBase = GameObject.Find(AriadneMapPartsName.MapBase);
            GameObject mapHall = GameObject.Find(AriadneMapPartsName.MapHall);
            GameObject mapIcon = GameObject.Find(AriadneMapPartsName.MapIcon);
            GameObject mapGrid = GameObject.Find(AriadneMapPartsName.MapGrid);
            mapParts = new List<GameObject>(){mapBackground, mapBase, mapHall, mapIcon, mapGrid};
            */
        }

        /// <Summary>
        /// Set UGUI buttons.
        /// </Summary>
        protected virtual void SetUGUIButtons()
        {
            if (useUGUIButton)
            {
                okButtonParent.SetActive(true);
                arrowButtonParent.SetActive(true);
            }
            else
            {
                okButtonParent.SetActive(false);
                arrowButtonParent.SetActive(false);
            }
        }

        /// <Summary>
        /// Set the state of the parent of UGUI arrow buttons.
        /// </Summary>
        protected virtual void SetArrowButtonState(bool isActive)
        {
            if (useUGUIButton)
            {
                arrowButtonParent.SetActive(isActive);
            }
            else
            {
                arrowButtonParent.SetActive(false);
            }
        }

        /// <Summary>
        /// Fade in the uGUI map image.
        /// </Summary>
        protected virtual void FadeInMap()
        {
            isInDungeon = true;
            StartCoroutine(AriadneFadeManager.FadeOutImage(mapMaskImage, mapFadeTime));
        }
        
        protected virtual void Update()
        {
            if (!isInDungeon)
            {
                return;
            }

            if (canMove)
            {
                PlayerMove();
            }
        }

        /// <Summary>
        /// Check key inputs about movement.
        /// </Summary>
        protected virtual void PlayerMove()
        {
            var keyCurrent = Keyboard.current;
            if (isEventReady)
            {
                if (keyCurrent.spaceKey.wasPressedThisFrame || keyCurrent.upArrowKey.wasPressedThisFrame)
                {
                    canMove = false;
                    OnEventKeyPressed();
                    return;
                }
            }

            if (keyCurrent.upArrowKey.wasPressedThisFrame || isPressedMoveFront)
            {
                MoveFrontProcess();
                return;
            }

            if (keyCurrent.leftShiftKey.wasPressedThisFrame || keyCurrent.rightShiftKey.wasPressedThisFrame)
            {
                if (keyCurrent.leftArrowKey.wasPressedThisFrame || isPressedTurnLeft)
                {
                    Direction dir = new Direction();
                    DungeonDir targetDir = dir.GetCounterclockwiseDir(PlayerPosition.Instance.direction);
                    MoveToTargetDirectionProcess(targetDir);
                    return;
                }

                if (keyCurrent.rightArrowKey.wasPressedThisFrame || isPressedTurnRight)
                {
                    Direction dir = new Direction();
                    DungeonDir targetDir = dir.GetClockwiseDir(PlayerPosition.Instance.direction);
                    MoveToTargetDirectionProcess(targetDir);
                    return;
                }

                if (keyCurrent.downArrowKey.wasPressedThisFrame || isPressedTurnBack)
                {
                    Direction dir = new Direction();
                    DungeonDir targetDir = dir.GetReverseDir(PlayerPosition.Instance.direction);
                    MoveToTargetDirectionProcess(targetDir);
                    return;
                }
            }
            else
            {
                if (keyCurrent.leftArrowKey.wasPressedThisFrame || isPressedTurnLeft)
                {
                    TurnProcess(TurnLeft);
                    return;
                }

                if (keyCurrent.rightArrowKey.wasPressedThisFrame || isPressedTurnRight)
                {
                    TurnProcess(TurnRight);
                    return;
                }

                if (keyCurrent.downArrowKey.wasPressedThisFrame || isPressedTurnBack)
                {
                    TurnProcess(TurnBack);
                    return;
                }
            }
/* 最上に移動
            if (isEventReady)
            {
                if (Input.GetKeyUp(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow))
                {
                    canMove = false;
                    OnEventKeyPressed();
                }
            }
*/
        }

        /// <Summary>
        /// Action of ok button.
        /// This method is used in mobile devices.
        /// </Summary>
        public virtual void OnPressedOkButton()
        {
            if (!isInDungeon)
            {
                return;
            }

            if (!canMove)
            {
                return;
            }

            if (isEventReady)
            {
                canMove = false;
                OnEventKeyPressed();
            }
        }

        /// <Summary>
        /// Action on button pressed.
        /// This method is used for UGUI button.
        /// </Summary>
        public virtual void OnPressedButton(int buttonId)
        {
            switch (buttonId)
            {
                case TurnLeft:
                    isPressedTurnLeft = true;
                    break;
                case TurnRight:
                    isPressedTurnRight = true;
                    break;
                case TurnBack:
                    isPressedTurnBack = true;
                    break;
                case MoveFront:
                    isPressedMoveFront = true;
                    break;
            }
        }

        /// <Summary>
        /// Action on button released.
        /// This method is used for UGUI button.
        /// </Summary>
        public virtual void OnReleasedButton(int buttonId)
        {
            switch (buttonId)
            {
                case TurnLeft:
                    isPressedTurnLeft = false;
                    break;
                case TurnRight:
                    isPressedTurnRight = false;
                    break;
                case TurnBack:
                    isPressedTurnBack = false;
                    break;
                case MoveFront:
                    isPressedMoveFront = false;
                    break;
            }
        }

        /// <Summary>
        /// Initialize states of uGUI buttuns.
        /// </Summary>
        protected virtual void InitializeButtonPressedState()
        {
            isPressedTurnLeft = false;
            isPressedTurnRight = false;
            isPressedTurnBack = false;
            isPressedMoveFront = false;
        }

        /// <Summary>
        /// Move forward process.
        /// </Summary>
        public virtual void MoveFrontProcess()
        {
            PreMove();

            // Check forward wall in current position
            Vector2Int currentPos = PlayerPosition.Instance.playerPos;
            Vector2Int positionByDirection = PlayerPosition.Instance.GetPositionByDirection(currentPos,PlayerPosition.Instance.direction);
            int mapAttrId = PlayerPosition.Instance.GetMapInfoByDirection(floorMapData, currentPos, PlayerPosition.Instance.direction);
            if (!CheckCanMove(mapAttrId) && !CheckCanMoveEvent(positionByDirection))
            {
                canMove = true;
                if (!isExecutingEvent)
                {
                    PostMove();
                }
                return;
            }

            Vector2Int ps = PlayerPosition.Instance.GetForwardPosition(1);
            if (CheckPositionIsValid(ps))
            {
                StartCoroutine(MoveForward(1));
                PlayerPosition.Instance.SetTraverseData();
                SendSetDirtyMsg();
            }
        }

        /// <Summary>
        /// Move the character to the specified direction process.
        /// </Summary>
        public virtual void MoveToTargetDirectionProcess(DungeonDir targetDirection)
        {
            PreMove();

            // Check a wall of the specified direction from current position
            Vector2Int currentPos = PlayerPosition.Instance.playerPos;
            int mapAttrId = PlayerPosition.Instance.GetMapInfoByDirection(floorMapData, currentPos, targetDirection);
            if (!CheckCanMove(mapAttrId))
            {
                canMove = true;
                if (!isExecutingEvent)
                {
                    PostMove();
                }
                return;
            }

            Vector2Int ps = PlayerPosition.Instance.GetSpecifiedDirPosition(targetDirection, 1);
            if (CheckPositionIsValid(ps))
            {
                StartCoroutine(MoveToTargetDirection(targetDirection, 1));
                PlayerPosition.Instance.SetTraverseData();
                SendSetDirtyMsg();
            }
        }

        /// <Summary>
        /// Turn process of the player.
        /// </Summary>
        /// <param name="turnDirection">Direction of turning.</param>
        public virtual void TurnProcess(int turnDirection)
        {
            PreMove();
            StartCoroutine(TurnCamera(turnDirection));
        }

        /// <Summary>
        /// Returns the event data of target position.
        /// </Summary>
        /// <param name="targetPos">Target position.</param>
        protected virtual EventMasterData GetEventData(Vector2Int targetPos)
        {
            EventMasterData eventData = null;
            if (!CheckPositionIsValid(targetPos))
            {
                return eventData;
            }

            int index = targetPos.x + targetPos.y * floorMapData.floorSizeHorizontal;
            int targetPosEventId = floorMapData.mapInfo[index].eventId;
            if (targetPosEventId > 0)
            {
                eventData = eventDataHolder.GetEventDataById(targetPosEventId);
            }
            return eventData;
        }

        /// <Summary>
        /// Returns the flag that the event is executed.
        /// This method is called after the player has moved.
        /// </Summary>
        /// <param name="eventData">Event data of the position.</param>
        /// <param name="targetPos">Target position.</param>
        /// <param name="eventStartPos">Starting position of the event defined in the event parts.</param>
        protected virtual bool ExecuteEventOnPostMove(EventMasterData eventData, Vector2Int targetPos, AriadneEventPosition eventStartPos)
        {
            bool isEventExecute = false;
            if (eventData == null)
            {
                return isEventExecute;
            }

            AriadneEventParts parts = eventProcessor.GetExecutionEventParts(eventData);
            if (parts == null)
            {
                return isEventExecute;
            }

            if (parts.eventPos != eventStartPos)
            {
                return isEventExecute;
            }

            if (!CheckDirectionCondition(parts))
            {
                return isEventExecute;
            }

            if (parts.eventTrigger == AriadneEventTrigger.Auto)
            {
                // Execute event immediately.
                isEventExecute = true;
                SetEventTraverse(targetPos);
                this.isExecutingEvent = true;
                InitializeButtonPressedState();
                SetArrowButtonState(false);
                eventProcessor.EventExecuter(parts, targetPos);
            }
            else if (parts.eventTrigger == AriadneEventTrigger.KeyPress)
            {
                // Set key wait flag.
                this.isEventReady = true;
                StartCoroutine(ShowKeyWaitWindow());
            }
            return isEventExecute;
        }

        /// <Summary>
        /// Returns the flag that the event is executed.
        /// This method is called when the key is pressed.
        /// </Summary>
        /// <param name="eventData">Event data of the position.</param>
        /// <param name="targetPos">Target position.</param>
        /// <param name="eventStartPos">Starting position of the event defined in the event parts.</param>
        protected virtual bool ExecuteEventOnKeyPressed(EventMasterData eventData, Vector2Int targetPos, AriadneEventPosition eventStartPos)
        {
            bool isEventExecute = false;

            if (eventData == null)
            {
                return isEventExecute;
            }

            AriadneEventParts parts = eventProcessor.GetExecutionEventParts(eventData);
            if (parts == null)
            {
                return isEventExecute;
            }

            if (parts.eventPos != eventStartPos)
            {
                return isEventExecute;
            }

            if (!CheckDirectionCondition(parts))
            {
                return isEventExecute;
            }

            if (parts.eventTrigger == AriadneEventTrigger.KeyPress)
            {
                // Execute event.
                isEventExecute = true;
                SetEventTraverse(targetPos);
                this.isExecutingEvent = true;
                SetArrowButtonState(false);
                eventProcessor.EventExecuter(parts, targetPos);
            }
            return isEventExecute;
        }

        /// <Summary>
        /// Check direction conditions in the event parts data.
        /// If the event parts does not have direction codition, this method returns true.
        /// </Summary>
        /// <param name="parts">Event parts data.</param>
        protected virtual bool CheckDirectionCondition(AriadneEventParts parts)
        {
            if (!parts.hasDirectionCondition)
            {
                return true;
            }

            bool matched = false;
            foreach (DungeonDir dir in parts.directionConditions)
            {
                if (dir == PlayerPosition.Instance.direction)
                {
                    matched = true;
                    break;
                }
            }

            return matched;
        }

        /// <Summary>
        /// Execute event when a key is pressed.
        /// </Summary>
        protected virtual void OnEventKeyPressed()
        {
            StartCoroutine(HideKeyWaitWindow());

            // Check an event ID of target position.
            Vector2Int ps = PlayerPosition.Instance.playerPos;
            EventMasterData eventData = GetEventData(ps);
            bool isExecuted = ExecuteEventOnKeyPressed(eventData, ps, AriadneEventPosition.ThisPosition);

            // Check an event ID of forward position
            bool isForwardEventExecuted = false;
            if (!isExecuted)
            {
                ps = PlayerPosition.Instance.GetForwardPosition(1);
                EventMasterData eventDataForward = GetEventData(ps);
                isForwardEventExecuted = ExecuteEventOnKeyPressed(eventDataForward, ps, AriadneEventPosition.Around);
            }

            // When any event has not been executed, set true to the move flag.
            if (!isExecuted && !isForwardEventExecuted)
            {
                canMove = true;
            }
        }

        /// <Summary>
        /// Set traverse data of the event position.
        /// </Summary>
        /// <param name="eventPos">Position of the event.</param>
        protected virtual void SetEventTraverse(Vector2Int eventPos)
        {
            // This position is validated in GetEventData method.
            TraverseManager.Instance.SetTraverseData(PlayerPosition.Instance.currentDungeonId, PlayerPosition.Instance.currentFloorId, eventPos, true);
            SendSetDirtyMsgImmediately();
        }

        /// <Summary>
        /// Pre process of the player moving.
        /// </Summary>
        protected virtual void PreMove()
        {
            // Set move flag to false.
            canMove = false;

            // Fade out key wait window.
            StartCoroutine(HideKeyWaitWindow());
        }

        /// <Summary>
        /// Post process of the player moving.
        /// </Summary>
        protected virtual void PostMove()
        {
            Debug.Log("PostMove");
            isExecutingEvent = false;
            isEventReady = false;

            SetArrowButtonState(true);

            // Check an event ID of target position.
            Vector2Int ps = PlayerPosition.Instance.playerPos;
            EventMasterData eventData = GetEventData(ps);
            bool isExecuted = ExecuteEventOnPostMove(eventData, ps, AriadneEventPosition.ThisPosition);

            // Check an event ID of forward position
            bool isForwardEventExecuted = false;
            if (!isExecuted)
            {
                ps = PlayerPosition.Instance.GetForwardPosition(1);
                EventMasterData eventDataForward = GetEventData(ps);
                isForwardEventExecuted = ExecuteEventOnPostMove(eventDataForward, ps, AriadneEventPosition.Around);
            }
            
            // When no event has been executed, set true to the move flag.
            if (!isExecuted && !isForwardEventExecuted)
            {
                NotifyPostMoveEvent(postMoveEventObj);
            }
        }

        /// <Summary>
        /// Initialize flags of events.
        /// </Summary>
        protected virtual void InitializeEventFlags()
        {
            isExecutingEvent = false;
            isEventReady = false;
            hasPostCheck = false;
        }

        /// <Summary>
        /// Check the post process of after moving.
        /// </Summary>
        public virtual void OnFinishedPostMoveEvent()
        {
            Debug.Log("OnFinishedPostMoveEvent");
            ReadyToMove();
        }

        /// <Summary>
        /// Ready the state for moving.
        /// </Summary>
        protected virtual void ReadyToMove()
        {
            canMove = true;
            AriadneFadeManager.InitializeWaitFlags();
            SetArrowButtonState(true);
        }

        /// <Summary>
        /// Move process for the event.
        /// </Summary>
        public virtual IEnumerator MoveAStep()
        {
            // Move a step
            StartCoroutine(MoveForward(1));
            PlayerPosition.Instance.SetTraverseData();
            SendSetDirtyMsg();
            yield return new WaitForSeconds(moveWait);
        }

        /// <Summary>
        /// Set a flag for checking after event process. 
        /// </Summary>
        public virtual void SetCheckFlagAfterEvent(bool checkEvent)
        {
            hasPostCheck = checkEvent;
        }

        /// <Summary>
        /// Check event to execute on entering the floor.
        /// </Summary>
        protected virtual void CheckEventsOnEnteringFloor()
        {
            enterFloorEventPosQueue = new Queue<Vector2Int>();

            for (int i = 0; i < floorMapData.mapInfo.Count; i++)
            {
                int eventId = floorMapData.mapInfo[i].eventId;
                if (eventId == 0)
                {
                    continue;
                }

                EventMasterData data = eventDataHolder.GetEventDataById(eventId);
                if (data == null)
                {
                    continue;
                }

                List<AriadneEventParts> partsList = data.eventParts.FindAll(parts => parts.eventTrigger == AriadneEventTrigger.OnEnterFloor);
                if (partsList.Count > 0)
                {
                    int xPos = i % floorMapData.floorSizeHorizontal;
                    int yPos = i / floorMapData.floorSizeHorizontal;
                    Vector2Int eventPos = new Vector2Int(xPos, yPos);
                    enterFloorEventPosQueue.Enqueue(eventPos);
                }
            }
        }

        /// <Summary>
        /// Execute events on entering the floor.
        /// </Summary>
        public virtual void ExecuteEventsOnEnteringFloor()
        {
            if (enterFloorEventPosQueue.Count == 0)
            {
                PostEvent();
                return;
            }

            CheckEventInQueue();
        }

        /// <Summary>
        /// Execute an event in the queue.
        /// </Summary>
        protected virtual void CheckEventInQueue()
        {
            isExecutingEvent = false;

            SetArrowButtonState(true);

            // Check an event ID of target position.
            Vector2Int eventPos = enterFloorEventPosQueue.Dequeue();
            EventMasterData eventData = GetEventData(eventPos);
            bool isExecuted = ExecuteEventInQueue(eventData, eventPos);
            
            // When no event has been executed, check the next entity in the queue.
            if (!isExecuted)
            {
                ExecuteEventsOnEnteringFloor();
            }
        }

        /// <Summary>
        /// Returns the flag that the event is executed.
        /// This method is called on entering floor.
        /// </Summary>
        /// <param name="eventData">Event data of the position.</param>
        /// <param name="targetPos">Target position.</param>
        protected virtual bool ExecuteEventInQueue(EventMasterData eventData, Vector2Int targetPos)
        {
            bool isEventExecute = false;
            if (eventData == null)
            {
                return isEventExecute;
            }

            AriadneEventParts parts = eventProcessor.GetExecutionEventPartsOnEnteringFloor(eventData);
            if (parts == null)
            {
                return isEventExecute;
            }

            if (parts.eventTrigger == AriadneEventTrigger.OnEnterFloor)
            {
                // Execute event immediately.
                isEventExecute = true;
                this.isExecutingEvent = true;
                InitializeButtonPressedState();
                SetArrowButtonState(false);
                eventProcessor.EventExecuter(parts, targetPos);
            }
            return isEventExecute;
        }

        /// <Summary>
        /// Receiver of OnPostMove message from IEventProcessor.
        /// </Summary>
        public virtual void OnPostMove()
        {
            PostMove();
        }

        /// <Summary>
        /// Receiver of OnMovePosition message from IEventProcessor.
        /// </Summary>
        /// <param name="eventParts">Event parts data.</param>
        public virtual void OnMovePosition(DungeonMasterData destDungeon, FloorMapMasterData destFloor, Vector2Int destPos, DungeonDir destDirection, bool redrawDungeon, GameObject eventCallbackObj)
        {
            StartCoroutine(EventMovePosition(destDungeon, destFloor, destPos, destDirection, redrawDungeon, eventCallbackObj));
        }

        /// <Summary>
        /// Move position event process.
        /// </Summary>
        /// <param name="eventParts">Event parts data.</param>
        protected virtual IEnumerator EventMovePosition(DungeonMasterData destDungeon, FloorMapMasterData destFloor, Vector2Int destPos, DungeonDir destDirection, bool redrawDungeon, GameObject eventCallbackObj)
        {
            // Fade out screen
            yield return StartCoroutine(AriadneFadeManager.FadeInImage(screenMaskImage, screenFadeTime));

            // Send dungeon data to DungeonSetting.
            moveDestDungeon = destDungeon;
            SendDungeonData(gameController);

            int preDungeonId = PlayerPosition.Instance.currentDungeonId;
            int preFloorId = PlayerPosition.Instance.currentFloorId;

            PlayerPosition.Instance.currentDungeonId = destDungeon.dungeonId;
            PlayerPosition.Instance.currentFloorId = destFloor.floorId;
            PlayerPosition.Instance.playerPos = destPos;
            PlayerPosition.Instance.direction = destDirection;

            if (preDungeonId == PlayerPosition.Instance.currentDungeonId && preFloorId == PlayerPosition.Instance.currentFloorId)
            {
                didChangeFloor = false;
            }
            else
            {
                didChangeFloor = true;
            }

            // Get new floor data
            DungeonSettings ds = gameController.GetComponent<DungeonSettings>();
            dungeonData = ds.dungeonData;
            floorMapData = ds.GetCurrentFloorData();

            // Add traverse data
            TraverseManager.Instance.AddDungeonTraverseData(PlayerPosition.Instance.currentDungeonId, PlayerPosition.Instance.currentFloorId, floorMapData);
            PlayerPosition.Instance.SetTraverseData();
            yield return null;

            // Remove dungeon walls and redraw dungeon
            if (redrawDungeon)
            {
                SendRedrawMessage(gameController);
            }

            // Move camera
            SetCameraPos();
            float targetAngle = CurrentDirAngle();
            player.transform.eulerAngles = new Vector3(0, targetAngle, 0);
            SendSetNewMap();
            SendSetDirtyMsg();

            yield return null;

            // Fade in
            StartCoroutine(AriadneFadeManager.FadeOutImage(screenMaskImage, screenFadeTime));
            yield return null;

            SendMoveEndMessage(eventCallbackObj);
        }

        /// <Summary>
        /// Notify the end of moving position.
        /// </Summary>
        protected virtual void SendMoveEndMessage(GameObject obj)
        {
            ExecuteEvents.Execute<IMoveNotify>(
                    target: obj,
                    eventData: null,
                    functor: CallMovingFinish
            );
        }

        /// <Summary>
        /// The functor of SendMoveEndMessage method.
        /// </Summary>
        protected virtual void CallMovingFinish(IMoveNotify inf, BaseEventData eventData)
        {
            inf.OnFinishedMove();
        }

        /// <Summary>
        /// Receiver of OnExitDungeon message from IEventProcessor.
        /// </Summary>
        public virtual void OnExitDungeon(GameObject notifyObj)
        {
            StartCoroutine(EventExit(notifyObj));
        }

        /// <Summary>
        /// Exiting the dungeon process.
        /// </Summary>
        protected virtual IEnumerator EventExit(GameObject notifyObj)
        {
            // Send hiding map messages.
            isInDungeon = false;
            StartCoroutine(AriadneFadeManager.FadeInImage(mapMaskImage, mapFadeTime));

            // Fade out screen
            yield return StartCoroutine(AriadneFadeManager.FadeInImage(screenMaskImage, screenFadeTime));

            // Remove dungeon objects
            SendRemoveMessage(gameController);
            yield return null;
            
            // Exit dungeon
            SendExitDungeonMessage(notifyObj);

            PostEvent();
            canMove = false;

            if (useUGUIButton)
            {
                okButtonParent.SetActive(false);
                arrowButtonParent.SetActive(false);
            }
        }

        /// <Summary>
        /// Receiver of OnEnterDungeon message from IEnterDungeon.
        /// </Summary>
        public virtual void OnEnterDungeon()
        {
            SetUpMoveController();
            SendDrawMessage(gameController);
            SendSetNewMap();
            //StartCoroutine(DelayFadeIn());
            ExecuteEventsOnEnteringFloor();
        }

        /// <Summary>
        /// Post process of event execution.
        /// </Summary>
        protected virtual void PostEvent()
        {
            Debug.Log("PostEvent");
            InitializeEventFlags();
            NotifyPostMoveEvent(postMoveEventObj);
            ReadyToMove();
        }

        /// <Summary>
        /// Receiver of OnFinishedEvent message from IEventProcessor.
        /// </Summary>
        public virtual void OnFinishedEvent()
        {
            if (didChangeFloor)
            {
                didChangeFloor = false;
                CheckEventsOnEnteringFloor();
            }

            // Check the queue which holds events on entering the floor.
            if (enterFloorEventPosQueue.Count > 0)
            {
                ExecuteEventsOnEnteringFloor();
                return;
            }

            if (hasPostCheck)
            {
                hasPostCheck = false;
                PostMove();
            }
            else
            {
                PostEvent();
            }
        }

        /// <Summary>
        /// Moving forward process.
        /// </Summary>
        /// <param name="steps">Number of steps.</param>
        protected virtual IEnumerator MoveForward(int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                Vector3 currentPos = Vector3.zero;
                currentPos.x += PlayerPosition.Instance.playerPos.x * unitSize.x;
                currentPos.y = player.transform.position.y;
                currentPos.z += PlayerPosition.Instance.playerPos.y * unitSize.z;
                PlayerPosition.Instance.MoveForward();
                Vector3 targetPos = currentPos;
                switch (PlayerPosition.Instance.direction)
                {
                    case DungeonDir.North:
                        targetPos = new Vector3(currentPos.x, currentPos.y, currentPos.z + unitSize.z);
                        break;
                    case DungeonDir.East:
                        targetPos = new Vector3(currentPos.x + unitSize.x, currentPos.y, currentPos.z);
                        break;
                    case DungeonDir.South:
                        targetPos = new Vector3(currentPos.x, currentPos.y, currentPos.z - unitSize.z);
                        break;
                    case DungeonDir.West:
                        targetPos = new Vector3(currentPos.x - unitSize.x, currentPos.y, currentPos.z);
                        break;
                }

                float finishTime = Time.time + moveWait;
                while (true)
                {
                    float diff = finishTime - Time.time;
                    if (diff <= 0)
                    {
                        break;
                    }

                    float rate = 1 - Mathf.Clamp01(diff / moveWait);
                    player.transform.position = Vector3.Lerp(currentPos, targetPos, rate);
                    yield return null;
                }
                player.transform.position = targetPos;
            }
            
            if (!isExecutingEvent)
            {
                PostMove();
            }
        }

        /// <Summary>
        /// Moving process to the target direction.
        /// </Summary>
        /// <param name="targetDir">Target direction.</param>
        /// <param name="steps">Number of steps.</param>
        protected virtual IEnumerator MoveToTargetDirection(DungeonDir targetDir, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                Vector3 currentPos = Vector3.zero;
                currentPos.x += PlayerPosition.Instance.playerPos.x * unitSize.x;
                currentPos.y = player.transform.position.y;
                currentPos.z += PlayerPosition.Instance.playerPos.y * unitSize.z;
                PlayerPosition.Instance.MoveToTargetDirection(targetDir);
                Vector3 targetPos = currentPos;
                switch (targetDir)
                {
                    case DungeonDir.North:
                        targetPos = new Vector3(currentPos.x, currentPos.y, currentPos.z + unitSize.z);
                        break;
                    case DungeonDir.East:
                        targetPos = new Vector3(currentPos.x + unitSize.x, currentPos.y, currentPos.z);
                        break;
                    case DungeonDir.South:
                        targetPos = new Vector3(currentPos.x, currentPos.y, currentPos.z - unitSize.z);
                        break;
                    case DungeonDir.West:
                        targetPos = new Vector3(currentPos.x - unitSize.x, currentPos.y, currentPos.z);
                        break;
                }

                float finishTime = Time.time + moveWait;
                while (true)
                {
                    float diff = finishTime - Time.time;
                    if (diff <= 0)
                    {
                        break;
                    }

                    float rate = 1 - Mathf.Clamp01(diff / moveWait);
                    player.transform.position = Vector3.Lerp(currentPos, targetPos, rate);
                    yield return null;
                }
                player.transform.position = targetPos;
            }
            
            if (!isExecutingEvent)
            {
                PostMove();
            }
        }

        /// <Summary>
        /// Turn the player camera process.
        /// </Summary>
        /// <param name="turnDir">Direction to turn.</param>
        protected virtual IEnumerator TurnCamera(int turnDir)
        {
            float currentAngle = CurrentDirAngle();
            float targetAngle = 0f;
            switch (turnDir)
            {
                case TurnLeft:
                    PlayerPosition.Instance.TurnLeft();
                    targetAngle = CurrentDirAngle();
                    break;
                case TurnRight:
                    PlayerPosition.Instance.TurnRight();
                    targetAngle = CurrentDirAngle();
                    break;
                case TurnBack:
                    PlayerPosition.Instance.TurnBack();
                    targetAngle = CurrentDirAngle();
                    break;
            }

            float finishTime = Time.time + moveWait;
            while (true)
            {
                float diff = finishTime - Time.time;
                if (diff <= 0)
                {
                    break;
                }

                float rate = 1 - Mathf.Clamp01(diff / moveWait);
                
                float angle = Mathf.LerpAngle(currentAngle, targetAngle, rate);
                player.transform.eulerAngles = new Vector3(0, angle, 0);
                yield return null;
            }
            player.transform.eulerAngles = new Vector3(0, targetAngle, 0);
            PostMove();
        }

        /// <Summary>
        /// Returns the angle that corresponds to player direction.
        /// </Summary>
        protected virtual float CurrentDirAngle()
        {
            float angle = 0f;
            switch (PlayerPosition.Instance.direction)
            {
                case DungeonDir.North:
                    angle = 0f;
                    break;
                case DungeonDir.East:
                    angle = 90f;
                    break;
                case DungeonDir.South:
                    angle = 180f;
                    break;
                case DungeonDir.West:
                    angle = 270f;
                    break;
            }
            return angle;
        }

        /// <Summary>
        /// Check if the position is valid.
        /// </Summary>
        /// <param name="position">Position to check.</param>
        protected virtual bool CheckPositionIsValid(Vector2Int position)
        {
            bool isValid = true;

            // Check x axis position
            if (position.x < 0 || position.x >= floorMapData.floorSizeHorizontal)
            {
                isValid = false;
            }

            // Check y axis position
            if (position.y < 0 || position.y >= floorMapData.floorSizeVertical)
            {
                isValid = false;
            }

            return isValid;
        }

        /// <Summary>
        /// Check if that player can move.
        /// </Summary>
        protected virtual bool CheckCanMove(int mapAttrId)
        {
            bool isPass = true;

            if (mapAttributeDataList == null)
            {
                return isPass;
            }

            // Get MapAttributeRecord.
            MapAttributeRecord record = DataRecordUtil.GetMapAttributeRecordById(mapAttributeDataList, mapAttrId);
            if (record == null)
            {
                return isPass;
            }

            isPass = record.canWalk;
            return isPass;
        }

        /// <summary>
        /// イベント消去済みなら通過可能
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool CheckCanMoveEvent(Vector2Int position)
        {
            GameObject eventObj = GetDrawDungeonWall()?.GetWallObjectByAxis(position.x, position.y);
            if (eventObj != null)
            {
                return !eventObj.activeSelf;
            }
            return false;
        }

        /// <Summary>
        /// Call fade in with delay time.
        /// </Summary>
        public virtual IEnumerator DelayFadeIn()
        {
            yield return StartCoroutine(AriadneFadeManager.FadeOutImage(screenMaskImage, screenFadeTime));
        }

        /// <Summary>
        /// Show the key wait window image and text.
        /// </Summary>
        protected virtual IEnumerator ShowKeyWaitWindow()
        {
            StartCoroutine(AriadneFadeManager.FadeInImage(keyWaitBg, keyWaitFadeTime));
            StartCoroutine(AriadneFadeManager.FadeInText(keyWaitText, keyWaitFadeTime));
            yield return new WaitForSeconds(keyWaitFadeTime);
        }

        /// <Summary>
        /// Hide the key wait window image and text.
        /// </Summary>
        protected virtual IEnumerator HideKeyWaitWindow()
        {
            StartCoroutine(AriadneFadeManager.FadeOutImage(keyWaitBg, keyWaitFadeTime));
            StartCoroutine(AriadneFadeManager.FadeOutText(keyWaitText, keyWaitFadeTime));
            yield return new WaitForSeconds(keyWaitFadeTime);
        }

        /// <Summary>
        /// Send SetDirty message to DrawMap components.
        /// </Summary>
        protected virtual void SendSetDirtyMsg()
        {
            foreach (GameObject obj in mapParts)
            {
                ExecuteEvents.Execute<IDirtyMarkerMap>(
                    target: obj,
                    eventData: null,
                    functor: CallSetDirty
                );
            }
        }

        /// <Summary>
        /// The functor of SendSetDirtyMsg method.
        /// </Summary>
        void CallSetDirty(IDirtyMarkerMap marker, BaseEventData eventData)
        {
            marker.OnSetDirtyLerp(startmoveWait);
        }

        /// <Summary>
        /// Send SetDirty message to DrawMap components.
        /// This message will update map immediately.
        /// </Summary>
        protected virtual void SendSetDirtyMsgImmediately()
        {
            foreach (GameObject obj in mapParts)
            {
                ExecuteEvents.Execute<IDirtyMarkerMap>(
                    target: obj,
                    eventData: null,
                    functor: CallSetDirtyImmediately
                );
            }
        }

        /// <Summary>
        /// The functor of SendSetDirtyMsgImmediately method.
        /// </Summary>
        void CallSetDirtyImmediately(IDirtyMarkerMap marker, BaseEventData eventData)
        {
            marker.OnSetDirty();
        }

        /// <Summary>
        /// Send SetNewMap message to DrawMap components.
        /// </Summary>
        protected virtual void SendSetNewMap()
        {
            foreach (GameObject obj in mapParts)
            {
                ExecuteEvents.Execute<IDirtyMarkerMap>(
                    target: obj,
                    eventData: null,
                    functor: CallSetNewMap
                );
            }
        }

        /// <Summary>
        /// The functor of SendSetNewMap method.
        /// </Summary>
        void CallSetNewMap(IDirtyMarkerMap marker, BaseEventData eventData)
        {
            marker.OnSetNewMap();
        }

        /// <Summary>
        /// Send ExitDungeon message to EnterDungeonManager.
        /// </Summary>
        /// <param name="obj">Specify the game controller object.</param>
        protected virtual void SendExitDungeonMessage(GameObject obj)
        {
            ExecuteEvents.Execute<IExitDungeon>(
                target: obj,
                eventData: null,
                functor: ExitDungeonMsg
            );
        }

        /// <Summary>
        /// The functor of SendExitDungeonMessage method.
        /// </Summary>
        void ExitDungeonMsg(IExitDungeon exitDungeon, BaseEventData eventData)
        {
            exitDungeon.OnExitDungeon();
        }

        /// <Summary>
        /// Send Draw message to DrawManager.
        /// </Summary>
        /// <param name="obj">Specify the game controller object.</param>
        protected virtual void SendDrawMessage(GameObject obj)
        {
            ExecuteEvents.Execute<IDungeonObjects>(
                target: obj,
                eventData: null,
                functor: DrawDungeonMsg
            );
        }

        /// <Summary>
        /// The functor of SendDrawMessage method.
        /// </Summary>
        void DrawDungeonMsg(IDungeonObjects dungeon, BaseEventData eventData)
        {
            dungeon.OnDrawObj();
        }

        /// <Summary>
        /// Send Redraw message to DrawManager.
        /// </Summary>
        /// <param name="obj">Specify the game controller object.</param>
        protected virtual void SendRedrawMessage(GameObject obj)
        {
            ExecuteEvents.Execute<IDungeonObjects>(
                target: obj,
                eventData: null,
                functor: RedrawDungeonMsg
            );
        }

        /// <Summary>
        /// The functor of SendRedrawMessage method.
        /// </Summary>
        void RedrawDungeonMsg(IDungeonObjects dungeon, BaseEventData eventData)
        {
            dungeon.OnRedrawObj();
        }

        /// <Summary>
        /// Send Remove message to DrawManager for removing dungeon objects.
        /// </Summary>
        /// <param name="obj">Specify the game controller object.</param>
        protected virtual void SendRemoveMessage(GameObject obj)
        {
            ExecuteEvents.Execute<IDungeonObjects>(
                target: obj,
                eventData: null,
                functor: RemoveDungeonMsg
            );
        }

        /// <Summary>
        /// The functor of SendRemoveMessage method.
        /// </Summary>
        void RemoveDungeonMsg(IDungeonObjects dungeon, BaseEventData eventData)
        {
            dungeon.OnRemoveObj();
        }

        /// <Summary>
        /// Send dungeon data to DungeonSettings.
        /// </Summary>
        /// <param name="obj">Specify the game controller object.</param>
        protected virtual void SendDungeonData(GameObject obj)
        {
            ExecuteEvents.Execute<IDungeonSetter>(
                target: obj,
                eventData: null,
                functor: SendDungeonMsg
            );
        }

        /// <Summary>
        /// The functor of SendDungeonData method.
        /// </Summary>
        void SendDungeonMsg(IDungeonSetter dungeon, BaseEventData eventData)
        {
            dungeon.OnSetDungeon(moveDestDungeon);
        }

        /// <Summary>
        /// Notify the post event of after moving.
        /// </Summary>
        /// <param name="obj">Specify the gameObject which holds the script for post events.</param>
        protected virtual void NotifyPostMoveEvent(GameObject obj)
        {
            Debug.Log("NotifyPostMoveEvent");
            //GameSystem.DungeonViewManager.PostMoveChecker.OnPostMoveEvent();
            
            ExecuteEvents.Execute<IPostMoveNotify>(
                target: obj,
                eventData: null,
                functor: PostMoveEventMsg
            );
        }

        /// <Summary>
        /// The functor of NotifyPostMoveEvent method.
        /// </Summary>
        void PostMoveEventMsg(IPostMoveNotify inf, BaseEventData eventData)
        {
            inf.OnPostMoveEvent();
        }

        /// <Summary>
        /// Notify the post event of after moving.
        /// </Summary>
        /// <param name="obj">Specify the gameObject which holds the script for post events.</param>
        protected virtual void NotifyPostGameEvent(GameObject obj)
        {
            ExecuteEvents.Execute<IPostMoveNotify>(
                target: obj,
                eventData: null,
                functor: PostGameEventMsg
            );
        }

        /// <Summary>
        /// The functor of NotifyPostMoveEvent method.
        /// </Summary>
        void PostGameEventMsg(IPostMoveNotify inf, BaseEventData eventData)
        {
            inf.OnPostGameEvent();
        }

        public void ShowUI()
        {
            dungeonUI.SetActive(true);
        }

        public void HideUI()
        {
            dungeonUI.SetActive(false);
        }

        public void SetPlayerPosition(int x, int y, int direction)
        {
            PlayerPosition.Instance.playerPos = new Vector2Int(x,y);
            PlayerPosition.Instance.direction = (DungeonDir)direction;
            float targetAngle = CurrentDirAngle();
            player.transform.eulerAngles = new Vector3(0, targetAngle, 0);
            SetCameraPos();
            // Add traverse data
            SetTraverse();
            SendSetDirtyMsgImmediately();
        }

        private DrawDungeonWall GetDrawDungeonWall()
        {
            GameObject wallPratenObj = GameObject.Find(AriadneSceneObjectName.WallParent);
            if (wallPratenObj == null)
            {
                return null;
            }
            DrawDungeonWall drawDungeonWall = wallPratenObj.GetComponent<DrawDungeonWall>();
            if (drawDungeonWall == null)
            {
                return null;
            }
            return drawDungeonWall;
        }

        public void SetDeactiveParentObj(int positionX, int positionY)
        {
            GameObject eventObj = GetDrawDungeonWall()?.GetWallObjectByAxis(positionX, positionY);
            if (eventObj != null)
            {
                eventObj.SetActive(false);
            }
        }

        public void SetDeactiveChildObj(int positionX, int positionY)
        {
            GameObject eventObj = GetDrawDungeonWall()?.GetWallObjectByAxis(positionX, positionY);
            if (eventObj != null)
            {
                for (int i = 0;i < eventObj.transform.childCount;i++)
                {
                    eventObj.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        public void SetActiveEventObj(int positionX,int positionY)
        {
            GameObject eventObj = GetDrawDungeonWall()?.GetWallObjectByAxis(positionX, positionY);
            if (eventObj != null)
            {
                eventObj.SetActive(true);
            }
        }

        public void SetEventEndDeactiveEventObj(int positionX,int positionY)
        {
            GameObject eventObj = GetDrawDungeonWall()?.GetWallObjectByAxis(positionX, positionY);
            if (eventObj != null)
            {
                var animator = eventObj.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.enabled = true;
                    animator.Play("Take 001", 0, 0.3f);
                    animator.speed = 0;
                }
                var emitters = eventObj.GetComponentsInChildren<Effekseer.EffekseerEmitter>();
                if (emitters != null)
                {
                    foreach (var emitter in emitters)
                    {
                        emitter.enabled = false;
                    }
                }
            }
        }

        public void UpdateKey(List<InputKeyType> inputKeyTypes)
        {
            if (!isInDungeon)
            {
                return;
            }

            if (canMove)
            {
                PlayerMove(inputKeyTypes);
            }
        }

        /// <Summary>
        /// Check key inputs about movement.
        /// </Summary>
        protected void PlayerMove(List<InputKeyType> inputKeyTypes)
        {
            if (isEventReady)
            {
                if (inputKeyTypes.Contains(InputKeyType.Decide) || inputKeyTypes.Contains(InputKeyType.Up))
                {
                    canMove = false;
                    OnEventKeyPressed();
                    return;
                }
            }

            if (inputKeyTypes.Contains(InputKeyType.Up) || isPressedMoveFront)
            {
                MoveFrontProcess();
                return;
            }

            if (inputKeyTypes.Contains(InputKeyType.SideLeft1) || inputKeyTypes.Contains(InputKeyType.SideRight1))
            {
                if (inputKeyTypes.Contains(InputKeyType.Left) || isPressedTurnLeft)
                {
                    Direction dir = new Direction();
                    DungeonDir targetDir = dir.GetCounterclockwiseDir(PlayerPosition.Instance.direction);
                    MoveToTargetDirectionProcess(targetDir);
                    return;
                }

                if (inputKeyTypes.Contains(InputKeyType.Right) || isPressedTurnRight)
                {
                    Direction dir = new Direction();
                    DungeonDir targetDir = dir.GetClockwiseDir(PlayerPosition.Instance.direction);
                    MoveToTargetDirectionProcess(targetDir);
                    return;
                }

                if (inputKeyTypes.Contains(InputKeyType.Down) || isPressedTurnBack)
                {
                    Direction dir = new Direction();
                    DungeonDir targetDir = dir.GetReverseDir(PlayerPosition.Instance.direction);
                    MoveToTargetDirectionProcess(targetDir);
                    return;
                }
            }
            else
            {
                if (inputKeyTypes.Contains(InputKeyType.Left) || isPressedTurnLeft)
                {
                    TurnProcess(TurnLeft);
                    return;
                }

                if (inputKeyTypes.Contains(InputKeyType.Right) || isPressedTurnRight)
                {
                    TurnProcess(TurnRight);
                    return;
                }

                if (inputKeyTypes.Contains(InputKeyType.Down) || isPressedTurnBack)
                {
                    TurnProcess(TurnBack);
                    return;
                }
            }
/* 最上に移動
            if (isEventReady)
            {
                if (Input.GetKeyUp(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow))
                {
                    canMove = false;
                    OnEventKeyPressed();
                }
            }
*/
        }

        public Vector2Int GetForwardPosition()
        {
            return PlayerPosition.Instance.GetForwardPosition(1);
        }
    }
}