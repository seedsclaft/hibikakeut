using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text;
using NPOI.SS.UserModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ryneus
{
	public class CommonEventImporter : AssetPostprocessor 
	{
        static readonly string ImportPath = "Assets/Data";
		static readonly string FileName = "CommonEvents.json";
        static readonly string ExportPath = "Assets/ADVScene/AdvFiles/";

        // Fileがあったら呼ばれる
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
        {	
            foreach (string asset in importedAssets) 
            {
                if (CheckOnPostprocessAsset(asset,FileName))
                {
                    ConvertJson(asset);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        public static bool CheckOnPostprocessAsset(string asset,string FileName)
        {
            string ext = Path.GetExtension(asset);
            if (ext != ".json") return false;

            string fileName = Path.GetFileName(asset);
            // 同じパスのみ
            string filePath = Path.GetDirectoryName(asset);
            filePath = filePath.Replace("\\", "/");
            if (filePath != ImportPath) return false;
            // 同じファイルのみ
            if (fileName != FileName) return false;

            return true;
        }

		static void ConvertJson(string asset)
		{
            Debug.Log("ConvertJson");
            string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

            // jsonを開く
            var data = AssetDatabase.LoadAssetAtPath<TextAsset>(ImportPath + "/" + FileName + ".json");
            var stringData = data.ToString();
            var convert = "{\"data\":[";
            convert += stringData.Substring(1,stringData.Length-1);
            convert += "}";
            var CommonEventDates = JsonUtility.FromJson<CommonEventMasterDates>(convert);
            var CommonEventSoundDates = JsonUtility.FromJson<CommonEventMasterSoundDates>(convert);
            foreach (var CommonEventData in CommonEventDates.data)
            {
                if (CommonEventData.list == null)
                {
                    continue;
                }
                var idx = 0;
                foreach (var item in CommonEventData.list)
                {
                    if (item.code is 241 or 245 or 250)
                    {
                        var soundData = CommonEventSoundDates.data.ToList().Find(a => a.id == CommonEventData.id).list[idx].parameters;
                        if (soundData != null)
                        {
                            item.soundDate = soundData[0];
                        }
                    }
                    idx++;
                }
            }

			CommonEventDates Data = AssetDatabase.LoadAssetAtPath<CommonEventDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<CommonEventDates>();
				AssetDatabase.CreateAsset(Data, ExportPath);
			}
            // 情報の初期化
            Data.hideFlags = HideFlags.None;
            Data.data = CommonEventDates.data.ToList();
        }
        
        [MenuItem ("Resources/CommonEvent")]
        static void CommonEvent() 
        {
            var CommonEventDates = Resources.Load<CommonEventDates>("Data/CommonEvents").data;
            if (CommonEventDates != null)
            {
                foreach (var d in CommonEventDates)
                {
                    var csvColList = new List<List<string>>();
                    var csvStrings = new List<string>
                    {
                        // パラメータ設定
                        "Command,Arg1,Arg2,Arg3,Arg4,Arg5,Arg6,WaitType,Text,PageCtrl,Voice,WindowType" + "\n"
                    };
                    if (d.list == null)
                    {
                        continue;
                    }
                    if (d.id < 100)
                    {
                        continue;
                    }
                    var lastName = "";
                    var lastFace = 0;
                    var lastPosition = -1;
                    var doubleText = false;
                    var selection2 = new List<string>();
                    var paramFlag = -1;
                    var codeIdx = 0;
                    foreach (var l in d.list)
                    {
                        var waitCommand = false;
                        var fadeOutCommand = false;
                        var csvCol = new List<string>();
                        switch (l.code)
                        {
                            case 101: // メッセージ準備
                                lastName = l.parameters[0];
                                lastFace = int.Parse(l.parameters[1]);
                                lastPosition = int.Parse(l.parameters[3]);
                                doubleText = false;
                            break;
                            case 401: // メッセージ表示
                                if (doubleText)
                                {
                                    doubleText = false;
                                    break;
                                }
                                csvCol.Add("");
                                // アクター名
                                if (lastName != "")
                                {
                                    csvCol.Add(lastName + "_" + lastFace.ToString("00")); 
                                } else
                                {
                                    csvCol.Add("");
                                }
                                csvCol.Add("");
                                 // ポジション
                                if (lastName != "" && lastPosition == 0)
                                {
                                    csvCol.Add("Character0");
                                } else
                                if (lastName != "" && lastPosition == 1)
                                {
                                    csvCol.Add("Character1");
                                } else
                                if (lastName != "" && lastPosition == 2)
                                {
                                    csvCol.Add("Character2");
                                } else
                                {
                                    csvCol.Add("");
                                }
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("0");
                                csvCol.Add("");
                                // 文章
                                var nextCode = d.list[codeIdx+1];
                                var mainText = "";
                                if (nextCode.code == 401)
                                {
                                    // 2行データ
                                    mainText = l.parameters[0] +"\\n" + nextCode.parameters[0];
                                    doubleText = true;
                                } else
                                {
                                    mainText = l.parameters[0];
                                }
                                // キャラ名を置き換え
                                mainText = mainText.Replace("\\N[1]","エリシャ");
                                mainText = mainText.Replace("\\N[2]","ソラ");
                                mainText = mainText.Replace("\\N[3]","リジェ");
                                mainText = mainText.Replace("\\N[4]","ミシェル");
                                mainText = mainText.Replace("\\N[5]","シイナ");
                                mainText = mainText.Replace("\\N[6]","ルネ");
                                mainText = mainText.Replace("\\N[7]","マリー");
                                mainText = mainText.Replace("\\N[11]","出航所の係");
                                mainText = mainText.Replace("\\N[12]","子供");
                                mainText = mainText.Replace("\\N[13]","子供エリシャ");
                                mainText = mainText.Replace("\\N[14]","母親（？）");
                                csvCol.Add(mainText);
                                // 次が選択肢ならInput
                                if (nextCode.code == 102)
                                {
                                    csvCol.Add("Input");
                                }
                                break;
                            case 102: // 選択肢
                                csvCol.Add("Selection");
                                csvCol.Add("*" + d.id.ToString() + "_0");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                // 402データから取得
                                var SelectionList = d.list.ToList().FindAll(a => a.code == 402);
                                var selectionText = SelectionList[0].parameters[1];
                                selectionText = selectionText.Replace("\\N[1]","エリシャ");
                                selectionText = selectionText.Replace("\\N[2]","ソラ");
                                selectionText = selectionText.Replace("\\N[3]","リジェ");
                                selectionText = selectionText.Replace("\\N[4]","ミシェル");
                                selectionText = selectionText.Replace("\\N[5]","シイナ");
                                selectionText = selectionText.Replace("\\N[6]","ルネ");
                                selectionText = selectionText.Replace("\\N[7]","マリー");
                                csvCol.Add(selectionText);

                                selection2.Add("Selection");
                                selection2.Add("*" + d.id.ToString() + "_1");
                                selection2.Add("");
                                selection2.Add("");
                                selection2.Add("");
                                selection2.Add("");
                                selection2.Add("");
                                selection2.Add("");
                                selectionText = SelectionList[1].parameters[1];
                                selectionText = selectionText.Replace("\\N[1]","エリシャ");
                                selectionText = selectionText.Replace("\\N[2]","ソラ");
                                selectionText = selectionText.Replace("\\N[3]","リジェ");
                                selectionText = selectionText.Replace("\\N[4]","ミシェル");
                                selectionText = selectionText.Replace("\\N[5]","シイナ");
                                selectionText = selectionText.Replace("\\N[6]","ルネ");
                                selectionText = selectionText.Replace("\\N[7]","マリー");
                                selection2.Add(selectionText);
                                break;
                            case 402: // 選択肢のラベル
                                csvCol.Add("*" + d.id.ToString() + "_" + l.parameters[0]);
                                paramFlag = int.Parse(l.parameters[0]);
                                break;
                            case 118: // ラベル
                                // 選択肢前にLayerReset
                                csvCol.Add("*" + d.id.ToString() + "_" + l.parameters[0]);
                                if (l.parameters[0] == "選択肢前")
                                {
                                    selection2.Add("LayerReset");
                                    selection2.Add("All");
                                }
                                break;
                            case 119: // ラベルジャンプ
                                csvCol.Add("Jump");
                                csvCol.Add("*" + d.id.ToString() + "_" + l.parameters[0]);
                                // 選択肢後
                                if (l.parameters[0] == "選択肢後")
                                {
                                    // 条件を追加
                                    csvCol.Add("SelectionParam_0 && SelectionParam_1");
                                }
                                break;
                            case 108: // 注釈
                                break;
                            case 213: // 吹き出し表示
                                csvCol.Add("Balloon");
                                switch (l.parameters[0])
                                {
                                    case "1":
                                    csvCol.Add("Character2");
                                    break;
                                    case "2":
                                    csvCol.Add("Character0");
                                    break;
                                }
                                csvCol.Add(l.parameters[1]);
                            break;
                            case 221: // フェードアウト
                                fadeOutCommand = true;
                                csvCol.Add("FadeOut");
                                csvCol.Add("black");
                                break;
                            case 222: // フェードイン
                                csvCol.Add("FadeIn");
                                csvCol.Add("black");
                                break;
                            case 223: // 色調変更（カラーは変換不可）
                                csvCol.Add("FadeOut");
                                csvCol.Add("#00000068");
                                break;
                            case 224: // 画面のフラッシュ
                            break;
                            case 225: // 画面のシェイク
                                csvCol.Add("Shake");
                                csvCol.Add("Camera");
                                break;
                            case 230: // ウェイト
                                waitCommand = true;
                                csvCol.Add("Wait");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add((int.Parse(l.parameters[0]) / 60f).ToString());
                                break;
                            case 231: // ピクチャの表示
                                csvCol.Add("Bg");
                                csvCol.Add(l.parameters[1]);
                                csvCol.Add("");
                                csvCol.Add("Picture" + l.parameters[0]);
                                break;
                            case 235: // ピクチャの消去
                                csvCol.Add("LayerOff");
                                csvCol.Add("Picture" + l.parameters[0]);
                                break;
                            case 241: // BGM再生 (ファイル指定・音量不可)
                                if (l.soundDate.name != "")
                                {
                                    csvCol.Add("PlayBgm");
                                    csvCol.Add(l.soundDate.name);
                                    csvCol.Add(l.soundDate.volume.ToString());
                                    csvCol.Add(l.soundDate.pitch.ToString());
                                } else
                                {
                                    csvCol.Add("StopBgm2");
                                }
                                break;
                            case 242: // BGMフェードアウト
                                csvCol.Add("StopBgm2");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("2");
                                break;
                            case 245: // BGS再生(ファイル指定・音量不可)
                                if (l.soundDate.name != "")
                                {
                                    csvCol.Add("PlayBgs");
                                    csvCol.Add(l.soundDate.name);
                                    csvCol.Add(l.soundDate.volume.ToString());
                                    csvCol.Add(l.soundDate.pitch.ToString());
                                } else
                                {
                                    csvCol.Add("StopBgs");
                                }
                                break;
                            case 246: // BGSフェードアウト
                                csvCol.Add("StopBgs");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("2");
                                break;
                            case 250: // Se再生(ファイル指定・音量不可)
                                csvCol.Add("PlaySe");
                                csvCol.Add(l.soundDate.name);
                                csvCol.Add(l.soundDate.volume.ToString());
                                csvCol.Add(l.soundDate.pitch.ToString());
                                break;
                            case 320: // アクター名変更（変換不可）
                                break;
                        }
                        codeIdx++;
                        if (csvCol.Count == 0)
                        {
                            continue;
                        }
                        // ","を挿入
                        if (csvCol.Count < 11)
                        {
                            for (int i = csvCol.Count;i < 11;i++)
                            {   
                                csvCol.Add("");
                            }
                        }
                        var csvText = "";
                        csvColList.Add(csvCol);
                        foreach (var csvC in csvCol)
                        {
                            csvText += csvC + ",";
                        }
                        if (csvText != "")
                        {
                            // waitの前にCharacterOffを挿入
                            if (waitCommand)
                            {
                                var characterOffText = "CharacterOff,";
                                characterOffText += ",,,,,0,,,,,";
                                csvStrings.Add(characterOffText);
                            }
                            csvStrings.Add(csvText);  
                            // waitの前にCharacterOffを挿入
                            if (fadeOutCommand)
                            {
                                var characterOffText = "CharacterOff,";
                                characterOffText += ",,,,,0,,,,,";
                                csvStrings.Add(characterOffText);
                            }
                        }
                        if (selection2.Count > 0)
                        {
                            // ","を挿入
                            if (selection2.Count < 11)
                            {
                                for (int i = selection2.Count;i < 11;i++)
                                {   
                                    selection2.Add("");
                                }
                            }
                            csvColList.Add(selection2);
                            var selection2Text = "";
                            foreach (var selection2C in selection2)
                            {
                                selection2Text += selection2C + ",";
                            }
                            csvStrings.Add(selection2Text); 
                            selection2.Clear();
                        }
                        if (paramFlag > -1)
                        {
                            var paramText = "Param,";
                            paramText += "SelectionParam_"+paramFlag + "=true," + ",,,,,,,,,,";
                            csvStrings.Add(paramText);
                            paramFlag = -1;
                        }
                    }

                    if (csvStrings.Count > 0)
                    {
                        csvStrings.Add("EndScenario,,,,,,,,,,,");
                    }
                    
                    using(StreamWriter sw = new StreamWriter(ExportPath + d.id + "_output.csv", false))
                    {
                        foreach (var csvString in csvStrings)
                        {
                            sw.WriteLine(csvString);
                        }
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
        }
	}
}