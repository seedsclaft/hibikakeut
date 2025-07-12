using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
    public class BuildingsImporter : AssetPostprocessor
    {
        static readonly string ExcelName = "Buildings.xlsx";
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
                {
                    CreateBuildings(asset);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        private static void CreateBuildings(string asset)
        {
            string FileName = Path.GetFileNameWithoutExtension(asset);
            string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			BuildingsDates Data = AssetDatabase.LoadAssetAtPath<BuildingsDates>(ExportPath);
            if (!Data)
            {
                // データがなければ作成
                Data = ScriptableObject.CreateInstance<BuildingsDates>();
                AssetDatabase.CreateAsset(Data, ExportPath);
            }
            Data.hideFlags = HideFlags.None;

            try
            {
                // ファイルを開く
                using (var Mainstream = File.Open(asset, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // エクセルブックを作成
                    AssetPostImporter.CreateBook(asset, Mainstream, out IWorkbook Book);
                    List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(1));

                    // 情報の初期化
                    Data.Data.Clear();

                    ISheet BaseSheet = Book.GetSheetAt(0);
                    var KeyRow = BaseSheet.GetRow(0);
                    AssetPostImporter.SetKeyNames(KeyRow.Cells);

                    for (int i = 1; i <= BaseSheet.LastRowNum; i++)
                    {
                        IRow BaseRow = BaseSheet.GetRow(i);
                        if (BaseRow == null)
                        {
                            continue;
                        }
                        var Id = AssetPostImporter.ImportNumeric(BaseRow, "Id");
                        if (Id <= 0)
                        {
                            continue;
                        }
                        var ItemData = new BuildingsData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Text,
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Help,
                            ImagePath = AssetPostImporter.ImportString(BaseRow, "ImagePath"),
                            Cost = AssetPostImporter.ImportNumeric(BaseRow, "Cost"),
                            Chapter = AssetPostImporter.ImportNumeric(BaseRow, "Chapter"),
                            SkillId = AssetPostImporter.ImportNumeric(BaseRow, "SkillId"),
                            NeedBuildingId = AssetPostImporter.ImportNumeric(BaseRow, "NeedBuildingId"),
                        };
                        Data.Data.Add(ItemData);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            EditorUtility.SetDirty(Data);
        }
    }
}
