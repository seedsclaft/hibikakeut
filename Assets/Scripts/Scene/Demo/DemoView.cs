using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Ryneus
{
    public class DemoView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private TweensController firstTween;
        [SerializeField] private TweensController firstTween2;
        [SerializeField] private TweensController firstTween3;
        [SerializeField] private SpriteRenderer secondImage;
        [SerializeField] private TweensController secondTween;
        [SerializeField] private GameObject particlObject;
        [SerializeField] private MainMenuReliefAnimation reliefAnimation;
        [SerializeField] private TweensController thirdTween;
        [SerializeField] private TweensController thirdTween2;
        [SerializeField] private TweensController fourthTween;
        [SerializeField] private TweensController fifthTween;
        [SerializeField] private TweensController fifthTween2;
        [SerializeField] private TweensController fifthTween3;
        [SerializeField] private TweensController fifthTween4;
        [SerializeField] private TweensController sixthTween;
        [SerializeField] private TweensController sixthTween2;
        [SerializeField] private TweensController sixthTween3;
        [SerializeField] private TweensController sixthTween4;
        [SerializeField] private TweensController seventhTween;
        [SerializeField] private TweensController seventhTween2;
        [SerializeField] private TweensController seventhTween3;
        [SerializeField] private TweensController seventhTween4;
        [SerializeField] private List<TweensController> eighthTweens;
        [SerializeField] private GameObject eighthRoot;
        [SerializeField] private TweensController eighthBgTween;
        [SerializeField] private TweensController ninethTween;
        [SerializeField] private TweensController ninethTween2;
        [SerializeField] private TweensController ninethTween3;
        [SerializeField] private TweensController ninethTween4;
        [SerializeField] private TweensController tenthTween;
        [SerializeField] private TweensController tenthTween2;
        [SerializeField] private TweensController tenthTween3;
        [SerializeField] private TweensController tenthTween4;
        [SerializeField] private TweensController elevenTween;
        [SerializeField] private TweensController elevenTween2;

        [SerializeField] private float BPM = 120;
        [SerializeField] private float idelTime = 0.1f;

        private float _time = -1;
        private float _lastCount = 1;
        private float _lastMiniCount = 1;
        public override void Initialize()
        {
            base.Initialize();
            if (reliefAnimation != null)
            {
                reliefAnimation.Initialize();
            }
            _ = new DemoPresenter(this);
        }

        public void StartAnimation()
        {
            _time = 0;
            var miniTiming = 60f / BPM;
        }

        private void FixedUpdate()
        {
            if (_time >= 0)
            {
                _time += Time.fixedDeltaTime;
                var miniTiming = 60f / BPM;
                var timing = miniTiming * 4;
                if ((_time + idelTime) >= timing * _lastCount)
                {
                    LogOutput.Log("count: " + _lastCount);
                    _lastCount++;
                }
                if ((_time + idelTime) >= miniTiming * _lastMiniCount)
                {
                    LogOutput.Log("mini count: " + _lastMiniCount);

                    if (_lastMiniCount == 1)
                    {
                        PlayTween(firstTween, miniTiming);
                    }
                    if (_lastMiniCount == 3)
                    {
                        PlayTween(firstTween2, miniTiming);
                    }
                    if (_lastMiniCount == 5)
                    {
                        PlayTween(firstTween3, miniTiming);
                    }
                    if (_lastMiniCount == 8)
                    {
                        RenderSettings.skybox = null;
                        //playerCamera.SetActive(false);
                        if (particlObject != null)
                        {
                            particlObject.SetActive(true);
                        }
                        if (secondImage != null)
                        {
                            secondImage.DOFade(1, miniTiming);
                        }
                    }
                    if (_lastMiniCount == 8)
                    {
                        var actor = new ActorInfo(DataSystem.FindActor(101));
                        reliefAnimation.PlayAnimation(() => {}, new ActorInfo(DataSystem.FindActor(102)), new List<ActorInfo>(){actor});
                    }
                    if (_lastMiniCount == 9)
                    {
                        PlayTween(secondTween, miniTiming);
                    }
                    if (_lastMiniCount == 21)
                    {
                        if (secondImage != null)
                        {
                            secondImage.DOFade(0, miniTiming);
                        }
                        PlayTween(thirdTween, miniTiming);
                    }
                    if (_lastMiniCount == 29)
                    {
                        PlayTween(thirdTween2, miniTiming);
                    }
                    if (_lastMiniCount == 35)
                    {
                        PlayTween(fourthTween, miniTiming);
                    }
                    if (_lastMiniCount == 38)
                    {
                        if (particlObject != null)
                        {
                            particlObject.SetActive(false);
                        }
                    }
                    if (_lastMiniCount == 37)
                    {
                        if (secondImage != null)
                        {
                            secondImage.DOFade(1, 0);
                        }
                    }

                    if (_lastMiniCount == 37)
                    {
                        PlayTween(fifthTween, miniTiming);
                    }
                    if (_lastMiniCount == 38)
                    {
                        PlayTween(fifthTween2, miniTiming);
                    }
                    if (_lastMiniCount == 39)
                    {
                        PlayTween(fifthTween3, miniTiming);
                    }
                    if (_lastMiniCount == 40)
                    {
                        PlayTween(fifthTween4, miniTiming);
                    }

                    if (_lastMiniCount == 41)
                    {
                        PlayTween(sixthTween, miniTiming);
                    }
                    if (_lastMiniCount == 42)
                    {
                        PlayTween(sixthTween2, miniTiming);
                    }
                    if (_lastMiniCount == 43)
                    {
                        PlayTween(sixthTween3, miniTiming);
                    }
                    if (_lastMiniCount == 44)
                    {
                        PlayTween(sixthTween4, miniTiming);
                    }
                    
                    if (_lastMiniCount == 45)
                    {
                        PlayTween(seventhTween, miniTiming);
                    }
                    if (_lastMiniCount == 46)
                    {
                        PlayTween(seventhTween2, miniTiming);
                    }
                    if (_lastMiniCount == 47)
                    {
                        PlayTween(seventhTween3, miniTiming);
                    }
                    if (_lastMiniCount == 48)
                    {
                        PlayTween(seventhTween4, miniTiming);
                    }

                    if (_lastMiniCount == 49)
                    {
                        eighthRoot.SetActive(true);
                        PlayTween(eighthBgTween, miniTiming);
                        foreach (var eighthTween in eighthTweens)
                        {
                            PlayTween(eighthTween, miniTiming);
                        }
                    }

                    if (_lastMiniCount == 53)
                    {
                        eighthRoot.SetActive(false);
                        PlayTween(ninethTween, miniTiming);
                    }
                    if (_lastMiniCount == 54)
                    {
                        PlayTween(ninethTween2, miniTiming);
                    }
                    if (_lastMiniCount == 55)
                    {
                        PlayTween(ninethTween3, miniTiming);
                    }
                    if (_lastMiniCount == 56)
                    {
                        PlayTween(ninethTween4, miniTiming);
                    }

                    if (_lastMiniCount == 57)
                    {
                        PlayTween(tenthTween, miniTiming);
                    }
                    if (_lastMiniCount == 58)
                    {
                        PlayTween(tenthTween2, miniTiming);
                    }
                    if (_lastMiniCount == 59)
                    {
                        PlayTween(tenthTween3, miniTiming);
                    }
                    if (_lastMiniCount == 60)
                    {
                        PlayTween(tenthTween4, miniTiming);
                    }

                    if (_lastMiniCount == 61)
                    {
                        PlayTween(elevenTween, miniTiming);
                        PlayTween(elevenTween2, miniTiming);
                    }
                    _lastMiniCount++;
                }
            }
        }

        private void PlayTween(TweensController tweensController, float miniTiming)
        {
            if (tweensController == null)
            {
                return;
            }
            tweensController.ConvertDurationTiming(miniTiming);
            tweensController.PlayTween();
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (InputSystem.GetInputDate(InputKeyType.Option2).IsDownTrigger())
            {
                SoundManager.Instance.StopBgm();
                CommandSceneChange(Scene.Demo);
            }
        }
    }
}
