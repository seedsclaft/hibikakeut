using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ariadne
{
    /// <Summary>
    /// Event processing class.
    /// Events are defined in MapEditor and EventEditor.
    /// </Summary>
    public class EventProcessor : AriadneSystemBase, IEventProcessNotify
    {
        protected bool isWaitingMsg;

        protected GameObject gameController;

        [SerializeField]
        protected AriadneEventStrategyFactory eventStrategyFactory;

        [SerializeField]
        protected bool debugMode;

        protected List<EventCategoryData> eventCategoryDataList;

        protected AriadneEventParts processingEventParts;
        protected Vector2Int processingEventPos;
        protected int currentProcessIndex;
        protected GameObject currentEventObj;

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            if (currentEventObj != null)
            {
                CheckReturnKey();
            }
        }

        /// <Summary>
        /// Get the key input during the event.
        /// </Summary>
        protected virtual void CheckReturnKey()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                SendKeyPressedMsg(currentEventObj, SendKeyPressedNotification);
            }
        }

        /// <Summary>
        /// Action of the enter button for waiting message.
        /// This method is used in mobile devices.
        /// </Summary>
        public virtual void OnPressedOkButton()
        {
            if (currentEventObj != null)
            {
                SendKeyPressedMsg(currentEventObj, SendButtonPressedNotification);
            }
        }

        /// <Summary>
        /// Returns the event parts data that matched the condition.
        /// </Summary>
        /// <param name="eventData">The event data of target position.</param>
        public virtual AriadneEventParts GetExecutionEventParts(EventMasterData eventData)
        {
            if (eventData.eventParts == null)
            {
                return null;
            }

            AriadneEventParts parts = null;
            // Check the condition of event parts from last index.
            for (int i = eventData.eventParts.Count - 1; i >= 0; i--)
            {
                parts = eventData.eventParts[i];
                if (parts.eventTrigger == AriadneEventTrigger.OnEnterFloor)
                {
                    continue;
                }

                if (CheckEventPartsCondition(parts))
                {
                    break;
                }
            }
            return parts;
        }

        /// <Summary>
        /// Returns the event parts data that matched the condition on entering the floor.
        /// </Summary>
        /// <param name="eventData">The event data of target position.</param>
        public virtual AriadneEventParts GetExecutionEventPartsOnEnteringFloor(EventMasterData eventData)
        {
            if (eventData.eventParts == null)
            {
                return null;
            }

            AriadneEventParts parts = null;
            // Check the condition of event parts from last index.
            for (int i = eventData.eventParts.Count - 1; i >= 0; i--)
            {
                parts = eventData.eventParts[i];
                if (parts.eventTrigger != AriadneEventTrigger.OnEnterFloor)
                {
                    continue;
                }

                if (CheckEventPartsCondition(parts))
                {
                    break;
                }
            }
            return parts;
        }

        /// <Summary>
        /// Returns that event parts condition is matched.
        /// </Summary>
        /// <param name="parts">The event parts to check the condition.</param>
        protected virtual bool CheckEventPartsCondition(AriadneEventParts parts)
        {
            bool isMatched = false;
            switch (parts.startCondition)
            {
                case AriadneEventCondition.Flag:
                    CheckEventFlagManagerReference();
                    isMatched = eventFlagManager.CheckEventFlag(parts.startFlagName);
                    break;
                case AriadneEventCondition.Item:
                    CheckItemDataManagerReference();
                    isMatched = itemDataManager.CheckEventConditionItem(parts.startItemId, parts.comparisonOperator, parts.startNum);
                    break;
                case AriadneEventCondition.Money:
                    CheckItemDataManagerReference();
                    isMatched = itemDataManager.CheckEventConditionMoney(parts.comparisonOperator, parts.startNum);
                    break;
                case AriadneEventCondition.NoCondition:
                    isMatched = true;
                    break;
            }
            return isMatched;
        }

        /// <Summary>
        /// Check the reference for EventCategoryData.
        /// </Summary>
        protected virtual void CheckEventCategoryDataReference()
        {
            if (eventCategoryDataList == null)
            {
                gameController = GameObject.FindGameObjectWithTag(AriadneSceneObjectTag.GameController);
                DungeonSettings ds = gameController.GetComponent<DungeonSettings>();
                eventCategoryDataList = ds.eventCategoryDataList;
            }
        }

        /// <Summary>
        /// Execute events according to event category.
        /// </Summary>
        /// <param name="eventParts">The event parts to execute.</param>
        /// <param name="eventPos">The position of event.</param>
        public virtual void EventExecuter(AriadneEventParts eventParts, Vector2Int eventPos)
        {
            CheckEventCategoryDataReference();

            processingEventParts = eventParts;
            processingEventPos = eventPos;
            currentProcessIndex = 0;

            ExecuteEventProcess();
        }

        /// <Summary>
        /// Execute an event process in the event process list.
        /// </Summary>
        protected virtual void ExecuteEventProcess()
        {
            if (processingEventParts.eventProcessList == null)
            {
                Debug.LogWarning("Specified event in Pos : " + processingEventPos + " is not defined.");
                OnFinishedProcess();
                return;
            }

            if (currentProcessIndex < 0 || currentProcessIndex >= processingEventParts.eventProcessList.Count)
            {
                Debug.LogWarning("Specified event process index : " + currentProcessIndex + " is invalid.");
                OnFinishedProcess();
                return;
            }

            int eventCategoryOfProcess = processingEventParts.eventProcessList[currentProcessIndex].eventCategoryId;
            currentEventObj = eventStrategyFactory.GenerateEventExecuter(eventCategoryOfProcess);
            if (currentEventObj == null)
            {
                Debug.LogWarning("Specified event script object is missing. Event category ID : " + eventCategoryOfProcess);
                OnFinishedProcess();
                return;
            }
            else
            {
                string eventObjName = currentEventObj.name.Replace("(Clone)", "");
                currentEventObj.name = processingEventPos.x.ToString() + AriadneSystemUseName.AxisConnecter + processingEventPos.y.ToString() + "_" + eventObjName;

                IAriadneEvent iEventExecuter = currentEventObj.GetComponent<IAriadneEvent>();
                iEventExecuter.ExecuteAriadneEvent(gameObject, processingEventPos, processingEventParts.eventProcessList[currentProcessIndex], debugMode);
            }
        }

        /// <Summary>
        /// Callback method for after event process finished.
        /// </Summary>
        public virtual void OnFinishedProcess()
        {
            currentProcessIndex++;
            if (currentProcessIndex >= processingEventParts.eventProcessList.Count)
            {
                // If there is no event processes are left, finish the event process.
                PostEvent(processingEventParts);
            }
            else
            {
                // If event processes are left, execute the next process.
                ExecuteEventProcess();
            }
        }


        /// <Summary>
        /// The post process of event.
        /// Check the event executed flag in this method.
        /// </Summary>
        /// <param name="parts">The executed event parts data.</param>
        protected virtual void PostEvent(AriadneEventParts parts)
        {
            SetEventExecutedFlag(parts);
            CheckMoveControllerReference();
            SendEventFinishedMessage(moveController.gameObject);
        }

        /// <Summary>
        /// Set the event executed flag if the event parts had.
        /// </Summary>
        /// <param name="parts">The executed event parts data.</param>
        protected virtual void SetEventExecutedFlag(AriadneEventParts parts)
        {
            if (parts.hasExecutedFlag && parts.executedFlagName != "")
            {
                CheckEventFlagManagerReference();
                eventFlagManager.SetEventFlagDict(parts.executedFlagName, true);
            }
        }

        /// <Summary>
        /// Send a message which finished the event to the game controller.
        /// </Summary>
        /// <param name="obj">Specify the game controller object.</param>
        protected virtual void SendEventFinishedMessage(GameObject obj)
        {
            ExecuteEvents.Execute<IEventProcessor>(
                target: obj,
                eventData: null,
                functor: EventFinished
            );
        }

        /// <Summary>
        /// The functor of SendEventFinishedMessage method.
        /// </Summary>
        void EventFinished(IEventProcessor eventProcessor, BaseEventData eventData)
        {
            eventProcessor.OnFinishedEvent();
        }

        /// <Summary>
        /// Send notification of key event.
        /// </Summary>
        /// <param name="obj">Specify the game controller object.</param>
        protected virtual void SendKeyPressedMsg(GameObject obj, ExecuteEvents.EventFunction<IEventKeyNotify> func)
        {
            ExecuteEvents.Execute<IEventKeyNotify>(
                target: obj,
                eventData: null,
                functor: func
            );
        }

        /// <Summary>
        /// The functor of SendKeyPressedMsg method.
        /// </Summary>
        void SendKeyPressedNotification(IEventKeyNotify inf, BaseEventData eventData)
        {
            inf.OnPressedReturnKey();
        }

        /// <Summary>
        /// The functor of SendKeyPressedMsg method.
        /// </Summary>
        void SendButtonPressedNotification(IEventKeyNotify inf, BaseEventData eventData)
        {
            inf.OnPressedReturnButton();
        }
    }
}