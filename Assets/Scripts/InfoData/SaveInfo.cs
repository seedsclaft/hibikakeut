using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class SaveInfo
    {
        private PlayerInfo _playerInfo = null;
        public PlayerInfo PlayerInfo => _playerInfo;

        private List<SaveFileInfo> _saveFileInfos = new();
        public List<SaveFileInfo> SaveFileInfos => _saveFileInfos;
        public SaveFileInfo AutoSave()
        {
            var autoSave = _saveFileInfos.Find(a => a.SaveNo == 0);
            if (autoSave == null)
            {
                autoSave = new SaveFileInfo
                {
                    SaveNo = 0
                };
            }
            return autoSave;
        }

        public void PushSaveFile(SaveFileInfo saveFileInfo)
        {
            var findIndex = _saveFileInfos.FindIndex(a => a.SaveNo == saveFileInfo.SaveNo);
            if (findIndex > -1)
            {
                _saveFileInfos.RemoveAt(findIndex);
                _saveFileInfos.Insert(findIndex, saveFileInfo);
            }
            else
            {
                _saveFileInfos.Add(saveFileInfo);
            }
            _saveFileInfos.Sort((a, b) => a.SaveNo - b.SaveNo > 0 ? 1 : -1);
            LastSaveIndex.SetValue(saveFileInfo.SaveNo);
            //UpdateSaveLastSaveIndex();
        }

        public void DeleteSaveFile(int saveFileNo)
        {
            var file = _saveFileInfos.Find(a => a.SaveNo == saveFileNo);
            if (file != null)
            {
                _saveFileInfos.Remove(file);
            }
        }

        public ParameterInt LastSaveIndex = new();
        private void UpdateSaveLastSaveIndex()
        {
            // 1番最新のデータ
            var idx = 0;
            var saveIndex = 0;
            long saveTime = 0;
            foreach (var saveFileInfo in _saveFileInfos)
            {
                if (saveFileInfo.SaveTimeLong > saveTime)
                {
                    saveTime = saveFileInfo.SaveTimeLong;
                    saveIndex = idx;
                }
                idx++;
            }
            LastSaveIndex.SetValue(saveIndex);
        }

        public SaveInfo()
        {
            _playerInfo = new PlayerInfo();
            _saveFileInfos.Add(new SaveFileInfo());
            LastSaveIndex.SetValue(0);
        }

        public void SetPlayerName(string name)
        {
            _playerInfo.PlayerName.SetValue(name);
            _playerInfo.SetUserId();
        }
    }
}