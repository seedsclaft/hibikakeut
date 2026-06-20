using System;
using UnityEngine;
using UnityEngine.UI;
using Effekseer;
using System.Collections.Generic;

namespace Ryneus
{
    public class BattleFieldBattler : MonoBehaviour
    {
        [SerializeField] private BattlerInfoComponent battlerInfoComponent = null;
        [SerializeField] private GameObject damageRoot;
        [SerializeField] private GameObject magicCircle;
        [SerializeField] private EffekseerEmitter magicCircleEmitter;
        [SerializeField] private GameObject selectArrow;
        [SerializeField] private Button selectButton = null;
        [SerializeField] private List<EffekseerEffectAsset> attributeTypeEffects = null;

        private BattlerInfo _battlerInfo;

        public void Initialize(Action<BattlerInfo> decideEvent, Action<BattlerInfo> selectEvent)
        {
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

        public void UpdateInfo(BattlerInfo battlerInfo, SpriteRenderer spriteRenderer, SpriteRenderer candidateSelect)
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

        public void ReplaceDamageRoot(GameObject parent)
        {
            damageRoot.transform.SetParent(parent.transform);
        }

        public void SetActiveCircle(bool isActive)
        {
            UIComponent.SetActive(magicCircle, isActive);
        }

        public void SetMagicCircleEffect(AttributeType attributeType)
        {
            magicCircleEmitter.effectAsset = attributeTypeEffects[(int)attributeType];
        }

        public void SetSelectArrow(bool isSelect)
        {
            UIComponent.SetActive(selectArrow, isSelect);
        }
    }
}
