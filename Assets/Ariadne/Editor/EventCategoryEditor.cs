using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Ariadne
{
    /// <Summary>
    /// Editor script for event category data.
    /// </Summary>
    public class EventCategoryEditor : EditorWindow
    {
        [SerializeField]
        EventCategoryData eventCategoryData;
        EventCategoryRecord removeTargetRecord;

        Vector2 scrollPos = Vector2.zero;

        List<EventArgumentTypeData> eventArgumentTypeDataList;
        EventArgs removeTargetArg;

        int[] argTypeIds;
        string[] argTypeDisplayNames;

        bool showChangeIdPane = false;
        int changeFromId = 0;
        int changeToId = 0;
        bool canChangeFromId = false;
        bool canChangeToId = false;
        string fromMessage = "";
        string toMessage = "";
        
        readonly string UndoNameAttributeChange = "Changed Event Category Record";

        /// <Summary>
        /// Open EventCategoryEditor window.
        /// </Summary>
        [MenuItem("Window/Ariadne/EventCategoryEditor")]
        static void Open()
        {
            var window = GetWindow<EventCategoryEditor>();
            GUIContent icon = EditorGUIUtility.IconContent("PreTextureAlpha");
            window.titleContent = new GUIContent("EventCategoryEditor", icon.image);
        }

        /// <Summary>
        /// Processes of opening EventCategoryEditor window.
        /// </Summary>
        public void Awake()
        {
            InitializeEventCategoryEditor();
        }

        void OnDestroy()
        {
            SaveEventCategoryData();
            ClearUndoStack();
        }

        /// <Summary>
        /// When lost focus, save the asset file.
        /// </Summary>
        void OnLostFocus()
        {
            SaveEventCategoryData();
        }

        /// <Summary>
        /// Show map attributes information.
        /// </Summary>
        void OnGUI()
        {
            EditorGUILayout.LabelField("Ariadne Event Category Editor");
            EditorGUILayout.Space();

            // Show a file pane.
            ShowEventCategoryFileParts();
            EditorGUILayout.Space();

            // Show button parts.
            ShowOperationButtonParts();
            EditorGUILayout.Space();

            // Show labels.
            ShowLabelParts();

            // Show event data mappings.
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box);
            ShowEventCategoryData();
            EditorGUILayout.EndScrollView();
        }

        /// <Summary>
        /// Show setting GUI for an event category data file.
        /// </Summary>
        void ShowEventCategoryFileParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    EventCategoryData typeData = (EventCategoryData)EditorGUILayout.ObjectField("Event Category File", eventCategoryData, typeof(EventCategoryData), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(this, "Change EventCategoryData");
                        eventCategoryData = typeData;
                        CheckEventCategoryRecord();
                    }

                    if (eventCategoryData != null)
                    {
                        // Save Button
                        if (!eventCategoryData.isSystemData)
                        {
                            if (GUILayout.Button(EditorStyles.SaveButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                SaveEventCategoryData();
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (eventCategoryData == null)
                {
                    // Information
                    EditorGUILayout.HelpBox("Assign EventCategoryData. You can create EventCategoryData at [Create] -> [Ariadne] -> [EventCategoryData].", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show buttons for operate event category records.
        /// </Summary>
        void ShowOperationButtonParts()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (eventCategoryData != null)
                    {
                        if (eventCategoryData.isSystemData)
                        {
                            EditorGUILayout.HelpBox("This is a system data. To add or edit EventCategories, create your file." + '\n'
                                                    + "You can create EventCategoryData at [Create] -> [Ariadne] -> [EventCategoryData].", MessageType.Info);
                        }
                        else
                        {
                            // Add Button
                            if (GUILayout.Button(EditorStyles.AddButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                AddNewRecordToList();
                            }

                            // Change ID Button
                            if (GUILayout.Button(EditorStyles.ChangeIdButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                showChangeIdPane = !showChangeIdPane;
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                if (showChangeIdPane)
                {
                    ShowChangeIdParts();
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Add a new record to EventCategoryData.
        /// </Summary>
        void AddNewRecordToList()
        {
            // Add a new record.
            EventCategoryRecord eventCategoryRecord = new EventCategoryRecord();
            eventCategoryRecord.eventCategoryId = GetNewRecordId();
            eventCategoryRecord.eventCategoryName = "";
            eventCategoryRecord.scriptPrefab = null;
            eventCategoryRecord.eventArgs = new List<EventArgs>();

            Undo.RecordObject(eventCategoryData, "Add New Event Category Record");
            eventCategoryData.eventCategoryRecords.Add(eventCategoryRecord);

            // Save asset.
            SaveEventCategoryData();
        }

        /// <Summary>
        /// Generate ID for the new record.
        /// </Summary>
        int GetNewRecordId()
        {
            int newId = DefaultEventCategory.EventCategoryId;
            if (eventCategoryData.eventCategoryRecords == null)
            {
                return newId;
            }

            if (eventCategoryData.eventCategoryRecords.Count == 0)
            {
                return newId;
            }

            newId = eventCategoryData.eventCategoryRecords.Max(cat => cat.eventCategoryId) + 1;
            return newId;
        }

        /// <Summary>
        /// Show GUI parts for change ID of an event category.
        /// </Summary>
        void ShowChangeIdParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                // Header
                EditorGUILayout.LabelField("Chenge Category ID");

                EditorGUILayout.Space();

                // From ID
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    int fromId = EditorGUILayout.IntField("From ID", changeFromId, GUILayout.Width(EditorPreferences.ChangeIdWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        changeFromId = fromId;
                        EventCategoryRecord record = eventCategoryData.eventCategoryRecords.Find(cat => cat.eventCategoryId == fromId);
                        if (record == null)
                        {
                            fromMessage = "Specified ID is missing.";
                            canChangeFromId = false;
                        }
                        else
                        {
                            canChangeFromId = true;
                        }
                    }

                    if (!canChangeFromId && fromMessage != "")
                    {
                        EditorGUILayout.HelpBox(fromMessage, MessageType.Warning);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

                // To ID
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    int toId = EditorGUILayout.IntField("To ID", changeToId, GUILayout.Width(EditorPreferences.ChangeIdWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        changeToId = toId;
                        EventCategoryRecord record = eventCategoryData.eventCategoryRecords.Find(cat => cat.eventCategoryId == toId);
                        if (record == null)
                        {
                            if (changeToId < 0)
                            {
                                toMessage = "The category ID should be greater or equal 0.";
                                canChangeToId = false;
                            }
                            else
                            {
                                canChangeToId = true;
                            }
                        }
                        else
                        {
                            toMessage = "Specified ID already exists.";
                            canChangeToId = false;
                        }
                    }

                    if (!canChangeToId && toMessage != "")
                    {
                        EditorGUILayout.HelpBox(toMessage, MessageType.Warning);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

                if (canChangeFromId && canChangeToId)
                {
                    if (GUILayout.Button(EditorStyles.ApplyButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                    {
                        ChangeCategoryId();
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Change the event category ID.
        /// This method affects all EventMasterData. 
        /// </Summary>
        void ChangeCategoryId()
        {
            // Change ID in EventCategoryRecord.
            EventCategoryRecord record = eventCategoryData.eventCategoryRecords.Find(cat => cat.eventCategoryId == changeFromId);
            if (record == null)
            {
                return;
            }

            record.eventCategoryId = changeToId;

            // Reorder records in EventCategoryData.
            var query = eventCategoryData.eventCategoryRecords.OrderBy(cat => cat.eventCategoryId);
            var sortedList = query.ToList();
            eventCategoryData.eventCategoryRecords = sortedList;

            // Check EventMasterData.
            string filter = "t:" + typeof(EventMasterData).ToString();
            string[] guids = AssetDatabase.FindAssets(filter, null);

            List<EventMasterData> eventList = new List<EventMasterData>();
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                EventMasterData eventMasterData = AssetDatabase.LoadAssetAtPath<EventMasterData>(assetPath);
                if (eventMasterData != null)
                {
                    eventList.Add(eventMasterData);
                }
            }

            List<EventMasterData> saveTargetList = new List<EventMasterData>();
            foreach (EventMasterData data in eventList)
            {
                if (data.eventParts == null)
                {
                    continue;
                }

                foreach (AriadneEventParts parts in data.eventParts)
                {
                    if (parts.eventProcessList == null)
                    {
                        continue;
                    }

                    foreach (EventProcessData process in parts.eventProcessList)
                    {
                        if (process.eventCategoryId == changeFromId)
                        {
                            process.eventCategoryId = changeToId;

                            if (!saveTargetList.Contains(data))
                            {
                                saveTargetList.Add(data);
                            }
                        }
                    }
                }
            }

            // Serialize changed EventMasterData.
            foreach (EventMasterData data in saveTargetList)
            {
                string path = AssetDatabase.GetAssetPath(data);
                var asset = (EventMasterData)AssetDatabase.LoadAssetAtPath(path, typeof(EventMasterData));
                if (asset == null)
                {
                    AssetDatabase.CreateAsset(data, path);
                }
                else
                {
                    EditorUtility.CopySerialized(data, asset);
                    AssetDatabase.SaveAssets();
                }
            }
            AssetDatabase.Refresh();

            changeFromId = 0;
            changeToId = 0;

            fromMessage = "";
            toMessage = "";

            canChangeFromId = false;
            canChangeToId = false;
        }

        /// <Summary>
        /// Label parts of map attributes.
        /// </Summary>
        void ShowLabelParts()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(EditorPreferences.RemoveRecordButtonWidth));
                    EditorGUILayout.LabelField("ID", GUILayout.Width(EditorPreferences.IdLabelWidth));
                    EditorGUILayout.LabelField("Event Category Name", GUILayout.Width(EditorPreferences.NameLabelWidth));
                    EditorGUILayout.LabelField("Prefab Object", GUILayout.Width(EditorPreferences.NameLabelWidth));
                    EditorGUILayout.LabelField("Arguments", GUILayout.Width(EditorPreferences.NameLabelWidth));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show all event categories in data.
        /// </Summary>
        void ShowEventCategoryData()
        {
            if (eventCategoryData == null)
            {
                return;
            }

            if (eventCategoryData.eventCategoryRecords == null)
            {
                return;
            }

            if (eventCategoryData.eventCategoryRecords.Count == 0)
            {
                return;
            }

            var query = eventCategoryData.eventCategoryRecords.OrderBy(et => et.eventCategoryId);
            var sortedList = query.ToList();

            foreach (EventCategoryRecord record in sortedList)
            {
                ShowEventCategoryRecordParts(record);
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            }

            // Check remove target record.
            if (removeTargetRecord != null)
            {
                Undo.RecordObject(eventCategoryData, "Remove Category Record");
                eventCategoryData.eventCategoryRecords.Remove(removeTargetRecord);
                Repaint();
            }
            removeTargetRecord = null;
        }

        /// <Summary>
        /// Show each record of event category data.
        /// </Summary>
        /// <param name="eventCategoryRecord">Each record of event category data.</param>
        void ShowEventCategoryRecordParts(EventCategoryRecord eventCategoryRecord)
        {
            EditorGUILayout.BeginHorizontal();
            {
                // Remove Record Button
                if (!eventCategoryData.isSystemData)
                {
                    if (GUILayout.Button(EditorStyles.RemoveRecordButtonContent, GUILayout.Width(EditorPreferences.RemoveRecordButtonWidth)))
                    {
                        removeTargetRecord = eventCategoryRecord;
                    }
                }

                // Event Category ID
                EditorGUILayout.LabelField(eventCategoryRecord.eventCategoryId.ToString(), GUILayout.Width(EditorPreferences.IdLabelWidth));

                // Event Category Name
                if (eventCategoryData.isSystemData)
                {
                    EditorGUILayout.TextField(eventCategoryRecord.eventCategoryName, GUILayout.Width(EditorPreferences.NameLabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    string typeName = EditorGUILayout.TextField(eventCategoryRecord.eventCategoryName, GUILayout.Width(EditorPreferences.NameLabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(eventCategoryData, UndoNameAttributeChange);
                        eventCategoryRecord.eventCategoryName = typeName;
                    }
                }

                // Script Prefab
                if (eventCategoryData.isSystemData)
                {
                    EditorGUILayout.ObjectField(eventCategoryRecord.scriptPrefab, typeof(GameObject), false, GUILayout.Width(EditorPreferences.NameLabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    GameObject prefab = (GameObject) EditorGUILayout.ObjectField(eventCategoryRecord.scriptPrefab, typeof(GameObject), false, GUILayout.Width(EditorPreferences.NameLabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(eventCategoryData, UndoNameAttributeChange);
                        eventCategoryRecord.scriptPrefab = prefab;
                    }
                }

                // Event Arguments
                EditorGUILayout.BeginVertical();
                {
                    if (eventCategoryRecord.eventArgs.Count > 0)
                    {
                        foreach (EventArgs args in eventCategoryRecord.eventArgs)
                        {
                            ShowEventArgsParts(args);
                            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                        }
                    }

                    // Check remove target record.
                    if (removeTargetArg != null)
                    {
                        Undo.RecordObject(eventCategoryData, "Remove Arg Record");
                        eventCategoryRecord.eventArgs.Remove(removeTargetArg);
                        Repaint();
                    }
                    removeTargetArg = null;
                    
                    if (!eventCategoryData.isSystemData)
                    {
                        if (GUILayout.Button(EditorStyles.AddArgumentButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                        {
                            AddNewEventArgument(eventCategoryRecord);
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <Summary>
        /// Show each record of event args.
        /// </Summary>
        /// <param name="args">Event argument data.</param>
        void ShowEventArgsParts(EventArgs args)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    // Event Arg Name
                    if (eventCategoryData.isSystemData)
                    {
                        EditorGUILayout.TextField("Arg Name", args.argName);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        string argName = EditorGUILayout.TextField("Arg Name", args.argName);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(eventCategoryData, UndoNameAttributeChange);
                            args.argName = argName;
                        }
                    }

                    // Event Arg Type ID
                    if (eventCategoryData.isSystemData)
                    {
                        EditorGUILayout.IntPopup("Arg Type", args.argTypeId, argTypeDisplayNames, argTypeIds);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        int argId = EditorGUILayout.IntPopup("Arg Type", args.argTypeId, argTypeDisplayNames, argTypeIds);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(eventCategoryData, UndoNameAttributeChange);
                            args.argTypeId = argId;
                        }
                    }

                    // Event Arg Is List
                    if (eventCategoryData.isSystemData)
                    {
                        EditorGUILayout.Toggle("Is List", args.isList);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        bool isList = EditorGUILayout.Toggle("Is List", args.isList);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(eventCategoryData, UndoNameAttributeChange);
                            args.isList = isList;
                        }
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                // Event Arg Remove Button
                if (!eventCategoryData.isSystemData)
                {
                    if (GUILayout.Button(EditorStyles.RemoveArgumentButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                    {
                        removeTargetArg = args;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <Summary>
        /// Add a new event argument to EventArgs list.
        /// </Summary>
        void AddNewEventArgument(EventCategoryRecord eventCategoryRecord)
        {
            if (eventCategoryRecord.eventArgs == null)
            {
                eventCategoryRecord.eventArgs = new List<EventArgs>();
            }

            EventArgs newArg = new EventArgs();
            newArg.argName = "";
            newArg.argTypeId = 0;
            newArg.isList = false;

            newArg.argListValue = new List<string>();
            newArg.argObjListValue = new List<UnityEngine.Object>();

            eventCategoryRecord.eventArgs.Add(newArg);
        }

        /// <Summary>
        /// Initialize state of EventCategoryEditor.
        /// </Summary>
        void InitializeEventCategoryEditor()
        {
            LoadDefaultEventCategoryData();
            LoadDefaultEventArgumentTypeData();

            CheckEventCategoryRecord();

            SetEventArgTypeList();

            SetCallbackForUndo();
        }

        /// <Summary>
        /// Load default file of EventCategoryData.
        /// </Summary>
        void LoadDefaultEventCategoryData()
        {
            // Load default EventCategoryData data.
            string[] guids = AssetDatabase.FindAssets(DefaultEventCategory.DefaultEventCategoryDataName, null);

            EventCategoryData eventCategoryData = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                eventCategoryData = AssetDatabase.LoadAssetAtPath<EventCategoryData>(assetPath);
                if (eventCategoryData != null)
                {
                    break;
                }
            }
            this.eventCategoryData = eventCategoryData;
        }

        /// <Summary>
        /// Load EventArgumentTypeData files in the project.
        /// </Summary>
        void LoadDefaultEventArgumentTypeData()
        {
            // Load EventArgumentTypeData files.
            string filter = "t:" + typeof(EventArgumentTypeData).ToString();
            string[] guids = AssetDatabase.FindAssets(filter, null);

            eventArgumentTypeDataList = new List<EventArgumentTypeData>();
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                EventArgumentTypeData eventArgumentTypeData = AssetDatabase.LoadAssetAtPath<EventArgumentTypeData>(assetPath);
                if (eventArgumentTypeData != null)
                {
                    eventArgumentTypeDataList.Add(eventArgumentTypeData);
                }
            }
        }

        /// <Summary>
        /// Check if the loaded event category data has type records.
        /// If there is no record, this method adds a new record.
        /// </Summary>
        void CheckEventCategoryRecord()
        {
            if (eventCategoryData == null)
            {
                return;
            }

            if (eventCategoryData.eventCategoryRecords != null)
            {
                if (eventCategoryData.eventCategoryRecords.Count > 0)
                {
                    return;
                }
            }

            // Add a new record.
            EventCategoryRecord eventCategoryRecord = new EventCategoryRecord();
            eventCategoryRecord.eventCategoryId = DefaultEventCategory.EventCategoryId;
            eventCategoryRecord.eventCategoryName = DefaultEventCategory.EventCategoryName;
            eventCategoryRecord.scriptPrefab = null;
            eventCategoryRecord.eventArgs = new List<EventArgs>();

            eventCategoryData.eventCategoryRecords = new List<EventCategoryRecord>();
            eventCategoryData.eventCategoryRecords.Add(eventCategoryRecord);

            // Save asset.
            SaveEventCategoryData();
        }

        /// <Summary>
        /// Set up argument type lists.
        /// </Summary>
        void SetEventArgTypeList()
        {
            List<int> argTypeIdList = new List<int>();
            List<string> argTypeNameList = new List<string>();

            if (eventArgumentTypeDataList == null)
            {
                return;
            }

            foreach (EventArgumentTypeData data in eventArgumentTypeDataList)
            {
                if (data == null)
                {
                    continue;
                }

                if (data.eventArgTypeRecords == null)
                {
                    continue;
                }

                foreach (EventArgumentTypeRecord record in data.eventArgTypeRecords)
                {
                    argTypeIdList.Add(record.eventArgTypeId);
                    argTypeNameList.Add(record.eventArgTypeDisplayName);
                }
            }

            argTypeIds = argTypeIdList.ToArray();
            argTypeDisplayNames = argTypeNameList.ToArray();
        }

        /// <Summary>
        /// Register action for the callback of Undo/Redo.
        /// </Summary>
        void SetCallbackForUndo()
        {
            // To avoid additional registeration.
            Undo.undoRedoPerformed -= UndoRedoCallbackAction;
            Undo.undoRedoPerformed += UndoRedoCallbackAction;
        }

        /// <Summary>
        /// Define callback action of Undo/Redo.
        /// </Summary>
        void UndoRedoCallbackAction()
        {
            Repaint();
        }

        /// <Summary>
        /// Clear Undo/Redo stacks.
        /// </Summary>
        void ClearUndoStack()
        {
            Undo.FlushUndoRecordObjects();
            Undo.ClearAll();
        }

        /// <Summary>
        /// Save EventCategoryData data.
        /// </Summary>
        void SaveEventCategoryData()
        {
            string path = AssetDatabase.GetAssetPath(eventCategoryData);
            var asset = (EventCategoryData)AssetDatabase.LoadAssetAtPath(path, typeof(EventCategoryData));
            if (asset != null)
            {
                EditorUtility.CopySerialized(eventCategoryData, asset);
            }
            AssetDatabase.Refresh();
        }
    }
}