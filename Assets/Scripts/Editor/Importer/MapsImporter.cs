using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
    public class MapsImporter : AssetPostprocessor
    {
        private static readonly string ExcelName = "Map";

        // アセット更新があると呼ばれる
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (AssetPostImporter.CheckOnPostprocessAllAssets(asset, ExcelName, true))
                {
                    CreateMapsData(asset);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        private static void CreateMapsData(string asset)
        {
            Debug.Log("CreateMapsData");
        }
    }
}
