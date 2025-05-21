using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Base class of event strategy classes.
    /// </Summary>
    public class AriadneEventStrategyBase : MonoBehaviour
    {
        protected GameObject callBackObject;
        protected List<string> sendShowingMsgList;
        [Obsolete]
        protected AriadneEventParts sendParts;
        protected ItemDataManager itemDataManager;
        protected bool isDebugging = false;
        protected float delayDestroyTime = 3.0f;

        /// <Summary>
        /// Returns a GameObject which is searched by position and map attribute ID.
        /// </Summary>
        /// <param name="eventPos">The position of event on the dungeon.</param>
        protected GameObject GetEventObj(Vector2Int eventPos)
        {
            GameObject wallPratenObj = GameObject.Find(AriadneSceneObjectName.WallParent);
            DrawDungeonWall drawDungeonWall = wallPratenObj.GetComponent<DrawDungeonWall>();
            GameObject obj = drawDungeonWall.GetWallObjectByAxis(eventPos.x, eventPos.y);
            if (obj == null)
            {
                Debug.LogError("Specified eventPos " + eventPos + " does not have an object.");
            }
            return obj;
        }

        /// <Summary>
        /// Show warning message about animation.
        /// </Summary>
        /// <param name="obj">The object for animating.</param>
        [Obsolete]
        protected void AnimatorWarning(GameObject obj)
        {
            Debug.LogWarning("Animator script is not attached to object : " + obj.name);
        }

        /// <Summary>
        /// Show error message about missing the event object.
        /// </Summary>
        /// <param name="eventPos">The position of event on the dungeon.</param>
        [Obsolete]
        protected void MissingEventObjectError(Vector2Int eventPos)
        {
            Debug.LogError("Event object is missing. The event will be finished without executing. Position : " + eventPos);
        }

        /// <Summary>
        /// The pre process of each event process.
        /// </Summary>
        protected void PreEventProcess(GameObject callBackObj, bool debug)
        {
            callBackObject = callBackObj;
            isDebugging = debug;
        }

        /// <Summary>
        /// The post process of each event process.
        /// </Summary>
        protected void PostEventProcess()
        {
            SendEventProcessFinishedMessage(callBackObject);

            if (!isDebugging)
            {
                StartCoroutine(DelayDestroyProcessObject());
            }
        }

        /// <Summary>
        /// Destroy this gameObject after delay time.
        /// If the debugging flag is true, this object will not be destroyed.
        /// </Summary>
        IEnumerator DelayDestroyProcessObject()
        {
            yield return new WaitForSeconds(delayDestroyTime);
            Destroy(gameObject);
        }

        /// <Summary>
        /// The post process of event.
        /// Check the event executed flag in this method.
        /// </Summary>
        /// <param name="parts">The executed event parts data.</param>
        [Obsolete]
        protected void PostEvent(AriadneEventParts parts)
        {
            SetEventExecutedFlag(parts);
            SendEventFinishedMessage(callBackObject);
        }

        /// <Summary>
        /// Set the event executed flag if the event parts had.
        /// </Summary>
        /// <param name="parts">The executed event parts data.</param>
        [Obsolete]
        protected void SetEventExecutedFlag(AriadneEventParts parts)
        {
            // if (parts.hasExecutedFlag && parts.executedFlagName != "")
            // {
            //     eventFlagManager.SetEventFlagDict(parts.executedFlagName, true);
            // }
        }

        /// <Summary>
        /// Check the reference for EventFlagManager.
        /// </Summary>
        protected void CheckItemDataManagerReference()
        {
            if (itemDataManager == null)
            {
                GameObject itemDataManagerObj = GameObject.Find(AriadneSceneObjectName.ItemDataManager);
                itemDataManager = itemDataManagerObj.GetComponent<ItemDataManager>();
            }
        }

        /// <Summary>
        /// Send a message which finished the event to the game controller.
        /// </Summary>
        /// <param name="obj">Specify the game controller object.</param>
        [Obsolete]
        protected void SendEventFinishedMessage(GameObject obj)
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
        [Obsolete]
        protected void EventFinished(IEventProcessor eventProcessor, BaseEventData eventData)
        {
            eventProcessor.OnFinishedEvent();
        }

        /// <Summary>
        /// Send a message which is showing message UI to the game controller.
        /// </Summary>
        /// <param name="obj">Specify the game controller object.</param>
        [Obsolete]
        protected void SendShowingMessage(GameObject obj)
        {
            ExecuteEvents.Execute<IShowingMsg>(
                target: obj,
                eventData: null,
                functor: ShowMsg
            );
        }

        /// <Summary>
        /// The functor of SendShowingMessage method.
        /// </Summary>
        [Obsolete]
        protected void ShowMsg(IShowingMsg showMsg, BaseEventData eventData)
        {
            showMsg.OnShowingMsg(sendShowingMsgList, sendParts);
        }

        /// <Summary>
        /// Send a message to notify the event process has been finished.
        /// </Summary>
        /// <param name="obj">Specify the callback object.</param>
        protected void SendEventProcessFinishedMessage(GameObject obj)
        {
            ExecuteEvents.Execute<IEventProcessNotify>(
                target: obj,
                eventData: null,
                functor: EventProcessFinished
            );
        }

        /// <Summary>
        /// The functor of SendEventProcessFinishedMessage method.
        /// </Summary>
        protected void EventProcessFinished(IEventProcessNotify inf, BaseEventData eventData)
        {
            inf.OnFinishedProcess();
        }
    }
}