using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Ariadne
{
    /// <Summary>
    /// Editor script for ItemDataList.
    /// </Summary>
    [CustomEditor(typeof(ItemDataList))]
    public class ItemDataListEditor : Editor
    {
        /// <Summary>
        /// Show a list generation button on Inspector.
        /// </Summary>
        public override void OnInspectorGUI()
        {
            var itemDataList = target as ItemDataList;

            if (GUILayout.Button("Generate List"))
            {
                GenerateListProcess(itemDataList);
            }

            EditorGUILayout.Space();

            DrawDefaultInspector();
        }

        /// <Summary>
        /// Generate a list which references item data in the project.
        /// </Summary>
        void GenerateListProcess(ItemDataList itemDataList)
        {
            // Load all ItemMasterData in the project.
            string filter = "t:" + typeof(ItemMasterData).ToString();
            string[] guids = AssetDatabase.FindAssets(filter, null);

            // Add loaded data to itemList.
            List<ItemMasterData> itemList = new List<ItemMasterData>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemMasterData data = (ItemMasterData) AssetDatabase.LoadAssetAtPath(path, typeof(ItemMasterData));
                if (data != null)
                {
                    itemList.Add(data);
                }
            }

            // Sort the list by data ID.
            var query = itemList.OrderBy(i => i.itemId);
            var sortedList = query.ToList();

            // Set generated list to ItemDataList data.
            itemDataList.itemDataList = sortedList;

            // Save asset.
            string listAssetPath = AssetDatabase.GetAssetPath(itemDataList);
            var asset = (ItemDataList)AssetDatabase.LoadAssetAtPath(listAssetPath, typeof(ItemDataList));
            EditorUtility.CopySerialized(itemDataList, asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}