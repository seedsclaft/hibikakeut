using System.Threading.Tasks;
using UnityEngine;
using Ryneus;
using UtageExtensions;

namespace Utage
{
    public class AdvCommandShowEventActor : AdvCommand
    {
        private string _layerName = "";
        private string _fileName = "";
        private float _posX = 0;
        private float _posY = 0;
        private bool _flip = false;
        public AdvCommandShowEventActor(StringGridRow row)
            : base(row)
        {
            _layerName = ParseCell<string>(AdvColumnName.Arg1);
            _fileName = ParseCell<string>(AdvColumnName.Arg2);
            _posX = ParseCell<float>(AdvColumnName.Arg3);
            _posY = ParseCell<float>(AdvColumnName.Arg4);
            _flip = ParseCellOptional<int>(AdvColumnName.Arg5, 1) == -1;
        }

        public override async void DoCommand(AdvEngine engine)
        {
            var layer = engine.GraphicManager.FindLayer(_layerName);
            var prefabObject = await ResourceSystem.LoadAsset<GameObject>("Texture/Character/" + _fileName);
            var prefab = GameObject.Instantiate(prefabObject);
            prefab.transform.SetParent(layer.gameObject.transform, false);
            prefab.transform.SetAsLastSibling();
            prefab.ChangeLayerDeep(11);
            prefab.transform.localPosition = new Vector3(_posX, _posY, 1);
            prefab.name = _fileName;
            if (_flip)
            {
                var character = prefab.gameObject.GetComponent<CharacterAnimationImages>();
                if (character != null)
                {
                    character.Flip();
                }
            }
        }
    }
}
