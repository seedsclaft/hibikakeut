using DG.Tweening;

namespace Ryneus
{
    public class CursorRectAnimation : BaseAnimation
    {
        private Sequence _sequence;
        public void SelectAnimation(float duration = 0.6f)
        {
            if (BaseCanvas == null)
            {
                return;
            }
            BaseCanvas.alpha = 0.75f;
            _sequence = DOTween.Sequence()
                .Append(BaseCanvas.DOFade(0.25f, duration)
                .SetEase(Ease.InOutQuad))
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void StopAnimation()
        {
            if (_sequence != null)
            {
                _sequence.Kill();
                _sequence = null;
            }
        }
    }
}
