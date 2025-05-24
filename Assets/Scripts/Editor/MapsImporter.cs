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
        static readonly string ExcelName = "Map_dummy";

        // アセット更新があると呼ばれる
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
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
            // 拡張子なしのファイル名を取得
            string FileName = Path.GetFileNameWithoutExtension(asset);

            // ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
            string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

            var Data = AssetDatabase.LoadAssetAtPath<MapDates>(ExportPath);
            if (!Data)
            {
                // データがなければ作成
                Data = ScriptableObject.CreateInstance<MapDates>();
                AssetDatabase.CreateAsset(Data, ExportPath);
                //Data.hideFlags = HideFlags.NotEditable;
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
                    var stageId = int.Parse(FileName.Replace("Map",""));

                    // エクセルシートからセル単位で読み込み
                    ISheet MapSheet = Book.GetSheetAt(0);
                    var KeyRow = MapSheet.GetRow(0);
                    AssetPostImporter.SetKeyNames(KeyRow.Cells);

                    var StageSymbols = new List<StageSymbolData>();
                    var idx = 0;
                    for (int j = 0; j <= MapSheet.LastRowNum; j++)
                    {
                        IRow SymbolRow = MapSheet.GetRow(j);
                        var cols = KeyRow.Cells.Count;
                        for (int i = 0;i < cols;i++)
                        {
                            var SymbolData = new StageSymbolData();
                            SymbolData.Id = idx;
                            idx++;
                            SymbolData.StageId = stageId;
                            SymbolData.InitX = i;
                            SymbolData.InitY = j;
                            var cell = AssetPostImporter.ImportString(SymbolRow, i.ToString());
                            if (cell == "-" || i == 0 || j == 0)
                            {
                                SymbolData.UnitType = HexUnitType.None;
                            } else
                            {
                                continue;
                            }
                            StageSymbols.Add(SymbolData);
                        }
                        idx++;
                    }

                    ISheet SymbolSheet = Book.GetSheetAt(1);
                    KeyRow = SymbolSheet.GetRow(0);
                    AssetPostImporter.SetKeyNames(KeyRow.Cells);
                    for (int j = 1; j <= SymbolSheet.LastRowNum; j++)
                    {
                        IRow SymbolRow = SymbolSheet.GetRow(j);
                        var SymbolData = new StageSymbolData
                        {
                            Id = AssetPostImporter.ImportNumeric(SymbolRow, "Id"),
                            StageId = stageId,
                            InitX = AssetPostImporter.ImportNumeric(SymbolRow, "InitX"),
                            InitY = AssetPostImporter.ImportNumeric(SymbolRow, "InitY"),
                            //SymbolData.SymbolType = (SymbolType)AssetPostImporter.ImportNumeric(SymbolRow, "SymbolType");
                            UnitType = (HexUnitType)AssetPostImporter.ImportNumeric(SymbolRow, "UnitType"),
                            InitTeamId = (TeamIdType)AssetPostImporter.ImportNumeric(SymbolRow, "InitTeamId"),
                            Rate = AssetPostImporter.ImportNumeric(SymbolRow, "Rate"),
                            Param1 = AssetPostImporter.ImportNumeric(SymbolRow, "Param1"),
                            Param2 = AssetPostImporter.ImportNumeric(SymbolRow, "Param2"),
                            PrizeSetId = AssetPostImporter.ImportNumeric(SymbolRow, "PrizeSetId"),
                            ClearCount = AssetPostImporter.ImportNumeric(SymbolRow, "ClearCount"),
                            MoveType = (UnitMoveType)AssetPostImporter.ImportNumeric(SymbolRow, "MoveType")
                        };
                        //SymbolData.MoveTypeParam = (MoveTypeParam)AssetPostImporter.ImportNumeric(SymbolRow, "MoveParam");

                        StageSymbols.Add(SymbolData);
                    }

                    Data.Data = StageSymbols;
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
