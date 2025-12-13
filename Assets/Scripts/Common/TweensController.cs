using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace Ryneus
{
    public class TweensController : MonoBehaviour
    {
        private List<DOTweenAnimation> _tweens;
        [SerializeField] private List<GameObject> tweenObjects;

        public void PlayTween()
        {
            tweenObjects.ForEach(a => a.SetActive(true));
            _tweens.ForEach(a => a.tween.Play());
        }

        public void PlayTweenOnly()
        {
            _tweens.ForEach(a => a.tween.Play());
        }

        public void ConvertDurationTiming(float timing)
        {
            _tweens = new List<DOTweenAnimation>();
            tweenObjects.ForEach(a => _tweens.AddRange(a.GetComponents<DOTweenAnimation>()));
            _tweens.ForEach(a => a.duration *= timing);
            _tweens.ForEach(a => a.delay *= timing);
        }
    }
}
