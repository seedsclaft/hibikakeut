using UnityEngine.EventSystems;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// The interface of event processor.
    /// </Summary>
    public interface IEventProcessNotify : IEventSystemHandler
    {
        /// <Summary>
        /// Send an event process finished message.
        /// </Summary>
        void OnFinishedProcess();
    }
}