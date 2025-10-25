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
        private static readonly string ExcelName = "Dungeon";

        // アセット更新があると呼ばれる
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
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
            Debug.Log("CreateDungeonsData" + asset);
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

                    CreateDungeonMain(Book, Data);

                    Data.FloorData.Clear();
                    ISheet DungeonSheet = Book.GetSheetAt(1);
                    var KeyRow = DungeonSheet.GetRow(0);
                    AssetPostImporter.SetKeyNames(KeyRow.Cells);
                    // 全て平面にする
                    var MapInfos = new List<Ariadne.MapInfo>();
                    var cols = Data.Data.Height;
                    for (int j = 1; j <= cols; j++)
                    {
                        IRow SymbolRow = DungeonSheet.GetRow(j);
                        for (int i = 1;i <= Data.Data.Width;i++)
                        {
                            var map = CreateCells(SymbolRow, i.ToString(), (i-1) + (j-1) * cols);
                            MapInfos.Add(map);
                        }
                    }
                    Data.FloorData = MapInfos;

                    ISheet RegeonSheet = Book.GetSheetAt(2);
                    var KeyRow2 = RegeonSheet.GetRow(0);
                    AssetPostImporter.SetKeyNames(KeyRow2.Cells);
                    for (int j = 1; j <= cols; j++)
                    {
                        IRow SymbolRow = RegeonSheet.GetRow(j);
                        for (int i = 1;i <= Data.Data.Width;i++)
                        {
                            var reageonNo = CreateRegeons(SymbolRow, i.ToString(), (i-1) + (j-1) * cols);
                            var map = Data.FloorData.Find(a => a.eventId == (i-1) + (j-1) * cols);
                            if (map != null && reageonNo > 0)
                            {
                                map.regeonNo = reageonNo;
                            }
                        }
                    }

                    // 専用データを作成
                    var floorData = ConvertFloorData(Data, FileName);

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
                    EditorUtility.SetDirty(AriadoneMasterData);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            EditorUtility.SetDirty(Data);
        }

        private static Ariadne.MapInfo CreateCells(IRow SymbolRow, string cellText, int cellIndex)
        {
            var cell = AssetPostImporter.ImportString(SymbolRow, cellText);
            Debug.Log(cell);
            var attr = 0;
            var front = 0;
            if (cell != "")
            {
                if (cell.Contains("□"))
                {
                    attr = 99;
                } else
                if (cell.Contains("■"))
                {
                    attr = 1;
                } else
                if (cell.Contains("DrS"))
                {
                    attr = 3;
                } else
                if (cell.Contains("Dr"))
                {
                    attr = 2;
                } else
                if (cell.Contains("Ta"))
                {
                    attr = 4;
                } else
                if (cell.Contains("Ba"))
                {
                    attr = 5;
                } else
                if (cell.Contains("Tr"))
                {
                    attr = 6;
                } else
                if (cell.Contains("Ar"))
                {
                    attr = 7;
                } else
                if (cell.Contains("Mg"))
                {
                    attr = 8;
                } else
                if (cell.Contains("No"))
                {
                    attr = 9;
                } else
                if (cell.Contains("Ev"))
                {
                    attr = 10;
                } else
                if (cell.Contains("Out"))
                {
                    attr = 11;
                } else
                if (cell.Contains("Un"))
                {
                    attr = 12;
                } else
                if (cell.Contains("Mv"))
                {
                    attr = 13;
                } else
                if (cell.Contains("Dm"))
                {
                    attr = 14;
                } else
                if (cell.Contains("◆"))
                {
                    attr = 15;
                }

                if (cell.Contains("D1"))
                {
                    front = 1;
                } else
                if (cell.Contains("D2"))
                {
                    front = 2;
                } else
                if (cell.Contains("D3"))
                {
                    front = 3;
                }
            }
            var map = new Ariadne.MapInfo
            {
                eventId = cellIndex,
                mapAttr = attr,
                objectTypeId = 0,
                objectFront = (Ariadne.DungeonDir)front
            };
            return map;
        }

        private static int CreateRegeons(IRow SymbolRow, string cellText, int cellIndex)
        {
            var cell = AssetPostImporter.ImportString(SymbolRow, cellText);
            Debug.Log(cell);
            var regeon = 0;
            if (cell != "")
            {
                if (cell.Contains("①"))
                {
                    regeon = 1;
                } else
                if (cell.Contains("②"))
                {
                    regeon = 2;
                } else
                if (cell.Contains("③"))
                {
                    regeon = 3;
                } else
                if (cell.Contains("④"))
                {
                    regeon = 4;
                }
            }
            return regeon;
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
            AriadoneFloorData.floorSizeVertical = Data.Data.Height;
            AriadoneFloorData.floorSizeHorizontal = Data.Data.Width;
            AriadoneFloorData.floorId = Data.Data.FloorId;
            AriadoneFloorData.floorName = Data.Data.Name;
            AriadoneFloorData.entrancePos = new Vector2Int(Data.Data.InitX,Data.Data.InitY);
            AriadoneFloorData.enteringDir = (Ariadne.DungeonDir)Data.Data.InitDir;
            AriadoneFloorData.mapInfo = Data.FloorData;
            AriadoneFloorData.DungeonCompletion = Data.FloorData.FindAll(a => a.mapAttr is 0 or 2 or 3 or 8 or 10 or 11).Count;
            EditorUtility.SetDirty(AriadoneFloorData);
            return AriadoneFloorData;
        }
    }
}
