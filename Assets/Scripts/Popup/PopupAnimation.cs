using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class PopupAnimation : BaseAnimation, IBaseAnimation
    {
        public void OpenAnimation(Transform transform, System.Action endEvent, float duration = 0.1f)
        {
            transform.DOScale(0, duration);
            BaseCanvas.alpha = 0;
            _ = DOTween.Sequence()
                .Append(transform.DOScale(1, duration))
                .Join(BaseCanvas.DOFade(1, duration)
                .OnComplete(() =>
                {
                    endEvent?.Invoke();
                })
                .SetEase(Ease.InOutQuad));
        }

        public void MoveYAndFade(Transform transform, float moveY, float duration = 0.1f, System.Action endEvent = null)
        {
            _ = DOTween.Sequence()
                .Append(transform.DOLocalMoveY(moveY, duration))
                .Join(BaseCanvas.DOFade(1, duration)
                .OnComplete(() =>
                {
                    endEvent?.Invoke();
                })
                .SetEase(Ease.InOutQuad));
        }

        public void AlphaZero()
        {
            BaseCanvas.alpha = 0;
        }
    }
}
