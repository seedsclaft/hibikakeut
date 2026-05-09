using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Ryneus
{
    public class BattleThumb : MonoBehaviour
    {
        [SerializeField] private GameObject mainThumbRoot = null;
        [SerializeField] private CanvasGroup canvasGroup = null;
        private Dictionary<int, GameObject> _prefabDicts = new();

        private Sequence _sequence;

        private bool _animationBusy = false;

        public void ShowBattleThumb(BattlerInfo battlerInfo)
        {
            UIComponent.SetActive(gameObject, false);
            UpdateThumb(battlerInfo, () =>
            {
                gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-24, 0, 0);
                canvasGroup.alpha = 1;
                MoveAndFade(gameObject.GetComponent<RectTransform>(), 0, 1, 0.1f);
            });
        }

        public void HideThumb()
        {
            UIComponent.SetActive(mainThumbRoot, false);
            UIComponent.SetActive(gameObject, false);
            Clear();
        }

        private void UpdateThumb(BattlerInfo battlerInfo, Action endEvent)
        {
            if (battlerInfo.ActorInfo == null)
            {
                return;
            }
            if (!_prefabDicts.ContainsKey(battlerInfo.ActorInfo.ActorId.Value))
            {
                UIComponent.SetPrefab(mainThumbRoot, ResourceSystem.ActorBattleThumbPath(battlerInfo.ActorInfo.Master.ImagePath), (a) =>
                {
                    _prefabDicts[battlerInfo.ActorInfo.ActorId.Value] = a;
                    UpdateEffects(battlerInfo, endEvent);
                });
            } else
            {
                UpdateEffects(battlerInfo, endEvent);
            }
        }

        private void UpdateEffects(BattlerInfo battlerInfo, Action endEvent)
        {
            if (!_prefabDicts.ContainsKey(battlerInfo.ActorInfo.ActorId.Value))
            {
                return;
            }
            foreach (var prefabDicts in _prefabDicts)
            {
                _prefabDicts[prefabDicts.Key].gameObject.SetActive(false);
            }
            var prefab = _prefabDicts[battlerInfo.ActorInfo.ActorId.Value];
            var thumb = prefab.GetComponent<BattleActorThumb>();
            thumb.SetActorData(battlerInfo.ActorInfo.Master);
            thumb.SetAwaken(battlerInfo.IsAwaken);
            prefab.SetActive(true);
            UIComponent.SetActive(mainThumbRoot, true);
            UIComponent.SetActive(gameObject, true);
            endEvent?.Invoke();
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
            UpdateThumb(battlerInfo, () =>
            {
                gameObject.GetComponent<RectTransform>().localPosition = new Vector3(20, 0, 0);
                canvasGroup.alpha = 1;
                _animationBusy = true;
                var waitFrame = 0.8f / GameSystem.OptionData.BattleSpeed;
                MoveAndFade(gameObject.GetComponent<RectTransform>(), 0, 0, waitFrame, () =>
                {
                    _animationBusy = false;
                    Clear();
                });
            });
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
            UIComponent.SetActive(mainThumbRoot, false);
            UIComponent.SetActive(gameObject, false);
        }
    }
}