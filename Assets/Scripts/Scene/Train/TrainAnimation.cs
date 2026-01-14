using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Ryneus
{
    public class TrainAnimation : BaseAnimation, IBaseAnimation
    {
        [SerializeField] private CanvasGroup detailCanvas = null;
        [SerializeField] private CanvasGroup characterListCanvas = null;
        public void OpenAnimation(Transform transform, System.Action endEvent, float duration = 0.1f)
        {
        }

        public void OpenCharacterListAnimation(Transform transform, System.Action endEvent, float duration = 0.08f)
        {
            characterListCanvas.alpha = 0;
            DOTween.Sequence()
                //.Append(transform.DOScaleY(1,duration))
                .Join(characterListCanvas.DOFade(1, duration)
                .OnComplete(() =>
                {
                    endEvent?.Invoke();
                })
                .SetEase(Ease.InOutQuad));
        }
    }
}
