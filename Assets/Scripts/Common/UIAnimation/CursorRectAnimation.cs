using DG.Tweening;

namespace Ryneus
{
    public class CursorRectAnimation : BaseAnimation 
    {
        public void SelectAnimation(float duration = 0.6f)
        {
            BaseCanvas.alpha = 1;
            DOTween.Sequence()
                .Append(BaseCanvas.DOFade(0.5f,duration)
                .SetEase(Ease.InOutQuad))
                .SetLoops(-1,LoopType.Yoyo);
        }
    }
}
