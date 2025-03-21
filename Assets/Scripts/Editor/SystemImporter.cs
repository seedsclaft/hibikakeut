using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;
using System;

namespace Ryneus
{
	public class SystemImporter : AssetPostprocessor
	{
		enum BaseColumn
		{
			Id = 0,
			Key,
			NameTextId,
		}

		enum BaseDefineColumn
		{
			Key = 0,
			Param,
		}

		static readonly string ExcelName = "System.xlsx";
		
		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (string asset in importedAssets) 
			{
				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateMenuCommandInfo(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateMenuCommandInfo(string asset)
		{
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			// ExportPath内のMenuCommandInfoListを検索
			SystemData Data = AssetDatabase.LoadAssetAtPath<SystemData>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<SystemData>();
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
					List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(6));

					// 情報の初期化
					Data.TacticsCommandData = new ();
					Data.StatusCommandData = new ();
					Data.OptionCommandData = new ();
					Data.TitleCommandData = new ();
					Data.SystemTextData = new ();
					Data.SystemTextData = textData;
					Data.InputDataList = new ();

					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var TitleCommandInfo = new SystemData.CommandData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Id),
                            Key = AssetPostImporter.ImportString(BaseRow, (int)BaseColumn.Key),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameTextId)).Text,
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameTextId)).Help
                        };
                        Data.TacticsCommandData.Add(TitleCommandInfo);
					}
					
					BaseSheet = Book.GetSheetAt(1);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var TitleCommandInfo = new SystemData.CommandData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Id),
                            Key = AssetPostImporter.ImportString(BaseRow, (int)BaseColumn.Key),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameTextId)).Text,
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameTextId)).Help
                        };
                        Data.TitleCommandData.Add(TitleCommandInfo);
					}

					BaseSheet = Book.GetSheetAt(2);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var StatusCommandInfo = new SystemData.CommandData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Id),
                            Key = AssetPostImporter.ImportString(BaseRow, (int)BaseColumn.Key),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameTextId)).Text,
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameTextId)).Help
                        };
                        Data.StatusCommandData.Add(StatusCommandInfo);
					}

					BaseSheet = Book.GetSheetAt(3);
					var KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var OptionCommand = new SystemData.OptionCommand
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Key = AssetPostImporter.ImportString(BaseRow, "Key"),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameTextId")).Text,
                            Category = AssetPostImporter.ImportNumeric(BaseRow, "Category"),
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameTextId")).Help,
                            ButtonType = (OptionButtonType)AssetPostImporter.ImportNumeric(BaseRow, "ButtonType"),
                            ToggleText1 = AssetPostImporter.ImportNumeric(BaseRow, "ToggleText1"),
                            ToggleText2 = AssetPostImporter.ImportNumeric(BaseRow, "ToggleText2"),
                            ToggleText3 = AssetPostImporter.ImportNumeric(BaseRow, "ToggleText3"),
                            
							ExistWindows = AssetPostImporter.ImportNumeric(BaseRow, "ExistWindows") == 1,
							ExistAndroid = AssetPostImporter.ImportNumeric(BaseRow, "ExistAndroid") == 1
						};
                        Data.OptionCommandData.Add(OptionCommand);
					}
					
					BaseSheet = Book.GetSheetAt(4);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);
                        var inputData = new SystemData.InputData
                        {
                            Key = AssetPostImporter.ImportString(BaseRow, (int)BaseColumn.Id),
                            KeyId = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Key),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameTextId))?.Text
                        };

                        Data.InputDataList.Add(inputData);
					}

					BaseSheet = Book.GetSheetAt(5);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

						var KeyName = AssetPostImporter.ImportString(BaseRow,(int)BaseDefineColumn.Key);
						
						if (KeyName == "initCurrency")
						{
							Data.InitCurrency = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
						}
						if (KeyName == "trainCount")
						{
							Data.TrainCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
						}
						if (KeyName == "alchemyCount")
						{
							Data.AlchemyCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
						}
						if (KeyName == "recoveryCount")
						{
							Data.RecoveryCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
						}
						if (KeyName == "battleCount")
						{
							Data.BattleCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
						}
						if (KeyName == "resourceCount")
						{
							Data.ResourceCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
						}
						if (KeyName == "alcanaSelectCount")
						{
							Data.AlcanaSelectCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
						}
						if (KeyName == "battleBonusValue")
						{
							Data.BattleBonusValue = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
						}
						if (KeyName == "WeakPointRate")
						{
							Data.WeakPointRate = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
						}
						if (KeyName == "StartStageId")
						{
							Data.StartStageId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
						}
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