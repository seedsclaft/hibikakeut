using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Ryneus
{
    public class BattleThumb : MonoBehaviour
    {
        [SerializeField] private ActorInfoComponent actorInfoComponent = null;
        [SerializeField] private GameObject mainThumbRoot = null;
        [SerializeField] private GameObject awakenThumbRoot = null;
        [SerializeField] private CanvasGroup canvasGroup = null;

        private Sequence _sequence;

        private bool _animationBusy = false;

        public void ShowBattleThumb(BattlerInfo battlerInfo)
        {
            UIComponent.SetActive(gameObject, false);
            var image = actorInfoComponent.MainThumb;
            gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-24, 0, 0);
            //image.color = new Color(255,255,255,0);
            canvasGroup.alpha = 1;
            MoveAndFade(gameObject.GetComponent<RectTransform>(), 0, 1, 0.1f);
            UIComponent.SetActive(mainThumbRoot, true);
            UIComponent.SetActive(gameObject, true);
            UpdateThumb(battlerInfo.ActorInfo.Master);
        }

        public void HideThumb()
        {
            UIComponent.SetActive(mainThumbRoot, false);
            UIComponent.SetActive(awakenThumbRoot, false);
            UIComponent.SetActive(gameObject, false);
            Clear();
        }

        private void UpdateThumb(ActorData actorData)
        {
            actorInfoComponent.UpdateData(actorData);
        }

        public void ShowCutinBattleThumb(BattlerInfo battlerInfo)
        {
            if (_animationBusy)
            {
                Kill();
                _animationBusy = false;
            };
            UIComponent.SetActive(gameObject, false);
            if (!battlerInfo.IsActor)
            {
                return;
            }
            if (battlerInfo.ActorInfo == null)
            {
                return;
            }
            var image = actorInfoComponent.MainThumb;
            gameObject.GetComponent<RectTransform>().localPosition = new Vector3(20, 0, 0);
            canvasGroup.alpha = 1;
            _animationBusy = true;
            var waitFrame = 0.8f / GameSystem.OptionData.BattleSpeed;
            MoveAndFade(gameObject.GetComponent<RectTransform>(), 0, 0, waitFrame, () =>
            {
                _animationBusy = false;
            });
            UIComponent.SetActive(mainThumbRoot, true);
            UIComponent.SetActive(gameObject, true);
            UpdateThumb(battlerInfo.ActorInfo.Master);
        }

        public void MoveAndFade(RectTransform rect, float moveX, float fade, float duration = 0.1f, System.Action endEvent = null)
        {
            _sequence = DOTween.Sequence()
                .Append(rect.DOLocalMoveX(moveX, duration))
                .Join(canvasGroup.DOFade(fade, duration)
                .OnComplete(() =>
                {
                    if (endEvent != null) endEvent();
                })
                .SetEase(Ease.InOutQuad));
        }

        public void Kill()
        {
            _sequence?.Complete();
        }

        private void Clear()
        {
            actorInfoComponent.Clear();
        }
    }
}