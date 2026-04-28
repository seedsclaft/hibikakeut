using System.Threading.Tasks;
using UnityEngine;
using Ryneus;
using UtageExtensions;
using System.Linq;

namespace Utage
{
    public class AdvCommandActiveDungeon : AdvCommand
    {
        private bool _active = false;
        public AdvCommandActiveDungeon(StringGridRow row)
            : base(row)
        {
            _active = ParseCellOptional<float>(AdvColumnName.Arg1, 0) == 1;
        }

        public override void DoCommand(AdvEngine engine)
        {
            if (_active)
            {
                GameSystem.Instance.CurrentScene.CallSystemCommand(Ryneus.Base.CommandType.ShowMap, true, true);
            } else
            {
                GameSystem.Instance.CurrentScene.CallSystemCommand(Ryneus.Base.CommandType.HideMap, true, true);
            }
        }
    }
}
