using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Hold event data list in the game.
    /// </Summary>
    public class EventDataHolder : AriadneSystemBase
    {
        [SerializeField]
        EventDataList eventList;

        /// <Summary>
        /// Setter method for eventDataList.
        /// </Summary>
        /// <param name="list">The EventDataList to set the field.</param>
        public void SetEventDataList(EventDataList list)
        {
            eventList = list;
        }

        /// <Summary>
        /// Get EventMasterData by event id.
        /// </Summary>
        /// <param name="eventId">ID of the EventMasterData</param>
        public EventMasterData GetEventDataById(int eventId)
        {
            EventMasterData data = null;
            if (eventList == null)
            {
                Debug.LogWarning("The eventList field is not assigned. Please set eventDataList in EventDataHolder.");
                return data;
            }

            if (eventList.eventDataList == null)
            {
                Debug.LogWarning("Specified eventList data has no lists. Check the eventList file.");
            }

            data = eventList.eventDataList.Find(e => e.eventId == eventId);
            if (data == null)
            {
                Debug.LogWarning("Specified ID : " + eventId + " is not exist in eventDataList.");
            }
            return data;
        }

        /// <Summary>
        /// Return all EventMasterData.
        /// </Summary>
        public List<EventMasterData> GetAllEventData()
        {
            return eventList.eventDataList;
        }
    }
}