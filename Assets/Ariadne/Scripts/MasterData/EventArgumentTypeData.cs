using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Definition of EventArgumentType.
    /// </Summary>
    [CreateAssetMenu(fileName = "EventArgumentTypeData", menuName = "Ariadne/EventArgumentTypeData", order = AriadneMenuOrder.EventArgumentTypeData)]
    public class EventArgumentTypeData : ScriptableObject
    {
        public List<EventArgumentTypeRecord> eventArgTypeRecords;
        public bool isSystemData;
    }

    /// <Summary>
    /// Each record of event argument type data.
    /// </Summary>
    [System.Serializable]
    public class EventArgumentTypeRecord
    {
        public int eventArgTypeId;
        public string eventArgTypeDisplayName;
        public string eventArgTypeName;
    }
}