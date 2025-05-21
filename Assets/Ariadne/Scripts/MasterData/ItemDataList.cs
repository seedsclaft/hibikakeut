using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{    
    /// <Summary>
    /// Holds all item data as a list.
    /// </Summary>
	[CreateAssetMenu(fileName = "ItemDataList", menuName = "Ariadne/ItemDataList", order = AriadneMenuOrder.ItemDataList)]
    public class ItemDataList : ScriptableObject
    {
        public List<ItemMasterData> itemDataList;
    }
}