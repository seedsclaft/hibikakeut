using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Effekseer;
using TMPro;
using System.Collections.Generic;

namespace Ryneus
{
    public class MainMenuReliefAnimation : MonoBehaviour
    {
        [SerializeField] private EffekseerEmitter emitter1;
        [SerializeField] private float emitter1Speed = 0.5f;
        [SerializeField] private EffekseerEmitter emitter2;
        [SerializeField] private float emitter2Speed = 0.5f;
        [SerializeField] private List<TextMeshProUGUI> serifs;
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private Image valkilyImage;
        [SerializeField] private Button decideButton;
        [SerializeField] private _2dxFX_SkyCloud skyCloud;
        [SerializeField] private float firstDuration = 3.7f;
        [SerializeField] private float secondDuration = 1.4f;
        [SerializeField] private float thirdDuration = 1f;
        [SerializeField] private float fourthDuration = 3f;
        private Action _endEvent;

        private float _targetAlpha = -1;
        public void Initialize()
        {
            decideButton.onClick.AddListener(() =>
            {
                _endEvent?.Invoke();
            });
        }

        public void PlayAnimation(Action endEvent, ActorInfo actorInfo, List<ActorInfo> releifActorInfos)
        {
            actorInfoComponent.UpdateInfo(actorInfo, null);
            valkilyImage.DOFade(0f, 0);
            ClearReleifSerif();
            var idx = 0;
            foreach (var releifActorInfo in releifActorInfos)
            {
                SetReleifSerif(releifActorInfo.Master.GetRelief(), idx);
                idx++;
            }
            _endEvent = endEvent;
            UIComponent.SetActive(decideButton?.gameObject, false);
            emitter1.speed = emitter1Speed;
            emitter1.Play();

            var time1 = firstDuration;
            DOTween.Sequence()
                .Append(emitter1.transform.DOScaleY(1f, time1))
                .OnComplete(() =>
                {
                    var idx = 0;
                    foreach (var releifActorInfo in releifActorInfos)
                    {
                        PlayReliefSerif(idx);
                        idx++;
                    }
                    PlayAnimationAfter(endEvent);
                });

            var time2 = secondDuration;
            DOTween.Sequence()
                .Append(emitter1.transform.DOScaleY(1f, time2))
                .Append(valkilyImage.DOFade(0.5f, time2))
                .OnComplete(() =>
                {
                    valkilyImage.DOFade(0f, time2);
                });
        }

        private void ClearReleifSerif()
        {
            foreach (var serif in serifs)
            {
                UIComponent.ClearText(serif);
            }
        }

        private void SetReleifSerif(string relief, int index)
        {
            UIComponent.SetText(serifs[index], relief);
            serifs[index].DOFade(0f, 0);
            serifs[index].DOScale(1f, 0);
        }

        private void PlayReliefSerif(int index)
        {
            var selif = serifs[index];
            var time1 = 1f;
            DOTween.Sequence()
                .Append(emitter1.transform.DOScaleY(1f, time1))
                .Join(selif.DOFade(1f, 0.5f))
                .Join(selif.DOScale(1.05f, 1.5f))
                .OnComplete(() =>
                {
                    selif.DOFade(0, 0.5f).SetDelay(1f);
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

            emitter2.speed = emitter2Speed;
            emitter2.Play();
            var time1 = thirdDuration;
            var time2 = fourthDuration;
            _targetAlpha = 0f;
            
            DOTween.Sequence()
                .Append(emitter1.transform.DOScaleY(1f, time1))
                .OnComplete(() =>
                {
                    emitter2.speed = emitter2Speed / 2;
                    DOTween.Sequence()
                        .Append(emitter1.transform.DOScaleY(1f, time2))
                        .OnComplete(() =>
                        {
                            _targetAlpha = -1f;
                            skyCloud._Alpha = 1;
                            endEvent?.Invoke();
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
