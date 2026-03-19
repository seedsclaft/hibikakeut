using UnityEngine;
using Ryneus;
using System.Collections.Generic;

namespace Utage
{
    public class AdvCommandBalloon : AdvCommand
    {
        private string _layerName = "";
        private int _type = 0;
        private List<AnimationBalloon> _animationBalloons = new();
        private bool _isInitialized = false;
        public AdvCommandBalloon(StringGridRow row)
            : base(row)
        {
            _layerName = ParseCell<string>(AdvColumnName.Arg1);
            _type = ParseCell<int>(AdvColumnName.Arg2);
        }

        //コマンド実行
        public override void DoCommand(AdvEngine engine)
        {
            AdvGraphicLayer layer = engine.GraphicManager.FindLayer(_layerName);
            AdvGraphicLayer balloonLayer = engine.GraphicManager.FindLayer("Balloon");
            if (layer == null)
            {
                return;
            }
            var pixelsToUnits = engine.GraphicManager.PixelsToUnits;
            var balloon = CreateBalloon(layer,balloonLayer,pixelsToUnits);
            balloon.Play(layer,(AnimationBalloonType)_type);
            _animationBalloons.Add(balloon);
            if (!_isInitialized)
            {
                engine.MessageWindowManager.OnTextChange.AddListener(OnBeginCommand);
                //engine.GraphicManager.CharacterManager.SetBalloonEvent(BalloonEndEvent);
                _isInitialized = true;
            }
        }

        private AnimationBalloon CreateBalloon(AdvGraphicLayer layer,AdvGraphicLayer balloonLayer,float pixelsToUnits)
        {
            var prefabObject = ResourceSystem.LoadResource<GameObject>(ResourceSystem.PrefabPath + "Common/Balloon");
            var prefab = GameObject.Instantiate(prefabObject);
            prefab.transform.SetParent(balloonLayer.gameObject.transform,false);
            prefab.transform.SetAsLastSibling();
            prefab.transform.GetComponent<Transform>().localPosition = layer.transform.localPosition * pixelsToUnits;
            return prefab.GetComponent<AnimationBalloon>();
        }

        private void OnBeginCommand(AdvMessageWindowManager messageWindowManager = null)
        {
            BalloonEndEvent();
        }

        public void BalloonEndEvent()
        {
            var deleteList = new List<AnimationBalloon>();
            foreach (var animationBalloons in _animationBalloons)
            {
                var deleteFlag = animationBalloons.StopAnimation();
                if (deleteFlag)
                {
                    deleteList.Add(animationBalloons);
                }
            }
            for(int i = _animationBalloons.Count-1; i >= 0; i--)
            {
                if (deleteList.Contains(_animationBalloons[i]))
                {
                    _animationBalloons.Remove(_animationBalloons[i]);
                }
            }
        }
    }
}
