using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Event process of None type which does nothing.
    /// </Summary>
    public class EventNone : AriadneEventStrategyBase, IAriadneEvent
    {
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
            base.PostEventProcess();
        }
    }
}