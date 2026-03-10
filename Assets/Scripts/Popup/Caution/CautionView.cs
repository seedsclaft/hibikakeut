using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Ryneus
{
    public class CautionView : BaseView
    {
        [SerializeField] private TextMeshProUGUI titleText = null;
        [SerializeField] private TextMeshProUGUI evaluateText = null;
        [SerializeField] private TextMeshProUGUI levelPlusText = null;
        [SerializeField] private CanvasGroup canvasGroup = null;

        private List<Sequence> _sequences = new();
        public override void Initialize()
        {
            AnimationUtility.Clear(_sequences);
            base.Initialize();
        }


        public void SetTitle(string title)
        {
            ClearText();
            UIComponent.SetText(titleText, title);
            canvasGroup.alpha = 1;
            var sequence = AnimationUtility.AlphaToTransform(canvasGroup,
                1f,
                0,
                1,
                3);
            _sequences.Add(sequence);
        }

        public void SetLevelup(int from, int to)
        {
            ClearText();
            UIComponent.SetText(levelPlusText, "+" + (to - from).ToString());
            AnimationUtility.CountUpText(evaluateText, from, to);
            canvasGroup.alpha = 1;
            var sequence = AnimationUtility.AlphaToTransform(canvasGroup,
                1f,
                0,
                1,
                3);
            _sequences.Add(sequence);
        }

        private void ClearText()
        {
            UIComponent.ClearText(titleText);
            UIComponent.ClearText(evaluateText);
            UIComponent.ClearText(levelPlusText);
        }
    }

    public class CautionInfo
    {
        public ParameterString Title = new();
        public ParameterInt From = new();
        public ParameterInt To = new();
        public void SetLevelUp(int from, int to)
        {
            From.SetValue(from);
            To.SetValue(to);
        }
    }
}