using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
    public class ItemsImporter : AssetPostprocessor
    {
        static readonly string ExcelName = "Items.xlsx";
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
                {
                    CreateItemInfo(asset);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        private static void CreateItemInfo(string asset)
        {
            string FileName = Path.GetFileNameWithoutExtension(asset);
            string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			ItemDates Data = AssetDatabase.LoadAssetAtPath<ItemDates>(ExportPath);
            if (!Data)
            {
                // データがなければ作成
                Data = ScriptableObject.CreateInstance<ItemDates>();
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
                        var ItemData = new ItemData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            IconIndex = AssetPostImporter.ImportNumeric(BaseRow, "IconIndex"),
                            ItemType = (ItemType)AssetPostImporter.ImportNumeric(BaseRow, "ItemType"),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, "Param1"),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, "Param2"),
                            Param3 = AssetPostImporter.ImportNumeric(BaseRow, "Param3"),
                        };
                        ItemData.Name = textData.Find(a => a.Id == ItemData.Id)?.Text;
                        ItemData.Help = textData.Find(a => a.Id == ItemData.Id)?.Help;
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
