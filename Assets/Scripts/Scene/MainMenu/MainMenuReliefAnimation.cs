using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Effekseer;
using TMPro;

namespace Ryneus
{
    public class MainMenuReliefAnimation : MonoBehaviour
    {
        [SerializeField] private EffekseerEmitter emitter1;
        [SerializeField] private EffekseerEmitter emitter2;
        [SerializeField] private TextMeshProUGUI serif;
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private Image valkilyImage;
        [SerializeField] private Button decideButton;
        [SerializeField] private _2dxFX_SkyCloud skyCloud;
        private Action _endEvent;

        private float _targetAlpha = -1;
        public void Initialize()
        {
            decideButton.onClick.AddListener(() =>
            {
                _endEvent?.Invoke();
            });
        }

        public void PlayAnimation(Action endEvent, ActorInfo actorInfo)
        {
            actorInfoComponent.UpdateInfo(actorInfo, null);
            valkilyImage.DOFade(0f, 0);
            serif.SetText("「立場に縛られて……\n　　　　　　　　私は…」");
            serif.DOFade(0f, 0);
            serif.DOScale(1f, 0);
            _endEvent = endEvent;
            decideButton.gameObject.SetActive(false);
            emitter1.speed = 0.5f;
            emitter1.Play();

            var time1 = 3.7f;
            DOTween.Sequence()
                .Append(emitter1.transform.DOScaleY(1f, time1))
                .OnComplete(() =>
                {
                    PlayAnimationAfter(endEvent);
                });

            var time2 = 1.4f;
            DOTween.Sequence()
                .Append(emitter1.transform.DOScaleY(1f, time2))
                .Append(valkilyImage.DOFade(0.5f, time2))
                .OnComplete(() =>
                {
                    valkilyImage.DOFade(0f, time2);
                });
        }
        
        private void PlayAnimationAfter(Action endEvent)
        {
            DOTween.Sequence()
                .Append(emitter1.transform.DOScaleY(1f, 0.5f))
                .OnComplete(() =>
                {
                    emitter1.Stop();
                });

            emitter2.speed = 0.5f;
            emitter2.Play();
            var time1 = 1f;
            var time2 = 1.8f;
            _targetAlpha = 0f;
            
            DOTween.Sequence()
                .Append(emitter1.transform.DOScaleY(1f, time1))
                .Join(serif.DOFade(1f, 0.5f))
                .Join(serif.DOScale(1.05f, 1.5f))
                .OnComplete(() =>
                {
                    emitter2.speed = 0.25f;
                    DOTween.Sequence()
                        .Append(emitter1.transform.DOScaleY(1f, time2))
                        .OnComplete(() =>
                        {
                            serif.DOFade(0, 0.5f);
                            endEvent?.Invoke();
                            _targetAlpha = -1f;
                            skyCloud._Alpha = 1;
                            //decideButton.gameObject.SetActive(true);
                        });
                });
        }

        void Update()
        {
            if (_targetAlpha == -1)
            {
                return;
            }
            
            skyCloud._Alpha = 1 - _targetAlpha;
            if (_targetAlpha < 1)
            {
                _targetAlpha += 0.015f;
            }
        }
    }
}
