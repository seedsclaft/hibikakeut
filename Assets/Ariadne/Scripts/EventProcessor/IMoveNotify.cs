using UnityEngine.EventSystems;

namespace Ariadne
{
    /// <Summary>
    /// The interface to notify the end of moving event.
    /// </Summary>
    public interface IMoveNotify : IEventSystemHandler
    {
        /// <Summary>
        /// Notify the end of move event.
        /// </Summary>
        void OnFinishedMove();
    }
}