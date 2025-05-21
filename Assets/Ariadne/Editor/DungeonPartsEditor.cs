using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Ariadne
{
    /// <Summary>
    /// Editor script for dungeon parts data.
    /// </Summary>
    public class DungeonPartsEditor : EditorWindow
    {
        [SerializeField]
        DungeonPartsData dungeonPartsData;

        List<MapAttributeData> mapAttributeDataList;
        int removeAttributeDataIndex = -1;

        DungeonPartsRecord removeTargetRecord;

        Vector2 scrollPos = Vector2.zero;

        int[] heightAnchorValues = new int[]{};
        string[] heightAnchorLabels = new string[]{};

        readonly string DefaultPartsDataName = "DungeonParts";
        readonly string UndoNamePartsChange = "Changed Dungeon Parts Record";

        /// <Summary>
        /// Open DungeonPartsEditor window.
        /// </Summary>
        [MenuItem("Window/Ariadne/DungeonPartsEditor")]
        static void Open()
        {
            var window = GetWindow<DungeonPartsEditor>();
            GUIContent icon = EditorGUIUtility.IconContent("PreTextureAlpha");
            window.titleContent = new GUIContent("DungeonPartsEditor", icon.image);
        }

        /// <Summary>
        /// Processes of opening MapAttributeEditor window.
        /// </Summary>
        public void Awake()
        {
            InitializeDugeonPartsEditor();
        }

        void OnDestroy()
        {
            SaveDungeonPartsData();
            Undo.ClearAll();
        }

        /// <Summary>
        /// When lost focus, save the asset file.
        /// </Summary>
        void OnLostFocus()
        {
            SaveDungeonPartsData();
        }

        /// <Summary>
        /// Show dungeon parts information.
        /// </Summary>
        void OnGUI()
        {
            EditorGUILayout.LabelField("Ariadne Dungeon Parts Editor");
            EditorGUILayout.Space();

            // Show a file pane.
            ShowDungeonPartsFileParts();
            EditorGUILayout.Space();

            // Show a reference attribute pane.
            ShowReferenceAttributeFileParts();
            EditorGUILayout.Space();

            // Show button parts.
            ShowOperationButtonParts();
            EditorGUILayout.Space();

            // Show labels.
            ShowLabelParts();

            // Show event data mappings.
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box);
            ShowPartsData();
            EditorGUILayout.EndScrollView();
        }

        /// <Summary>
        /// Show setting GUI for an dungeon parts data file.
        /// </Summary>
        void ShowDungeonPartsFileParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    DungeonPartsData partsData = (DungeonPartsData)EditorGUILayout.ObjectField("DungeonParts File", dungeonPartsData, typeof(DungeonPartsData), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(this, "Change DungeonPartsData");
                        dungeonPartsData = partsData;
                    }

                    if (dungeonPartsData != null)
                    {
                        // Save Button
                        if (!dungeonPartsData.isSystemData)
                        {
                            if (GUILayout.Button(EditorStyles.SaveButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                SaveDungeonPartsData();
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (dungeonPartsData == null)
                {
                    // Information
                    EditorGUILayout.HelpBox("Assign DungeonPartsData. You can create DungeonPartsData at [Create] -> [Ariadne] -> [DungeonPartsData].", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show reference GUI for an map attribute data file.
        /// </Summary>
        void ShowReferenceAttributeFileParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("Map Attribute");
                EditorGUILayout.Space();

                for (int i = 0; i < mapAttributeDataList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginChangeCheck();
                        MapAttributeData attrData = (MapAttributeData)EditorGUILayout.ObjectField("Attribute File " + i, mapAttributeDataList[i], typeof(MapAttributeData), false);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, "Change MapAttributeData");
                            mapAttributeDataList[i] = attrData;
                        }

                        if (mapAttributeDataList.Count > 1)
                        {
                            if (GUILayout.Button(EditorStyles.RemoveButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                removeAttributeDataIndex = i;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (mapAttributeDataList.Count == 1 && mapAttributeDataList[0] == null)
                {
                    EditorGUILayout.HelpBox("Assign MapAttributeData. You can create MapAttributeData at [Create] -> [Ariadne] -> [MapAttributeData].", MessageType.Info);
                }

                // Check remove target record.
                if (removeAttributeDataIndex != -1)
                {
                    Undo.RecordObject(this, "Remove Map Attribute Data");
                    mapAttributeDataList.RemoveAt(removeAttributeDataIndex);
                    Repaint();
                }
                removeAttributeDataIndex = -1;

                EditorGUILayout.Space();

                // Add Button
                if (GUILayout.Button(EditorStyles.AddButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                {
                    MapAttributeData attrData = null;
                    mapAttributeDataList.Add(attrData);
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
                    if (dungeonPartsData != null)
                    {
                        if (dungeonPartsData.isSystemData)
                        {
                            EditorGUILayout.HelpBox("This is a system data. To add or edit DungeonPartsData, create your file." + '\n'
                                                    + "You can create DungeonPartsData at [Create] -> [Ariadne] -> [DungeonPartsData].", MessageType.Info);
                        }
                        else
                        {
                            // Add Button
                            if (GUILayout.Button(EditorStyles.AddButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                AddNewRecordToList();
                            }

                            // Sort Button
                            if (GUILayout.Button(EditorStyles.SortButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                SortRecords();
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Add a new record to DungeonPartsData.
        /// </Summary>
        void AddNewRecordToList()
        {
            // Add a new record.
            DungeonPartsRecord record = new DungeonPartsRecord();
            record.attributeId = 0;
            record.partsTypeId = 0;
            record.partsObject = null;
            record.heightAnchor = 0;

            Undo.RecordObject(dungeonPartsData, "Add New Parts Record");
            dungeonPartsData.dungeonPartsRecords.Add(record);

            // Save asset.
            SaveDungeonPartsData();
        }

        /// <Summary>
        /// Sort records of DungeonPartsData.
        /// </Summary>
        void SortRecords()
        {
            var query = dungeonPartsData.dungeonPartsRecords.OrderBy(parts => parts.attributeId)
                                                .ThenBy(parts => parts.partsTypeId);
            var sortedList = query.ToList();
            dungeonPartsData.dungeonPartsRecords = sortedList;

            // Save asset.
            SaveDungeonPartsData();
        }

        /// <Summary>
        /// Label parts of dungeon parts.
        /// </Summary>
        void ShowLabelParts()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Attribute ID", GUILayout.Width(EditorPreferences.LabelWidth));
                    EditorGUILayout.LabelField("Attribute Name", GUILayout.Width(EditorPreferences.NameLabelWidth + EditorPreferences.MapIconSize));
                    EditorGUILayout.LabelField("Parts Type ID", GUILayout.Width(EditorPreferences.LabelWidth));
                    EditorGUILayout.LabelField("Parts Prefab", GUILayout.Width(EditorPreferences.NameLabelWidth));
                    EditorGUILayout.LabelField("Height Anchor", GUILayout.Width(EditorPreferences.NameLabelWidth));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show all parts in data.
        /// </Summary>
        void ShowPartsData()
        {
            if (dungeonPartsData == null)
            {
                return;
            }

            if (dungeonPartsData.dungeonPartsRecords == null)
            {
                return;
            }

            if (dungeonPartsData.dungeonPartsRecords.Count == 0)
            {
                return;
            }

            foreach (DungeonPartsRecord record in dungeonPartsData.dungeonPartsRecords)
            {
                ShowDungeonPartsRecordParts(record);
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            }

            // Check remove target record.
            if (removeTargetRecord != null)
            {
                Undo.RecordObject(dungeonPartsData, "Remove Parts Record");
                dungeonPartsData.dungeonPartsRecords.Remove(removeTargetRecord);
                Repaint();
            }
            removeTargetRecord = null;
        }

        /// <Summary>
        /// Show each record of dungeon parts data.
        /// </Summary>
        /// <param name="dungeonPartsRecord">Each record of dungeon parts record.</param>
        void ShowDungeonPartsRecordParts(DungeonPartsRecord dungeonPartsRecord)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorPreferences.AttributeRecordHeight));
            {
                // Attribute ID
                if (dungeonPartsData.isSystemData)
                {
                    EditorGUILayout.TextField(dungeonPartsRecord.attributeId.ToString(), GUILayout.Width(EditorPreferences.LabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    int attrId = EditorGUILayout.IntField(dungeonPartsRecord.attributeId, GUILayout.Width(EditorPreferences.LabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(dungeonPartsData, UndoNamePartsChange);
                        dungeonPartsRecord.attributeId = attrId;
                    }
                }

                string attrName = "";
                Sprite attrIcon = null;
                if (mapAttributeDataList != null)
                {
                    MapAttributeRecord attrRecord = DataRecordUtil.GetMapAttributeRecordById(mapAttributeDataList, dungeonPartsRecord.attributeId);
                    if (attrRecord != null)
                    {
                        attrName = attrRecord.attributeName;
                        attrIcon = attrRecord.mapIcon;
                    }
                }

                // Attribute Map Icon
                if (attrIcon != null)
                {
                    Texture t = attrIcon.texture;
                    GUILayout.Box(t, GUILayout.Width(EditorPreferences.MapIconSize), GUILayout.Height(EditorPreferences.MapIconSize));
                }
                else
                {
                    GUILayout.Box("", GUILayout.Width(EditorPreferences.MapIconSize), GUILayout.Height(EditorPreferences.MapIconSize));
                }

                // Attribute Name
                EditorGUILayout.LabelField(attrName, GUILayout.Width(EditorPreferences.NameLabelWidth));

                // Parts Type ID
                if (dungeonPartsData.isSystemData)
                {
                    EditorGUILayout.IntField(dungeonPartsRecord.partsTypeId, GUILayout.Width(EditorPreferences.LabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    int typeId = EditorGUILayout.IntField(dungeonPartsRecord.partsTypeId, GUILayout.Width(EditorPreferences.LabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(dungeonPartsData, UndoNamePartsChange);
                        dungeonPartsRecord.partsTypeId = typeId;
                    }
                }

                // Parts Object
                if (dungeonPartsData.isSystemData)
                {
                    EditorGUILayout.ObjectField(dungeonPartsRecord.partsObject, typeof(GameObject), false, GUILayout.Width(EditorPreferences.NameLabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    GameObject partsObj = (GameObject) EditorGUILayout.ObjectField(dungeonPartsRecord.partsObject, typeof(GameObject), false, GUILayout.Width(EditorPreferences.NameLabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(dungeonPartsData, UndoNamePartsChange);
                        dungeonPartsRecord.partsObject = partsObj;
                    }
                }

                // Parts Height Anchor
                if (dungeonPartsData.isSystemData)
                {
                    EditorGUILayout.IntPopup((int)dungeonPartsRecord.heightAnchor, heightAnchorLabels, heightAnchorValues, GUILayout.Width(EditorPreferences.NameLabelWidth));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    int anchor = EditorGUILayout.IntPopup((int)dungeonPartsRecord.heightAnchor, heightAnchorLabels, heightAnchorValues, GUILayout.Width(EditorPreferences.NameLabelWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(dungeonPartsData, UndoNamePartsChange);
                        dungeonPartsRecord.heightAnchor = (PartsHeightAnchor) System.Enum.ToObject(typeof(PartsHeightAnchor), anchor);
                    }
                }

                // Remove Button
                if (!dungeonPartsData.isSystemData)
                {
                    if (GUILayout.Button(EditorStyles.RemoveButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                    {
                        removeTargetRecord = dungeonPartsRecord;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <Summary>
        /// Initialize state of DugeonPartsEditor.
        /// </Summary>
        void InitializeDugeonPartsEditor()
        {
            LoadDefaultPartsData();

            LoadDefaultMapAttributeData();

            SetUpAnchorArrays();

            SetCallbackForUndo();
        }

        /// <Summary>
        /// Load default file of DungeonPartsData.
        /// </Summary>
        void LoadDefaultPartsData()
        {
            // Load DefaultParts data.
            string[] guids = AssetDatabase.FindAssets(DefaultPartsDataName, null);

            DungeonPartsData partsData = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                partsData = AssetDatabase.LoadAssetAtPath<DungeonPartsData>(assetPath);
                if (partsData != null)
                {
                    break;
                }
            }
            dungeonPartsData = partsData;
        }

        /// <Summary>
        /// Load default file of MapAttributeData.
        /// </Summary>
        void LoadDefaultMapAttributeData()
        {
            // Load default MapAttributeData data to reference map attribute names.
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
            mapAttributeDataList = new List<MapAttributeData>(){attrData};
        }

        /// <Summary>
        /// Set up height anchor labels.
        /// </Summary>
        void SetUpAnchorArrays()
        {
            heightAnchorValues = new int[]{(int)PartsHeightAnchor.Center, (int)PartsHeightAnchor.Ground, (int)PartsHeightAnchor.Ceiling};
            heightAnchorLabels = new string[]{PartsHeightAnchor.Center.ToString(), PartsHeightAnchor.Ground.ToString(), PartsHeightAnchor.Ceiling.ToString()};
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
            ClearUndoStack();
        }

        /// <Summary>
        /// Save DungeonPartsData data.
        /// </Summary>
        void SaveDungeonPartsData()
        {
            string path = AssetDatabase.GetAssetPath(dungeonPartsData);
            var asset = (DungeonPartsData)AssetDatabase.LoadAssetAtPath(path, typeof(DungeonPartsData));
            if (asset != null)
            {
                EditorUtility.CopySerialized(dungeonPartsData, asset);
            }
            AssetDatabase.Refresh();
        }
    }
}