using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
    public class AchievementsImporter : AssetPostprocessor
    {
        static readonly string ExcelName = "Achievements.xlsx";
        
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
                {
                    CreateAchievementInfo(asset);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        private static void CreateAchievementInfo(string asset)
        {
            string FileName = Path.GetFileNameWithoutExtension(asset);
            string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";
            
			AchievementDates Data = AssetDatabase.LoadAssetAtPath<AchievementDates>(ExportPath);
            if (!Data)
            {
                // データがなければ作成
                Data = ScriptableObject.CreateInstance<AchievementDates>();
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

					List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(1));
                    ISheet BaseSheet = Book.GetSheetAt(0);
                    var KeyRow = BaseSheet.GetRow(0);
                    AssetPostImporter.SetKeyNames(KeyRow.Cells);

                    for (int i = 1; i <= BaseSheet.LastRowNum; i++)
                    {
                        IRow BaseRow = BaseSheet.GetRow(i);
                        var AchievementData = new AchievementData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Rank = AssetPostImporter.ImportNumeric(BaseRow, "Rank"),
                            ConditionType = (AchievementConditionType)AssetPostImporter.ImportNumeric(BaseRow, "ConditionType"),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, "Param1"),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, "Param2"),
                            PriseSetId = AssetPostImporter.ImportNumeric(BaseRow, "PriseSetId"),
                                    
                        };
                        AchievementData.Text = textData.Find(a => a.Id == AchievementData.Id).Text;
                        AchievementData.Help = textData.Find(a => a.Id == AchievementData.Id).Help;
                        Data.Data.Add(AchievementData);
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
