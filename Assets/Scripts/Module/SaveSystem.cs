using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
	public class SaveSystem : MonoBehaviour
	{
		private static string _gameKey = "norm_m";

#if !UNITY_WEBGL
		private static FileStream TempFileStream = null;
#endif
		private static readonly string debugFilePath = Application.persistentDataPath;
		private static string SaveFilePath(string saveKey,int fileId = 0)
		{
			return debugFilePath + "/" + saveKey + fileId.ToString() + ".dat";
		}
		
		private static readonly string _playerDataKey = _gameKey + "PlayerData";
		private static readonly string _playerStageDataKey = _gameKey + "PlayerStageData";
		private static string PlayerStageDataKey(int fileId)
		{
			return _playerStageDataKey + fileId.ToString();
		}
		private static readonly string _optionDataKey = _gameKey + "OptionData";
		private static readonly string _replayDataKey = _gameKey + "ReplayData_";
		private static string ReplayDataKey(string stageKey)
		{
			return _replayDataKey + stageKey;
		}

		private static void SaveFile<T>(string key,T data)
		{
			var TempBinaryFormatter = new BinaryFormatter();
			var memoryStream = new MemoryStream();
			TempBinaryFormatter.Serialize (memoryStream,data);
			var saveData = Convert.ToBase64String (memoryStream.GetBuffer());
			ES3.Save(key,saveData,key);
		}

		private static T LoadFile<T>(string key,Action<T> successAction)
		{
			try
			{
				var data = ES3.Load<string>(key,key);
				var bytes = Convert.FromBase64String(data);
				var	TempBinaryFormatter = new BinaryFormatter();
				var memoryStream = new MemoryStream(bytes);
				var saveData = (T)TempBinaryFormatter.Deserialize(memoryStream);
				successAction(saveData);
				return saveData;
			} catch(Exception e)
			{
				Debug.LogException(e);
			} finally 
			{
			}
			return default;
		}
		private static async UniTask<T> LoadFileAsync<T>(string key)
		{
			try
			{
				var data = ES3.Load<string>(key,key);
				var bytes = Convert.FromBase64String(data);
				var	TempBinaryFormatter = new BinaryFormatter();
				var memoryStream = new MemoryStream(bytes);
				var saveData = (T)TempBinaryFormatter.Deserialize(memoryStream);
				await UniTask.WaitUntil(() => saveData != null);
				return saveData;
			} catch(Exception e)
			{
				Debug.LogException(e);
				return default;
			} finally 
			{
			}
		}

		public static void SavePlayerInfo(SaveInfo userSaveInfo = null)
		{
			//	保存情報
			if( userSaveInfo == null )
			{
				userSaveInfo = new SaveInfo();
			}
			SaveFile(_playerDataKey,userSaveInfo);
		}

			
		public static async UniTask<bool> LoadPlayerInfo()
		{
			var playerInfo = await LoadFileAsync<SaveInfo>(_playerDataKey);
			if (playerInfo != null)
			{
				GameSystem.CurrentData = playerInfo;
			}
			return playerInfo != null;
		}

		private static bool ExistsLoadFile(string key)
		{
			return ES3.FileExists(key);
		}

		public static bool ExistsLoadPlayerFile()
		{
			return ExistsLoadFile(_playerDataKey);
		}

		public static void SaveStageInfo(SaveGameInfo userSaveInfo = null,int fileId = 0)
		{
			SaveFile(PlayerStageDataKey(fileId),userSaveInfo);
		}

		public static async UniTask<bool> LoadStageInfo(int fileId = 0)
		{
			var gameInfo = await LoadFileAsync<SaveGameInfo>(PlayerStageDataKey(fileId));
			if (gameInfo != null)
			{
				GameSystem.GameInfo = gameInfo;
			}
			return gameInfo != null;
		}

		public static bool ExistsStageFile(int fileId = 0)
		{
			return ExistsLoadFile(PlayerStageDataKey(fileId));
		}

		public static void SaveOptionStart(SaveOptionInfo userSaveInfo)
		{
			SaveFile(_optionDataKey,userSaveInfo);
		}

		public static async void LoadOptionStart()
		{
			GameSystem.OptionData = await LoadFileAsync<SaveOptionInfo>(_optionDataKey);
		}

		public static bool ExistsOptionFile()
		{
			return ExistsLoadFile(_optionDataKey);
		}

		public static void DeleteAllData(int fileId = 0)
		{
			DeletePlayerData();
			DeleteStageData(fileId);
			DeleteOptionData();
	}

		public static void DeletePlayerData()
		{
			ES3.DeleteFile(_playerDataKey);
		}

		public static void DeleteStageData(int fileId = 0)
		{
			ES3.DeleteFile(PlayerStageDataKey(fileId));
		}

		public static void DeleteOptionData()
		{
			ES3.DeleteFile(_optionDataKey);
		}
	}
}