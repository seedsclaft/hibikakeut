using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Ryneus
{
    public class OptionAnimation : BaseAnimation, IBaseAnimation
    {
        public void OpenAnimation(Transform transform, System.Action endEvent, float duration = 0.1f)
        {
            transform.DOScale(0, duration);
            BaseCanvas.alpha = 0;
            DOTween.Sequence()
                .Append(transform.DOScale(1, duration))
                .Join(BaseCanvas.DOFade(1, duration)
                .OnComplete(() =>

                {
                    endEvent?.Invoke();
                })
                .SetEase(Ease.InOutQuad));
        }
    }
}
