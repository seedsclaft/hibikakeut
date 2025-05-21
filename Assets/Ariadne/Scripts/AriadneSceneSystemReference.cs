using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Holds references for system scripts in the scene.
    /// </Summary>
    public class AriadneSceneSystemReference : MonoBehaviour
    {
        [SerializeField]
        protected GameObject eventProcessorObject;

        [SerializeField]
        protected GameObject eventDataHolderObject;

        [SerializeField]
        protected GameObject eventFlagManagerObject;

        [SerializeField]
        protected GameObject itemDataHolderObject;

        [SerializeField]
        protected GameObject itemDataManagerObject;

        [SerializeField]
        protected GameObject moveControllerObject;

        /// <Summary>
        /// Returns the eventProcessorObject.
        /// </Summary>
        public virtual GameObject GetEventProcessorObject()
        {
            return eventProcessorObject;
        }

        /// <Summary>
        /// Set the eventProcessorObject.
        /// </Summary>
        /// <param name="obj">Object to set.</param>
        public virtual void SetEventProcessorObject(GameObject obj)
        {
            eventProcessorObject = obj;
        }

        /// <Summary>
        /// Returns the eventDataHolderObject.
        /// </Summary>
        public virtual GameObject GetEventDataHolderObject()
        {
            return eventDataHolderObject;
        }

        /// <Summary>
        /// Set the eventDataHolderObject.
        /// </Summary>
        /// <param name="obj">Object to set.</param>
        public virtual void SetEventDataHolderObject(GameObject obj)
        {
            eventDataHolderObject = obj;
        }

        /// <Summary>
        /// Returns the eventFlagManagerObject.
        /// </Summary>
        public virtual GameObject GetEventFlagManagerObject()
        {
            return eventFlagManagerObject;
        }

        /// <Summary>
        /// Set the eventFlagManagerObject.
        /// </Summary>
        /// <param name="obj">Object to set.</param>
        public virtual void SetEventFlagManagerObject(GameObject obj)
        {
            eventFlagManagerObject = obj;
        }

        /// <Summary>
        /// Returns the itemDataHolderObject;
        /// </Summary>
        public virtual GameObject GetItemDataHolderObject()
        {
            return itemDataHolderObject;
        }

        /// <Summary>
        /// Set the itemDataHolderObject.
        /// </Summary>
        /// <param name="obj">Object to set.</param>
        public virtual void SetItemDataHolderObject(GameObject obj)
        {
            itemDataHolderObject = obj;
        }

        /// <Summary>
        /// Returns the itemDataManagerObject;
        /// </Summary>
        public virtual GameObject GetItemDataManagerObject()
        {
            return itemDataManagerObject;
        }

        /// <Summary>
        /// Set the itemDataHolderObject.
        /// </Summary>
        /// <param name="obj">Object to set.</param>
        public virtual void SetItemDataManagerObject(GameObject obj)
        {
            itemDataHolderObject = obj;
        }

        /// <Summary>
        /// Returns the moveControllerObject;
        /// </Summary>
        public virtual GameObject GetMoveControllerObject()
        {
            return moveControllerObject;
        }

        /// <Summary>
        /// Set the moveControllerObject.
        /// </Summary>
        /// <param name="obj">Object to set.</param>
        public virtual void SetMoveControllerObject(GameObject obj)
        {
            moveControllerObject = obj;
        }
    }
}