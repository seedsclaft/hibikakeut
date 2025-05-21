using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Ariadne
{
    /// <Summary>
    /// Editor script for EventDataList.
    /// </Summary>
    [CustomEditor(typeof(EventDataList))]
    public class EventDataListEditor : Editor
    {
        /// <Summary>
        /// Show a list generation button on Inspector.
        /// </Summary>
        public override void OnInspectorGUI()
        {
            var eventDataList = target as EventDataList;

            if (GUILayout.Button("Generate List"))
            {
                GenerateListProcess(eventDataList, eventDataList.includeSample);
            }

            EditorGUILayout.Space();

            DrawDefaultInspector();
        }

        /// <Summary>
        /// Generate a list which references event data in the project.
        /// </Summary>
        public void GenerateListProcess(EventDataList eventDataList, bool includeSample)
        {
            // Load all EventMasterData in the project.
            string filter = "t:" + typeof(EventMasterData).ToString();
            string[] guids = AssetDatabase.FindAssets(filter, null);

            // Add loaded data to eventList.
            List<EventMasterData> eventList = new List<EventMasterData>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EventMasterData data = (EventMasterData)AssetDatabase.LoadAssetAtPath(path, typeof(EventMasterData));
                if (data != null)
                {
                    if (includeSample)
                    {
                        eventList.Add(data);
                    }
                    else
                    {
                        if (!data.isSampleData)
                        {
                            eventList.Add(data);
                        }
                    }
                }
            }

            // Sort the list by data ID.
            var query = eventList.OrderBy(e => e.eventId);
            var sortedList = query.ToList();

            // Set generated list to ItemDataList data.
            eventDataList.eventDataList = sortedList;

            // Save asset.
            string listAssetPath = AssetDatabase.GetAssetPath(eventDataList);
            var asset = (EventDataList)AssetDatabase.LoadAssetAtPath(listAssetPath, typeof(EventDataList));
            EditorUtility.CopySerialized(eventDataList, asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}