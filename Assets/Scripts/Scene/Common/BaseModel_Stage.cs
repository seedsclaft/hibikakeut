using System.Collections.Generic;

namespace Ryneus
{
    public partial class BaseModel
    {
        public void MakeStageInfo(int stageId,bool startStage,int clearCount = 0)
        {
            SaveDungeonPlayerData();
            var stageInfo = new StageInfo(stageId);

            // ダンジョンターン数を設定
            if (startStage)
            {
                PartyInfo.TurnCount.SetValue(50);
            }
            CurrentGameInfo.SetStageInfo(stageInfo);
            PartyInfo.StageId.SetValue(stageId);
        }

        public void UpdateUnitStatus()
        {
            foreach (var actorInfo in PartyInfo.ActorInfos)
            {
                actorInfo.ChangeHp(actorInfo.MaxHp);
            }
        }
    }
}
