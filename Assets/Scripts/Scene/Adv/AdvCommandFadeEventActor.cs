using System.Threading.Tasks;
using UnityEngine;
using Ryneus;
using UtageExtensions;
using System.Linq;

namespace Utage
{
    public class AdvCommandFadeEventActor : AdvCommand
    {
        private string _layerName = "";
        private string _fileName = "";
        private float _value = 0;
        private float _fadeTime = 0;
        public AdvCommandFadeEventActor(StringGridRow row)
            : base(row)
        {
            _layerName = ParseCell<string>(AdvColumnName.Arg1);
            _fileName = ParseCell<string>(AdvColumnName.Arg2);
            _value = ParseCell<float>(AdvColumnName.Arg3);
            _fadeTime = ParseCellOptional<float>(AdvColumnName.Arg6, 0);
        }

        public override void DoCommand(AdvEngine engine)
        {
            var layer = engine.GraphicManager.FindLayer(_layerName);
            var character = layer.gameObject.GetComponentsInChildren<CharacterAnimationImages>().ToList();
            var find = character.Find(a => a.name == _fileName);
            if (find != null)
            {
                find.Fade(_value, _fadeTime);
            }
        }
    }
}
