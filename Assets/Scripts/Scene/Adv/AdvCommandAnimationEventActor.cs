using UnityEngine;
using Ryneus;
using System.Linq;

namespace Utage
{
    public class AdvCommandAnimationEventActor : AdvCommand
    {
        private string _layerName = "";
        private string _fileName = "";
        private string _animation = "";
        public AdvCommandAnimationEventActor(StringGridRow row)
            : base(row)
        {
            _layerName = ParseCell<string>(AdvColumnName.Arg1);
            _fileName = ParseCell<string>(AdvColumnName.Arg2);
            _animation = ParseCell<string>(AdvColumnName.Arg3);
        }

        public override void DoCommand(AdvEngine engine)
        {
            var layer = engine.GraphicManager.FindLayer(_layerName);
            var character = layer.GetComponentsInChildren<CharacterAnimationImages>().ToList();
            var find = character.Find(a => a.name == _fileName);
            if (find != null)
            {
                var state = (Ryneus.AnimationState)System.Enum.Parse(typeof(Ryneus.AnimationState), _animation);
                find.SetAnimationState(state);
            }
        }
    }
}
