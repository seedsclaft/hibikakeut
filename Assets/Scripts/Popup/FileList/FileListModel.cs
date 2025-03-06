using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

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
                return CurrentData.SaveFileInfos.FindAll(a => a.SaveNo > 0);
            }
            var saveFileInfos = new List<SaveFileInfo>();
            for (int i = 1;i < 21;i++)
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
                return CurrentData.LastSaveIndex.Value;
            }
            return 0;
        }

        public bool DecideFile(SaveFileInfo saveFileInfo)
        {
            if (_isLoad)
            {
                if (saveFileInfo != null)
                {
                    // ロード
                    LoadFile(saveFileInfo);
                    return true;
                }
            } else
            {
                // セーブ
                SaveFile(saveFileInfo);
                return true;
            }
            return false;
        }

        private void SaveFile(SaveFileInfo saveFileInfo)
        {
            saveFileInfo.StageNo = CurrentStage.StageId.Value;
            saveFileInfo.SaveTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            saveFileInfo.PlayTime = (int)TempInfo.PlayingTime;
            if (CurrentGameInfo.PartyInfo.ActorInfos != null && CurrentGameInfo.PartyInfo.ActorInfos.Count > 0)
            {
                saveFileInfo.ActorId = CurrentGameInfo.PartyInfo.ActorInfos[0].ActorId.Value;
            }
            CurrentData.PushSaveFile(saveFileInfo);
            SavePlayerData();
            SaveSystem.SaveStageInfo(GameSystem.GameInfo,saveFileInfo.SaveNo);
        }

        private void LoadFile(SaveFileInfo saveFileInfo)
        {
            SaveSystem.LoadStageInfo(saveFileInfo.SaveNo);
            TempInfo.SetPlayingTime(saveFileInfo.PlayTime);
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