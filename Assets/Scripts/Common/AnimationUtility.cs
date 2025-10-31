using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace Ryneus
{
    public class AnimationUtility
    {
        private static List<Sequence> _sequences = new();
        public static void LocalMoveToTransform(GameObject target, Vector3 from, Vector3 to, float duration)
        {
            target.GetComponent<RectTransform>().localPosition = from;
            var sequence = DOTween.Sequence()
                .Append(target.transform.DOLocalMove(to, duration))
                .SetEase(Ease.OutQuart);
            _sequences.Add(sequence);
        }

        public static void LocalMoveToLoopTransform(GameObject target, Vector3 from, Vector3 to, float duration)
        {
            target.GetComponent<RectTransform>().localPosition = from;
            var sequence = DOTween.Sequence()
                .Append(target.transform.DOLocalMove(to, duration))
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo);
            _sequences.Add(sequence);
        }

        public static void AlphaToTransform(CanvasGroup canvasGroup, float from, float to, float duration, float delay = 0f)
        {
            canvasGroup.alpha = from;
            if (delay > 0)
            {
                var sequence = DOTween.Sequence()
                    .SetDelay(delay)
                    .Append(canvasGroup.DOFade(to, duration))
                    .SetEase(Ease.OutQuart);
                _sequences.Add(sequence);
            } else
            {
                var sequence = DOTween.Sequence()
                    .Append(canvasGroup.DOFade(to, duration))
                    .SetEase(Ease.OutQuart);
                _sequences.Add(sequence);
            }
        }

        public static void CountUpText(TextMeshProUGUI text, int from, int to)
        {
            int nowNumber = from;
            int updateNumber = to;
            // 指定したupdateNumberまでカウントアップ・カウントダウンする
            DOTween.To(() => nowNumber, (n) => nowNumber = n, updateNumber, 0.5f)
                .OnUpdate(() => text.text = nowNumber.ToString("#,0"));
        }

        public static void Clear()
        {
            _sequences.ForEach(a => a.Kill());
        }
    }
}