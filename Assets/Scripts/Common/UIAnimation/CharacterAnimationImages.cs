using System;
using System.Collections.Generic;
using DG.Tweening;
using Effekseer;
using UnityEngine;

namespace Ryneus
{
    public class CharacterAnimationImages : MonoBehaviour
    {
        [SerializeField] private Animator animator = null;
        [SerializeField] private AnimationState state = AnimationState.None;
        public bool IsStateDeath => state == AnimationState.Death;
        [SerializeField] private SpriteRenderer spriteRenderer = null;
        [SerializeField] private SpriteRenderer candidateSelect;
        [SerializeField] private _2dxFX_Blur blur = null;
        [SerializeField] private BattleFieldBattler fieldBattler = null;
        
        private AnimationState _lastState = 0;
        private bool _isEndMode = false;
        private List<Sequence> _sequences = new();
        private float _animationDuration => 1f / GameSystem.OptionData.BattleSpeed;

        public void Initialize(Action<BattlerInfo> decideEvent, Action<BattlerInfo> selectEvent)
        {
            AnimationUtility.Clear(_sequences);
            fieldBattler.Initialize(decideEvent, selectEvent);
        }

        public void SetAnimationState(AnimationState animationState, bool isEndMode = false)
        {
            _isEndMode = isEndMode;
            state = animationState;
            SetActiveCircle(animationState == AnimationState.Magic);
            if (animator == null)
            {
                // 敵汎用処理
                SetEnemyAnimation(state);
            }
        }

        public void Move(float x, float y, float duration)
        {
            if (_sequences.Count > 0)
            {
                _sequences.ForEach(a => a.Complete());
                _sequences.Clear();
            }
            var sequence = AnimationUtility.LocalMoveToTransform(transform, transform.localPosition, new Vector3(x, y, 1), duration);
            _sequences.Add(sequence);
        }

        public void Flip()
        {
            var scaleX = transform.localScale.x;
            transform.localScale = new Vector3(scaleX * -1, 1, 0);
        }

        public void Fade(float targetValue, float duration)
        {   
            if (_sequences.Count > 0)
            {
                _sequences.ForEach(a => a.Complete());
                _sequences.Clear();
            }
            if (blur != null)
            {
                AnimationUtility.FadeBlur(blur, blur._Alpha, targetValue, duration);
            }
        }

        public void Hide(float duration)
        {   
            if (_sequences.Count > 0)
            {
                _sequences.ForEach(a => a.Complete());
                _sequences.Clear();
            }
            if (blur != null)
            {
                AnimationUtility.FadeBlur(blur, blur._Alpha, 0, duration);
            }
        }

        private void SetEnemyAnimation(AnimationState animationState)
        {
            Sequence sequences = null;
            switch (animationState)
            {
                case AnimationState.Idle:
                    spriteRenderer.color = new Color(255, 255, 255, 255);
                    break;
                case AnimationState.BeforeStart:
                    sequences = AnimationUtility.AlphaToTransform(spriteRenderer, 0, 1, _animationDuration);
                    break;
                case AnimationState.Death:
                    sequences = AnimationUtility.AlphaToTransform(spriteRenderer, 1, 0, _animationDuration);
                    break;
            }
            if (sequences != null)
            {
                AnimationUtility.Clear(_sequences);
                _sequences.Add(sequences);
            }
        }

        public void SetActiveCircle(bool isActive)
        {
            if (fieldBattler == null)
            {
                return;
            }
            fieldBattler.SetActiveCircle(isActive);
        }

        public void SetMagicCircleEffect(AttributeType attributeType)
        {
            fieldBattler.SetMagicCircleEffect(attributeType);
        }

        public void UpdateInfo(BattlerInfo battlerInfo)
        {
            fieldBattler.UpdateInfo(battlerInfo, spriteRenderer, candidateSelect);
        }

        public void StartAnimation(EffekseerEffectAsset effectAsset, AnimationPosition animationPosition, float animationScale = 1.0f, float animationSpeed = 1.0f, bool soundPlay = true)
        {
            fieldBattler.StartAnimation(effectAsset, animationPosition, animationScale, animationSpeed, soundPlay);
        }

        public void ReplaceDamageRoot(GameObject parent)
        {
            fieldBattler.ReplaceDamageRoot(parent);
        }

        public void SetSelectArrow(bool isSelect)
        {
            fieldBattler.SetSelectArrow(isSelect);
        }

        public void SetActivecandidateSelect(bool isActive)
        {
            UIComponent.SetActive(candidateSelect?.gameObject, isActive);
        }

        private void Update()
        {
            if (_lastState != state && animator != null)
            {
                _lastState = state;
                animator.SetInteger("State", (int)state);
                var speed = fieldBattler != null ? GameSystem.OptionData.BattleSpeed : 1;
                animator.SetFloat("BattleSpeed", speed);
                if (_isEndMode)
                {
                    animator.Play(state.ToString(), 0 , 0.999f);
                    _isEndMode = false;
                }
            }
        }
    }

    public enum AnimationState
    {
        None = -1,
        Idle = 0,
        BeforeStart = 10, // 開始前
        Start = 11, // 開始演出
        Magic = 20, // 詠唱
        Hit = 30, // ダメージ
        Death = 100,
        Walk = 200,
        Run = 300,
        Special = 500,
    }
}
