using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
    public class DungeonsImporter : AssetPostprocessor
    {
        static readonly string ExcelName = "Dungeon";

        // アセット更新があると呼ばれる
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (AssetPostImporter.CheckOnPostprocessAllAssets(asset, ExcelName, true))
                {
                    CreateDungeonsData(asset);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        private static void CreateDungeonsData(string asset)
        {
            Debug.Log("CreateDungeonsData");
            // 拡張子なしのファイル名を取得
            string FileName = Path.GetFileNameWithoutExtension(asset);

            // ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
            string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

            var Data = AssetDatabase.LoadAssetAtPath<DungeonDates>(ExportPath);
            if (!Data)
            {
                // データがなければ作成
                Data = ScriptableObject.CreateInstance<DungeonDates>();
                //AssetDatabase.CreateAsset(Data, ExportPath);
                //Data.hideFlags = HideFlags.NotEditable;
            }
            //Data.hideFlags = HideFlags.None;

            try
            {
                // ファイルを開く
                using (var Mainstream = File.Open(asset, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // エクセルブックを作成
                    AssetPostImporter.CreateBook(asset, Mainstream, out IWorkbook Book);
                    
                    CreateDungeonMain(Book,Data);

                    Data.FloorData.Clear();
                    ISheet DungeonSheet = Book.GetSheetAt(1);
                    var KeyRow = DungeonSheet.GetRow(0);
                    AssetPostImporter.SetKeyNames(KeyRow.Cells);
                    // 全て平面にする
                    var MapInfos = new List<Ariadne.MapInfo>();
                    var cols = Data.Data.Height;
                    for (int j = 1; j <= Data.Data.Width; j++)
                    {
                        IRow SymbolRow = DungeonSheet.GetRow(j);
                        for (int i = 1;i <= cols;i++)
                        {
                            var cell = AssetPostImporter.ImportString(SymbolRow, i.ToString());
                            Debug.Log(cell);
                            var attr = 0;
                            if (cell != "")
                            {
                                switch (cell)
                                {
                                    case "■":
                                        attr = 1;
                                        break;
                                    case "Ev":
                                        attr = 10;
                                        break;
                                    case "Out":
                                        attr = 11;
                                        break;
                                    case "Un":
                                        attr = 12;
                                        break;
                                }
                            }
                            var map = new Ariadne.MapInfo
                            {
                                eventId = 0,
                                mapAttr = attr,
                                objectTypeId = 0,
                                objectFront = 0
                            };
                            MapInfos.Add(map);
                        }
                    }
                    Data.FloorData = MapInfos;

                    // 専用データを作成
                    var floorData = ConvertFloorData(Data,FileName);
                    
                    // ダンジョンマスタ更新
                    string AriadoneExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, "Dungeon"+ Data.Data.Id.ToString("D4") + "")}.asset";
                    var AriadoneMasterData = AssetDatabase.LoadAssetAtPath<Ariadne.DungeonMasterData>(AriadoneExportPath);
                    if (!AriadoneMasterData)
                    {
                        // データがなければ作成
                        AriadoneMasterData = ScriptableObject.CreateInstance<Ariadne.DungeonMasterData>();
                        
                        AssetDatabase.CreateAsset(AriadoneMasterData, AriadoneExportPath);
                    }
                    AriadoneMasterData.hideFlags = HideFlags.NotEditable;
                    AriadoneMasterData.dungeonId = Data.Data.Id;
                    AriadoneMasterData.dungeonName = Data.Data.Name;
                    if (floorData != null)
                    {
                        if (AriadoneMasterData.floorList == null)
                        {
                            AriadoneMasterData.floorList = new();
                        }
                        var findIndex = AriadoneMasterData.floorList.FindIndex(a => a.floorId == floorData.floorId);
                        if (findIndex > -1)
                        {
                            AriadoneMasterData.floorList.RemoveAt(findIndex);
                        }
                        AriadoneMasterData.floorList.Add(floorData);
                    }
                    /*
                    ISheet SymbolSheet = Book.GetSheetAt(1);
                    KeyRow = SymbolSheet.GetRow(0);
                    AssetPostImporter.SetKeyNames(KeyRow.Cells);
                    for (int j = 1; j <= SymbolSheet.LastRowNum; j++)
                    {
                        IRow SymbolRow = SymbolSheet.GetRow(j);
                        var SymbolData = new Ariadne.MapInfo
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

                        MapInfos.Add(SymbolData);
                    }
                    */

                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            EditorUtility.SetDirty(Data);
        }

        private static void CreateDungeonMain(IWorkbook Book,DungeonDates Data)
        {
            // 情報の初期化
            Data.Data = null;
            //var stageId = int.Parse(FileName.Replace("Dungeon",""));

            // エクセルシートからセル単位で読み込み
            ISheet DataSheet = Book.GetSheetAt(0);
            var KeyRow = DataSheet.GetRow(0);
            AssetPostImporter.SetKeyNames(KeyRow.Cells);
            for (int j = 0; j <= DataSheet.LastRowNum; j++)
            {
                IRow DataRow = DataSheet.GetRow(j);
                var dungeonData = new DungeonData
                {
                    Id = AssetPostImporter.ImportNumeric(DataRow, "Id"),
                    FloorId = AssetPostImporter.ImportNumeric(DataRow, "FloorId"),
                    Width = AssetPostImporter.ImportNumeric(DataRow, "Width"),
                    Height = AssetPostImporter.ImportNumeric(DataRow, "Height"),
                    InitX = AssetPostImporter.ImportNumeric(DataRow, "InitX"),
                    InitY = AssetPostImporter.ImportNumeric(DataRow, "InitY"),
                    InitDir = AssetPostImporter.ImportNumeric(DataRow, "InitDir"),
                };
                Data.Data = dungeonData;
            }
        }

        private static Ariadne.FloorMapMasterData ConvertFloorData(DungeonDates Data,string FileName)
        {
            string AriadoneExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName + "_" + Data.Data.FloorId)}.asset";
            var AriadoneFloorData = AssetDatabase.LoadAssetAtPath<Ariadne.FloorMapMasterData>(AriadoneExportPath);
            if (!AriadoneFloorData)
            {
                // データがなければ作成
                AriadoneFloorData = ScriptableObject.CreateInstance<Ariadne.FloorMapMasterData>();
                
                AssetDatabase.CreateAsset(AriadoneFloorData, AriadoneExportPath);
            }
            AriadoneFloorData.hideFlags = HideFlags.None;
            //AriadoneFloorData.hideFlags = HideFlags.None;
            AriadoneFloorData.floorSizeVertical = Data.Data.Width;
            AriadoneFloorData.floorSizeHorizontal = Data.Data.Height;
            AriadoneFloorData.floorId = Data.Data.FloorId;
            AriadoneFloorData.floorName = Data.Data.Name;
            AriadoneFloorData.entrancePos = new Vector2Int(Data.Data.InitX,Data.Data.InitY);
            AriadoneFloorData.enteringDir = (Ariadne.DungeonDir)Data.Data.InitDir;
            AriadoneFloorData.mapInfo = Data.FloorData;
            return AriadoneFloorData;
        }
    }
}
