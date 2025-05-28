using UnityEngine;

namespace Ariadne
{
    public class EventGameEvent : AriadneEventStrategyBase, IAriadneEvent
    {
        [SerializeField]
        protected string argNameGameEvent = "GameEvent";

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

            string argValue = processData.eventArgs.Find(arg => arg.argName == argNameGameEvent).argListValue[0];
            int eventId = 0;
            bool success = int.TryParse(argValue, out eventId);
            if (!success)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameGameEvent + " has invalid data. Check your event data in EventEditor.");
            }
            var eventObj = GetEventObj(eventPos);
            eventObj.SetActive(false);
            base.PostEventProcess();
        }
    }
}
