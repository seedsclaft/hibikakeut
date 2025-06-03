using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Base class of Ariadne system scripts.
    /// </Summary>
    public class AriadneSystemBase : MonoBehaviour
    {
        protected GameObject systemReferenceObject;
        protected AriadneSceneSystemReference systemReference;

        protected EventProcessor eventProcessor;
        protected EventDataHolder eventDataHolder;
        protected EventFlagManager eventFlagManager;
        protected ItemDataHolder itemDataHolder;
        protected ItemDataManager itemDataManager;
        protected MoveController moveController;

        /// <Summary>
        /// Check if AriadneSceneSystemReference is set.
        /// </Summary>
        protected virtual void CheckSystemReference()
        {
            if (systemReferenceObject == null)
            {
                systemReferenceObject = GameObject.FindGameObjectWithTag(AriadneSceneObjectTag.GameController);
            }

            if (systemReference == null)
            {
                systemReference = systemReferenceObject.GetComponent<AriadneSceneSystemReference>();
            }
        }

        /// <Summary>
        /// Check a references for EventProcessor.
        /// </Summary>
        protected virtual void CheckEventProcessorReference()
        {
            if (eventProcessor == null)
            {
                CheckSystemReference();
                GameObject obj = systemReference.GetEventProcessorObject();
                eventProcessor = obj.GetComponent<EventProcessor>();
            }
        }

        /// <Summary>
        /// Check a references for EventDataHolder.
        /// </Summary>
        protected virtual void CheckEventDataHolderReference()
        {
            if (eventDataHolder == null)
            {
                CheckSystemReference();
                GameObject obj = systemReference.GetEventDataHolderObject();
                eventDataHolder = obj.GetComponent<EventDataHolder>();
            }
        }

        /// <Summary>
        /// Check a references for EventFlagManager.
        /// </Summary>
        protected virtual void CheckEventFlagManagerReference()
        {
            if (eventFlagManager == null)
            {
                CheckSystemReference();
                GameObject obj = systemReference.GetEventFlagManagerObject();
                eventFlagManager = obj.GetComponent<EventFlagManager>();
            }
        }

        /// <Summary>
        /// Check a references for ItemDataHolder.
        /// </Summary>
        protected virtual void CheckItemDataHolderReference()
        {
            if (itemDataHolder == null)
            {
                CheckSystemReference();
                GameObject obj = systemReference.GetItemDataHolderObject();
                itemDataHolder = obj.GetComponent<ItemDataHolder>();
            }
        }

        /// <Summary>
        /// Check a references for ItemDataManager.
        /// </Summary>
        protected virtual void CheckItemDataManagerReference()
        {
            if (itemDataManager == null)
            {
                CheckSystemReference();
                GameObject obj = systemReference.GetItemDataManagerObject();
                itemDataManager = obj.GetComponent<ItemDataManager>();
            }
        }

        /// <Summary>
        /// Check a references for MoveController.
        /// </Summary>
        protected virtual void CheckMoveControllerReference()
        {
            if (moveController == null)
            {
                /*
                CheckSystemReference();
                GameObject obj = systemReference.GetMoveControllerObject();
                moveController = obj.GetComponent<MoveController>();
                */
            }
        }
    }
}