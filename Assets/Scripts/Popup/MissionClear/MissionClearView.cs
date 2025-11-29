using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Ryneus
{
    public class MissionClearView : BaseView
    {
        [SerializeField] private TextMeshProUGUI titleText = null;
        [SerializeField] private CanvasGroup canvasGroup = null;

        private List<Sequence> _sequences = new();
        public override void Initialize()
        {
            AnimationUtility.Clear(_sequences);
            ClearText();
            canvasGroup.alpha = 1;
            base.Initialize();
        }

        public void SetTitle(string title)
        {
            titleText?.SetText(title);
            var basePosition = canvasGroup.GetComponent<RectTransform>().localPosition;
            var from = new Vector3(basePosition.x-320,basePosition.y,0);
            var to = new Vector3(basePosition.x,basePosition.y,0);
            var sequence = AnimationUtility.LocalMoveToTransform(canvasGroup.gameObject,
                from,
                to,
                1f);
            _sequences.Add(sequence);
            var sequence2 = AnimationUtility.AlphaToTransform(canvasGroup,
                1f,
                0,
                1,
                5);
            _sequences.Add(sequence2);
        }


        private void ClearText()
        {
            titleText.SetText("");
        }
    }

    public class MissionClearInfo
    {
        private string _title = "";
        public string Title => _title;
        public void SetTitle(string title)
        {
            _title = title;
        }
    }
}