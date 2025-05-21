using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Factory class of event strategy classes.
    /// </Summary>
    public class AriadneEventStrategyFactory : MonoBehaviour
    {
        List<EventCategoryData> eventCategoryDataList;

        /// <Summary>
        /// Check the reference for eventCategoryData.
        /// </Summary>
        void CheckEventCategoryDataReference()
        {
            if (eventCategoryDataList == null)
            {
                GameObject gameControllerObj = GameObject.FindGameObjectWithTag(AriadneSceneObjectTag.GameController);
                DungeonSettings ds = gameControllerObj.GetComponent<DungeonSettings>();
                eventCategoryDataList = ds.eventCategoryDataList;
            }
        }

        /// <Summary>
        /// Returns IAriadneEventStrategy component that corresponds to the event category.
        /// </Summary>
        /// <param name="eventCategory">The event category to execute.</param>
        [Obsolete("Use GenerateEventExecuter instead.")]
        public IAriadneEventStrategy GetEventExecuter(AriadneEventCategory eventCategory)
        {
            Component component = (Component) gameObject.GetComponent<IAriadneEventStrategy>();
            if (component != null)
            {
                Destroy(component);
            }

            IAriadneEventStrategy iStrategy = null;

            return iStrategy;
        }

        /// <Summary>
        /// Returns a GameObject that corresponds to the event category.
        /// </Summary>
        /// <param name="eventCategoryId">ID of the event category to execute.</param>
        public GameObject GenerateEventExecuter(int eventCategoryId)
        {
            GameObject iStrategyObj = null;

            CheckEventCategoryDataReference();
            if (eventCategoryDataList == null)
            {
                Debug.LogWarning("EventCategoryDataList is null. Check the DungeonSettings component.");
                return iStrategyObj;
            }

            EventCategoryRecord record = DataRecordUtil.GetEventCategoryRecordById(eventCategoryDataList, eventCategoryId);
            if (record == null)
            {
                Debug.LogWarning("Specified event category ID : " + eventCategoryId + " is missing. Check the EventCategoryData.");
                return iStrategyObj;
            }

            if (record.scriptPrefab == null)
            {
                Debug.LogWarning("It seems the prefab of specified event category ID : " + eventCategoryId + " is missing. Check the EventCategoryData.");
                return iStrategyObj;
            }

            iStrategyObj = Instantiate(record.scriptPrefab, Vector3.zero, Quaternion.identity);
            iStrategyObj.transform.SetParent(gameObject.transform);

            return iStrategyObj;
        }
    }
}