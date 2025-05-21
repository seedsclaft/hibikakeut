using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{    
    /// <Summary>
    /// Definition of item data.
    /// </Summary>
    [CreateAssetMenu(fileName = "item_", menuName = "Ariadne/ItemData", order = AriadneMenuOrder.ItemData)]
    public partial class ItemMasterData : ScriptableObject
    {
        public int itemId;
        public string itemName;

        [Obsolete("Implement your fields about items in your partial file.")]
        public AriadneItemType itemType;

        [Obsolete("Use itemId instead.")]
        public DoorKeyType doorKeyType;
    }
}