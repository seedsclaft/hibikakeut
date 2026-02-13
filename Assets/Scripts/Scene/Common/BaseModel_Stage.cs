using System.Collections.Generic;

namespace Ryneus
{
    public partial class BaseModel
    {
        public List<StageInfo> StageInfos()
        {
            var claerList = new List<StageInfo>();
            var list = new List<StageInfo>();
            foreach (var stageData in DataSystem.Stages)
            {
                if (!stageData.Selectable)
                {
                    continue;
                }
                if (stageData.DisplayRank > PartyInfo.MissionRank.Value)
                {
                    continue;
                }
                if (stageData.Chapter > PartyInfo.Chapter.Value)
                {
                    continue;
                }
                if (stageData.OnlyOnce && PartyInfo.GetDungeonTraverse(stageData.Id) != null)
                {
                    continue;
                }
                var cleared = PartyInfo.IsClaeredStage(stageData.StageNo);
                var alarted = PartyInfo.IsAlartedStage(stageData.StageNo);
                var stageInfo = new StageInfo(stageData.Id, cleared, alarted);
                if (cleared)
                {
                    claerList.Add(stageInfo);
                } else
                {
                    list.Add(stageInfo);
                }
            }
            claerList.Sort((a, b) => a.Master.Category - b.Master.Category > 1 ? 1 : -1);
            list.Sort((a, b) => a.Master.Category - b.Master.Category > 1 ? 1 : -1);
            list.AddRange(claerList);
            return list;
        }

        public void MakeStageInfo(int stageId, bool startStage, int clearCount = 0)
        {
            SaveDungeonPlayerData();
            var cleared = PartyInfo.IsClaeredStage(stageId);
            var stageInfo = new StageInfo(stageId, cleared);

            // ダンジョンターン数を設定
            if (startStage)
            {
                // 歩数カウントをリセット
                CurrentDeckInfo.TurnCount.SetValue(0);
                CurrentDeckInfo.RecoveryCount.SetValue(DataSystem.System.RecoveryCount + PartyInfo.RecoveryCount.Value);
            }
            CurrentGameInfo.SetStageInfo(stageInfo);
            CurrentDeckInfo.StageNo.SetValue(stageId);
        }
    
        public void ReturnDungeon()
        {
            // 全回復
            foreach (var actorInfo in PartyInfo.CurrentDeckActorInfos())
            {
                actorInfo.ChangeHp(actorInfo.MaxHp);
            }
            PartyNextPeriod();
            CurrentDeckInfo.DungeonBgmTimeStamp.SetValue(0);
            SaveDungeonPlayerData();
        }

        public bool IsActiveDungeon()
        {
            if (CurrentStage != null)
            {
                // stageLvが0はダンジョン以外の扱い
                return CurrentStage.Master.StageLv > 0;
            }
            return false;
        }

        public SoundData DungeonBgmData()
        {
            if (CurrentStage != null && PartyInfo != null)
            {
                int bgmId;
                bgmId = CurrentStage.Master.BGMId;
                if (CurrentStage.Cleared.Value)
                {
                    bgmId = 1510;
                }
                return DataSystem.GetBGM(bgmId);
            }
            return null;
        }

        public void PartyNextPeriod(bool force = false)
        {
            if (!IsActiveDungeon() && !force)
            {
                return;
            }
            // NotSeekPeriod効果判定
            var notSeekPeriod = PartyInfo.AritifactSkills().Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.NotSeekPeriod) != null);
            if (notSeekPeriod == null)
            {
                PartyInfo.Period.GainValue(1);
            }
            else
            {
                var artifact = PartyInfo.GetOwnItemInfos(ItemType.Artifact).Find(a => a.Master.Param1 == notSeekPeriod.Id.Value);
                PartyInfo.ConsuneItemNum(artifact.Master.Id, 1);
            }
            PartyInfo.ClearTradeItemInfos();
            if (PartyInfo.Chapter.Value >= 2)
            {
                PartyInfo.EvaluationValue.GainValue(PartyInfo.EvaluationAddictValue(), 0);
            }
            PartyInfo.ClearSkillUseCount();
        }

        public string DungeonPrefabName()
        {
            if (CurrentStage != null)
            {
                return CurrentStage.Master.Id.ToString("D4");
            }
            return "";
        }

        public string DungeonSkyboxName()
        {
            if (CurrentStage != null)
            {
                return CurrentStage.Master.SkyboxName;
            }
            return "";
        }
    }
}
