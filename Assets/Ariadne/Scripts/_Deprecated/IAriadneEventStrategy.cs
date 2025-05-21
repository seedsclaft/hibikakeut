using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Ariadne
{
    /// <Summary>
    /// The interface of AriadneEventStrategy.
    /// </Summary>
    [Obsolete("Use IAriadneEvent method instead.")]
    public interface IAriadneEventStrategy : IEventSystemHandler
    {
        /// <Summary>
        /// Execute method of event process.
        /// </Summary>
        /// <param name="controller">Game controller object.</param>
        /// <param name="eventPos">The position of the event.</param>
        /// <param name="eventParts">The event parts data.</param>
        [Obsolete("Use ExecuteAriadneEvent method instead.")]
        void ExecuteEvent(GameObject contorller, Vector2Int eventPos, AriadneEventParts parts);
    }
}