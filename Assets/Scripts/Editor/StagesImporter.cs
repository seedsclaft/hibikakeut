using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
    public class StagesInfoImporter : AssetPostprocessor
    {
        private static readonly string ExcelName = "Stages.xlsx";

        // アセット更新があると呼ばれる
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (AssetPostImporter.CheckOnPostprocessAllAssets(asset, ExcelName))
                {
                    CreateStagesData(asset);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        private static void CreateStagesData(string asset)
        {
            // 拡張子なしのファイル名を取得
            string FileName = Path.GetFileNameWithoutExtension(asset);

            // ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
            string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

            var Data = AssetDatabase.LoadAssetAtPath<StageDates>(ExportPath);
            if (!Data)
            {
                // データがなければ作成
                Data = ScriptableObject.CreateInstance<StageDates>();
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
                    var textData = AssetPostImporter.CreateText(Book.GetSheetAt(4));

                    // 情報の初期化
                    Data.Data.Clear();

                    // エクセルシートからセル単位で読み込み
                    ISheet BaseSheet = Book.GetSheetAt(0);
                    ISheet EventSheet = Book.GetSheetAt(1);
                    ISheet EnemyRateSheet = Book.GetSheetAt(2);
                    ISheet TutorialSheet = Book.GetSheetAt(3);
                    for (int i = 1; i <= BaseSheet.LastRowNum; i++)
                    {
                        var KeyRow = BaseSheet.GetRow(0);
                        AssetPostImporter.SetKeyNames(KeyRow.Cells);
                        IRow BaseRow = BaseSheet.GetRow(i);

                        var StageData = new StageData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            StageNo = AssetPostImporter.ImportNumeric(BaseRow, "StageNo"),
                            Category = (StageCategory)AssetPostImporter.ImportNumeric(BaseRow, "Category"),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Text,
                            Selectable = AssetPostImporter.ImportNumeric(BaseRow, "Selectable") == 1,
                            Chapter = AssetPostImporter.ImportNumeric(BaseRow, "Chapter"),
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Help,
                            StageLv = AssetPostImporter.ImportNumeric(BaseRow, "StageLv"),
                            OnlyOnce = AssetPostImporter.ImportBool(BaseRow, "OnlyOnce"),
                            DisplayRank = AssetPostImporter.ImportNumeric(BaseRow, "DisplayRank"),
                            RandomTroopEnemyRates = new List<StageEnemyRate>(),
                            RandomTroopWeight = AssetPostImporter.ImportNumeric(BaseRow, "RandomTroopWeight"),
                            EncountMin = AssetPostImporter.ImportNumeric(BaseRow, "EncountMin"),
                            EncountMax = AssetPostImporter.ImportNumeric(BaseRow, "EncountMax"),
                            BackGround = AssetPostImporter.ImportString(BaseRow, "BackGround"),
                            BossTroopId = AssetPostImporter.ImportNumeric(BaseRow, "BossTroopId"),
                            BGMId = AssetPostImporter.ImportNumeric(BaseRow, "BGMId"),
                            BossBGMId = AssetPostImporter.ImportNumeric(BaseRow, "BossBGMId"),
                            BattleBGMId = AssetPostImporter.ImportNumeric(BaseRow, "BattleBGMId"),
                            SkyboxName = AssetPostImporter.ImportString(BaseRow, "SkyboxName"),
                            StageEvents = new List<StageEventData>()
                        };

                        KeyRow = EventSheet.GetRow(0);
                        AssetPostImporter.SetKeyNames(KeyRow.Cells);
                        for (int j = 1; j <= EventSheet.LastRowNum; j++)
                        {
                            IRow EventRow = EventSheet.GetRow(j);
                            var EventData = new StageEventData();
                            var StageId = AssetPostImporter.ImportNumeric(EventRow, "Id");

                            if (StageId == StageData.Id)
                            {
                                EventData.PositionX = AssetPostImporter.ImportNumeric(EventRow, "PositionX");
                                EventData.PositionY = AssetPostImporter.ImportNumeric(EventRow, "PositionY");
                                EventData.Timing = (EventTiming)AssetPostImporter.ImportNumeric(EventRow, "Timing");
                                EventData.Type = (StageEventType)AssetPostImporter.ImportNumeric(EventRow, "Type");
                                EventData.Param = AssetPostImporter.ImportNumeric(EventRow, "Param");
                                EventData.Param2 = AssetPostImporter.ImportNumeric(EventRow, "Param2");
                                EventData.Param3 = AssetPostImporter.ImportNumeric(EventRow, "Param3");
                                EventData.ReadFlag = AssetPostImporter.ImportBool(EventRow, "ReadFlag");
                                EventData.EventKey = StageId.ToString() + "_" + EventData.PositionX.ToString() + "_" + EventData.PositionY.ToString() + EventData.Timing.ToString() + EventData.Type.ToString() + EventData.Param.ToString();

                                StageData.StageEvents.Add(EventData);
                            }
                        }

                        KeyRow = EnemyRateSheet.GetRow(0);
                        AssetPostImporter.SetKeyNames(KeyRow.Cells);
                        for (int j = 1; j <= EnemyRateSheet.LastRowNum; j++)
                        {
                            IRow EnemyRateRow = EnemyRateSheet.GetRow(j);
                            var StageId = AssetPostImporter.ImportNumeric(EnemyRateRow, "Id");
                            var EnemyId = AssetPostImporter.ImportNumeric(EnemyRateRow, "EnemyId");
                            var Rate = AssetPostImporter.ImportNumeric(EnemyRateRow, "Weight");

                            if (StageId == StageData.Id)
                            {
                                var EnemyRateDate = new StageEnemyRate
                                {
                                    EnemyId = EnemyId,
                                    Weight = Rate,
                                };
                                StageData.RandomTroopEnemyRates.Add(EnemyRateDate);
                            }
                        }
                        Data.Data.Add(StageData);
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