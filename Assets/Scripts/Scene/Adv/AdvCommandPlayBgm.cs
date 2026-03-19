
using Ryneus;

namespace Utage
{
    // カスタムコマンド
    public class AdvCommandPlayBgm : AdvCommand
    {
        private string _fileName = "";
        private int? _volume = 80;
        //private int? pitch = 100;
        //private bool? loop = true;
        public AdvCommandPlayBgm(StringGridRow row)
            : base(row)
        {
            _fileName = ParseCell<string>(AdvColumnName.Arg1);
            _volume = ParseCell<int?>(AdvColumnName.Arg2);
            //pitch = ParseCell<int?>(AdvColumnName.Arg3);
            //loop = ParseCell<bool>(AdvColumnName.Arg2);
        }

        //コマンド実行
        public override async void DoCommand(AdvEngine engine)
        {
            var bgmData = DataSystem.GetBGMByKey(_fileName);
            if (bgmData != null)
            {
                var bgm = await ResourceSystem.LoadBGMAsset(bgmData.Key);
                Ryneus.SoundManager.Instance.PlayBgm(bgm, bgmData.Volume, bgmData.Loop);
            }
        }
    }
}
