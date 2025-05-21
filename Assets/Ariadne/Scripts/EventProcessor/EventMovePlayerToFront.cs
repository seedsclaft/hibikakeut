using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Move the player object to player's front.
    /// </Summary>
    public class EventMovePlayerToFront : AriadneEventStrategyBase, IAriadneEvent
    {
        [SerializeField]
        protected string argNameSteps = "Steps";

        [SerializeField]
        protected string argNameCheckPostEvent = "CheckPostEvent";

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

            // Get a reference of the move controller.
            GameObject moveControllerObj = GameObject.Find(AriadneSceneObjectName.MoveController);

            // Get move steps from args.
            string stepsString = processData.eventArgs.Find(arg => arg.argName == argNameSteps).argListValue[0];
            int steps = 0;
            bool success = int.TryParse(stepsString, out steps);
            if (!success)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameSteps + " has invalid data. Check your event data in EventEditor.");
            }

            // Get a flag of event checking after this event parts.
            string checkEventFlagString = processData.eventArgs.Find(arg => arg.argName == argNameCheckPostEvent).argListValue[0];
            bool checkEvent = false;
            success = bool.TryParse(checkEventFlagString, out checkEvent);
            if (!success)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameCheckPostEvent + " has invalid data. Check your event data in EventEditor.");
            }

            if (moveControllerObj != null)
            {
                StartCoroutine(MovePlayer(moveControllerObj, steps, checkEvent));
            }
            else
            {
                PostEventProcess();
            }
        }

        /// <Summary>
        /// Move the player.
        /// </Summary>
        /// <param name="moveControllerObj">MoveController object.</param>
        /// <param name="steps">Steps to move player.</param>
        protected virtual IEnumerator MovePlayer(GameObject moveControllerObj, int steps, bool checkEvent)
        {
            MoveController moveController = moveControllerObj.GetComponent<MoveController>();

            for (int i = 0; i < steps; i++)
            {
                yield return StartCoroutine(moveController.MoveAStep());
            }

            moveController.SetCheckFlagAfterEvent(checkEvent);

            base.PostEventProcess();
        }
    }
}