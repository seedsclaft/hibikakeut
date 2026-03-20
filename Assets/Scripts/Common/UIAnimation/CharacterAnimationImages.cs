using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Effekseer;
using UnityEngine;
using UnityEngine.UI;
using Utage;

namespace Ryneus
{
    public class CharacterAnimationImages : MonoBehaviour
    {
        [SerializeField] private Animator animator = null;
        [SerializeField] private AnimationState state = AnimationState.None;
        public bool IsStateDeath => state == AnimationState.Death;
        [SerializeField] private SpriteRenderer spriteRenderer = null;
        [SerializeField] private SpriteRenderer candidateSelect;
        [SerializeField] private BattlerInfoComponent battlerInfoComponent = null;
        [SerializeField] private GameObject damageRoot;
        [SerializeField] private GameObject magicCircle;
        [SerializeField] private GameObject selectArrow;
        [SerializeField] private Button selectButton = null;
        private AnimationState _lastState = 0;
        private BattlerInfo _battlerInfo;
        private List<Sequence> _sequences = new();
        private float _animationDuration => 1f / GameSystem.OptionData.BattleSpeed;

        public void Initilize(Action<BattlerInfo> decideEvent, Action<BattlerInfo> selectEvent)
        {
            AnimationUtility.Clear(_sequences);
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(() =>
                {
                    decideEvent.Invoke(_battlerInfo);
                });
                var enterListener = selectButton.gameObject.AddComponent<ContentEnterListener>();
                enterListener.SetEnterEvent(() =>
                {
                    selectEvent?.Invoke(_battlerInfo);
                });
            }
        }

        public void SetAnimationState(AnimationState animationState)
        {
            state = animationState;
            SetActiveCircle(animationState == AnimationState.Magic);
            if (animator == null)
            {
                // 敵汎用処理
                SetEnemyAnimation(state);
            }
        }

        public void Move(int x, int y, float duration)
        {
            AnimationUtility.LocalMoveToTransform(transform, transform.localPosition, new Vector3(x,y,1), duration);
        }

        private void SetEnemyAnimation(AnimationState animationState)
        {
            Sequence sequences = null;
            switch (animationState)
            {
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
            UIComponent.SetActive(magicCircle, isActive);
        }

        public void UpdateInfo(BattlerInfo battlerInfo)
        {
            _battlerInfo = battlerInfo;
            battlerInfoComponent.UpdateInfo(battlerInfo);
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = battlerInfo.Index.Value;
                candidateSelect.sortingOrder = battlerInfo.Index.Value + 1;
                if (!battlerInfo.IsActor)
                {
                    UIComponent.SetSpeiteImage(spriteRenderer, ResourceSystem.EnemySpritePath(battlerInfo.EnemyData.ImagePath));
                    UIComponent.SetSpeiteImage(candidateSelect, ResourceSystem.EnemySpritePath(battlerInfo.EnemyData.ImagePath));
                }
            }
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
            UIComponent.SetActive(selectArrow, isSelect);
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
                animator.SetFloat("BattleSpeed", GameSystem.OptionData.BattleSpeed);
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
    }
}
