using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Ariadne
{
    /// <Summary>
    /// Show event mappings to check event data references.
    /// </Summary>
    public class EventMappingEditor : EditorWindow
    {
        List<EventCategoryData> eventCategoryDataList;

        List<EventMappingData> eventMappingList;
        List<FloorMapMasterData> floorMapDataList;
        EventDataList eventDataList;

        Vector2 scrollPos = Vector2.zero;
        string saveFileName;
        Texture2D highlightTex;
        Color highlightColor = new Color(1.0f, 1.0f, 0.5f, 0.3f);
        Color[] highlightColors;

        /// <Summary>
        /// Ready the event mapping list.
        /// </Summary>
        /// <param name="floorName">Name of the floor.</param>
        public void GetEventRef(string floorName)
        {
            // Set highlight color to the texture.
            highlightColors = new Color[1]{highlightColor};
            highlightTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            highlightTex.SetPixels(0, 0, 1, 1, highlightColors);
            highlightTex.Apply();

            // Initialize event list.
            eventMappingList = new List<EventMappingData>();
            LoadDefaultEventDataList();
            SetFloorMapDataList();
            this.saveFileName = floorName;
            LoadDefaultEventCategoryData();

            // Check FloorMapData in project files.
            foreach (FloorMapMasterData floorMapData in floorMapDataList)
            {
                string mapFileName = floorMapData.name;
                string mapName = floorMapData.floorName;
                for (int y = 0; y < floorMapData.floorSizeVertical; y++)
                {
                    for (int x = 0; x < floorMapData.floorSizeHorizontal; x++)
                    {
                        int index = x + y * floorMapData.floorSizeHorizontal;
                        int eventId = floorMapData.mapInfo[index].eventId;
                        if (eventId <= 0)
                        {
                            continue;
                        }

                        var eventData = eventDataList.eventDataList.Find(e => e.eventId == eventId);
                        if (eventData == null)
                        {
                            continue;
                        }

                        Vector2Int pos = new Vector2Int(x, y);
                        if (eventData.eventParts == null)
                        {
                            continue;
                        }

                        for (int i = 0; i < eventData.eventParts.Count; i++)
                        {
                            string eventCategoryName = GetEventCategoryNameById(eventData.eventParts[i].eventCategoryId);
                            EventMappingData mapping = new EventMappingData(eventData.eventId, eventData.eventName, i, eventData.eventParts[i].startCondition, eventCategoryName, mapFileName, mapName, pos);
                            eventMappingList.Add(mapping);
                        }
                    }
                }
            }

            foreach (EventMasterData eventData in eventDataList.eventDataList)
            {
                EventMappingData mapping = eventMappingList.Find((e) => e.eventId == eventData.eventId);
                if (mapping != null)
                {
                    continue;
                }

                if (eventData.eventParts == null)
                {
                    continue;
                }

                for (int i = 0; i < eventData.eventParts.Count; i++)
                {
                    string eventCategoryName = GetEventCategoryNameById(eventData.eventParts[i].eventCategoryId);
                    EventMappingData mappingData = new EventMappingData(eventData.eventId, eventData.eventName, i, eventData.eventParts[i].startCondition, eventCategoryName, MapEditorUtil.NotAssignedText, MapEditorUtil.NotAssignedText, Vector2Int.zero);
                    eventMappingList.Add(mappingData);
                }
            }
        }

        /// <Summary>
        /// Returns an event category name.
        /// </Summary>
        string GetEventCategoryNameById(int typeId)
        {
            EventCategoryRecord record = DataRecordUtil.GetEventCategoryRecordById(eventCategoryDataList, typeId);
            string eventCategory = "";
            if (record != null)
            {
                eventCategory = record.eventCategoryName;
            }
            return eventCategory;
        }

        /// <Summary>
        /// Load default file of EventDataList.
        /// </Summary>
        void LoadDefaultEventDataList()
        {
            // Load default EventDataList data.
            string[] guids = AssetDatabase.FindAssets(DefaultEventDataList.DefaultEventDataListName, null);

            EventDataList dataList = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                dataList = AssetDatabase.LoadAssetAtPath<EventDataList>(assetPath);
                if (dataList != null)
                {
                    break;
                }
            }
            eventDataList = dataList;
        }

        /// <Summary>
        /// Search floor map data in the project and set it to the list.
        /// </Summary>
        void SetFloorMapDataList()
        {
            floorMapDataList = new List<FloorMapMasterData>();

            string filter = "t:" + typeof(FloorMapMasterData).ToString();
            string[] guids = AssetDatabase.FindAssets(filter, null);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                FloorMapMasterData floorData = AssetDatabase.LoadAssetAtPath<FloorMapMasterData>(path);
                if (floorData == null)
                {
                    continue;
                }

                if (floorData.name != MapEditorUtil.TempFileName)
                {
                    floorMapDataList.Add(floorData);
                }
            }
        }

        /// <Summary>
        /// Show event mapping information.
        /// </Summary>
        void OnGUI()
        {
            EditorGUILayout.LabelField("Ariadne Event Mapping Viewer");
            EditorGUILayout.Space();

            // Show labels.
            ShowLabelParts();

            // Show event data mappings.
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box);
            ShowEventMapping();
            EditorGUILayout.EndScrollView();
        }

        /// <Summary>
        /// When lost focus, close the window and destroy this window object.
        /// </Summary>
        void OnLostFocus()
        {
            Close();
        }

        /// <Summary>
        /// Label parts of event mapping.
        /// </Summary>
        void ShowLabelParts()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Event ID", GUILayout.Width(EditorPreferences.IdLabelWidth));
                    EditorGUILayout.LabelField("Event Name", GUILayout.Width(EditorPreferences.NameLabelWidth));
                    EditorGUILayout.LabelField("Event Index", GUILayout.Width(EditorPreferences.LabelWidth));
                    EditorGUILayout.LabelField("Start Condition", GUILayout.Width(EditorPreferences.LabelWidth));
                    EditorGUILayout.LabelField("Map File Name", GUILayout.Width(EditorPreferences.NameLabelWidth));
                    EditorGUILayout.LabelField("Map Name", GUILayout.Width(EditorPreferences.NameLabelWidth));
                    EditorGUILayout.LabelField("Position", GUILayout.Width(EditorPreferences.LabelWidth));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show event mapping data based on the list that is created in GetEventRef method.
        /// </Summary>
        void ShowEventMapping()
        {
            if (eventMappingList == null)
            {
                return;
            }

            var query = eventMappingList.OrderBy(mapping => mapping.eventId)
                                        .ThenBy(mapping => mapping.mapName)
                                        .ThenBy(mapping => mapping.pos.x)
                                        .ThenBy(mapping => mapping.pos.y)
                                        .ThenBy(mapping => mapping.eventId);
            var sortedList = query.ToList();

            foreach (EventMappingData mapping in sortedList)
            {
                if (mapping == null)
                {
                    continue;
                }

                GUIStyle editingMap = new GUIStyle();
                Color backColor = GUI.backgroundColor;

                if (mapping.mapFileName == saveFileName)
                {
                    editingMap.normal.textColor = GUI.skin.label.normal.textColor;
                    editingMap.normal.background = highlightTex;
                }
                else if (mapping.mapFileName == MapEditorUtil.NotAssignedText)
                {
                    editingMap.normal.textColor = Color.magenta;
                }
                else
                {
                    editingMap.normal.textColor = GUI.skin.label.normal.textColor;
                    editingMap.normal.background = GUI.skin.label.normal.background;
                }
                ShowEventMappingParts(mapping, editingMap);
            }
        }

        /// <Summary>
        /// Mapping data parts of event mapping.
        /// </Summary>
        /// <param name="mapping">Event mapping data.</param>
        /// <param name="editingMap">Style to highlight.</param>
        void ShowEventMappingParts(EventMappingData mapping, GUIStyle editingMap)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(mapping.eventId.ToString(), GUILayout.Width(EditorPreferences.IdLabelWidth));
                EditorGUILayout.LabelField(mapping.eventName, GUILayout.Width(EditorPreferences.NameLabelWidth));
                EditorGUILayout.LabelField(mapping.eventIndex.ToString(), GUILayout.Width(EditorPreferences.LabelWidth));
                EditorGUILayout.LabelField(mapping.startCondition, GUILayout.Width(EditorPreferences.LabelWidth));
                EditorGUILayout.LabelField(mapping.mapFileName, editingMap, GUILayout.Width(EditorPreferences.NameLabelWidth));
                EditorGUILayout.LabelField(mapping.mapName, editingMap, GUILayout.Width(EditorPreferences.NameLabelWidth));
                EditorGUILayout.LabelField(mapping.pos.ToString(), GUILayout.Width(EditorPreferences.LabelWidth));
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <Summary>
        /// Load EventCategoryData files in the project.
        /// </Summary>
        void LoadDefaultEventCategoryData()
        {
            // Load EventCategoryData files.
            string filter = "t:" + typeof(EventCategoryData).ToString();
            string[] guids = AssetDatabase.FindAssets(filter, null);

            eventCategoryDataList = new List<EventCategoryData>();
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                EventCategoryData eventCategoryData = AssetDatabase.LoadAssetAtPath<EventCategoryData>(assetPath);
                if (eventCategoryData != null)
                {
                    eventCategoryDataList.Add(eventCategoryData);
                }
            }
        }
    }
}
