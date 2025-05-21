using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Editor class for floor map data.
    /// </Summary>
    public class MapEditor : EditorWindow
    {
        // Const
        const int MaxFloorSize = 64;
        const string DefaultFloorName = "New Floor";
        const string DefaultSaveFileName = "NewMap";
        const int DefaultFloorId = 0;
        readonly string UndoName = "Change Map Setting";

        readonly string DefaultPartsDataName = "DungeonParts";
        readonly string DefaultBasePartsDataName = "DungeonBaseParts";

        readonly string SaveDialogMessage = "Please enter a file name to save the MapData to";
        readonly string SaveEventDataDialogMessage = "Please select a folder to save the EventMasterData to";

        const int FileLoadSucceeded = 0;
        const int FileLoadFailedWithNull = 1;
        const int FileLoadFailedWithTemp = 2;

        readonly int SettingPaneWidth = 400;
        readonly int InfoLabelWidth = 190;
        
        // Floor map data settings
        [SerializeField]
        int floorSizeHorizontal = FloorMapConst.InitHorizontalSize;
        [SerializeField]
        int floorSizeVertical = FloorMapConst.InitVerticalSize;
        [SerializeField]
        int floorId = DefaultFloorId;
        [SerializeField]
        string floorName = DefaultFloorName;
        [SerializeField]
        List<DungeonPartsData> dungeonPartsDataList;
        int removeDungeonPartsDataIndex = -1;
        [SerializeField]
        DungeonBasePartsData dungeonBasePartsData;
        [SerializeField]
        Vector2Int entrancePos;
        [SerializeField]
        DungeonDir enteringDir;

        [SerializeField]
        List<MapInfo> editMapInfo;
        int selectedPosEventId = 0;
        int selectedMapIcon = 0;
        [SerializeField]
        DungeonDir selectedPosObjectFront;
        [SerializeField]
        int selectedObjectTypeId;

        [SerializeField]
        List<MapAttributeData> mapAttributeDataList;
        int removeAttributeDataIndex = -1;

        [SerializeField]
        EventMasterData selectedEventMasterData;

        [SerializeField]
        int sliderHorizontal = FloorMapConst.InitHorizontalSize;
        [SerializeField]
        int sliderVertical = FloorMapConst.InitVerticalSize;

        bool isShowingLoadPane;
        bool isShowingSavePane;

        // Draw map GUI
        readonly int CellSize = 32;
        Vector2Int selectedGridPos = Vector2Int.zero;
        Vector2Int hoverGridPos = Vector2Int.zero;
        Vector2Int dragStartGridPos = Vector2Int.zero;
        List<Vector2Int> dragPosList;
        Color baseColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        Color gridColor = new Color(0.7f, 0.9f, 0.7f, 1.0f);
        Color hoverColor = new Color(0.0f, 0.5f, 1.0f, 0.2f);
        Color selectedColor = new Color(0.0f, 0.5f, 1.0f, 0.5f);
        Color transparentColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        Color[] hoverColors;
        Color[] selectedColors;
        Color[] transparentColors;

        // Base layer texture (base color, grid)
        Texture2D mapBg;

        // Icon layer
        bool isPosSelected = false;

        // Map icon settings
        Texture2D[] mapIconList;
        int[] mapIconIdList;
        Texture2D hoverTex;
        Texture2D selectedTex;
        bool isDragging = false;
        int selectedDrawTool = 0;
        string[] drawToolNameList = new string[]{"Select", "Draw", "Draw rect", "Draw filled rect"};
        Texture2D[] drawToolIconList;
        Texture2D eventIcon;
        Texture2D entranceIcon;
        Texture2D warningIcon;

        // Setting about save file
        string assetRootPathPrefix;
        string tempFileName;
        string tempFilePath;
        string infoFileName;

        [SerializeField]
        string saveFileName = DefaultSaveFileName;
        string saveFilePath;

        // Setting about load file
        [SerializeField]
        FloorMapMasterData loadFloor;
        int loadFileState = FileLoadSucceeded;

        GUIStyle boldLabelStyle;

        Vector2 leftPaneScrollPos = Vector2.zero;
        Vector2 rightPaneScrollPos = Vector2.zero;
        
        EventDataList eventDataList;
        EventEditor eventEditor;
        EventMappingEditor eventMappingEditor;

        /// <Summary>
        /// Open MapEditor window.
        /// </Summary>
        [MenuItem("Window/Ariadne/MapEditor")]
        static void Open()
        {
            GetWindow<MapEditor>();
        }

        /// <Summary>
        /// Processes of opening MapEditor window.
        /// </Summary>
        public void Awake()
        {
            InitializeMapEditor();
            LoadTempFile();
        }

        /// <Summary>
        /// Initialize state of MapEditor.
        /// </Summary>
        void InitializeMapEditor()
        {
            SetInitialFilePath();
            SetColorArrays();

            LoadDefaultMapAttributeData();
            LoadDefaultEventDataList();

            SetHoverTexture();
            SetSelectedTexture();

            SetToolIcons();
            SetEventIcons();

            InitializePosSelection();
            InitializeDraggingEvent();

            SetCallbackForUndo();

            SetMapBackgroundTexture();
        }

        /// <Summary>
        /// Processes to open a new file.
        /// </Summary>
        void SetNewFile()
        {
            InitializePosSelection();
            SetInitialFilePath();
            InitializeDraggingEvent();
            InitializeProperties();

            LoadDefaultDungeonParts();
            LoadDefaultDungeonBaseParts();

            SetNewMapData();

            SetMapBackgroundTexture();
        }

        /// <Summary>
        /// When get focus, initialize map editor state.
        /// </Summary>
        void OnFocus()
        {
            InitializeMapEditor();
            LoadTempFileInfo();
        }

        /// <Summary>
        /// When lost focus, save the state of editing data to temp file.
        /// </Summary>
        void OnLostFocus()
        {
            SaveTempFile();
        }

        /// <Summary>
        /// When this window is destroyed, save the state of editing data to temp file.
        /// </Summary>
        void OnDestroy()
        {
            SaveTempFile();
        }

        /// <Summary>
        /// Show GUI for setting of the floor map data.
        /// </Summary>
        void OnGUI()
        {
            CheckCurrentIconData();
            InitializeStyles();
            EditorGUILayout.LabelField("Ariadne Map Data Editor", boldLabelStyle);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                leftPaneScrollPos = EditorGUILayout.BeginScrollView(leftPaneScrollPos, GUI.skin.box, GUILayout.Width(SettingPaneWidth + 24f));
                {
                    ShowFileOperationButtons();

                    ShowLoadMapFileParts();
                    ShowSaveMapFileParts();
                    EditorGUILayout.Space();

                    ShowMapAttributeParts();
                    EditorGUILayout.Space();

                    ShowDungeonPartsListParts();
                    EditorGUILayout.Space();

                    ShowFloorSettingParts();
                    EditorGUILayout.Space();

                    ShowSelectedPositionInformationParts();
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndScrollView();

                rightPaneScrollPos = EditorGUILayout.BeginScrollView(rightPaneScrollPos, GUI.skin.box);
                {
                    ShowToolBarParts();
                    EditorGUILayout.BeginHorizontal();
                    {
                        ShowVerticalScaleParts();
                        ShowMapParts();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <Summary>
        /// Show setting GUI for file operation.
        /// </Summary>
        void ShowFileOperationButtons()
        {
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(SettingPaneWidth));
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("New"))
                    {
                        bool dialogChoice = EditorUtility.DisplayDialog("Create new file",
                                                                            "Editing data will be discarded. Create new file?",
                                                                            "Create New",
                                                                            "Cancel");
                        if (dialogChoice)
                        {
                            SetNewFile();
                            ClearUndoStack();
                        }
                    }

                    if (GUILayout.Button("Load"))
                    {
                        isShowingLoadPane = !isShowingLoadPane;
                        isShowingSavePane = false;
                    }

                    if (GUILayout.Button("Save"))
                    {
                        isShowingSavePane = !isShowingSavePane;
                        isShowingLoadPane = false;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show setting GUI for loading file.
        /// </Summary>
        void ShowLoadMapFileParts()
        {
            if (!isShowingLoadPane)
            {
                return;
            }

            EditorGUILayout.BeginVertical("Box", GUILayout.Width(SettingPaneWidth));
            {
                EditorGUI.BeginChangeCheck();
                FloorMapMasterData floor = (FloorMapMasterData)EditorGUILayout.ObjectField("Load floor map data", loadFloor, typeof(FloorMapMasterData), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    loadFloor = floor;
                }
                EditorGUILayout.Space();

                if (GUILayout.Button("Load Map File"))
                {
                    CheckLoadFileProcess();
                }
                CheckLoadingWarn();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Check the specified file and load it.
        /// </Summary>
        void CheckLoadFileProcess()
        {
            if (loadFloor == null)
            {
                loadFileState = FileLoadFailedWithNull;
                return;
            }

            if (loadFloor.name == MapEditorUtil.TempFileName)
            {
                loadFileState = FileLoadFailedWithTemp;
                return;
            }

            loadFileState = FileLoadSucceeded;
            bool dialogChoice = EditorUtility.DisplayDialog("Load floor map data",
                                                            "Editing data will be discarded. Open this file?",
                                                            "OK",
                                                            "Cancel");
            if (dialogChoice)
            {
                InitializePosSelection();
                LoadMapFile();
                SetMapBackgroundTexture();
                ClearUndoStack();
            }
        }

        /// <Summary>
        /// Check warnings about loading the file.
        /// </Summary>
        void CheckLoadingWarn()
        {
            switch (loadFileState)
            {
                case FileLoadFailedWithNull:
                    EditorGUILayout.HelpBox("Load file is not assigned.", MessageType.Error);
                    break;
                case FileLoadFailedWithTemp:
                    EditorGUILayout.HelpBox("You can not load the temp file directly.", MessageType.Error);
                    break;
            }
        }

        /// <Summary>
        /// Show setting GUI for saving the file.
        /// </Summary>
        void ShowSaveMapFileParts()
        {
            if (!isShowingSavePane)
            {
                return;
            }
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(SettingPaneWidth));
            {
                EditorGUI.BeginChangeCheck();
                string fileName = EditorGUILayout.TextField("File Name", saveFileName);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    saveFileName = fileName;
                }

                if (saveFileName == "")
                {
                    EditorGUILayout.HelpBox("Input map file name.", MessageType.Error);
                }
                EditorGUILayout.Space();

                if (GUILayout.Button("Save Map File"))
                {
                    // Initialize the save path.
                    saveFilePath = "";
                    saveFilePath = EditorUtility.SaveFilePanelInProject("Save Map File.", saveFileName, "asset", SaveDialogMessage);

                    if (saveFilePath.Length != 0)
                    {
                        SaveMapFile();
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show setting GUI about map attributes.
        /// </Summary>
        void ShowMapAttributeParts()
        {
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(SettingPaneWidth));
            {
                // Assign MapAttributeData
                EditorGUILayout.LabelField("Map Attribute", boldLabelStyle);
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
        /// Show setting GUI about dungeon parts.
        /// </Summary>
        void ShowDungeonPartsListParts()
        {
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(SettingPaneWidth));
            {
                // Assign DungeonPartsData
                EditorGUILayout.LabelField("Dungeon Base Parts", boldLabelStyle);
                EditorGUILayout.Space();

                // Dungeon base parts data
                EditorGUI.BeginChangeCheck();
                DungeonBasePartsData basePartsData = (DungeonBasePartsData)EditorGUILayout.ObjectField("Dungeon base parts data", dungeonBasePartsData, typeof(DungeonBasePartsData), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    dungeonBasePartsData = basePartsData;
                }
                EditorGUILayout.Space();
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

                EditorGUILayout.LabelField("Dungeon Parts", boldLabelStyle);
                EditorGUILayout.Space();

                // Dungeon parts data
                for (int i = 0; i < dungeonPartsDataList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginChangeCheck();
                        DungeonPartsData partsData = (DungeonPartsData)EditorGUILayout.ObjectField("Parts File " + i, dungeonPartsDataList[i], typeof(DungeonPartsData), false);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, "Change DungeonPartsData");
                            dungeonPartsDataList[i] = partsData;
                        }

                        if (dungeonPartsDataList.Count > 1)
                        {
                            if (GUILayout.Button(EditorStyles.RemoveButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                removeDungeonPartsDataIndex = i;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // Check remove target record.
                if (removeDungeonPartsDataIndex != -1)
                {
                    Undo.RecordObject(this, "Remove Dungeon Parts Data");
                    dungeonPartsDataList.RemoveAt(removeDungeonPartsDataIndex);
                    Repaint();
                }
                removeDungeonPartsDataIndex = -1;

                EditorGUILayout.Space();

                // Add Button
                if (GUILayout.Button(EditorStyles.AddButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                {
                    DungeonPartsData partsData = null;
                    dungeonPartsDataList.Add(partsData);
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show setting GUI about the floor.
        /// </Summary>
        void ShowFloorSettingParts()
        {
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(SettingPaneWidth));
            {
                // Input size of dungeon
                EditorGUILayout.LabelField("Floor Settings", boldLabelStyle);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Floor Size");

                // Floor size - Horizontal
                int preSizeHorizontal = floorSizeHorizontal;
                EditorGUI.BeginChangeCheck();
                int horizontal = EditorGUILayout.IntSlider("Horizontal Size", sliderHorizontal, 1, MaxFloorSize);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    sliderHorizontal = horizontal;
                }

                if (sliderHorizontal <= 0)
                {
                    EditorGUILayout.HelpBox("Floor size should be larger than 0.", MessageType.Error);
                }

                // Floor size - Vertical
                int preSizeVertical = floorSizeVertical;
                EditorGUI.BeginChangeCheck();
                int vertical = EditorGUILayout.IntSlider("Vertical Size", sliderVertical, 1, MaxFloorSize);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    sliderVertical = vertical;
                }

                if (sliderVertical <= 0)
                {
                    EditorGUILayout.HelpBox("Floor size should be larger than 0.", MessageType.Error);
                }

                if (GUILayout.Button("Apply"))
                {
                    if (preSizeHorizontal > sliderHorizontal || preSizeVertical > sliderVertical)
                    {
                        bool dialogChoice = EditorUtility.DisplayDialog("Apply new floor size",
                                                                        "The new floor size is smaller than old one. A part of map data will be cut off.",
                                                                        "Continue",
                                                                        "Cancel");
                        if (dialogChoice)
                        {
                            ChangeMapSizeProcess(preSizeHorizontal, preSizeVertical);
                        }
                    }
                    else
                    {
                        ChangeMapSizeProcess(preSizeHorizontal, preSizeVertical);
                    }
                }
                EditorGUILayout.Space();

                EditorGUI.BeginChangeCheck();
                string newFloorName = EditorGUILayout.TextField("Floor Name", floorName);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    floorName = newFloorName;
                }

                EditorGUI.BeginChangeCheck();
                int newFloorId = EditorGUILayout.IntField("Floor ID", floorId);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    floorId = newFloorId;
                }
                EditorGUILayout.Space();

                // Entrance position setting
                EditorGUI.BeginChangeCheck();
                Vector2Int newEntrancePos = EditorGUILayout.Vector2IntField("Entrance position", entrancePos);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    entrancePos = newEntrancePos;
                }
                ValidateEntrancePos();

                EditorGUI.BeginChangeCheck();
                DungeonDir enterDir = (DungeonDir)EditorGUILayout.EnumPopup("Entering direction", enteringDir);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    enteringDir = enterDir;
                }

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Process about changing the map size.
        /// </Summary>
        void ChangeMapSizeProcess(int preSizeHorizontal, int preSizeVertical)
        {
            if (preSizeHorizontal != sliderHorizontal)
            {
                InitializePosSelection();
                Undo.RecordObject(this, UndoName);
                floorSizeHorizontal = sliderHorizontal;
                SetMapBackgroundTexture();
                ConvertMapHorizontalChanged(preSizeHorizontal);
            }

            if (preSizeVertical != sliderVertical)
            {
                InitializePosSelection();
                Undo.RecordObject(this, UndoName);
                floorSizeVertical = sliderVertical;
                SetMapBackgroundTexture();
                ConvertMapVerticalChanged(preSizeVertical);
            }
        }

        /// <Summary>
        /// Show setting GUI for each position on the map.
        /// </Summary>
        void ShowSelectedPositionInformationParts()
        {
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(SettingPaneWidth));
            {
                EditorGUILayout.LabelField("Position Settings", boldLabelStyle);
                EditorGUILayout.Space();

                Vector2Int showPos = isPosSelected ? selectedGridPos : hoverGridPos;
                EditorGUILayout.Vector2IntField("Selected Position", showPos);

                int index = showPos.x + showPos.y * floorSizeHorizontal;

                // Detail information
                ShowPositionDetailParts(index);
                EditorGUILayout.Space();

                // Event editor
                ShowEventEditorParts(index);

                // Event list in this map
                ShowEventMappingButton();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show setting GUI for the position that is selected on the map.
        /// </Summary>
        /// <param name="index">Index in the MapInfo array.</param>
        void ShowPositionDetailParts(int index)
        {
            EditorGUILayout.BeginVertical();
            {
                // Map attribute
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Map Attribute", GUILayout.Width(InfoLabelWidth));
                    string toolName = MapEditorUtil.CheckEditMapIndexIsValid(index, editMapInfo.Count) ? GetAttributeName(editMapInfo[index].mapAttr) : "";
                    EditorGUILayout.LabelField(toolName, GUILayout.Width(InfoLabelWidth));
                }
                EditorGUILayout.EndHorizontal();

                // Object front
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Front direction of object", GUILayout.Width(InfoLabelWidth));
                    selectedPosObjectFront = MapEditorUtil.CheckEditMapIndexIsValid(index, editMapInfo.Count) ? editMapInfo[index].objectFront : 0;
                    EditorGUILayout.LabelField(selectedPosObjectFront.ToString(), GUILayout.Width(InfoLabelWidth));
                }
                EditorGUILayout.EndHorizontal();

                // Object type ID
                if (MapEditorUtil.CheckEditMapIndexIsValid(index, editMapInfo.Count))
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Object Type ID", GUILayout.Width(InfoLabelWidth));
                        selectedObjectTypeId = MapEditorUtil.CheckEditMapIndexIsValid(index, editMapInfo.Count) ? editMapInfo[index].objectTypeId : 0;
                        EditorGUILayout.LabelField(selectedObjectTypeId.ToString(), GUILayout.Width(InfoLabelWidth));
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Event", GUILayout.Width(InfoLabelWidth));
                    selectedPosEventId = MapEditorUtil.CheckEditMapIndexIsValid(index, editMapInfo.Count) ? editMapInfo[index].eventId : 0;
                    selectedEventMasterData = eventDataList.eventDataList.Find(e => e.eventId == selectedPosEventId);
                    EditorGUILayout.LabelField(selectedPosEventId.ToString(), GUILayout.Width(InfoLabelWidth));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show setting GUI about event settings.
        /// </Summary>
        /// <param name="index">Index in the MapInfo array.</param>
        void ShowEventEditorParts(int index)
        {
            if (selectedDrawTool != MapEditorTools.Select || !isPosSelected)
            {
                return;
            }

            Vector2Int grindPos = selectedGridPos;

            EditorGUILayout.BeginVertical();
            {
                // Object front define.
                EditorGUI.BeginChangeCheck();
                DungeonDir frontDir = (DungeonDir)EditorGUILayout.EnumPopup("Front Direction of Object", selectedPosObjectFront);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    selectedPosObjectFront = frontDir;

                    if (MapEditorUtil.CheckEditMapIndexIsValid(index, editMapInfo.Count))
                    {
                        MapInfo infoFront = editMapInfo[index];
                        infoFront.objectFront = selectedPosObjectFront;
                        editMapInfo[index] = infoFront;
                    }
                }

                // Parts type define.
                if (MapEditorUtil.CheckEditMapIndexIsValid(index, editMapInfo.Count))
                {
                    // Check if the parts prefab is not null.
                    bool isNullPrefab = false;
                    
                    List<int> typeIdList = new List<int>();
                    List<string> objNameList = new List<string>();
                    if (dungeonPartsDataList != null)
                    {
                        foreach (DungeonPartsData dungeonParts in dungeonPartsDataList)
                        {
                            if (dungeonParts == null)
                            {
                                continue;
                            }

                            foreach (DungeonPartsRecord record in dungeonParts.dungeonPartsRecords)
                            {
                                if (record.attributeId != editMapInfo[index].mapAttr)
                                {
                                    continue;
                                }

                                if (!typeIdList.Contains(record.partsTypeId))
                                {
                                    if (record.partsObject == null)
                                    {
                                        isNullPrefab = true;
                                    }
                                    else
                                    {
                                        typeIdList.Add(record.partsTypeId);
                                        objNameList.Add(record.partsObject.name);
                                    }
                                }
                            }
                        }
                    }

                    if (isNullPrefab)
                    {
                        EditorGUILayout.HelpBox("Provided DungeonPartsData has some null reference for the parts prefab. Assign your parts prefabs.", MessageType.Warning);
                    }

                    string[] objectNameArray = objNameList.ToArray();
                    int[] objectIdArray = typeIdList.ToArray();

                    if (objectIdArray.Length > 0)
                    {
                        EditorGUI.BeginChangeCheck();
                        int objType = EditorGUILayout.IntPopup("Object Type", selectedObjectTypeId, objectNameArray, objectIdArray);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, UndoName);
                            selectedObjectTypeId = objType;
                        }
                        
                        MapInfo infoObjType = editMapInfo[index];
                        infoObjType.objectTypeId = selectedObjectTypeId;
                        editMapInfo[index] = infoObjType;
                        EditorGUILayout.Space();
                    }
                }

                EditorGUI.BeginChangeCheck();
                EventMasterData eventData = (EventMasterData)EditorGUILayout.ObjectField("Selected pos event", selectedEventMasterData, typeof(EventMasterData), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Change EventMasterData");
                    selectedEventMasterData = eventData;
                    if (selectedEventMasterData == null)
                    {
                        selectedPosEventId = 0;
                    }
                    else
                    {
                        selectedPosEventId = eventData.eventId;
                    }
                }

                if (selectedPosEventId >= 0)
                {
                    SetMapInfoData(grindPos, selectedPosEventId);

                    if (selectedPosEventId > 0)
                    {
                        if (GUILayout.Button("Open Event Editor"))
                        {
                            if (eventEditor == null)
                            {
                                eventEditor = CreateInstance<EventEditor>();
                                eventEditor.InitializeEvent(selectedPosEventId, eventData);
                            }
                            eventEditor.ShowUtility();
                        }

                        string eventName = MapEditorUtil.EventDefaultName;
                        if (eventData != null)
                        {
                            eventName = eventData.eventName;
                        }

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Event Name", GUILayout.Width(InfoLabelWidth));
                            EditorGUILayout.LabelField(eventName, GUILayout.Width(InfoLabelWidth));
                        }
                        EditorGUILayout.EndHorizontal();

                    }
                    else
                    {
                        if (GUILayout.Button("Create new EventData"))
                        {
                            EventMasterData newData = CreateNewEventData();
                            if (newData != null)
                            {
                                selectedEventMasterData = newData;
                                selectedPosEventId = newData.eventId;
                                SetMapInfoData(grindPos, selectedPosEventId);
                            }
                        }
                        EditorGUILayout.HelpBox("To edit event, create new EventMasterData.", MessageType.Info);
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show the button that shows EventMappingData.
        /// </Summary>
        void ShowEventMappingButton()
        {
            EditorGUILayout.BeginVertical();
            {
                if (GUILayout.Button("Check Event Mapping"))
                {
                    if (eventMappingEditor == null)
                    {
                        eventMappingEditor = CreateInstance<EventMappingEditor>();
                        eventMappingEditor.GetEventRef(saveFileName);
                    }
                    eventMappingEditor.Show();
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show toolbar to set map attribute.
        /// </Summary>
        void ShowToolBarParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Drawing tool : " + drawToolNameList[selectedDrawTool]);
                    selectedDrawTool = GUILayout.Toolbar(selectedDrawTool, drawToolIconList, GUI.skin.button, GUI.ToolbarButtonSize.FitToContents);
                    if (selectedDrawTool != 0)
                    {
                        isPosSelected = false;
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical();
                {
                    if (mapIconIdList != null && mapIconList != null)
                    {
                        if (selectedMapIcon >= 0 && selectedMapIcon < mapIconIdList.Length)
                        {
                            int attrId = mapIconIdList[selectedMapIcon];
                            EditorGUILayout.LabelField("Map attribute : " + GetAttributeName(attrId));
                            selectedMapIcon = GUILayout.Toolbar(selectedMapIcon, mapIconList, GUI.skin.button, GUI.ToolbarButtonSize.FitToContents);
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show vertical scale of the map.
        /// </Summary>
        void ShowVerticalScaleParts()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(CellSize - GUI.skin.label.margin.right));
            {
                EditorGUILayout.LabelField("", GUILayout.Width(GUI.skin.label.margin.vertical));
                for (int i = floorSizeVertical; i > 0; i--)
                {
                    EditorGUILayout.LabelField((i - 1).ToString(), GUILayout.Width(CellSize - GUI.skin.label.margin.right), GUILayout.Height(CellSize - GUI.skin.label.margin.top));
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show the map GUI to edit the map on the floor.
        /// </Summary>
        void ShowMapParts()
        {
            EditorGUILayout.BeginVertical();
            {
                GUIContent content = new GUIContent(mapBg);
                GUILayout.Box(content);
                Rect mapRect = GUILayoutUtility.GetLastRect();

                // Draw map icons.
                Rect[,] iconRect = MapEditorUtil.GetMapRectArray(mapRect, floorSizeHorizontal, floorSizeVertical, CellSize);
                SetMapAttrIcon(iconRect);

                // Draw entrance position icon.
                Rect[,] entranceRect = MapEditorUtil.GetMapRectArray(mapRect, floorSizeHorizontal, floorSizeVertical, CellSize);
                SetEntranceIconToMap(entranceRect);

                // Draw event icons.
                Rect[,] eventRect = MapEditorUtil.GetMapRectArray(mapRect, floorSizeHorizontal, floorSizeVertical, CellSize);
                SetEventIconToMap(eventRect);

                Rect[,] highlightRect = MapEditorUtil.GetMapRectArray(mapRect, floorSizeHorizontal, floorSizeVertical, CellSize);
                if (isPosSelected)
                {
                    SetHoverColor(highlightRect[selectedGridPos.x, selectedGridPos.y], selectedTex);
                }

                wantsMouseMove = true;
                if (mapRect.Contains(Event.current.mousePosition))
                {
                    Vector2 pos = new Vector2(Event.current.mousePosition.x - mapRect.position.x, Event.current.mousePosition.y - mapRect.position.y);
                    hoverGridPos = MapEditorUtil.GetPosInGrid(pos, floorSizeHorizontal, floorSizeVertical, CellSize);
                    if (isDragging)
                    {
                        foreach (Vector2Int listPos in dragPosList)
                        {
                            SetHoverColor(highlightRect[listPos.x, listPos.y], hoverTex);
                        }
                    }
                    else
                    {
                        SetHoverColor(highlightRect[hoverGridPos.x, hoverGridPos.y], hoverTex);
                    }
                    MouseEventProcess(mapRect, highlightRect, pos);
                    Repaint();
                }
                ShowHorizontalScaleParts();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show horizontal scale of the map.
        /// </Summary>
        void ShowHorizontalScaleParts()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(CellSize));
            {
                EditorGUILayout.LabelField("", GUILayout.Width(GUI.skin.label.margin.horizontal));
                for (int i = 0; i < floorSizeHorizontal; i++)
                {
                    EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(CellSize - GUI.skin.label.margin.right));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <Summary>
        /// Initialize the selection of position on the map.
        /// </Summary>
        void InitializePosSelection()
        {
            isPosSelected = false;
            selectedGridPos = Vector2Int.zero;
            hoverGridPos = Vector2Int.zero;
        }

        /// <Summary>
        /// Initialize properties of MapEditor.
        /// </Summary>
        void InitializeProperties()
        {
            floorSizeHorizontal = FloorMapConst.InitHorizontalSize;
            floorSizeVertical = FloorMapConst.InitVerticalSize;
            sliderHorizontal = FloorMapConst.InitHorizontalSize;
            sliderVertical = FloorMapConst.InitVerticalSize;

            floorName = DefaultFloorName;
            floorId = DefaultFloorId;
            saveFileName = DefaultSaveFileName;

            loadFloor = null;

            selectedPosEventId = 0;
            selectedEventMasterData = null;

            entrancePos = Vector2Int.zero;
            enteringDir = DungeonDir.North;

            leftPaneScrollPos = Vector2.zero;
            rightPaneScrollPos = Vector2.zero;
        }

        /// <Summary>
        /// Set map background texture by using floor sizes.
        /// </Summary>
        void SetMapBackgroundTexture()
        {
            mapBg = MapEditorUtil.RepaintMapTexture(floorSizeHorizontal, floorSizeVertical, CellSize, gridColor, baseColor);
        }

        /// <Summary>
        /// Set color arrays to show the map for editing.
        /// </Summary>
        void SetColorArrays()
        {
            hoverColors = new Color[CellSize * CellSize];
            transparentColors = new Color[CellSize * CellSize];
            selectedColors = new Color[CellSize * CellSize];
            for (int i = 0; i < hoverColors.Length; i++)
            {
                hoverColors[i] = hoverColor;
                transparentColors[i] = transparentColor;
                selectedColors[i] = selectedColor;
            }
        }

        /// <Summary>
        /// Set tool icons for toolbars.
        /// </Summary>
        void SetToolIcons()
        {
            CheckCurrentIconData();
            drawToolIconList = MapEditorUtil.GetToolIconArray();
        }

        /// <Summary>
        /// Check a current MapAttributeData and get icon arrays.
        /// </Summary>
        void CheckCurrentIconData()
        {
            mapIconList = MapEditorUtil.GetMapIconArray(mapAttributeDataList);
            mapIconIdList = MapEditorUtil.GetMapAttributeArray(mapAttributeDataList);
        }

        /// <Summary>
        /// Set event icon which indicates that some event exists on the position.
        /// </Summary>
        void SetEventIcons()
        {
            eventIcon = MapEditorUtil.GetEventIcon();
            entranceIcon = MapEditorUtil.GetEntranceIcon();
            warningIcon = MapEditorUtil.GetWarningIcon();
        }

        /// <Summary>
        /// Set hover color to the texture.
        /// </Summary>
        /// <param name="r">Rect for the texture.</param>
        /// <param name="tex">Texture to draw.</param>
        void SetHoverColor(Rect r, Texture2D tex)
        {
            if (tex != null)
            {
                GUI.DrawTexture(r, tex);
            }
        }

        /// <Summary>
        /// Set map icons on the map.
        /// </Summary>
        /// <param name="r">Rect array for the texture.</param>
        void SetMapAttrIcon(Rect[,] r)
        {
            for (int y = 0; y < floorSizeVertical; y++)
            {
                for (int x = 0; x < floorSizeHorizontal; x++)
                {
                    int index = x + y * floorSizeHorizontal;
                    MapInfo info = editMapInfo[index];
                    if (MapEditorUtil.CheckDrawEditorIconFlag(info.mapAttr, mapAttributeDataList))
                    {
                        int iconIndex = Array.IndexOf(mapIconIdList, info.mapAttr);
                        if (iconIndex < mapIconList.Length && iconIndex >= 0)
                        {
                            GUI.DrawTexture(r[x, y], mapIconList[iconIndex]);
                        }
                        else
                        {
                            // If the attribute of the point is "Not In Use", show a warning icon.
                            GUI.DrawTexture(r[x, y], warningIcon);
                        }
                    }
                }
            }
        }

        /// <Summary>
        /// Set an entrance icon on the map.
        /// </Summary>
        /// <param name="r">Rect array for the texture.</param>
        void SetEntranceIconToMap(Rect[,] r)
        {
            for (int y = 0; y < floorSizeVertical; y++)
            {
                for (int x = 0; x < floorSizeHorizontal; x++)
                {
                    if (x == entrancePos.x && y == entrancePos.y)
                    {
                        GUI.DrawTexture(r[x, y], entranceIcon);
                    }
                }
            }
        }

        /// <Summary>
        /// Set event icons on the map.
        /// </Summary>
        /// <param name="r">Rect array for the texture.</param>
        void SetEventIconToMap(Rect[,] r)
        {
            for (int y = 0; y < floorSizeVertical; y++)
            {
                for (int x = 0; x < floorSizeHorizontal; x++)
                {
                    int index = x + y * floorSizeHorizontal;
                    MapInfo info = editMapInfo[index];
                    if (info.eventId > 0)
                    {
                        GUI.DrawTexture(r[x, y], eventIcon);
                    }
                }
            }
        }

        /// <Summary>
        /// Set the color array to the texture of a hover color.
        /// </Summary>
        void SetHoverTexture()
        {
            hoverTex = new Texture2D(CellSize, CellSize, TextureFormat.ARGB32, false);
            hoverTex.SetPixels(0, 0, CellSize, CellSize, hoverColors);
            hoverTex.Apply();
        }

        /// <Summary>
        /// Set the color array to the texture of a selected color.
        /// </Summary>
        void SetSelectedTexture()
        {
            selectedTex = new Texture2D(CellSize, CellSize, TextureFormat.ARGB32, false);
            selectedTex.SetPixels(0, 0, CellSize, CellSize, selectedColors);
            selectedTex.Apply();
        }

        /// <Summary>
        /// Load default file of DungeonPartsData.
        /// </Summary>
        void LoadDefaultDungeonParts()
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
            dungeonPartsDataList = new List<DungeonPartsData>(){partsData};
        }

        /// <Summary>
        /// Load default file of DungeonBasePartsData.
        /// </Summary>
        void LoadDefaultDungeonBaseParts()
        {
            // Load DefaultParts data.
            string[] guids = AssetDatabase.FindAssets(DefaultBasePartsDataName, null);

            DungeonBasePartsData basePartsData = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                basePartsData = AssetDatabase.LoadAssetAtPath<DungeonBasePartsData>(assetPath);
                if (basePartsData != null)
                {
                    break;
                }
            }
            dungeonBasePartsData = basePartsData;
        }

        /// <Summary>
        /// Process of mouse events.
        /// </Summary>
        /// <param name="mapRect">Rect for mouse event listener.</param>
        /// <param name="highlightRect">Highlighted rects.</param>
        /// <param name="pos">Position on the map.</param>
        void MouseEventProcess(Rect mapRect, Rect[,] highlightRect, Vector2 pos)
        {
            switch (selectedDrawTool)
            {
                case MapEditorTools.Select:
                    if (Event.current.type == EventType.MouseUp)
                    {
                        selectedGridPos = MapEditorUtil.GetPosInGrid(pos, floorSizeHorizontal, floorSizeVertical, CellSize);
                        isPosSelected = true;
                    }
                    break;
                case MapEditorTools.Draw:
                    if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDrag)
                    {
                        hoverGridPos = MapEditorUtil.GetPosInGrid(pos, floorSizeHorizontal, floorSizeVertical, CellSize);
                        SetMapAttribute(hoverGridPos);
                    }
                    break;
                case MapEditorTools.Rect:
                    DrawRect(highlightRect, pos, MapEditorTools.Rect);
                    break;
                case MapEditorTools.FillRect:
                    DrawRect(highlightRect, pos, MapEditorTools.FillRect);
                    break;
            }
        }

        /// <Summary>
        /// Set a map attribute ID to the selected position.
        /// </Summary>
        /// <param name="gridPos">Position on the map grids.</param>
        void SetMapAttribute(Vector2Int gridPos)
        {
            int index = gridPos.x + gridPos.y * floorSizeHorizontal;
            MapInfo info = editMapInfo[index];

            int attrId = mapIconIdList[selectedMapIcon];
            info.mapAttr = attrId;

            Undo.RecordObject(this, UndoName);
            editMapInfo[index] = info;
        }

        /// <Summary>
        /// Set a map info data to the selected position.
        /// </Summary>
        /// <param name="eventId">The event ID of the position.</param>
        void SetMapInfoData(Vector2Int gridPos, int eventId)
        {
            int selectedPosIndex = gridPos.x + gridPos.y * floorSizeHorizontal;
            if (MapEditorUtil.CheckEditMapIndexIsValid(selectedPosIndex, editMapInfo.Count))
            {
                MapInfo info = new MapInfo();
                info = editMapInfo[selectedPosIndex];
                info.eventId = eventId;
                Undo.RecordObject(this, UndoName);
                editMapInfo[selectedPosIndex] = info;
            }
        }

        /// <Summary>
        /// Draw rects by mouse event.
        /// </Summary>
        /// <param name="highlightRect">Highlighted rects.</param>
        /// <param name="pos">Position on the map.</param>
        /// <param name="toolId">Selected tool ID.</param>
        void DrawRect(Rect[,] highlightRect, Vector2 pos, int toolId)
        {
            if (Event.current.type == EventType.MouseDown)
            {
                dragStartGridPos = MapEditorUtil.GetPosInGrid(pos, floorSizeHorizontal, floorSizeVertical, CellSize);
                
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                isDragging = true;
                dragPosList = new List<Vector2Int>();
                Vector2Int mouseGridPos = MapEditorUtil.GetPosInGrid(pos, floorSizeHorizontal, floorSizeVertical, CellSize);
                dragPosList = GetHighlightList(mouseGridPos, toolId);

            }
            else if (Event.current.type == EventType.MouseUp)
            {
                dragPosList = new List<Vector2Int>();
                Vector2Int mouseGridPos = MapEditorUtil.GetPosInGrid(pos, floorSizeHorizontal, floorSizeVertical, CellSize);
                dragPosList = GetHighlightList(mouseGridPos, toolId);

                foreach (Vector2Int listPos in dragPosList)
                {
                    SetMapAttribute(listPos);
                }
                isDragging = false;
            }
        }

        /// <Summary>
        /// Initialize dragging events of mouse.
        /// </Summary>
        void InitializeDraggingEvent()
        {
            isDragging = false;
            dragPosList = new List<Vector2Int>();
            dragStartGridPos = Vector2Int.zero;
        }

        /// <Summary>
        /// Draw rects by mouse event.
        /// </Summary>
        /// <param name="mouseGridPos">Grid position of the mouse.</param>
        /// <param name="toolId">Selected tool ID.</param>
        List<Vector2Int> GetHighlightList(Vector2Int mouseGridPos, int toolId)
        {
            List<Vector2Int> list = new List<Vector2Int>();
            Vector2Int min = Vector2Int.zero;
            Vector2Int max = Vector2Int.zero;
            if (dragStartGridPos.x <= mouseGridPos.x)
            {
                min.x = dragStartGridPos.x;
                max.x = mouseGridPos.x;
            }
            else
            {
                min.x = mouseGridPos.x;
                max.x = dragStartGridPos.x;
            }

            if (dragStartGridPos.y <= mouseGridPos.y)
            {
                min.y = dragStartGridPos.y;
                max.y = mouseGridPos.y;
            }
            else
            {
                min.y = mouseGridPos.y;
                max.y = dragStartGridPos.y;
            }

            for (int y = min.y; y <= max.y; y++)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    if (toolId == MapEditorTools.Rect)
                    {
                        if (x == min.x || x == max.x || y == min.y || y == max.y)
                        {
                            list.Add(new Vector2Int(x, y));
                        }
                    }
                    else if (toolId == MapEditorTools.FillRect)
                    {
                        list.Add(new Vector2Int(x, y));
                    }
                    
                }
            }

            return list;
        }

        /// <Summary>
        /// Returns the name of attribute.
        /// </Summary>
        string GetAttributeName(int attributeId)
        {
            string attrName = "";
            if (mapAttributeDataList == null)
            {
                return attrName;
            }

            MapAttributeRecord record = DataRecordUtil.GetMapAttributeRecordById(mapAttributeDataList, attributeId);
            if (record == null)
            {
                return attrName;
            }

            attrName = record.attributeName;

            if (record.notInUse)
            {
                attrName = attrName + " (Not In Use)";
            }
            return attrName;
        }

        /// <Summary>
        /// Set new map data for editing.
        /// </Summary>
        void SetNewMapData()
        {
            editMapInfo = new List<MapInfo>();

            // Set edit data
            int listSize = floorSizeHorizontal * floorSizeVertical;
            for (int i = 0; i < listSize; i++)
            {
                MapInfo info = new MapInfo();
                info.eventId = 0;
                info.mapAttr = 0;
                editMapInfo.Add(info);
            }
        }

        /// <Summary>
        /// Set map settings to new map to avoid lack of information by changing index.
        /// </Summary>
        /// <param name="oldHorizontalSize">Pre size of horizontal floor size.</param>
        void ConvertMapHorizontalChanged(int oldHorizontalSize)
        {
            List<MapInfo> oldMapInfo = editMapInfo;
            MapInfo[,] oldArray = GetMapDataArrayFromList(oldMapInfo, oldHorizontalSize, floorSizeVertical);
            
            SetNewMapData();

            MapInfo[,] newArray = GetMapDataArrayFromList(editMapInfo, floorSizeHorizontal, floorSizeVertical);

            // Get lesser horizontal size.
            int max = oldHorizontalSize < floorSizeHorizontal ? oldHorizontalSize : floorSizeHorizontal;

            for (int y = 0; y < floorSizeVertical; y++)
            {
                for (int x = 0; x < max; x++)
                {
                    newArray[x, y] = oldArray[x, y];
                }
            }
            Undo.RecordObject(this, UndoName);
            editMapInfo = GetMapDataListFromArray(newArray, floorSizeHorizontal, floorSizeVertical);
        }

        /// <Summary>
        /// Set map settings to new map to avoid lack of information by changing index.
        /// </Summary>
        /// <param name="oldVerticalSize">Pre size of vertical floor size.</param>
        void ConvertMapVerticalChanged(int oldVerticalSize)
        {
            List<MapInfo> oldMapInfo = editMapInfo;
            MapInfo[,] oldArray = GetMapDataArrayFromList(oldMapInfo, floorSizeHorizontal, oldVerticalSize);
            
            SetNewMapData();

            MapInfo[,] newArray = GetMapDataArrayFromList(editMapInfo, floorSizeHorizontal, floorSizeVertical);

            // Get lesser vertical size.
            int max = oldVerticalSize < floorSizeVertical ? oldVerticalSize : floorSizeVertical;

            for (int y = 0; y < max; y++)
            {
                for (int x = 0; x < floorSizeHorizontal; x++)
                {
                    newArray[x, y] = oldArray[x, y];
                }
            }
            Undo.RecordObject(this, UndoName);
            editMapInfo = GetMapDataListFromArray(newArray, floorSizeHorizontal, floorSizeVertical);
        }

        /// <Summary>
        /// Convert the list to array for manipulating map data by index. 
        /// </Summary>
        /// <param name="mapInfoList">List of MapInfo data.</param>
        /// <param name="horizontalSize">Horizontal size of the floor.</param>
        /// <param name="verticalSize">Vertical size of the floor.</param>
        MapInfo[,] GetMapDataArrayFromList(List<MapInfo> mapInfoList, int horizontalSize, int verticalSize)
        {
            MapInfo[,] mapArray = new MapInfo[horizontalSize, verticalSize];
            for (int y = 0; y < verticalSize; y++)
            {
                for (int x = 0; x < horizontalSize; x++)
                {
                    mapArray[x, y] = mapInfoList[x + y * horizontalSize];
                }
            }
            return mapArray;
        }

        /// <Summary>
        /// Convert the array to list for holding map data. 
        /// </Summary>
        /// <param name="mapInfoArray">Array of MapInfo data.</param>
        /// <param name="horizontalSize">Horizontal size of the floor.</param>
        /// <param name="verticalSize">Vertical size of the floor.</param>
        List<MapInfo> GetMapDataListFromArray(MapInfo[,] mapInfoArray, int horizontalSize, int verticalSize)
        {
            List<MapInfo> mapList = new List<MapInfo>();
            for (int y = 0; y < verticalSize; y++)
            {
                for (int x = 0; x < horizontalSize; x++)
                {
                    mapList.Add(mapInfoArray[x, y]);
                }
            }
            return mapList;
        }

        /// <Summary>
        /// Validate the entrance position.
        /// </Summary>
        void ValidateEntrancePos()
        {
            // Horizontal axis validation.
            if (entrancePos.x < 0)
            {
                entrancePos.x = 0;
            }
            else if (entrancePos.x >= floorSizeHorizontal)
            {
                entrancePos.x = floorSizeHorizontal - 1;
            }

            // Vertical axis validation.
            if (entrancePos.y < 0)
            {
                entrancePos.y = 0;
            }
            else if (entrancePos.y >= floorSizeVertical)
            {
                entrancePos.y = floorSizeVertical - 1;
            }
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
            InitializePosSelection();
            SetMapBackgroundTexture();
            Repaint();
        }

        /// <Summary>
        /// Clear Undo/Redo stack to prevent out of index error of MapInfo array in another file.
        /// </Summary>
        void ClearUndoStack()
        {
            Undo.FlushUndoRecordObjects();
            Undo.ClearAll();
        }

        /// <Summary>
        /// Initialize the font style.
        /// </Summary>
        void InitializeStyles()
        {
            boldLabelStyle = new GUIStyle(GUI.skin.label);
            boldLabelStyle.fontStyle = FontStyle.Bold;
        }

        /// <Summary>
        /// Set initial strings for file path.
        /// </Summary>
        void SetInitialFilePath()
        {
            tempFileName = MapEditorUtil.TempFileName;
            assetRootPathPrefix = MapEditorUtil.AssetRootPrefix;
            tempFilePath = MapEditorUtil.TempFilePath;
            infoFileName = MapEditorUtil.InfoFileName;
        }

        /// <Summary>
        /// Load process of the temp file.
        /// </Summary>
        void LoadTempFile()
        {
            string path = assetRootPathPrefix + tempFilePath + tempFileName + ".asset";
            var tempData = CreateInstance<FloorMapMasterData>();
            
            var asset = (FloorMapMasterData)AssetDatabase.LoadAssetAtPath(path, typeof(FloorMapMasterData));
            if (asset == null)
            {
                SetNewMapData();
                tempData.mapInfo = editMapInfo;
                if (this.dungeonPartsDataList == null)
                {
                    LoadDefaultDungeonParts();
                }
            }
            else
            {
                tempData = AssetDatabase.LoadAssetAtPath<FloorMapMasterData>(path);
                if (tempData != null)
                {
                    this.editMapInfo = tempData.mapInfo;
                    this.floorSizeHorizontal = tempData.floorSizeHorizontal;
                    this.floorSizeVertical = tempData.floorSizeVertical;
                    sliderHorizontal = this.floorSizeHorizontal;
                    sliderVertical = this.floorSizeVertical;
                    this.floorName = tempData.floorName;
                    this.floorId = tempData.floorId;
                    this.entrancePos = tempData.entrancePos;
                    this.enteringDir = tempData.enteringDir;
                    this.dungeonBasePartsData = tempData.dungeonBasePartsData;
                    this.dungeonPartsDataList = tempData.dungeonPartsDataList;
                    if (this.dungeonPartsDataList == null)
                    {
                        LoadDefaultDungeonParts();
                    }

                    this.mapAttributeDataList = tempData.mapAttributeDataList;
                    if (this.mapAttributeDataList == null)
                    {
                        LoadDefaultMapAttributeData();
                    }
                }
            }

            LoadTempFileInfo();
        }

        /// <Summary>
        /// Set file name and path of editing file.
        /// </Summary>
        void LoadTempFileInfo()
        {
            string infoPath = assetRootPathPrefix + tempFilePath + infoFileName + ".asset";
            var infoAsset = (EditFileInfo)AssetDatabase.LoadAssetAtPath(infoPath, typeof(EditFileInfo));
            if (infoAsset != null)
            {
                this.saveFileName = infoAsset.editFileName;
            }
        }

        /// <Summary>
        /// Save process of the temp file.
        /// </Summary>
        void SaveTempFile()
        {
            var tempData = CreateInstance<FloorMapMasterData>();
            tempData.mapInfo = this.editMapInfo;
            tempData.floorSizeHorizontal = this.floorSizeHorizontal;
            tempData.floorSizeVertical = this.floorSizeVertical;
            tempData.floorName = this.floorName;
            tempData.floorId = this.floorId;
            tempData.entrancePos = this.entrancePos;
            tempData.enteringDir = this.enteringDir;
            tempData.dungeonBasePartsData = this.dungeonBasePartsData;
            tempData.dungeonPartsDataList = this.dungeonPartsDataList;
            tempData.mapAttributeDataList = this.mapAttributeDataList;
            
            string path = assetRootPathPrefix + tempFilePath + tempFileName + ".asset";
            AssetDatabase.CreateAsset(tempData, path);
            AssetDatabase.Refresh();

            // save editing file name.
            var tempFileInfo = CreateInstance<EditFileInfo>();
            tempFileInfo.editFileName = saveFileName;
            string infoPath = assetRootPathPrefix + tempFilePath + infoFileName + ".asset";
            AssetDatabase.CreateAsset(tempFileInfo, infoPath);
            AssetDatabase.Refresh();

        }

        /// <Summary>
        /// Load process of the FloorMapMasterData file.
        /// </Summary>
        void LoadMapFile()
        {
            var loadData = loadFloor;

            this.floorSizeHorizontal = loadData.floorSizeHorizontal;
            this.floorSizeVertical = loadData.floorSizeVertical;
            sliderHorizontal = this.floorSizeHorizontal;
            sliderVertical = this.floorSizeVertical;
            this.floorName = loadData.floorName;
            this.floorId = loadData.floorId;
            this.entrancePos = loadData.entrancePos;
            this.enteringDir = loadData.enteringDir;
            this.dungeonBasePartsData = loadData.dungeonBasePartsData;
            this.dungeonPartsDataList = loadData.dungeonPartsDataList;
            if (this.dungeonPartsDataList == null)
            {
                LoadDefaultDungeonParts();
            }

            this.mapAttributeDataList = loadData.mapAttributeDataList;
            if (this.mapAttributeDataList == null)
            {
                LoadDefaultMapAttributeData();
            }

            this.saveFileName = loadData.name;
            SetEditMapData(loadData);
        }

        /// <Summary>
        /// Set MapInfo data from loaded FloorMapMasterData file.
        /// </Summary>
        void SetEditMapData(FloorMapMasterData mapDataFile)
        {
            editMapInfo = new List<MapInfo>();

            // Set edit data
            int listSize = floorSizeHorizontal * floorSizeVertical;
            for (int i = 0; i < listSize; i++)
            {
                MapInfo info = new MapInfo();
                info.eventId = mapDataFile.mapInfo[i].eventId;
                info.mapAttr = mapDataFile.mapInfo[i].mapAttr;
                info.objectFront = mapDataFile.mapInfo[i].objectFront;
                info.objectTypeId = mapDataFile.mapInfo[i].objectTypeId;
                editMapInfo.Add(info);
            }
        }

        /// <Summary>
        /// Save process of the FloorMapMasterData file.
        /// </Summary>
        void SaveMapFile()
        {
            var mapData = CreateInstance<FloorMapMasterData>();
            mapData.mapInfo = this.editMapInfo;
            mapData.floorSizeHorizontal = this.floorSizeHorizontal;
            mapData.floorSizeVertical = this.floorSizeVertical;
            mapData.floorName = this.floorName;
            mapData.floorId = this.floorId;
            mapData.entrancePos = this.entrancePos;
            mapData.enteringDir = this.enteringDir;
            mapData.dungeonBasePartsData = this.dungeonBasePartsData;
            mapData.dungeonPartsDataList = this.dungeonPartsDataList;
            mapData.mapAttributeDataList = this.mapAttributeDataList;
            SetEditMapData(mapData);
            
            // string path = assetRootPathPrefix + saveFolderPath + saveFileName + saveFilePostFix;
            string path = saveFilePath;
            var asset = (FloorMapMasterData)AssetDatabase.LoadAssetAtPath(path, typeof(FloorMapMasterData));
            if (asset == null)
            {
                AssetDatabase.CreateAsset(mapData, path);
            }
            else
            {
                EditorUtility.CopySerialized(mapData, asset);
                AssetDatabase.SaveAssets();
            }
            AssetDatabase.Refresh();
        }

        /// <Summary>
        /// Load default file of MapAttributeData.
        /// </Summary>
        void LoadDefaultMapAttributeData()
        {
            if (mapAttributeDataList != null)
            {
                return;
            }

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
            mapAttributeDataList = new List<MapAttributeData>(){attrData};
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
        /// Create a new EventMasterData as ScriptableObject.
        /// </Summary>
        EventMasterData CreateNewEventData()
        {
            var eventData = ScriptableObject.CreateInstance<EventMasterData>();
            eventData.eventId = GetNewEventId();
            eventData.eventName = MapEditorUtil.EventDefaultName;

            string eventFileName = MapEditorUtil.EventFilePrefix + eventData.eventId.ToString(AriadneEventSettings.IdDigits);
            string path = EditorUtility.SaveFilePanelInProject("Save EventMasterData file.", eventFileName, "asset", SaveEventDataDialogMessage);
            if (path.Length == 0)
            {
                return null;
            }

            var asset = (EventMasterData)AssetDatabase.LoadAssetAtPath(path, typeof(EventMasterData));
            if (asset == null)
            {
                AssetDatabase.CreateAsset(eventData, path);
            }
            else
            {
                EditorUtility.CopySerialized(eventData, asset);
                AssetDatabase.SaveAssets();
            }
            AssetDatabase.Refresh();

            // Regenerate the eventDataList.
            EventDataListEditor listEditor = CreateInstance<EventDataListEditor>();
            listEditor.GenerateListProcess(eventDataList, eventDataList.includeSample);

            return eventData;
        }

        /// <Summary>
        /// Returns a new event data ID.
        /// </Summary>
        int GetNewEventId()
        {
            int candidate = 1;
            List<EventMasterData> userEventList = eventDataList.eventDataList.FindAll(e => !e.isSampleData);
            foreach (EventMasterData data in userEventList)
            {
                if (data.eventId >= candidate)
                {
                    candidate = data.eventId + 1;
                }
            }
            return candidate;
        }
    }
}
