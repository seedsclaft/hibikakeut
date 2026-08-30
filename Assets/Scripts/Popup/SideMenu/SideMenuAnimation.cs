using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Ryneus
{
    public class SideMenuAnimation : BaseAnimation, IBaseAnimation
    {
        public void OpenAnimation(Transform transform, System.Action endEvent, float duration = 0.1f)
        {
            Busy.SetValue(true);
            transform.DOLocalMoveX(240, duration);
            transform.DOScaleX(1, 0);
            transform.DOScaleY(1, 0);
            BaseCanvas.alpha = 0;
            DOTween.Sequence()
                .Append(transform.DOLocalMoveX(0, duration))
                .Join(transform.DOScaleX(1, duration))
                .Join(BaseCanvas.DOFade(1, duration)
                .OnComplete(() =>
                {
                    Busy.SetValue(false);
                    endEvent?.Invoke();
                })
                .SetEase(Ease.InOutQuad));
        }
    }
}
