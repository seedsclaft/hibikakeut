using System.Threading.Tasks;
using UnityEngine;
using Ryneus;
using UtageExtensions;
using System.Linq;
using UnityEngine.UI;

namespace Utage
{
    public class AdvCommandBgLoopAnimation : AdvCommand
    {
        private bool _play = true;
        private float _from = 0;
        private float _to = 0;
        private float _duration = 0;
        public AdvCommandBgLoopAnimation(StringGridRow row)
            : base(row)
        {
            _play = ParseCellOptional<int>(AdvColumnName.Arg3, 0) == 1;
            _from = ParseCellOptional<int>(AdvColumnName.Arg4, 0);
            _to = ParseCellOptional<int>(AdvColumnName.Arg5, 0);
            _duration = ParseCellOptional<int>(AdvColumnName.Arg6, 0);
        }

        public override void DoCommand(AdvEngine engine)
        {
            var bg = engine.GraphicManager.FindObject("BG");
            if (bg != null)
            {
                var image = bg.GetComponentInChildren<RawImage>();
                if (image != null)
                {
                    if (_play)
                    {
                        AnimationUtility.BgLoop(image, _from, _to, _duration);
                    } else
                    {
                        AnimationUtility.BgLoop(image, 0, 0, 0);
                    }
                }
            }
        }
    }
}
