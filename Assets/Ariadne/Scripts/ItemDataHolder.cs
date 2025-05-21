using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Hold item data list in the game.
    /// </Summary>
    public class ItemDataHolder : AriadneSystemBase
    {
        [SerializeField]
        ItemDataList itemList;

        /// <Summary>
        /// Setter method for itemList.
        /// </Summary>
        /// <param name="list">The ItemDataList to set the field.</param>
        public virtual void SetItemDataList(ItemDataList list)
        {
            itemList = list;
        }

        /// <Summary>
        /// Get ItemMasterData by item id.
        /// </Summary>
        /// <param name="itemId">ID of the ItemMasterData</param>
        public virtual ItemMasterData GetItemDataById(int itemId)
        {
            ItemMasterData data = null;
            if (itemList == null)
            {
                Debug.LogWarning("The itemList field is not assigned. Please set itemList in ItemDataHolder.");
                return data;
            }

            if (itemList.itemDataList == null)
            {
                Debug.LogWarning("Specified itemList data has no lists. Check the itemList file.");
            }

            data = itemList.itemDataList.Find(i => i.itemId == itemId);
            if (data == null)
            {
                Debug.LogWarning("Specified ID : " + itemId + " is not exist in itemDataList.");
            }
            return data;
        }

        /// <Summary>
        /// Return all ItemMasterData.
        /// </Summary>
        public virtual List<ItemMasterData> GetAllItemData()
        {
            return itemList.itemDataList;
        }
    }
}