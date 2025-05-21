using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Definition of EventCategory.
    /// </Summary>
    [CreateAssetMenu(fileName = "EventCategoryData", menuName = "Ariadne/EventCategoryData", order = AriadneMenuOrder.EventCategoryData)]
    public class EventCategoryData : ScriptableObject
    {
        public List<EventCategoryRecord> eventCategoryRecords;
        public bool isSystemData;
    }

    /// <Summary>
    /// Each record of event category data.
    /// </Summary>
    [System.Serializable]
    public class EventCategoryRecord
    {
        public int eventCategoryId;
        public string eventCategoryName;
        public GameObject scriptPrefab;
        public List<EventArgs> eventArgs;
    }
}