using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Definition of event parts data.
    /// </Summary>
    [System.Serializable]
    public class AriadneEventParts
    {
        [Obsolete("Use eventCategoryId field instead.")]
        public AriadneEventCategory eventCategory;
        public int eventCategoryId;
        public AriadneEventTrigger eventTrigger;
        public AriadneEventPosition eventPos;

        // event flag
        public AriadneEventCondition startCondition;
        public string startFlagName;
        public int startItemId;
        public int startNum;
        public AriadneComparison comparisonOperator;

        public bool hasExecutedFlag;
        public string executedFlagName;

        // direction
        public bool hasDirectionCondition;
        public List<DungeonDir> directionConditions;

        // event process data
        public List<EventProcessData> eventProcessList;

        // for locked door
        [Obsolete("To set arguments to the event, use eventProcessList instead.")]
        public DoorKeyType doorKeyType;

        // for move position
        [Obsolete("To set arguments to the event, use eventProcessList instead.")]
        public DungeonMasterData destDungeon;
        [Obsolete("To set arguments to the event, use eventProcessList instead.")]
        public FloorMapMasterData destMap;
        [Obsolete("To set arguments to the event, use eventProcessList instead.")]
        public Vector2Int destPos;
        [Obsolete("To set arguments to the event, use eventProcessList instead.")]
        public DungeonDir direction;

        // for treasure
        [Obsolete("To set arguments to the event, use eventProcessList instead.")]
        public TreasureType treasureType;
        [Obsolete("To set arguments to the event, use eventProcessList instead.")]
        public int itemId;
        [Obsolete("To set arguments to the event, use eventProcessList instead.")]
        public int itemNum;

        // for messenger
        [Obsolete("To set arguments to the event, use eventProcessList instead.")]
        public List<string> msgList;

        /// <Summary>
        /// Constructor of event parts data.
        /// </Summary>
        public AriadneEventParts()
        {
            eventProcessList = new List<EventProcessData>();
        }

        /// <Summary>
        /// Copy constructor of base parts.
        /// </Summary>
        /// <param name="baseParts">Base parts data for copying.</param>
        public AriadneEventParts(AriadneEventParts baseParts)
        {
            eventCategoryId = baseParts.eventCategoryId;
            eventTrigger = baseParts.eventTrigger;
            eventPos = baseParts.eventPos;

            startCondition = baseParts.startCondition;
            startFlagName = baseParts.startFlagName;
            startItemId = baseParts.startItemId;
            startNum = baseParts.startNum;
            comparisonOperator = baseParts.comparisonOperator;

            hasExecutedFlag = baseParts.hasExecutedFlag;
            executedFlagName = baseParts.executedFlagName;

            hasDirectionCondition = baseParts.hasDirectionCondition;
            directionConditions = baseParts.directionConditions;

            eventProcessList = baseParts.eventProcessList;
        }
    }

    /// <Summary>
    /// Definition of event data.
    /// </Summary>
    public class EventMasterData : ScriptableObject
    {
        public int eventId;
        public string eventName;
        public List<AriadneEventParts> eventParts;
        public bool isSampleData;
    }
}