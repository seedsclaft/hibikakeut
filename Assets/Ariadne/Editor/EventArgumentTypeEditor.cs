using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Ariadne
{
    /// <Summary>
    /// Editor script for event argument type data.
    /// </Summary>
    public class EventArgumentTypeEditor : EditorWindow
    {
        [SerializeField]
        EventArgumentTypeData eventArgumentTypeData;

        Vector2 scrollPos = Vector2.zero;
        
        readonly string UndoNameAttributeChange = "Changed Event Argument Type Record";

        /// <Summary>
        /// Open EventArgumentTypeEditor window.
        /// </Summary>
        [MenuItem("Window/Ariadne/EventArgumentTypeEditor")]
        static void Open()
        {
            var window = GetWindow<EventArgumentTypeEditor>();
            GUIContent icon = EditorGUIUtility.IconContent("PreTextureAlpha");
            window.titleContent = new GUIContent("EventArgumentTypeEditor", icon.image);
        }

        /// <Summary>
        /// Processes of opening EventArgumentTypeEditor window.
        /// </Summary>
        public void Awake()
        {
            InitializeEventArgumentTypeEditor();
        }

        void OnDestroy()
        {
            SaveEventArgumentTypeData();
            ClearUndoStack();
        }

        /// <Summary>
        /// When lost focus, save the asset file.
        /// </Summary>
        void OnLostFocus()
        {
            SaveEventArgumentTypeData();
        }

        /// <Summary>
        /// Show event argument type information.
        /// </Summary>
        void OnGUI()
        {
            EditorGUILayout.LabelField("Ariadne Event Argument Type Editor");
            EditorGUILayout.Space();

            // Show a file pane.
            ShowEventArgTypeFileParts();
            EditorGUILayout.Space();

            // Show button parts.
            ShowOperationButtonParts();
            EditorGUILayout.Space();

            // Show labels.
            ShowLabelParts();

            // Show event argument data mappings.
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box);
            ShowEventArgumentTypeData();
            EditorGUILayout.EndScrollView();
        }

        /// <Summary>
        /// Show setting GUI for an event argument type data file.
        /// </Summary>
        void ShowEventArgTypeFileParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    EventArgumentTypeData typeData = (EventArgumentTypeData)EditorGUILayout.ObjectField("Event Argument Type File", eventArgumentTypeData, typeof(EventArgumentTypeData), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(this, "Change EventArgumentTypeData");
                        eventArgumentTypeData = typeData;
                        CheckEventArgumentTypeRecord();
                    }

                    if (eventArgumentTypeData != null)
                    {
                        // Save Button
                        if (!eventArgumentTypeData.isSystemData)
                        {
                            if (GUILayout.Button(EditorStyles.SaveButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                SaveEventArgumentTypeData();
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (eventArgumentTypeData == null)
                {
                    // Information
                    EditorGUILayout.HelpBox("Assign EventArgumentTypeData. You can create EventArgumentTypeData at [Create] -> [Ariadne] -> [EventArgumentTypeData].", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show buttons for operate event argument type records.
        /// </Summary>
        void ShowOperationButtonParts()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (eventArgumentTypeData != null)
                    {
                        if (eventArgumentTypeData.isSystemData)
                        {
                            EditorGUILayout.HelpBox("This is a system data. To add or edit EventArgumentTypeData, create your file." + '\n'
                                                    + "You can create EventCategoryData at [Create] -> [Ariadne] -> [EventArgumentTypeData].", MessageType.Info);
                        }
                        else
                        {
                            // Add Button
                            if (GUILayout.Button(EditorStyles.AddButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                AddNewRecordToList();
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Add a new record to EventArgumentTypeData.
        /// </Summary>
        void AddNewRecordToList()
        {
            // Add a new record.
            EventArgumentTypeRecord argTypeRecord = new EventArgumentTypeRecord();
            argTypeRecord.eventArgTypeId = GetNewRecordId();
            argTypeRecord.eventArgTypeDisplayName = "";
            argTypeRecord.eventArgTypeName = "";

            Undo.RecordObject(eventArgumentTypeData, "Add New Event Arg Type Record");
            eventArgumentTypeData.eventArgTypeRecords.Add(argTypeRecord);

            // Save asset.
            SaveEventArgumentTypeData();
        }

        /// <Summary>
        /// Generate ID for the new record.
        /// </Summary>
        int GetNewRecordId()
        {
            int newId = DefaultEventArgumentType.EventArgumentTypeId;
            if (eventArgumentTypeData.eventArgTypeRecords == null)
            {
                return newId;
            }

            if (eventArgumentTypeData.eventArgTypeRecords.Count == 0)
            {
                return newId;
            }

            newId = eventArgumentTypeData.eventArgTypeRecords.Max(arg => arg.eventArgTypeId) + 1;
            return newId;
        }

        /// <Summary>
        /// Label parts of event argument type.
        /// </Summary>
        void ShowLabelParts()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("ID", GUILayout.Width(EditorPreferences.IdLabelWidth));
                    EditorGUILayout.LabelField("Argument Display Name", GUILayout.Width(EditorPreferences.NameLabelWidth));
                    EditorGUILayout.LabelField("Argument Type Name", GUILayout.Width(EditorPreferences.NameLabelWidth));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show all event argument types in data.
        /// </Summary>
        void ShowEventArgumentTypeData()
        {
            if (eventArgumentTypeData == null)
            {
                return;
            }

            if (eventArgumentTypeData.eventArgTypeRecords == null)
            {
                return;
            }

            if (eventArgumentTypeData.eventArgTypeRecords.Count == 0)
            {
                return;
            }

            var query = eventArgumentTypeData.eventArgTypeRecords.OrderBy(et => et.eventArgTypeId);
            var sortedList = query.ToList();

            foreach (EventArgumentTypeRecord record in sortedList)
            {
                ShowEventArgumentTypeRecordParts(record);
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            }
        }

        /// <Summary>
        /// Show each record of event argument type data.
        /// </Summary>
        /// <param name="argTypeRecord">Each record of event argument type data.</param>
        void ShowEventArgumentTypeRecordParts(EventArgumentTypeRecord argTypeRecord)
        {
            EditorGUILayout.BeginHorizontal();
            {
                // Event Argument Type ID
                EditorGUILayout.LabelField(argTypeRecord.eventArgTypeId.ToString(), GUILayout.Width(EditorPreferences.IdLabelWidth));

                // Event Argument Type Display Name
                if (eventArgumentTypeData.isSystemData)
                {
                    EditorGUILayout.TextField(argTypeRecord.eventArgTypeDisplayName, GUILayout.Width(EditorPreferences.NameLabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    string displayName = EditorGUILayout.TextField(argTypeRecord.eventArgTypeDisplayName, GUILayout.Width(EditorPreferences.NameLabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(eventArgumentTypeData, UndoNameAttributeChange);
                        argTypeRecord.eventArgTypeDisplayName = displayName;
                    }
                }

                // Event Argument Type Name
                if (eventArgumentTypeData.isSystemData)
                {
                    EditorGUILayout.TextField(argTypeRecord.eventArgTypeName, GUILayout.Width(EditorPreferences.NameLabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    string typeName = EditorGUILayout.TextField(argTypeRecord.eventArgTypeName, GUILayout.Width(EditorPreferences.NameLabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(eventArgumentTypeData, UndoNameAttributeChange);
                        argTypeRecord.eventArgTypeName = typeName;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <Summary>
        /// Initialize state of EventArgumentTypeEditor.
        /// </Summary>
        void InitializeEventArgumentTypeEditor()
        {
            LoadDefaultEventArgumentTypeData();

            CheckEventArgumentTypeRecord();

            SetCallbackForUndo();
        }

        /// <Summary>
        /// Load default file of EventArgumentTypeEditor.
        /// </Summary>
        void LoadDefaultEventArgumentTypeData()
        {
            // Load default EventArgumentTypeData data.
            string[] guids = AssetDatabase.FindAssets(DefaultEventArgumentType.DefaultEventArgumentTypeDataName, null);

            EventArgumentTypeData argTypeData = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                argTypeData = AssetDatabase.LoadAssetAtPath<EventArgumentTypeData>(assetPath);
                if (argTypeData != null)
                {
                    break;
                }
            }
            this.eventArgumentTypeData = argTypeData;
        }

        /// <Summary>
        /// Check if the loaded event argument type data has type records.
        /// If there is no record, this method adds a new record.
        /// </Summary>
        void CheckEventArgumentTypeRecord()
        {
            if (eventArgumentTypeData == null)
            {
                return;
            }

            if (eventArgumentTypeData.eventArgTypeRecords != null)
            {
                if (eventArgumentTypeData.eventArgTypeRecords.Count > 0)
                {
                    return;
                }
            }

            // Add a new record.
            EventArgumentTypeRecord argTypeRecord = new EventArgumentTypeRecord();
            argTypeRecord.eventArgTypeId = DefaultEventArgumentType.EventArgumentTypeId;
            argTypeRecord.eventArgTypeDisplayName = DefaultEventArgumentType.EventArgumentTypeName;
            argTypeRecord.eventArgTypeName = "";

            eventArgumentTypeData.eventArgTypeRecords = new List<EventArgumentTypeRecord>();
            eventArgumentTypeData.eventArgTypeRecords.Add(argTypeRecord);

            // Save asset.
            SaveEventArgumentTypeData();
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
        /// Save EventArgumentTypeData data.
        /// </Summary>
        void SaveEventArgumentTypeData()
        {
            string path = AssetDatabase.GetAssetPath(eventArgumentTypeData);
            var asset = (EventArgumentTypeData)AssetDatabase.LoadAssetAtPath(path, typeof(EventArgumentTypeData));
            if (asset != null)
            {
                EditorUtility.CopySerialized(eventArgumentTypeData, asset);
            }
            AssetDatabase.Refresh();
        }
    }
}