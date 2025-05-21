using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Manager class of event executed flags.
    /// </Summary>
    public class EventFlagManager : AriadneSystemBase
    {
        public Dictionary<string, bool> eventFlagDict;

        /// <Summary>
        /// Set event executed flag state.
        /// </Summary>
        /// <param name="flagName">Specify the flag name.</param>
        /// <param name="flag">Executed flag.</param>
        public virtual void SetEventFlagDict(string flagName, bool flag)
        {
            if (eventFlagDict == null)
            {
                InitializeEventFlagList();
            }
            
            if (eventFlagDict.ContainsKey(flagName))
            {
                eventFlagDict[flagName] = flag;
            }
        }

        /// <Summary>
        /// Set a new event executed flags dictionary.
        /// Searches all event data and if the event data has executed flag,
        /// register it to the dictionary.
        /// </Summary>
        public virtual void InitializeEventFlagList()
        {
            if (eventFlagDict != null)
            {
                return;
            }

            eventFlagDict = new Dictionary<string, bool>();
            CheckEventDataHolderReference();
            List<EventMasterData> eventList = eventDataHolder.GetAllEventData();
            foreach (EventMasterData eventData in eventList)
            {
                if (eventData == null)
                {
                    continue;
                }

                if (eventData.eventParts == null)
                {
                    continue;
                }

                for (int i = 0; i < eventData.eventParts.Count; i++)
                {
                    if (eventData.eventParts[i].hasExecutedFlag && !string.IsNullOrEmpty(eventData.eventParts[i].executedFlagName))
                    {
                        eventFlagDict.Add(eventData.eventParts[i].executedFlagName, false);
                    }
                }
            }
        }

        /// <Summary>
        /// Returns the flag state of the specified flag name.
        /// </Summary>
        /// <param name="eventHasFlag">The flag "hasExecutedFlag" of event parts.</param>
        /// <param name="flagName">Specify the flag name.</param>
        public virtual bool CheckEventHasBeenExecuted(bool eventHasFlag, string flagName)
        {
            bool hasExecuted = false;
            if (!eventHasFlag)
            {
                return hasExecuted;
            }

            if (eventFlagDict == null)
            {
                InitializeEventFlagList();
            }

            if (eventFlagDict.ContainsKey(flagName))
            {
                hasExecuted = eventFlagDict[flagName];
            }

            return hasExecuted;
        }

        /// <Summary>
        /// Returns the result of checking flag.
        /// </Summary>
        /// <param name="flagName">Specify the flag name.</param>
        public virtual bool CheckEventFlag(string flagName)
        {
            bool isExecuted = false;
            if (eventFlagDict == null)
            {
                InitializeEventFlagList();
            }

            if (eventFlagDict.ContainsKey(flagName))
            {
                isExecuted = eventFlagDict[flagName];
            }
            return isExecuted;
        }
    }
}
