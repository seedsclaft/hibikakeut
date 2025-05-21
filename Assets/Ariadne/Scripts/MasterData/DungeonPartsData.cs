using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Definition of dungeon parts data.
    /// </Summary>
    [CreateAssetMenu(fileName = "DungeonParts", menuName = "Ariadne/DungeonPartsData", order = AriadneMenuOrder.DungeonPartsData)]
    public class DungeonPartsData : ScriptableObject
    {
        public List<DungeonPartsRecord> dungeonPartsRecords;
        public bool isSystemData;
    }

    /// <Summary>
    /// Each record of parts data.
    /// </Summary>
    [System.Serializable]
    public class DungeonPartsRecord
    {
        public int attributeId;
        public int partsTypeId;
        public GameObject partsObject;
		public PartsHeightAnchor heightAnchor;
    }
}