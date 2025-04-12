using DG.Tweening;

namespace Ryneus
{
    public class CursorRectAnimation : BaseAnimation 
    {
        public void SelectAnimation(float duration = 0.6f)
        {
            BaseCanvas.alpha = 0.75f;
            DOTween.Sequence()
                .Append(BaseCanvas.DOFade(0.25f,duration)
                .SetEase(Ease.InOutQuad))
                .SetLoops(-1,LoopType.Yoyo);
        }
    }
}
