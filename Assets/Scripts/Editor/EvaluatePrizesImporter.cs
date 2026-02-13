using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
    public class EvaluatePrizesImporter : AssetPostprocessor
    {
        static readonly string ExcelName = "EvaluatePrizes.xlsx";
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
                {
                    CreateEvaluatePrizes(asset);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        private static void CreateEvaluatePrizes(string asset)
        {
            string FileName = Path.GetFileNameWithoutExtension(asset);
            string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			EvaluatePrizeDates Data = AssetDatabase.LoadAssetAtPath<EvaluatePrizeDates>(ExportPath);
            if (!Data)
            {
                // データがなければ作成
                Data = ScriptableObject.CreateInstance<EvaluatePrizeDates>();
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
                        var ItemData = new EvaluatePrizeData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Chapter = AssetPostImporter.ImportNumeric(BaseRow, "Chapter"),
                            Category = AssetPostImporter.ImportNumeric(BaseRow, "Category"),
                            ConditionType = (AchievementConditionType)AssetPostImporter.ImportNumeric(BaseRow, "ConditionType"),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, "Param1"),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, "Param2"),
                            PriseSetId = AssetPostImporter.ImportNumeric(BaseRow, "PriseSetId"),
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
