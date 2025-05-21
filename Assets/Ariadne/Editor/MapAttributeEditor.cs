using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Ariadne
{
    /// <Summary>
    /// Editor script for map attribute data.
    /// </Summary>
    public class MapAttributeEditor : EditorWindow
    {
        [SerializeField]
        MapAttributeData mapAttributeData;

        Vector2 scrollPos = Vector2.zero;

        readonly string UndoNameAttributeChange = "Changed Map Attribute Record";

        /// <Summary>
        /// Open MapAttributeEditor window.
        /// </Summary>
        [MenuItem("Window/Ariadne/MapAttributeEditor")]
        static void Open()
        {
            var window = GetWindow<MapAttributeEditor>();
            GUIContent icon = EditorGUIUtility.IconContent("PreTextureAlpha");
            window.titleContent = new GUIContent("MapAttributeEditor", icon.image);
        }

        /// <Summary>
        /// Processes of opening MapAttributeEditor window.
        /// </Summary>
        public void Awake()
        {
            InitializeMapAttributeEditor();
        }

        void OnDestroy()
        {
            SaveMapAttributeData();
            ClearUndoStack();
        }

        /// <Summary>
        /// When lost focus, save the asset file.
        /// </Summary>
        void OnLostFocus()
        {
            SaveMapAttributeData();
        }

        /// <Summary>
        /// Show map attributes information.
        /// </Summary>
        void OnGUI()
        {
            EditorGUILayout.LabelField("Ariadne Map Attribute Editor");
            EditorGUILayout.Space();

            // Show a file pane.
            ShowMapAttributeFileParts();
            EditorGUILayout.Space();

            // Show button parts.
            ShowOperationButtonParts();
            EditorGUILayout.Space();

            // Show labels.
            ShowLabelParts();

            // Show event data mappings.
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box);
            ShowAttributeData();
            EditorGUILayout.EndScrollView();
        }

        /// <Summary>
        /// Show setting GUI for an attribute data file.
        /// </Summary>
        void ShowMapAttributeFileParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    MapAttributeData attrData = (MapAttributeData)EditorGUILayout.ObjectField("Attribute File", mapAttributeData, typeof(MapAttributeData), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(this, "Change MapAttributeData");
                        mapAttributeData = attrData;
                        CheckAttributeRecord();
                    }

                    if (mapAttributeData != null)
                    {
                        // Save Button
                        if (!mapAttributeData.isSystemData)
                        {
                            if (GUILayout.Button(EditorStyles.SaveButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                SaveMapAttributeData();
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (mapAttributeData == null)
                {
                    // Information
                    EditorGUILayout.HelpBox("Assign MapAttributeData. You can create MapAttributeData at [Create] -> [Ariadne] -> [MapAttributeData].", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show buttons for operate attribute records.
        /// </Summary>
        void ShowOperationButtonParts()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (mapAttributeData != null)
                    {
                        if (mapAttributeData.isSystemData)
                        {
                            EditorGUILayout.HelpBox("This is a system data. To add or edit MapAttributes, create your file." + '\n'
                                                    + "You can create MapAttributeData at [Create] -> [Ariadne] -> [MapAttributeData].", MessageType.Info);
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
        /// Add a new record to MapAttributeData.
        /// </Summary>
        void AddNewRecordToList()
        {
            // Add a new record.
            MapAttributeRecord mapAttrRecord = new MapAttributeRecord();
            mapAttrRecord.attributeId = GetNewRecordId();
            mapAttrRecord.attributeName = "";
            mapAttrRecord.mapIcon = null;
            mapAttrRecord.drawMapIcon = false;
            mapAttrRecord.drawIconOnEditor = false;
            mapAttrRecord.drawAsWallOnMap = false;
            mapAttrRecord.canWalk = false;
            mapAttrRecord.notInUse = false;

            Undo.RecordObject(mapAttributeData, "Add New Attribute Record");
            mapAttributeData.mapAttributeRecords.Add(mapAttrRecord);

            // Save asset.
            SaveMapAttributeData();
        }

        /// <Summary>
        /// Generate ID for the new record.
        /// </Summary>
        int GetNewRecordId()
        {
            int newId = DefaultAttribute.AttrId;
            if (mapAttributeData.mapAttributeRecords == null)
            {
                return newId;
            }

            newId = mapAttributeData.mapAttributeRecords.Count + DefaultAttribute.AttrId;
            return newId;
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
                    EditorGUILayout.LabelField("ID", GUILayout.Width(EditorPreferences.IdLabelWidth));
                    EditorGUILayout.LabelField("Name", GUILayout.Width(EditorPreferences.NameLabelWidth));
                    EditorGUILayout.LabelField("Icon", GUILayout.Width(EditorPreferences.IconLabelWidth));
                    EditorGUILayout.LabelField("Draw Map Icon", GUILayout.Width(EditorPreferences.LabelWidth));
                    EditorGUILayout.LabelField("Draw Editor Icon", GUILayout.Width(EditorPreferences.LabelWidth));
                    EditorGUILayout.LabelField("Draw As Wall On Map", GUILayout.Width(EditorPreferences.NameLabelWidth));
                    EditorGUILayout.LabelField("Can Walk", GUILayout.Width(EditorPreferences.LabelWidth));
                    EditorGUILayout.LabelField("Not in use", GUILayout.Width(EditorPreferences.LabelWidth));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show all attributes in data.
        /// </Summary>
        void ShowAttributeData()
        {
            if (mapAttributeData == null)
            {
                return;
            }

            if (mapAttributeData.mapAttributeRecords == null)
            {
                return;
            }

            if (mapAttributeData.mapAttributeRecords.Count == 0)
            {
                return;
            }

            var query = mapAttributeData.mapAttributeRecords.OrderBy(attr => attr.attributeId);
            var sortedList = query.ToList();

            foreach (MapAttributeRecord record in sortedList)
            {
                ShowMapAttributeRecordParts(record);
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            }
        }

        /// <Summary>
        /// Show each record of map attribute data.
        /// </Summary>
        /// <param name="mapAttributeRecord">Each record of map attribute data.</param>
        void ShowMapAttributeRecordParts(MapAttributeRecord mapAttributeRecord)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorPreferences.AttributeRecordHeight));
            {
                // Attribute ID
                EditorGUILayout.LabelField(mapAttributeRecord.attributeId.ToString(), GUILayout.Width(EditorPreferences.IdLabelWidth));

                // Attribute Name
                if (mapAttributeData.isSystemData)
                {
                    EditorGUILayout.TextField(mapAttributeRecord.attributeName, GUILayout.Width(EditorPreferences.NameLabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    string attrName = EditorGUILayout.TextField(mapAttributeRecord.attributeName, GUILayout.Width(EditorPreferences.NameLabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(mapAttributeData, UndoNameAttributeChange);
                        mapAttributeRecord.attributeName = attrName;
                    }
                }

                // Attribute Map Icon
                EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorPreferences.IconLabelWidth));
                {
                    if (mapAttributeRecord.mapIcon != null)
                    {
                        Texture t = mapAttributeRecord.mapIcon.texture;
                        GUILayout.Box(t, GUILayout.Width(EditorPreferences.MapIconSize), GUILayout.Height(EditorPreferences.MapIconSize));
                    }

                    if (mapAttributeData.isSystemData)
                    {
                        EditorGUILayout.ObjectField(mapAttributeRecord.mapIcon, typeof(Sprite), false);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        Sprite mapIconSp = (Sprite) EditorGUILayout.ObjectField(mapAttributeRecord.mapIcon, typeof(Sprite), false);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(mapAttributeData, UndoNameAttributeChange);
                            mapAttributeRecord.mapIcon = mapIconSp;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Draw Icon
                if (mapAttributeData.isSystemData)
                {
                    EditorGUILayout.Toggle(mapAttributeRecord.drawMapIcon, GUILayout.Width(EditorPreferences.LabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    bool drawIcon = EditorGUILayout.Toggle(mapAttributeRecord.drawMapIcon, GUILayout.Width(EditorPreferences.LabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(mapAttributeData, UndoNameAttributeChange);
                        mapAttributeRecord.drawMapIcon = drawIcon;
                    }
                }

                // Draw Icon On Editor
                if (mapAttributeData.isSystemData)
                {
                    EditorGUILayout.Toggle(mapAttributeRecord.drawIconOnEditor, GUILayout.Width(EditorPreferences.LabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    bool drawIconOnEditor = EditorGUILayout.Toggle(mapAttributeRecord.drawIconOnEditor, GUILayout.Width(EditorPreferences.LabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(mapAttributeData, UndoNameAttributeChange);
                        mapAttributeRecord.drawIconOnEditor = drawIconOnEditor;
                    }
                }

                // Draw As Wall On Map
                if (mapAttributeData.isSystemData)
                {
                    EditorGUILayout.Toggle(mapAttributeRecord.drawAsWallOnMap, GUILayout.Width(EditorPreferences.NameLabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    bool drawAsWallOnMap = EditorGUILayout.Toggle(mapAttributeRecord.drawAsWallOnMap, GUILayout.Width(EditorPreferences.NameLabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(mapAttributeData, UndoNameAttributeChange);
                        mapAttributeRecord.drawAsWallOnMap = drawAsWallOnMap;
                    }
                }

                // Can walk
                if (mapAttributeData.isSystemData)
                {
                    EditorGUILayout.Toggle(mapAttributeRecord.canWalk, GUILayout.Width(EditorPreferences.LabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    bool canWalkFlag = EditorGUILayout.Toggle(mapAttributeRecord.canWalk, GUILayout.Width(EditorPreferences.LabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(mapAttributeData, UndoNameAttributeChange);
                        mapAttributeRecord.canWalk = canWalkFlag;
                    }
                }

                // Not in use flag
                if (mapAttributeData.isSystemData)
                {
                    EditorGUILayout.Toggle(mapAttributeRecord.notInUse, GUILayout.Width(EditorPreferences.LabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    bool notInUseFlag = EditorGUILayout.Toggle(mapAttributeRecord.notInUse, GUILayout.Width(EditorPreferences.LabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(mapAttributeData, UndoNameAttributeChange);
                        mapAttributeRecord.notInUse = notInUseFlag;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <Summary>
        /// Initialize state of MapAttributeEditor.
        /// </Summary>
        void InitializeMapAttributeEditor()
        {
            LoadDefaultMapAttributeData();

            CheckAttributeRecord();

            SetCallbackForUndo();
        }

        /// <Summary>
        /// Load default file of MapAttributeData.
        /// </Summary>
        void LoadDefaultMapAttributeData()
        {
            // Load default MapAttributeData data.
            string[] guids = AssetDatabase.FindAssets(DefaultAttribute.DefaultMapAttrDataName, null);

            MapAttributeData attrData = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                attrData = AssetDatabase.LoadAssetAtPath<MapAttributeData>(assetPath);
                if (attrData != null)
                {
                    break;
                }
            }
            mapAttributeData = attrData;
        }

        /// <Summary>
        /// Check if the loaded attribute data has attribute records.
        /// If there is no record, this method adds a new record.
        /// </Summary>
        void CheckAttributeRecord()
        {
            if (mapAttributeData == null)
            {
                return;
            }

            if (mapAttributeData.mapAttributeRecords != null)
            {
                if (mapAttributeData.mapAttributeRecords.Count > 0)
                {
                    return;
                }
            }

            // Add a new record.
            MapAttributeRecord mapAttrRecord = new MapAttributeRecord();
            mapAttrRecord.attributeId = DefaultAttribute.AttrId;
            mapAttrRecord.attributeName = DefaultAttribute.AttrName;
            mapAttrRecord.mapIcon = null;
            mapAttrRecord.drawMapIcon = false;
            mapAttrRecord.drawIconOnEditor = false;
            mapAttrRecord.drawAsWallOnMap = false;
            mapAttrRecord.canWalk = true;
            mapAttrRecord.notInUse = false;

            mapAttributeData.mapAttributeRecords = new List<MapAttributeRecord>();
            mapAttributeData.mapAttributeRecords.Add(mapAttrRecord);

            // Save asset.
            SaveMapAttributeData();
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
        /// Save MapAttributeData data.
        /// </Summary>
        void SaveMapAttributeData()
        {
            string path = AssetDatabase.GetAssetPath(mapAttributeData);
            var asset = (MapAttributeData)AssetDatabase.LoadAssetAtPath(path, typeof(MapAttributeData));
            if (asset != null)
            {
                EditorUtility.CopySerialized(mapAttributeData, asset);
            }
            AssetDatabase.Refresh();
        }
    }
}