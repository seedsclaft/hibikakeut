using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Utility class for Ariadne system data.
    /// </Summary>
    public static class DataRecordUtil
    {
        /// <Summary>
        /// Returns a MapAttributeRecord which corresponds to specified ID.
        /// </Summary>
        public static MapAttributeRecord GetMapAttributeRecordById(List<MapAttributeData> attrDataList, int attributeId)
        {
            MapAttributeRecord record = null;

            if (attrDataList == null)
            {
                return record;
            }

            foreach (MapAttributeData data in attrDataList)
            {
                if (data == null)
                {
                    continue;
                }

                if (data.mapAttributeRecords == null)
                {
                    Debug.LogWarning(data.name + " has no map attribute records.");
                    continue;
                }

                record = data.mapAttributeRecords.Find(attr => attr.attributeId == attributeId);
                if (record != null)
                {
                    break;
                }
            }

            return record;
        }

        /// <Summary>
        /// Returns a DungeonPartsRecord which corresponds to specified attribute ID and object type ID.
        /// </Summary>
        public static DungeonPartsRecord GetDungeonPartsRecordById(List<DungeonPartsData> partsDataList, int attributeId, int objectTypeId)
        {
            DungeonPartsRecord record = null;

            if (partsDataList == null)
            {
                return record;
            }

            foreach (DungeonPartsData data in partsDataList)
            {
                if (data == null)
                {
                    continue;
                }

                if (data.dungeonPartsRecords == null)
                {
                    Debug.LogWarning(data.name + " has no dungeon parts records.");
                    continue;
                }

                record = data.dungeonPartsRecords.Find(parts => parts.attributeId == attributeId && parts.partsTypeId == objectTypeId);
                if (record != null)
                {
                    break;
                }
            }

            return record;
        }

        /// <Summary>
        /// Returns a EventCategoryRecord which corresponds to specified ID.
        /// </Summary>
        public static EventCategoryRecord GetEventCategoryRecordById(List<EventCategoryData> categoryDataList, int categoryId)
        {
            EventCategoryRecord record = null;

            if (categoryDataList == null)
            {
                return record;
            }

            foreach (EventCategoryData data in categoryDataList)
            {
                if (data == null)
                {
                    continue;
                }

                if (data.eventCategoryRecords == null)
                {
                    Debug.LogWarning(data.name + " has no event category records.");
                    continue;
                }

                record = data.eventCategoryRecords.Find(cat => cat.eventCategoryId == categoryId);
                if (record != null)
                {
                    break;
                }
            }

            return record;
        }

        /// <Summary>
        /// Returns a EventArgumentTypeRecord which corresponds to specified ID.
        /// </Summary>
        public static EventArgumentTypeRecord GetEventArgumentTypeRecordById(List<EventArgumentTypeData> argTypeDataList, int argTypeId)
        {
            EventArgumentTypeRecord record = null;

            if (argTypeDataList == null)
            {
                return record;
            }

            foreach (EventArgumentTypeData data in argTypeDataList)
            {
                if (data == null)
                {
                    continue;
                }

                if (data.eventArgTypeRecords == null)
                {
                    Debug.LogWarning(data.name + " has no event arg type records.");
                    continue;
                }

                record = data.eventArgTypeRecords.Find(cat => cat.eventArgTypeId == argTypeId);
                if (record != null)
                {
                    break;
                }
            }

            return record;
        }
    }
}