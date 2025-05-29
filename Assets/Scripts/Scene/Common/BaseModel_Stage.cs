using System.Collections.Generic;

namespace Ryneus
{
    public partial class BaseModel
    {
        public void MakeStageInfo(int stageId,bool newGame,int clearCount = 0)
        {
            var stageInfo = new StageInfo(stageId);

            // ダンジョンターン数を設定
            PartyInfo.TurnCount.SetValue(50);
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
