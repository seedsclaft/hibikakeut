using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class BaseModel
    {
        public void InitSaveInfo()
        {
            GameSystem.CurrentData = new SaveInfo();
        }

        public void InitSaveStageInfo()
        {
            var saveGameInfo = new SaveGameInfo();
            saveGameInfo.Initialize();
            GameSystem.GameInfo = saveGameInfo;
            PartyInfoChecker.Instance.UpdateInfo();
        }

        public void SaveAutoFile()
        {
            var saveFileInfo = CurrentData.AutoSave();
            SaveFile(saveFileInfo);
        }

        public void SaveFile(SaveFileInfo saveFileInfo)
        {
            saveFileInfo.StageNo = CurrentStage.StageId.Value;
            saveFileInfo.UpdateTimeData(TempInfo);
            if (CurrentGameInfo.PartyInfo.ActorInfos != null && CurrentGameInfo.PartyInfo.ActorInfos.Count > 0)
            {
                saveFileInfo.ActorId = CurrentGameInfo.PartyInfo.LeaderActorId.Value;
            }
            saveFileInfo.UpdatePartyData(PartyInfo);
            saveFileInfo.State = PartyInfo.ResumeScene == Scene.Dungeon ? DataSystem.GetReplaceText(31060, DataSystem.FindStage(CurrentStage.StageId.Value).Name) : DataSystem.GetText(31061);
            CurrentData.PushSaveFile(saveFileInfo);
            SavePlayerData();
            SavePlayerStageData(GameSystem.SceneStackManager.Current);
            SaveSystem.SaveStageInfo(GameSystem.GameInfo, saveFileInfo.SaveNo);
        }

        public void InitOptionInfo()
        {
            GameSystem.OptionData = new SaveOptionInfo();
        }

        public void UpdateOptionData()
        {
            GameSystem.OptionData.UpdateSoundParameter(
                SoundManager.Instance.BgmVolume,
                SoundManager.Instance.BGMMute,
                SoundManager.Instance.SeVolume,
                SoundManager.Instance.SeMute
            );
            SaveSystem.SaveOptionStart(GameSystem.OptionData);
        }

        public void SavePlayerData()
        {
            SaveSystem.SavePlayerInfo(GameSystem.CurrentData);
        }

        public void SavePlayerStageData(Scene resumeScene)
        {
            SaveDungeonPlayerData();
            TempInfo.ClearRankingInfo();
            PartyInfo.ResumeScene = resumeScene;
            SaveSystem.SaveStageInfo(GameSystem.GameInfo);
            SavePlayerData();
        }

        public string SavePopupTitle()
        {
            return DataSystem.GetText(19500);
        }

        public string FailedSavePopupTitle()
        {
            var baseText = DataSystem.GetText(11082);
            return baseText;
        }

        public bool NeedAdsSave()
        {
            var needAds = false;
#if UNITY_ANDROID
            needAds = (CurrentStage.SavedCount + 1) >= CurrentStage.Master.SaveLimit;
#endif
            return needAds;
        }

        public void GainSaveCount()
        {
        }

        public List<int> SaveAdsCommandTextIds()
        {
            return new List<int>() { 3053, 3051 };
        }

    }
}
