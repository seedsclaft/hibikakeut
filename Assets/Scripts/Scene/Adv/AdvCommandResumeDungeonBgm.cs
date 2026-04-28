
using Ryneus;

namespace Utage
{
    // カスタムコマンド
    public class AdvCommandResumeDungeonBgm : AdvCommand
    {
        public AdvCommandResumeDungeonBgm(StringGridRow row)
            : base(row)
        {
        }

        //コマンド実行
        public override async void DoCommand(AdvEngine engine)
        {
            GameSystem.Instance.CurrentScene.CallSystemCommand(Ryneus.Base.CommandType.ResumeDungeonBgm, true, true);
        }
    }
}
