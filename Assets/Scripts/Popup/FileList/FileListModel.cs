using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ryneus
{
    public class FileListModel : BaseModel
    {
        private bool _isLoad = false;
        public bool IsLoad => _isLoad;
        public FileListModel()
        {
            FileListSceneInfo SceneParam = (FileListSceneInfo)GameSystem.SceneStackManager.LastTemplate;
            _isLoad = SceneParam.IsLoad;
        }

        public List<SaveFileInfo> SaveFileInfos()
        {
            if (_isLoad)
            {
                return CurrentData.SaveFileInfos.FindAll(a => a.SaveNo >= 0);
            }
            var saveFileInfos = new List<SaveFileInfo>();
            for (int i = 1; i < 21; i++)
            {
                var find = CurrentData.SaveFileInfos.Find(a => a.SaveNo == i);
                if (find != null)
                {
                    saveFileInfos.Add(find);
                } else
                {
                    var tempInfo = new SaveFileInfo
                    {
                        SaveNo = i
                    };
                    saveFileInfos.Add(tempInfo);
                }
            }
            return saveFileInfos;
        }

        public int SaveFileLastIndex()
        {
            if (CurrentData.LastSaveIndex != null)
            {
                var find = CurrentData.SaveFileInfos.FindIndex(a => a.SaveNo == CurrentData.LastSaveIndex.Value);
                if (find > -1)
                {
                    if (!_isLoad)
                    {
                        find -= 1;
                    }
                    return find;
                }
            }
            return 0;
        }

        public async Task<bool> DecideFile(SaveFileInfo saveFileInfo)
        {
            if (_isLoad)
            {
                if (saveFileInfo != null)
                {
                    // ロード
                    await LoadFile(saveFileInfo);
                    CurrentData.LastSaveIndex.SetValue(saveFileInfo.SaveNo);
                    return true;
                }
            }
            else
            {
                // セーブ
                SaveFile(saveFileInfo);
                return true;
            }
            return false;
        }

        private async Task LoadFile(SaveFileInfo saveFileInfo)
        {
            _ = await SaveSystem.LoadStageInfo(saveFileInfo.SaveNo);
            PartyInfoChecker.Instance.UpdateInfo();
            TempInfo.PlayingTime.SetValue((int)saveFileInfo.PlayTime);
            TempInfo.LastStartTime.SetValue((int)TempInfo.LocalEpochTime());
        }

        public void DeleteFile(SaveFileInfo saveFileInfo)
        {
            CurrentData.DeleteSaveFile(saveFileInfo.SaveNo);
            SaveSystem.DeleteStageInfo(saveFileInfo.SaveNo);
            SavePlayerData();
        }
    }

    public class FileListSceneInfo
    {
        public FileListSceneInfo()
        {
        }

        public bool IsLoad = false;
    }
}