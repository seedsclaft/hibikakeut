using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Definition of MapAttributes.
    /// </Summary>
    [CreateAssetMenu(fileName = "MapAttributeData", menuName = "Ariadne/MapAttributeData", order = AriadneMenuOrder.MapAttributeData)]
    public class MapAttributeData : ScriptableObject
    {
        public List<MapAttributeRecord> mapAttributeRecords;
        public bool isSystemData;
    }

    /// <Summary>
    /// Each record of map attribute data.
    /// </Summary>
    [System.Serializable]
    public class MapAttributeRecord
    {
        public int attributeId;
        public string attributeName;
        public Sprite mapIcon;
        public bool drawMapIcon;
        public bool drawIconOnEditor;
        public bool drawAsWallOnMap;
        public bool canWalk;
        public bool notInUse;
    }
}