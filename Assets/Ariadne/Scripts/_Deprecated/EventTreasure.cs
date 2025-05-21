using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Event process of getting items.
    /// </Summary>
    [Obsolete("Use 'IncreaseItem' and 'EventPlayAnimation' instead.")]
    public class EventTreasure : AriadneEventStrategyBase, IAriadneEventStrategy
    {
        readonly string UnitOfMoney = "gold";
        float treasureAnimTime = 1.0f;

        /// <Summary>
        /// Execute method of event process.
        /// </Summary>
        /// <param name="callBackObj">Call back object.</param>
        /// <param name="eventPos">The position of the event.</param>
        /// <param name="eventParts">The event parts data.</param>
        public void ExecuteEvent(GameObject callBackObj, Vector2Int eventPos, AriadneEventParts parts)
        {
            callBackObject = callBackObj;
            GameObject treasureObj = GetEventObj(eventPos);

            if (treasureObj != null)
            {
                StartCoroutine(TreasureAnimation(treasureObj, parts));
            }
            else
            {
                // Set item without animation.
                SetTreasureItem(parts);
            }
        }

        /// <Summary>
        /// Treasure box opening process.
        /// </Summary>
        /// <param name="treasureObj">The treasure box object to animate.</param>
        /// <param name="eventParts">The event parts data.</param>
        IEnumerator TreasureAnimation(GameObject treasureObj, AriadneEventParts eventParts)
        {
            // Get the TreasureAnimator component.
            TreasureAnimator treasureAnim = treasureObj.GetComponent<TreasureAnimator>();

            if (treasureAnim != null)
            {
                // Send treasure open message.
                treasureAnim.OpenTreasureBox();

                // Wait until the treasure box is opened.
                yield return new WaitForSeconds(treasureAnimTime);

                // Show item message.
                SetTreasureItem(eventParts);
            }
            else
            {
                AnimatorWarning(treasureObj);
            }
        }

        /// <Summary>
        /// Increasing (or decreasing) item process.
        /// This method is called when increase (or decrease) item without animation too.
        /// </Summary>
        /// <param name="treasureObj">The treasure box object to animate.</param>
        /// <param name="eventParts">The event parts data.</param>
        void SetTreasureItem(AriadneEventParts eventParts)
        {
            string msg = "";
            int num = eventParts.itemNum;
            int numOnShowing = Mathf.Abs(num);
            string gotOrLost = num >= 0 ? "got" : "lost";

            CheckItemDataManagerReference();

            if (eventParts.treasureType == TreasureType.Item)
            {
                string itemName = itemDataManager.GetItemNameById(eventParts.itemId);
                if (itemName == itemDataManager.ItemNameNone)
                {
                    Debug.LogWarning("It seems that specified item ID is not defined. Check the setting of the event parts.");
                }
                msg = "You " + gotOrLost + " follow item." + "\n"
                            + itemName + " : " + numOnShowing.ToString();
                itemDataManager.SetHoldItemList(eventParts.itemId, eventParts.itemNum);
            }
            else if (eventParts.treasureType == TreasureType.Money)
            {
                string unit = numOnShowing == 1 ? UnitOfMoney : UnitOfMoney + "s";
                msg = "You " + gotOrLost + " " + numOnShowing.ToString() + " " + unit + ".";
                itemDataManager.IncreasePlayerMoney(num);
            }

            List<string> msgList = new List<string>();
            msgList.Add(msg);

            this.sendShowingMsgList = msgList;
            this.sendParts = eventParts;
            SendShowingMessage(callBackObject);
        }
    }
}