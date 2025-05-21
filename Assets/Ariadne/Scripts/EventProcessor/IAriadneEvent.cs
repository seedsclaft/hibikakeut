using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Ariadne
{
    /// <Summary>
    /// The interface of AriadneEventStrategy.
    /// </Summary>
    public interface IAriadneEvent : IEventSystemHandler
    {
        /// <Summary>
        /// Execute method of event process.
        /// </Summary>
        /// <param name="callbackObj">GameObject which receives call.</param>
        /// <param name="eventPos">The position of the event.</param>
        /// <param name="processData">The event process data.</param>
        /// <param name="debugMode">The flag for debug.</param>
        void ExecuteAriadneEvent(GameObject callbackObj, Vector2Int eventPos, EventProcessData processData, bool debugMode);
    }
}