using Ryneus;

namespace Utage
{
    public class AdvCommandPlaySe : AdvCommand
    {
        private string _fileName = "";
        private int? _volume = 80;
        private int? _pitch = 100;
        public AdvCommandPlaySe(StringGridRow row)
            : base(row)
        {
            _fileName = ParseCell<string>(AdvColumnName.Arg1);
            _volume = ParseCellOptional(AdvColumnName.Arg2, 80);
            _pitch = ParseCellOptional(AdvColumnName.Arg3, 100);
        }

        //コマンド実行
        public override async void DoCommand(AdvEngine engine)
        {
            var se = await ResourceSystem.LoadSeAsset(_fileName);
            Ryneus.SoundManager.Instance.PlaySe(se, (int)_volume * 0.01f, (int)_pitch * 0.01f);
        }
    }
}
