using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Event process of wait.
    /// </Summary>
    public class EventWait : AriadneEventStrategyBase, IAriadneEvent
    {
        [SerializeField]
        protected string argNameWaitTime = "WaitTime";

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

            string argValue = processData.eventArgs.Find(arg => arg.argName == argNameWaitTime).argListValue[0];
            float waitTime = 0f;
            bool success = float.TryParse(argValue, out waitTime);
            if (!success)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameWaitTime + " has invalid data. Check your event data in EventEditor.");
            }

            StartCoroutine(WaitProcess(waitTime));
        }

        protected virtual IEnumerator WaitProcess(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            base.PostEventProcess();
        }
    }
}