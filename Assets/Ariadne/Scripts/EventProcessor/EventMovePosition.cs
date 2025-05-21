using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ariadne
{
    /// <Summary>
    /// Event process of exiting the dungeon.
    /// </Summary>
    public class EventMovePosition : AriadneEventStrategyBase, IAriadneEvent, IMoveNotify
    {
        [SerializeField]
        protected string argNameDestinationDungeon = "DestinationDungeon";

        [SerializeField]
        protected string argNameDestinationFloor = "DestinationFloor";

        [SerializeField]
        protected string argNameDestinationPosition = "DestinationPosition";

        [SerializeField]
        protected string argNameDestinationDirection = "DestinationDirection";

        [SerializeField]
        protected string argNameRedrawDungeon = "RedrawDungeon";

        protected DungeonMasterData destDungeon;
        protected FloorMapMasterData destFloor;
        protected Vector2Int destPos;
        protected DungeonDir destDirection;
        protected bool redrawDungeon;

        /// <Summary>
        /// Execute method of event process.
        /// </Summary>
        /// <param name="callbackObj">GameObject which receives call.</param>
        /// <param name="eventPos">The position of the event.</param>
        /// <param name="processData">The event process data.</param>
        /// <param name="debugMode">The flag for debug.</param>
        public virtual void ExecuteAriadneEvent(GameObject callbackObj, Vector2Int eventPos, EventProcessData processData, bool debugMode)
        {
            PreEventProcess(callbackObj, debugMode);

            GameObject moveControllerObj = GameObject.Find(AriadneSceneObjectName.MoveController);

            destDungeon = (DungeonMasterData)processData.eventArgs.Find(arg => arg.argName == argNameDestinationDungeon).argObjListValue[0];
            if (destDungeon == null)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameDestinationDungeon + " is null. Check your event data in EventEditor.");
                PostEventProcess();
                return;
            }

            destFloor = (FloorMapMasterData)processData.eventArgs.Find(arg => arg.argName == argNameDestinationFloor).argObjListValue[0];
            if (destFloor == null)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameDestinationFloor + " is null. Check your event data in EventEditor.");
                PostEventProcess();
                return;
            }

            string argValue = processData.eventArgs.Find(arg => arg.argName == argNameDestinationPosition).argListValue[0];
            destPos = ArgValueCaster.GetVector2IntValue(argValue);

            argValue = processData.eventArgs.Find(arg => arg.argName == argNameDestinationDirection).argListValue[0];
            int directionValue = 0;
            bool success = int.TryParse(argValue, out directionValue);
            if (!success)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameDestinationDirection + " has invalid data. Check your event data in EventEditor.");
            }

            destDirection = (DungeonDir)System.Enum.ToObject(typeof(DungeonDir), directionValue);

            argValue = processData.eventArgs.Find(arg => arg.argName == argNameRedrawDungeon).argListValue[0];
            success = bool.TryParse(argValue, out redrawDungeon);
            if (!success)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameRedrawDungeon + " has invalid data. Check your event data in EventEditor.");
            }
            
            SendMovePositionMessage(moveControllerObj);
        }

        /// <Summary>
        /// Send a message which notifies exiting the dungeon to MoveController.
        /// </Summary>
        /// <param name="obj">Specify the move controller object.</param>
        protected virtual void SendMovePositionMessage(GameObject obj)
        {
            ExecuteEvents.Execute<IEventProcessor>(
                target: obj,
                eventData: null,
                functor: NotifyMovePosition
            );
        }

        /// <Summary>
        /// The functor of SendMovePositionMessage method.
        /// </Summary>
        void NotifyMovePosition(IEventProcessor eventProcessor, BaseEventData eventData)
        {
            eventProcessor.OnMovePosition(destDungeon, destFloor, destPos, destDirection, redrawDungeon, gameObject);
        }

        /// <Summary>
        /// Call back method for moving position.
        /// </Summary>
        public virtual void OnFinishedMove()
        {
            base.PostEventProcess();
        }
    }
}