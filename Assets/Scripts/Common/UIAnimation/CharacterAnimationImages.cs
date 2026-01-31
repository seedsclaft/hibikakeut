using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class CharacterAnimationImages : MonoBehaviour
    {
        [SerializeField] private Image image = null;

        private Sprite[] _animationSprites = null;
        private int count = 0;
        private float delta = 0;

        void Start()
        {
            _animationSprites = ResourceSystem.LoadActorAnimation("0001/Animation/Idle");
        }

        void Update()
        {
            if (_animationSprites != null)
            {
			    delta += Time.deltaTime;
                if (delta > 0.066f)
                {
                    delta = 0;
                    count += 2;
                    if (count > 34)
                    {
                        count = 0;
                    }
                }
                image.sprite = _animationSprites[count];
            }
        }
    }
}
