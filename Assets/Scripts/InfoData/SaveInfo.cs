using System;
using System.Collections.Generic;

namespace Ryneus
{
	[Serializable]
	public class SaveInfo
	{
		private PlayerInfo _playerInfo = null;
		public PlayerInfo PlayerInfo => _playerInfo;

		private	List<SaveFileInfo> _saveFileInfos = new();
		public List<SaveFileInfo> SaveFileInfos => _saveFileInfos;
		public void PushSaveFile(SaveFileInfo saveFileInfo)
		{
			var findIndex = _saveFileInfos.FindIndex(a => a.SaveNo == saveFileInfo.SaveNo);
			if (findIndex > -1)
			{
				_saveFileInfos.RemoveAt(findIndex);
				_saveFileInfos.Insert(findIndex,saveFileInfo);
				LastSaveIndex.SetValue(findIndex);
			} else
			{
				_saveFileInfos.Add(saveFileInfo);
			}
			_saveFileInfos.Sort((a,b) => a.SaveNo - b.SaveNo > 0 ? 1 : -1);
		}

        public ParameterInt LastSaveIndex = new();

		public SaveInfo()
		{
			_playerInfo = new PlayerInfo();
			_saveFileInfos.Add(new SaveFileInfo());
			LastSaveIndex.SetValue(1);
		}

		public void SetPlayerName(string name)
		{
			_playerInfo.PlayerName.SetValue(name);
			_playerInfo.SetUserId();
		}
	}
}