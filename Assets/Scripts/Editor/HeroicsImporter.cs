using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
    public class HeroicsImporter : AssetPostprocessor
    {
        static readonly string ExcelName = "Heroics.xlsx";
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (AssetPostImporter.CheckOnPostprocessAllAssets(asset, ExcelName))
                {
                    CreateHeroicInfo(asset);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        private static void CreateHeroicInfo(string asset)
        {
            string FileName = Path.GetFileNameWithoutExtension(asset);
            string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

            HeroicDates Data = AssetDatabase.LoadAssetAtPath<HeroicDates>(ExportPath);
            if (!Data)
            {
                // データがなければ作成
                Data = ScriptableObject.CreateInstance<HeroicDates>();
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
                        if (BaseRow == null)
                        {
                            continue;
                        }
                        var heroicData = new HeroicData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Param = AssetPostImporter.ImportNumeric(BaseRow, "Param"),
                            MinLv = AssetPostImporter.ImportNumeric(BaseRow, "MinLv"),
                            MaxLv = AssetPostImporter.ImportNumeric(BaseRow, "MaxLv"),
                        };
                        heroicData.Name = textData.Find(a => a.Id == heroicData.Id)?.Text;
                        heroicData.Help = textData.Find(a => a.Id == heroicData.Id)?.Help;
                        Data.Data.Add(heroicData);
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
