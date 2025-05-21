using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ariadne
{
    /// <Summary>
    /// Upgrade tool from version 1.0.x to 1.5.0.
    /// </Summary>
    public class UpgradeUtil : EditorWindow
    {
        GUIStyle boldLabelStyle;

        bool upgraded = false;

        /// <Summary>
        /// Open UpgradeTool window.
        /// </Summary>
        [MenuItem("Window/Ariadne/UpgradeUtility", false, 10000)]
        static void Open()
        {
            GetWindow<UpgradeUtil>();
        }

        /// <Summary>
        /// Show GUI for the upgrade tool.
        /// </Summary>
        void OnGUI()
        {
            InitializeStyles();
            EditorGUILayout.LabelField("Ariadne Upgrade Utility", boldLabelStyle);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("This tool supports upgrading 1.0.x -> 1.5.0");

            EditorGUILayout.Space();
            if (GUILayout.Button("Upgrade"))
            {
                UpgradeProcess();
            }

            EditorGUILayout.Space();
            if (upgraded)
            {
                EditorGUILayout.LabelField("Done!");
            }
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
        /// Upgrade Ariadne settings for 1.5.0.
        /// </Summary>
        void UpgradeProcess()
        {
            upgraded = false;

            UpgradeDemoMap();
            UpgradeScripts();
            
            upgraded = true;
        }

        /// <Summary>
        /// Upgrade demo map data.
        /// </Summary>
        void UpgradeDemoMap()
        {
            string fileName = DefaultAttribute.DefaultMapAttrDataName;
            MapAttributeData mapAttrData = LoadSystemAttributeFile(fileName);
            if (mapAttrData == null)
            {
                Debug.LogWarning(fileName + "is missing. Please re-import the following file : " + fileName);
                return;
            }

            fileName = DefaultDungeonPartsData.DefaultDungeonPartsName;
            DungeonPartsData dungeonPartsData = LoadSystemDungeonPartsFile(fileName);
            if (dungeonPartsData == null)
            {
                Debug.LogWarning(fileName + "is missing. Please re-import the following file : " + fileName);
                return;
            }

            fileName = DefaultDungeonPartsData.DefaultDungeonBasePartsName;
            DungeonBasePartsData dungeonPartsBaseData = LoadSystemDungeonBasePartsFile(fileName);
            if (dungeonPartsBaseData == null)
            {
                Debug.LogWarning(fileName + "is missing. Please re-import the following file : " + fileName);
                return;
            }

            // Upgrade DemoDungeonB1F and DemoDungeonB2F.
            List<string> mapDataNameList = new List<string>();
            mapDataNameList.Add("DemoDungeonB1F");
            mapDataNameList.Add("DemoDungeonB2F");

            foreach (string mapName in mapDataNameList)
            {
                FloorMapMasterData mapData = LoadFloorMapDataFile(mapName);
                if (mapData != null)
                {
                    SetUpFloorMapData(mapData, mapAttrData, dungeonPartsData, dungeonPartsBaseData);
                }
            }

            AssetDatabase.Refresh();
        }

        /// <Summary>
        /// Load default file of MapAttributeData.
        /// </Summary>
        MapAttributeData LoadSystemAttributeFile(string fileName)
        {
            // Load default MapAttributeData data.
            string[] guids = AssetDatabase.FindAssets(fileName, null);

            MapAttributeData data = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                data = AssetDatabase.LoadAssetAtPath<MapAttributeData>(assetPath);
                if (data != null)
                {
                    break;
                }
            }
            return data;
        }

        /// <Summary>
        /// Load default file of DungeonPartsData.
        /// </Summary>
        DungeonPartsData LoadSystemDungeonPartsFile(string fileName)
        {
            // Load default DungeonPartsData data.
            string[] guids = AssetDatabase.FindAssets(fileName, null);

            DungeonPartsData data = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                data = AssetDatabase.LoadAssetAtPath<DungeonPartsData>(assetPath);
                if (data != null)
                {
                    break;
                }
            }
            return data;
        }

        /// <Summary>
        /// Load default file of DungeonBasePartsData.
        /// </Summary>
        DungeonBasePartsData LoadSystemDungeonBasePartsFile(string fileName)
        {
            // Load default DungeonBasePartsData data.
            string[] guids = AssetDatabase.FindAssets(fileName, null);

            DungeonBasePartsData data = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                data = AssetDatabase.LoadAssetAtPath<DungeonBasePartsData>(assetPath);
                if (data != null)
                {
                    break;
                }
            }
            return data;
        }

        /// <Summary>
        /// Load map file.
        /// </Summary>
        FloorMapMasterData LoadFloorMapDataFile(string fileName)
        {
            // Load FloorMapMasterData data.
            string[] guids = AssetDatabase.FindAssets(fileName, null);

            FloorMapMasterData data = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                data = AssetDatabase.LoadAssetAtPath<FloorMapMasterData>(assetPath);
                if (data != null)
                {
                    break;
                }
            }
            return data;
        }

        /// <Summary>
        /// Set up floor map data for new version.
        /// </Summary>
        void SetUpFloorMapData(FloorMapMasterData mapData, MapAttributeData attrData, DungeonPartsData partsData, DungeonBasePartsData basePartsData)
        {
            mapData.mapAttributeDataList = new List<MapAttributeData>();
            mapData.mapAttributeDataList.Add(attrData);

            mapData.dungeonBasePartsData = basePartsData;

            mapData.dungeonPartsDataList = new List<DungeonPartsData>();
            mapData.dungeonPartsDataList.Add(partsData);

            SaveFloorMapDataFile(mapData);
        }

        /// <Summary>
        /// Save map file.
        /// </Summary>
        void SaveFloorMapDataFile(FloorMapMasterData mapData)
        {
            string path = AssetDatabase.GetAssetPath(mapData);
            var asset = (FloorMapMasterData)AssetDatabase.LoadAssetAtPath(path, typeof(FloorMapMasterData));
            if (asset != null)
            {
                EditorUtility.CopySerialized(mapData, asset);
            }
            AssetDatabase.Refresh();
        }

        /// <Summary>
        /// Upgrade script files.
        /// </Summary>
        void UpgradeScripts()
        {
            // Check Deprecated Directory.
            string folderPath = "Assets/Ariadne/Scripts";
            string folderName = "_Deprecated";
            string path = folderPath + "/" + folderName;

            bool folderExists = AssetDatabase.IsValidFolder(path);
            if (!folderExists)
            {
                AssetDatabase.CreateFolder(folderPath, folderName);
            }

            // Move scripts.
            List<string> deprecatedScripts = new List<string>();
            deprecatedScripts.Add("DungeonPartsManager");
            deprecatedScripts.Add("DungeonPartsMasterData");
            deprecatedScripts.Add("EventDoor");
            deprecatedScripts.Add("EventDoorBase");
            deprecatedScripts.Add("EventLockedDoor");
            deprecatedScripts.Add("EventMessenger");
            deprecatedScripts.Add("EventTreasure");
            deprecatedScripts.Add("FadeManager");
            deprecatedScripts.Add("FlagManager");
            deprecatedScripts.Add("IAriadneEventStrategy");
            deprecatedScripts.Add("IDoorOpen");
            deprecatedScripts.Add("IShowingMsg");
            deprecatedScripts.Add("ItemManager");
            deprecatedScripts.Add("MapAttributeDefine");
            deprecatedScripts.Add("EnterDungeonManager");
            deprecatedScripts.Add("DoorAnimator");
            deprecatedScripts.Add("TreasureAnimator");

            List<string> filterName = new List<string>();
            filterName.Add("AriadneFadeManager");
            filterName.Add("EventFlagManager");

            string[] subFolder = new string[]{"Assets/Ariadne"};

            foreach (string scriptName in deprecatedScripts)
            {
                string[] guids = AssetDatabase.FindAssets(scriptName, subFolder);

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var assetFile = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                    if (assetFile == null)
                    {
                        continue;
                    }

                    bool matched = false;
                    foreach (string filter in filterName)
                    {
                        if (assetFile.name == filter)
                        {
                            matched = true;
                            break;
                        }
                    }

                    if (matched)
                    {
                        continue;
                    }

                    string targetPath = path + "/" + scriptName + ".cs";

                    if (assetPath == targetPath)
                    {
                        continue;
                    }

                    AssetDatabase.MoveAsset(assetPath, targetPath);
                }
            }
            AssetDatabase.Refresh();
        }
    }
}