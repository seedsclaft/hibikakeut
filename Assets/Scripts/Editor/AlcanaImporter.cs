
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class AlcanaInfoImporter : AssetPostprocessor {
		enum BaseColumn
		{
			Id = 0,
			NameId,
			FilePath,
			SkillId
		}
		static readonly string ExcelName = "Alcana.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (string asset in importedAssets) {

				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateAlcanaData(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateAlcanaData(string asset)
		{
			Debug.Log("CreateAlcanaData");
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			AlcanaData Data = AssetDatabase.LoadAssetAtPath<AlcanaData>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<AlcanaData>();
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
					List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(1));

					// 情報の初期化
					Data._data.Clear();

					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

						var Alcana = new AlcanaData.Alcana();
						Alcana.Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Id);
						Alcana.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameId)).Text;
						Alcana.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameId)).Help;
						Alcana.FilePath = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.FilePath);
						Alcana.SkillId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.SkillId);
						
						Data._data.Add(Alcana);
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