
using Ryneus;

namespace Utage
{
    public class AdvCommandPlayBgs : AdvCommand
    {
        private string _bgsKey = "";
        public AdvCommandPlayBgs(StringGridRow row)
            : base(row)
        {
            _bgsKey = ParseCell<string>(AdvColumnName.Arg1);
        }

        //コマンド実行
        public override async void DoCommand(AdvEngine engine)
        {
            var bgs = await ResourceSystem.LoadBGSAsset(_bgsKey);
            Ryneus.SoundManager.Instance.PlayBgs(bgs, 1.0f, true);
        }
    }
}
