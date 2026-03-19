namespace Utage
{
    public class AdvCommandStopBgm2 : AdvCommand
    {
        public AdvCommandStopBgm2(StringGridRow row)
            : base(row)
        {
        }

        public override void DoCommand(AdvEngine engine)
        {
            Ryneus.SoundManager.Instance.StopBgm();
        }
    }
}
