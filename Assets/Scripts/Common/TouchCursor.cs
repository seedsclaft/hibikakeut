using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;

namespace Ryneus
{
    public class TouchCursor : MonoBehaviour
    {
        [SerializeField] private RectTransform touchCursorRect = null;
        [SerializeField] private ParticleSystem particle = null;
        [SerializeField] private Image circle = null;

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                StartTouchAnimation();
            }
        }

        private void StartTouchAnimation()
        {
            var pos = Mouse.current.position.ReadValue();
            Vector3 target = Camera.main.ScreenToWorldPoint(pos);
            pos.x = target.x;
            pos.y = target.y;
            //pos.z = 10f;

            touchCursorRect.transform.position = new Vector3(pos.x, pos.y, 10f);
            UIComponent.SetActive(gameObject, false);
            UIComponent.SetActive(gameObject, true);
            particle.Play();

            circle.DOFade(1, 0);
            circle.transform.DOScale(0, 0);

            var duration = 0.4f;
            circle.DOFade(0, duration);
            circle.transform.DOScale(1, duration);
        }
    }
}
