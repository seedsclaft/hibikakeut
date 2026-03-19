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
        private int _posX = 0;
        private int _posY = 0;
        public AdvCommandShowEventActor(StringGridRow row)
            : base(row)
        {
            _layerName = ParseCell<string>(AdvColumnName.Arg1);
            _fileName = ParseCell<string>(AdvColumnName.Arg2);
            _posX = ParseCell<int>(AdvColumnName.Arg3);
            _posY = ParseCell<int>(AdvColumnName.Arg4);
        }

        public override async void DoCommand(AdvEngine engine)
        {
            AdvGraphicLayer layer = engine.GraphicManager.FindLayer(_layerName);
            var prefabObject = await ResourceSystem.LoadAsset<GameObject>("FieldBattler/Actors/FieldBattler_0001");
            var prefab = GameObject.Instantiate(prefabObject);
            prefab.transform.SetParent(layer.gameObject.transform, false);
            prefab.transform.SetAsLastSibling();
            prefab.ChangeLayerDeep(11);
        }
    }
}
