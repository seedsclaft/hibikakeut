namespace Utage
{
    public class AdvCommandStopBgs : AdvCommand
    {
        public AdvCommandStopBgs(StringGridRow row)
            : base(row)
        {
        }

        public override void DoCommand(AdvEngine engine)
        {
            Ryneus.SoundManager.Instance.StopBgs();
        }
    }
}
