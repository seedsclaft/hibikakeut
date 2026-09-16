using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace Ryneus
{
    public class BattleStartAnim : MonoBehaviour
    {
        [SerializeField] private Image backBlack;
        [SerializeField] private Image lineWhite;
        [SerializeField] private TextMeshProUGUI mainText;
        [SerializeField] private TextMeshProUGUI subText;

        private bool _busy = false;
        public bool IsBusy => _busy;
        private List<Sequence> _sequences = new();


        private void OnEnable()
        {
            //Reset();
        }

        public void Reset()
        {
            if (mainText != null)
            {
                mainText.color = new Color(255, 255, 255, 0);
                mainText.transform.DOMoveX(0, 0);
            }
            if (subText != null)
            {
                subText.color = new Color(255, 255, 255, 0);
                subText.transform.DOMoveX(0, 0);
            }
            if (backBlack != null)
            {
                backBlack.color = new Color(0, 0, 0, 0);
            }
            if (lineWhite != null)
            {
                lineWhite.color = new Color(255, 255, 255, 255);
                lineWhite.transform.DOScaleY(0, 0);
            }
            foreach (var sequences in _sequences)
            {
                sequences.Kill();
            }
            _sequences.Clear();
        }

        public void SetText(string text)
        {
            UIComponent.SetText(mainText, text);
            UIComponent.SetText(subText, text);
        }

        public void StartAnim(bool inBattle, float delay = 0, System.Action endEvent = null)
        {
            _busy = true;
            Reset();
            var speedRate = inBattle ? GameSystem.OptionData.BattleSpeed : 1;
            var duration = 0.1f / speedRate;
            mainText.transform.DOScaleY(0.95f, 0);
            var main = DOTween.Sequence()
                .SetDelay(duration + delay)
                .Append(mainText.DOFade(1f, 0f))
                .Append(mainText.transform.DOScale(0.95f, duration * 4))
                .AppendInterval(duration * 12)
                .Append(mainText.DOFade(0f, duration))
                .Join(mainText.transform.DOLocalMoveX(-480, duration));
                _sequences.Add(main);


            subText.transform.DOScaleY(0.95f, 0);
            var sub = DOTween.Sequence()
                .SetDelay(duration + delay)
                .Append(subText.DOFade(1f, 0f))
                .Append(subText.transform.DOScale(1.25f, duration * 8))
                .Join(subText.DOFade(0, duration * 8))
                .AppendInterval(duration * 8)
                .Append(subText.transform.DOScale(1f, 0f))
                .Join(subText.DOFade(1, 0f))
                .Append(subText.DOFade(0f, duration))
                .Join(subText.transform.DOLocalMoveX(480, duration));
                _sequences.Add(sub);

            lineWhite.transform.DOScaleY(0.95f, 0);
            var white = DOTween.Sequence()
                .SetDelay(delay)
                .Append(lineWhite.transform.DOScaleY(1f, duration / 5))
                .Append(lineWhite.transform.DOScaleY(0.08f, duration))
                .Join(lineWhite.DOFade(0f, duration * 16))
                .SetEase(Ease.InOutQuad);
                _sequences.Add(white);

            var black = DOTween.Sequence()
                .SetDelay(duration + delay)
                .Append(backBlack.DOFade(1f, duration))
                .Join(backBlack.transform.DOScaleY(1f, duration))
                .AppendInterval(duration * 16)
                .Append(backBlack.DOFade(0f, duration))
                .Join(backBlack.transform.DOScaleY(0f, duration))
                .OnComplete(() =>
                {
                    endEvent?.Invoke();
                    _busy = false;
                });
                _sequences.Add(black);
        }

        private void OnDestroy()
        {
            foreach (var sequences in _sequences)
            {
                sequences.Kill();
            }
            _sequences.Clear();
            DOTween.Kill(this); 
        }
    }
}