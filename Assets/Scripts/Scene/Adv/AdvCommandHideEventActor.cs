using System.Threading.Tasks;
using UnityEngine;
using Ryneus;
using UtageExtensions;
using System.Linq;

namespace Utage
{
    public class AdvCommandHideEventActor : AdvCommand
    {
        private string _layerName = "";
        private string _fileName = "";
        public AdvCommandHideEventActor(StringGridRow row)
            : base(row)
        {
            _layerName = ParseCell<string>(AdvColumnName.Arg1);
            _fileName = ParseCell<string>(AdvColumnName.Arg2);
        }

        public override void DoCommand(AdvEngine engine)
        {
            var layer = engine.GraphicManager.FindLayer(_layerName);
            var character = layer.gameObject.GetComponentsInChildren<CharacterAnimationImages>().ToList();
            var find = character.Find(a => a.name == _fileName);
            if (find != null)
            {
                GameObject.DestroyImmediate(find.gameObject);
            }
        }
    }
}
