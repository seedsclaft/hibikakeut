using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace Ryneus
{
    public class MainMenuStartAnim : MonoBehaviour
    {
        [SerializeField] private Image lineWhite;
        [SerializeField] private CanvasGroup bgCanvas;
        [SerializeField] private Image backBlack;
        [SerializeField] private CanvasGroup chapterTextCanvas;
        [SerializeField] private TextMeshProUGUI chapterText;
        [SerializeField] private CanvasGroup periodTextCanvas;
        [SerializeField] private TextMeshProUGUI periodText;
        [SerializeField] private TextMeshProUGUI maxPeriodText;
        [SerializeField] private CanvasGroup remainTextCanvas;
        [SerializeField] private TextMeshProUGUI remainText;

        public ParameterBool IsBusy = new();

        private Sequence _sequence;

        public void Reset()
        {
            chapterTextCanvas.alpha = 0;
            periodTextCanvas.alpha = 0;
            remainTextCanvas.alpha = 0;
            backBlack.GetComponent<RectTransform>().DOScaleY(0, 0);
            chapterTextCanvas.GetComponent<RectTransform>().DOLocalMoveX(0, 0);
            periodTextCanvas.GetComponent<RectTransform>().DOLocalMoveX(0, 0);
            remainTextCanvas.GetComponent<RectTransform>().DOLocalMoveX(0, 0);
        }

        public void SetText(int chapter, int period, int periodMax, int remain)
        {
            chapterText.SetText(chapter.ToString());
            periodText.SetText(period.ToString());
            maxPeriodText.SetText(periodMax.ToString());
            remainText.SetText(remain.ToString());
        }

        public void StartAnim(float delay = 0, System.Action endEvent = null)
        {
            IsBusy.SetValue(true);
            Reset();
            var speedRate = 1;
            var duration = 0.1f / speedRate;
            var animationDuration = 0.8f;
            _sequence = DOTween.Sequence()
                //.SetDelay(duration + delay)
                .Append(backBlack.GetComponent<RectTransform>().DOScaleY(1, 0.4f))
                .Join(chapterTextCanvas.DOFade(1f, animationDuration))
                .Join(periodTextCanvas.DOFade(1f, animationDuration))
                .Join(chapterTextCanvas.GetComponent<RectTransform>().DOLocalMoveX(-40, animationDuration))
                .Join(periodTextCanvas.GetComponent<RectTransform>().DOLocalMoveX(-40, animationDuration))
                .Append(chapterTextCanvas.DOFade(1f, 0.2f))
                .Append(remainTextCanvas.DOFade(1f, animationDuration))
                .Join(remainTextCanvas.GetComponent<RectTransform>().DOLocalMoveX(-40, animationDuration)).OnUpdate(() =>
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(backBlack.transform.parent.GetComponent<RectTransform>());
                });

        }

        public void EndAnimation()
        {
            if (_sequence != null)
            {
                _sequence.Complete();
                _sequence.Kill();
            }
            _sequence = DOTween.Sequence()
                //.SetDelay(duration + delay)
                .Append(gameObject.GetComponent<RectTransform>().DOScaleY(0, 0.4f));
        }
    }
}