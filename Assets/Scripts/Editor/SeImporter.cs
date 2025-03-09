using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class SeImporter : AssetPostprocessor 
	{
		//static readonly string ExcelPath = "Assets/Resources/Data";
		static readonly string ExcelName = "SE.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
		{
			foreach (string asset in importedAssets) 
			{
				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateInfo(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateInfo(string asset)
		{
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			SoundDates Data = AssetDatabase.LoadAssetAtPath<SoundDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<SoundDates>();
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

					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);
					var KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var BGM = new SoundData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Key = AssetPostImporter.ImportString(BaseRow, "Key"),
                            FileName = AssetPostImporter.ImportString(BaseRow, "FileName"),
                            Volume = AssetPostImporter.ImportFloat(BaseRow, "Volume"),
                            //Loop = AssetPostImporter.ImportBool(BaseRow, "Loop"),
                            //CrossFade = AssetPostImporter.ImportString(BaseRow, "CrossFade")
                        };
                        Data.Data.Add(BGM);
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