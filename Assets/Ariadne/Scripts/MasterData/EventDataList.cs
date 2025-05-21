using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{    
    /// <Summary>
    /// Holds all event data as a list.
    /// </Summary>
    [CreateAssetMenu(fileName = "EventDataList", menuName = "Ariadne/EventDataList", order = AriadneMenuOrder.EventDataList)]
    public class EventDataList : ScriptableObject
    {
        public bool includeSample;
        public List<EventMasterData> eventDataList;
    }
}