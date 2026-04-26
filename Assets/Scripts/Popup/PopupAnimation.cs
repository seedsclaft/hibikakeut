using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class PopupAnimation : BaseAnimation, IBaseAnimation
    {
        public void Initialize(Transform transform)
        {
            transform.DOScale(0, 0);
            BaseCanvas.alpha = 0;
        }

        public void OpenAnimation(Transform transform, System.Action endEvent, float duration = 0.1f)
        {
            Busy.SetValue(true);
            transform.DOScale(0, duration);
            BaseCanvas.alpha = 0;
            _ = DOTween.Sequence()
                .Append(transform.DOScale(1, duration))
                .Join(BaseCanvas.DOFade(1, duration)
                .OnComplete(() =>
                {
                    Busy.SetValue(false);
                    endEvent?.Invoke();
                })
                .SetEase(Ease.InOutQuad));
        }

        public void MoveXAndFade(Transform transform, float moveX, float duration = 0.1f, System.Action endEvent = null)
        {
            _ = DOTween.Sequence()
                .Append(transform.DOLocalMoveX(moveX, duration))
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
