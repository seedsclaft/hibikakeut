using UnityEngine.EventSystems;
using System;

namespace Ariadne
{
    /// <Summary>
    /// The interface of moving on the opening door event.
    /// </Summary>
    [Obsolete("Use 'EventMovePlayerToFront' and 'EventPlayAnimation' instead.")]
    public interface IDoorOpen : IEventSystemHandler
    {
        /// <Summary>
        /// Send a message which notifies finishing of moving.
        /// </Summary>
        void OnMoveFinished();
    }
}