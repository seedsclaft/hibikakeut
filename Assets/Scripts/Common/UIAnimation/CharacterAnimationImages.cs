using UnityEngine;

namespace Ryneus
{
    public class CharacterAnimationImages : MonoBehaviour
    {
        [SerializeField] private Animator animator = null;
        [SerializeField] private int state = 0;
        private int _lastState = 0;

        private void Update()
        {
            if (_lastState != state)
            {
                _lastState = state;
                animator.SetInteger("State", state);
            }
        }
    }
}
