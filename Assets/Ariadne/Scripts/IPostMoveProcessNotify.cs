using UnityEngine.EventSystems;

namespace Ariadne
{
    /// <Summary>
    /// The callback interface for the post process of after moving.
    /// </Summary>
    public interface IPostMoveProcessNotify : IEventSystemHandler
    {
        /// <Summary>
        /// Callback of the post move event.
        /// </Summary>
        void OnFinishedPostMoveEvent();
    }
}