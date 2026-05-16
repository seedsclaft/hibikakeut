
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
    public class EquipmentImporter : AssetPostprocessor
    {
        static readonly string ExcelName = "Equipments.xlsx";

        // アセット更新があると呼ばれる
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (AssetPostImporter.CheckOnPostprocessAllAssets(asset, ExcelName))
                {
                    CreateEquipmentData(asset);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        private static void CreateEquipmentData(string asset)
        {
            // 拡張子なしのファイル名を取得
            string FileName = Path.GetFileNameWithoutExtension(asset);

            // ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
            string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

            EquipmentDates Data = AssetDatabase.LoadAssetAtPath<EquipmentDates>(ExportPath);
            if (!Data)
            {
                // データがなければ作成
                Data = ScriptableObject.CreateInstance<EquipmentDates>();
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
                    List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(2));

                    // 情報の初期化
                    Data.Data.Clear();

                    // エクセルシートからセル単位で読み込み
                    ISheet BaseSheet = Book.GetSheetAt(0);
                    var KeyRow = BaseSheet.GetRow(0);
                    AssetPostImporter.SetKeyNames(KeyRow.Cells);

                    for (int i = 1; i <= BaseSheet.LastRowNum; i++)
                    {
                        IRow BaseRow = BaseSheet.GetRow(i);

                        var EquipmentData = new EquipmentData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Text,
                            ImagePath = AssetPostImporter.ImportString(BaseRow, "ImagePath"),
                            Rank = AssetPostImporter.ImportNumeric(BaseRow, "Rank"),
                            Attribute = (AttributeType)AssetPostImporter.ImportNumeric(BaseRow, "Attribute"),
                        };
                        if (EquipmentData.Id == 0)
                        {
                            continue;
                        }

                        EquipmentData.LearningDates = new List<EquipmentLearningData>();
                        Data.Data.Add(EquipmentData);
                    }

                    BaseSheet = Book.GetSheetAt(1);
                    KeyRow = BaseSheet.GetRow(0);
                    AssetPostImporter.SetKeyNames(KeyRow.Cells);

                    for (int i = 1; i <= BaseSheet.LastRowNum; i++)
                    {
                        IRow BaseRow = BaseSheet.GetRow(i);

                        int EquipmentId = AssetPostImporter.ImportNumeric(BaseRow, "EquipmentId");
                        EquipmentData Equipment = Data.Data.Find(a => a.Id == EquipmentId);
                        if (Equipment == null)
                        {
                            continue;
                        }
                        EquipmentLearningData equipmentLearningData = new();
                        
                        equipmentLearningData.SkillId = AssetPostImporter.ImportNumeric(BaseRow, "SkillId");
                        equipmentLearningData.Rate = AssetPostImporter.ImportNumeric(BaseRow, "LearningRate");
                        Equipment.LearningDates.Add(equipmentLearningData);
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