using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Event process of increasing or decreasing items.
    /// </Summary>
    public class EventIncreaseItem : AriadneEventStrategyBase, IAriadneEvent
    {
        [SerializeField]
        protected string argNameItemID = "ItemID";

        [SerializeField]
        protected string argNameItemNumDelta = "ItemNumDelta";

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

            string argValue = processData.eventArgs.Find(arg => arg.argName == argNameItemID).argListValue[0];
            int itemId = 0;
            bool success = int.TryParse(argValue, out itemId);
            if (!success)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameItemID + " has invalid data. Check your event data in EventEditor.");
            }

            argValue = processData.eventArgs.Find(arg => arg.argName == argNameItemNumDelta).argListValue[0];
            int itemNumDelta = 0;
            success = int.TryParse(argValue, out itemNumDelta);
            if (!success)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameItemNumDelta + " has invalid data. Check your event data in EventEditor.");
            }

            IncreaseItem(itemId, itemNumDelta, eventPos);
        }

        /// <Summary>
        /// Increase (or decrease) items specified in the event process.
        /// </Summary>
        /// <param name="itemId">Item data ID.</param>
        /// <param name="itemNumDelta">Item num to increase or decrease.</param>
        protected virtual void IncreaseItem(int itemId, int itemNumDelta, Vector2Int eventPos)
        {
            CheckItemDataManagerReference();
            if (itemDataManager.CheckIfItemExits(itemId))
            {
                itemDataManager.SetHoldItemList(itemId, itemNumDelta);
            }
            else
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Item ID : " + itemId + " does not exists in ItemDataList. Check your event data in EventEditor.");
            }

            base.PostEventProcess();
        }
    }
}