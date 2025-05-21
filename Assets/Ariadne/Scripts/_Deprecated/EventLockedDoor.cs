using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Event process of opening the locked door.
    /// </Summary>
    [Obsolete("Use 'EventMovePlayerToFront' and 'EventPlayAnimation' instead.")]
    public class EventLockedDoor : EventDoorBase, IAriadneEventStrategy, IDoorOpen
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
                OpenLockedDoor(doorObj, parts);
            }
            else
            {
                MissingEventObjectError(eventPos);
            }
        }

        /// <Summary>
        /// Key type checking of the locked door.
        /// </Summary>
        /// <param name="doorObj">The door object to animate.</param>
        /// <param name="eventParts">The event parts data.</param>
        void OpenLockedDoor(GameObject doorObj, AriadneEventParts eventParts)
        {
            // Check the key type of door
            int keyTypeId = 0;
            // DoorKeyType keyType = eventParts.doorKeyType;
            CheckItemDataManagerReference();
            bool isKeyMatched = itemDataManager.CheckCorrespondingKey(keyTypeId);

            // If the key type was None, treat the door as unlocked.
            if (keyTypeId == 0)
            {
                isKeyMatched = true;
            }

            if (isKeyMatched)
            {
                StartCoroutine(OpenDoor(doorObj, eventParts));
            }
            else
            {
                string msg = "You don't have any corresponding key.";
                List<string> msgList = new List<string>();
                msgList.Add(msg);

                this.sendShowingMsgList = msgList;
                this.sendParts = eventParts;
                SendShowingMessage(callBackObject);
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