using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Manager class of holding items.
    /// </Summary>
    public class ItemDataManager : AriadneSystemBase
    {
        public readonly string ItemNameNone = "None";

        // Key: ItemID Value: ItemNum
        public Dictionary<int, int> holdItemDict;
        protected int money;

        /// <Summary>
        /// Set amount of an item that is specified by item ID.
        /// </Summary>
        /// <param name="itemId">Specify item ID.</param>
        /// <param name="delta">Delta num of holding item.</param>
        public virtual void SetHoldItemList(int itemId, int delta)
        {
            if (holdItemDict == null)
            {
                InitializeHoldItemDict();
            }

            if (holdItemDict.ContainsKey(itemId))
            {
                int itemNum = holdItemDict[itemId];
                itemNum += delta;
                if (itemNum < 0)
                {
                    itemNum = 0;
                }
                holdItemDict[itemId] = itemNum;
            }
            else
            {
                int itemNum = delta > 0 ? delta : 0;
                holdItemDict.Add(itemId, itemNum);
            }
        }

        /// <Summary>
        /// Check if the itemId is exists in ItemDataList.
        /// </Summary>
        /// <param name="itemId">Item ID to check.</param>
        public virtual bool CheckIfItemExits(int itemId)
        {
            ItemMasterData itemData = itemDataHolder.GetAllItemData().Find(item => item.itemId == itemId);
            if (itemData == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <Summary>
        /// Get player's money value.
        /// </Summary>
        public virtual int GetMoneyValue()
        {
            return money;
        }

        /// <Summary>
        /// Set amount of money.
        /// </Summary>
        /// <param name="moneyValue">New money value.</param>
        public virtual void SetPlayerMoney(int moneyValue)
        {
            money = moneyValue;
            if (money < 0)
            {
                money = 0;
            }
        }

        /// <Summary>
        /// Increase amount of money.
        /// </Summary>
        /// <param name="delta">Delta value of money.</param>
        public virtual void IncreasePlayerMoney(int delta)
        {
            money += delta;
            if (money < 0)
            {
                money = 0;
            }
        }

        /// <Summary>
        /// Set a new item dictionary.
        /// Searches all item data in the project and register it to the dictionary.
        /// </Summary>
        public virtual void InitializeHoldItemDict()
        {
            if (holdItemDict != null)
            {
                return;
            }

            holdItemDict = new Dictionary<int, int>();
            CheckItemDataHolderReference();
            List<ItemMasterData> itemList = itemDataHolder.GetAllItemData();
            foreach (ItemMasterData itemData in itemList)
            {
                if (itemData != null)
                {
                    holdItemDict.Add(itemData.itemId, 0);
                }
            }
        }

        /// <Summary>
        /// Returns the item name that is specified by item ID.
        /// </Summary>
        /// <param name="itemId">Specify item ID.</param>
        public virtual string GetItemNameById(int itemId)
        {
            string itemName = ItemNameNone;

            ItemMasterData item = itemDataHolder.GetItemDataById(itemId);
            if (item != null)
            {
                itemName = item.itemName;
            }
            return itemName;
        }

        /// <Summary>
        /// Returns the checking result of key type.
        /// </Summary>
        /// <param name="keyItemId">Item ID of the key.</param>
        public virtual bool CheckCorrespondingKey(int keyItemId)
        {
            bool hasDoorKey = false;
            foreach (KeyValuePair<int, int> pair in holdItemDict)
            {
                if (pair.Value <= 0)
                {
                    continue;
                }

                ItemMasterData item = itemDataHolder.GetItemDataById(pair.Key);
                if (item == null)
                {
                    continue;
                }

                if (item.itemId == keyItemId)
                {
                    hasDoorKey = true;
                }
            }
            return hasDoorKey;
        }

        /// <Summary>
        /// Returns the result of checking condition about holding items.
        /// </Summary>
        /// <param name="itemId">Specify item ID.</param>
        /// <param name="comparison">The comparison operator of the event parts.</param>
        /// <param name="compareNum">The number of conparing to.</param>
        public virtual bool CheckEventConditionItem(int itemId, AriadneComparison comparison, int compareNum)
        {
            bool isMatched = false;
            if (holdItemDict == null)
            {
                return isMatched;
            }

            int holdingItemNum = holdItemDict.ContainsKey(itemId) ? holdItemDict[itemId] : 0;
            isMatched = AriadneComparer.GetComparedResult(comparison, holdingItemNum, compareNum);

            return isMatched;
        }

        /// <Summary>
        /// Returns the result of checking condition about money.
        /// </Summary>
        /// <param name="comparison">The comparison operator of the event parts.</param>
        /// <param name="compareNum">The criterion number of conparing to.</param>
        public virtual bool CheckEventConditionMoney(AriadneComparison comparison, int compareNum)
        {
            bool isMatched = false;
            
            isMatched = AriadneComparer.GetComparedResult(comparison, this.money, compareNum);

            return isMatched;
        }
    }
}