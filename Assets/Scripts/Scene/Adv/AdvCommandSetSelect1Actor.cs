using Ryneus;

namespace Utage
{

    public class AdvCommandSetSelect1Actor : AdvCommand
    {

        public AdvCommandSetSelect1Actor(StringGridRow row)
            : base(row)
        {
        }

        public override void DoCommand(AdvEngine engine)
        {
            if (GameSystem.GameInfo == null)
            {
                return;
            }
            //if (Ryneus.GameSystem.CurrentStageData.CurrentStage == null) return;
            if (GameSystem.GameInfo.PartyInfo.ActorInfos.Count == 0)
            {
                return;
            }
            int actorId = GameSystem.GameInfo.PartyInfo.ActorInfos[0].ActorId.Value;
            var actorData = DataSystem.FindActor(actorId);
            if (actorData != null)
            {
                engine.Param.SetParameterString("Select1", actorData.GetName());
            }
        }
    }
}
