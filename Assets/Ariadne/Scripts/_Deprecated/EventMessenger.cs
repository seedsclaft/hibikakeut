using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Event process of messenger.
    /// </Summary>
    [Obsolete("Use 'EventShowMessage' instead.")]
    public class EventMessenger : AriadneEventStrategyBase, IAriadneEventStrategy
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
            this.sendShowingMsgList = parts.msgList;
            this.sendParts = parts;
            SendShowingMessage(callBackObject);
        }
    }
}