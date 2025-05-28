using UnityEngine.EventSystems;

namespace Ariadne
{
    /// <Summary>
    /// The interface to notify the post event of moving.
    /// </Summary>
    public interface IPostMoveNotify : IEventSystemHandler
    {
        /// <Summary>
        /// Notify the event of post move.
        /// </Summary>
        void OnPostMoveEvent();
        void OnPostGameEvent();
    }
}