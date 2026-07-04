
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
    public class AdvImporter : AssetPostprocessor
    {
        static readonly string ExcelName = "Adventures.xlsx";

        // アセット更新があると呼ばれる
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (AssetPostImporter.CheckOnPostprocessAllAssets(asset, ExcelName))
                {
                    CreateAdvInfo(asset);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        private static void CreateAdvInfo(string asset)
        {
            Debug.Log("CreateAdvInfo");
            // 拡張子なしのファイル名を取得
            string FileName = Path.GetFileNameWithoutExtension(asset);

            // ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
            string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

            AdvDates Data = AssetDatabase.LoadAssetAtPath<AdvDates>(ExportPath);
            if (!Data)
            {
                // データがなければ作成
                Data = ScriptableObject.CreateInstance<AdvDates>();
                AssetDatabase.CreateAsset(Data, ExportPath);
                Data.hideFlags = HideFlags.NotEditable;
            }

            try
            {
                // ファイルを開く
                using (var Mainstream = File.Open(asset, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // エクセルブックを作成
                    AssetPostImporter.CreateBook(asset, Mainstream, out IWorkbook Book);

                    // 情報の初期化
                    Data.Data.Clear();

                    // エクセルシートからセル単位で読み込み
                    ISheet BaseSheet = Book.GetSheetAt(0);
                    var KeyRow = BaseSheet.GetRow(0);
                    AssetPostImporter.SetKeyNames(KeyRow.Cells);

                    for (int i = 1; i <= BaseSheet.LastRowNum; i++)
                    {
                        IRow BaseRow = BaseSheet.GetRow(i);

                        var AdvData = new AdvData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            AdvName = AssetPostImporter.ImportString(BaseRow, "AdvName"),
                            Timing = (EventTiming)AssetPostImporter.ImportNumeric(BaseRow, "Timing"),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, "Param1"),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, "Param2"),
                            Param3 = AssetPostImporter.ImportNumeric(BaseRow, "Param3"),
                            ReadFlag = AssetPostImporter.ImportBool(BaseRow, "ReadFlag"),
                            PrizeSetId = AssetPostImporter.ImportNumeric(BaseRow, "PrizeSetId")
                        };
                        AdvData.EventKey = AdvData.Id.ToString() + "_" + AdvData.AdvName;
                        Data.Data.Add(AdvData);
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