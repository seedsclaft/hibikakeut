using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class MissionClearView : BaseView
    {
        [SerializeField] private TextMeshProUGUI titleText = null;
        [SerializeField] private CanvasGroup canvasGroup = null;

        public override void Initialize()

        {
            base.Initialize();
        }

        public void SetTitle(string title)
        {
            ClearText();
            titleText?.SetText(title);
            canvasGroup.alpha = 1;
            var basePosition = canvasGroup.GetComponent<RectTransform>().localPosition;
            var from = new Vector3(basePosition.x-320,basePosition.y,0);
            var to = new Vector3(basePosition.x,basePosition.y,0);
            AnimationUtility.LocalMoveToTransform(canvasGroup.gameObject,
                from,
                to,
                1f);
            AnimationUtility.AlphaToTransform(canvasGroup,
                1f,
                0,
                1,
                5);
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