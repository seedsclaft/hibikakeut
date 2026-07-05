using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class DemoActorEffect : MonoBehaviour
    {
        private _2dxFX_NewTeleportation2 fadeEffect;

        private float _time = 0;

        public void PlayAnimation()
        {
        }

        private void FixedUpdate()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            if (fadeEffect == null)
            {
                fadeEffect = gameObject.GetComponent<_2dxFX_NewTeleportation2>();
                return;
            }
            _time += 1;
            fadeEffect._Fade = 1f - _time * 0.2f;
            if (fadeEffect._Fade <= 0)
            {
                //gameObject.SetActive(false);
            }
        }
    }
}
