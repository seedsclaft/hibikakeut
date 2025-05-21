using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ariadne
{
    /// <Summary>
    /// Event process of exiting the dungeon.
    /// </Summary>
    public class EventExitDungeon : AriadneEventStrategyBase, IAriadneEvent
    {
        [SerializeField]
        protected string argNameRecieverObjName = "RecieverObjName";

        protected GameObject recieverObj;

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

            string argValue = processData.eventArgs.Find(arg => arg.argName == argNameRecieverObjName).argListValue[0];
            recieverObj = GameObject.Find(argValue);
            if (recieverObj == null)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameRecieverObjName + " is not found. Check your event data in EventEditor.");
            }
            else
            {
                SendExitDungeonMessage(moveControllerObj);
            }

            base.PostEventProcess();
        }

        /// <Summary>
        /// Send a message which notifies exiting the dungeon to MoveController.
        /// </Summary>
        /// <param name="obj">Specify the game controller object.</param>
        protected virtual void SendExitDungeonMessage(GameObject obj)
        {
            ExecuteEvents.Execute<IEventProcessor>(
                target: obj,
                eventData: null,
                functor: NotifyExit
            );
        }

        /// <Summary>
        /// The functor of SendExitDungeonMessage method.
        /// </Summary>
        void NotifyExit(IEventProcessor eventProcessor, BaseEventData eventData)
        {
            eventProcessor.OnExitDungeon(recieverObj);
        }
    }
}