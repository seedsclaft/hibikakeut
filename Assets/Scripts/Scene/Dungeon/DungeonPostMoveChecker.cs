using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ariadne
{
    /// <Summary>
    /// Check the post event in the Demo scene.
    /// </Summary>
    public class DungeonPostMoveChecker : AriadneSystemBase, IPostMoveNotify
    {
        private System.Action _moveEndEvent = null;
        /// <Summary>
        /// Recieve the notification of post moving.
        /// </Summary>
        public void OnPostMoveEvent()
        {
            // Implement your process of after moving.
            // Such as encountering enemies, check player's HP, and so on.
            _moveEndEvent?.Invoke();

            // When your process has finished, call PostMoveEventFinished method.
            CheckMoveControllerReference();
            PostMoveEventFinished(moveController.gameObject);
        }

        /// <Summary>
        /// Notify the post event has been finished.
        /// </Summary>
        /// <param name="obj">The GameObject that holds the MoveConteroller component.</param>
        void PostMoveEventFinished(GameObject obj)
        {
            ExecuteEvents.Execute<IPostMoveProcessNotify>(
                target: obj,
                eventData: null,
                functor: FinishedMsg
            );
        }

        /// <Summary>
        /// The functor of PostMoveEventFinished method.
        /// </Summary>
        void FinishedMsg(IPostMoveProcessNotify inf, BaseEventData eventData)
        {
            inf.OnFinishedPostMoveEvent();
        }

        public void SetMoveEndEvent(System.Action moveEndEvent)
        {
            _moveEndEvent = moveEndEvent;
        }
    }
}