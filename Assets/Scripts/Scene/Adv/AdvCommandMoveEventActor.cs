using Ryneus;
using System.Linq;

namespace Utage
{
    public class AdvCommandMoveEventActor : AdvCommand
    {
        private string _layerName = "";
        private string _fileName = "";
        private int _posX = 0;
        private int _posY = 0;
        private int _duration = 0;

        public AdvCommandMoveEventActor(StringGridRow row)
            : base(row)
        {
            _layerName = ParseCell<string>(AdvColumnName.Arg1);
            _fileName = ParseCell<string>(AdvColumnName.Arg2);
            _posX = ParseCell<int>(AdvColumnName.Arg3);
            _posY = ParseCell<int>(AdvColumnName.Arg4);
            _duration = ParseCell<int>(AdvColumnName.Arg5);
        }

        public override void DoCommand(AdvEngine engine)
        {
            var layer = engine.GraphicManager.FindLayer(_layerName);
            var character = layer.GetComponentsInChildren<CharacterAnimationImages>().ToList();
            var find = character.Find(a => a.name == _fileName);
            if (find != null)
            {
                find.Move(_posX, _posY, _duration);
            }
        }
    }
}
