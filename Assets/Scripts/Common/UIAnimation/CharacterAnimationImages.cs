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
        [SerializeField] private BattlerInfoComponent battlerInfoComponent = null;
        [SerializeField] private GameObject damageRoot;
        [SerializeField] private GameObject magicCircle;
        [SerializeField] private GameObject candidateSelect;
        [SerializeField] private GameObject selectArrow;
        private AnimationState _lastState = 0;

        public void SetAnimationState(AnimationState animationState)
        {
            state = animationState;
            SetActiveCircle(animationState == AnimationState.Magic);
        }

        public void SetActiveCircle(bool isActive)
        {
            magicCircle.SetActive(isActive);
        }

        public void UpdateInfo(BattlerInfo battlerInfo)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = battlerInfo.Index.Value;
            }
            battlerInfoComponent.UpdateInfo(battlerInfo);
            SetDamageRoot();
        }

        public void StartAnimation(EffekseerEffectAsset effectAsset, AnimationPosition animationPosition, float animationScale = 1.0f, float animationSpeed = 1.0f, bool soundPlay = true)
        {
            battlerInfoComponent.StartFieldAnimation(effectAsset, animationPosition, animationScale, animationSpeed, soundPlay);
        }

        public void SetDamageRoot()
        {
            battlerInfoComponent.SetDamageRoot(damageRoot);
        }

        public void SetSelectArrow(bool isSelect)
        {
            if (selectArrow == null)
            {
                return;
            }
            selectArrow.SetActive(isSelect);
        }

        public void SetActivecandidateSelect(bool isActive)
        {
            if (candidateSelect == null)
            {
                return;
            }
            candidateSelect.SetActive(isActive);
        }

        private void Update()
        {
            if (_lastState != state)
            {
                _lastState = state;
                animator.SetInteger("State", (int)state);
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
    }
}
