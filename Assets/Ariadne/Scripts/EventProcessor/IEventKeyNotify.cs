using UnityEngine.EventSystems;

namespace Ariadne
{
    /// <Summary>
    /// The interface to notify key input during processing event.
    /// </Summary>
    public interface IEventKeyNotify : IEventSystemHandler
    {
        /// <Summary>
        /// Notify key input event.
        /// </Summary>
        void OnPressedReturnKey();

        /// <Summary>
        /// Notify button input event.
        /// </Summary>
        void OnPressedReturnButton();
    }
}