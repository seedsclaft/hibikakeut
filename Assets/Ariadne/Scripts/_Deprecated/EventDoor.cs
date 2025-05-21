using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne 
{
    /// <Summary>
    /// Event process of opening the door.
    /// </Summary>
    [Obsolete("Use 'EventMovePlayerToFront' and 'EventPlayAnimation' instead.")]
    public class EventDoor : EventDoorBase, IAriadneEvent, IDoorOpen
    {
        /// <Summary>
        /// Execute method of event process.
        /// </Summary>
        /// <param name="callBackObj">Call back object.</param>
        /// <param name="eventPos">The position of the event.</param>
        /// <param name="eventParts">The event parts data.</param>
        public void ExecuteEvent(GameObject callBackObj, Vector2Int eventPos, AriadneEventParts parts)
        {
            callBackObject = callBackObj;
            GameObject doorObj = GetEventObj(eventPos);

            if (doorObj != null)
            {
                StartCoroutine(OpenDoor(doorObj, parts));
            }
            else
            {
                MissingEventObjectError(eventPos);
            }
        }

        /// <Summary>
        /// Execute method of event process.
        /// </Summary>
        /// </Summary>
        /// <param name="callbackObj">GameObject which receives call.</param>
        /// <param name="eventPos">The position of the event.</param>
        /// <param name="processData">The event process data.</param>
        /// <param name="debugMode">The flag for debug.</param>
        public void ExecuteAriadneEvent(GameObject callbackObj, Vector2Int eventPos, EventProcessData processData, bool debugMode)
        {
            PreEventProcess(callbackObj, debugMode);
            GameObject doorObj = GetEventObj(eventPos);

            if (doorObj != null)
            {
                // StartCoroutine(OpenDoor(doorObj, parts));
            }
            else
            {
                MissingEventObjectError(eventPos);
            }
        }

        /// <Summary>
        /// Receiver of the notification which is moving during the door opening event from MoveController.
        /// </Summary>
        public void OnMoveFinished()
        {
            isWaitingMove = false;
        }
    }
}