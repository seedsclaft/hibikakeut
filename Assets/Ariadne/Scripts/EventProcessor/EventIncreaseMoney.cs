using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Event process of increasing or decreasing money.
    /// </Summary>
    public class EventIncreaseMoney : AriadneEventStrategyBase, IAriadneEvent
    {
        [SerializeField]
        protected string argNameMoneyDeltaValue = "MoneyDeltaValue";

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

            string argValue = processData.eventArgs.Find(arg => arg.argName == argNameMoneyDeltaValue).argListValue[0];
            int moneyNumDelta = 0;
            bool success = int.TryParse(argValue, out moneyNumDelta);
            if (!success)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameMoneyDeltaValue + " has invalid data. Check your event data in EventEditor.");
            }

            IncreaseMoney(moneyNumDelta, eventPos);
        }

        /// <Summary>
        /// Increase (or decrease) items specified in the event process.
        /// </Summary>
        /// <param name="moneyNumDelta">Amount of money to increase or decrease.</param>
        protected virtual void IncreaseMoney(int moneyNumDelta, Vector2Int eventPos)
        {
            CheckItemDataManagerReference();
            itemDataManager.IncreasePlayerMoney(moneyNumDelta);
            base.PostEventProcess();
        }
    }
}